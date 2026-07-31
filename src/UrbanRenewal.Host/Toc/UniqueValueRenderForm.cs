using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace UrbanRenewal.Host
{
    /// <summary>
    /// 点/线/面图层唯一值渲染（UniqueValueRenderer）。
    /// </summary>
    public partial class UniqueValueRenderForm : Form
    {
        private readonly IFeatureLayer _featureLayer;
        private esriGeometryType _shapeType = esriGeometryType.esriGeometryNull;

        public bool Applied { get; private set; }

        public UniqueValueRenderForm(IFeatureLayer featureLayer)
        {
            InitializeComponent();
            _featureLayer = featureLayer;
            if (featureLayer != null)
            {
                lblLayer.Text = featureLayer.Name;
                if (featureLayer.FeatureClass != null)
                {
                    _shapeType = featureLayer.FeatureClass.ShapeType;
                }
            }

            lblGeom.Text = GeometryText(_shapeType);
            chkDrawOutline.Visible = _shapeType == esriGeometryType.esriGeometryPolygon;
            chkDrawOutline.Checked = true;
            LoadFields();
        }

        private void LoadFields()
        {
            cboField.Items.Clear();
            if (_featureLayer == null || _featureLayer.FeatureClass == null)
            {
                return;
            }

            IFields fields = _featureLayer.FeatureClass.Fields;
            for (int i = 0; i < fields.FieldCount; i++)
            {
                IField field = fields.get_Field(i);
                if (field == null)
                {
                    continue;
                }

                if (field.Type == esriFieldType.esriFieldTypeOID ||
                    field.Type == esriFieldType.esriFieldTypeGeometry ||
                    field.Type == esriFieldType.esriFieldTypeBlob ||
                    field.Type == esriFieldType.esriFieldTypeRaster ||
                    field.Type == esriFieldType.esriFieldTypeXML ||
                    field.Type == esriFieldType.esriFieldTypeGUID ||
                    field.Type == esriFieldType.esriFieldTypeGlobalID)
                {
                    continue;
                }

                cboField.Items.Add(field.Name);
            }

            if (cboField.Items.Count > 0)
            {
                cboField.SelectedIndex = 0;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (_featureLayer == null || _featureLayer.FeatureClass == null)
                {
                    MessageBox.Show("图层无效。", "提示");
                    return;
                }

                if (_shapeType != esriGeometryType.esriGeometryPoint &&
                    _shapeType != esriGeometryType.esriGeometryMultipoint &&
                    _shapeType != esriGeometryType.esriGeometryPolyline &&
                    _shapeType != esriGeometryType.esriGeometryLine &&
                    _shapeType != esriGeometryType.esriGeometryPolygon)
                {
                    MessageBox.Show("仅支持点、线、面图层的唯一值渲染。", "提示");
                    return;
                }

                if (cboField.SelectedItem == null)
                {
                    MessageBox.Show("请选择字段。", "提示");
                    return;
                }

                string fieldName = cboField.SelectedItem.ToString();
                List<string> uniqueValues = ReadUniqueValues(fieldName);
                if (uniqueValues.Count == 0)
                {
                    MessageBox.Show("字段中没有有效唯一值。", "提示");
                    return;
                }

                const int maxValues = 64;
                if (uniqueValues.Count > maxValues)
                {
                    DialogResult tip = MessageBox.Show(
                        string.Format("唯一值共 {0} 个，超过 {1} 个时图例会较密。是否只渲染前 {1} 个唯一值？", uniqueValues.Count, maxValues),
                        "提示",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);
                    if (tip == DialogResult.Cancel)
                    {
                        return;
                    }
                    if (tip == DialogResult.Yes)
                    {
                        uniqueValues = uniqueValues.GetRange(0, maxValues);
                    }
                }

                IUniqueValueRenderer renderer = new UniqueValueRendererClass();
                renderer.FieldCount = 1;
                renderer.set_Field(0, fieldName);
                renderer.DefaultLabel = "其他";
                renderer.UseDefaultSymbol = false;

                ISymbol defaultSymbol = CreateSymbol(Color.LightGray, 0, 1);
                renderer.DefaultSymbol = defaultSymbol;

                for (int i = 0; i < uniqueValues.Count; i++)
                {
                    string value = uniqueValues[i];
                    Color color = ColorFromIndex(i, uniqueValues.Count);
                    ISymbol symbol = CreateSymbol(color, i, uniqueValues.Count);
                    // 第二参数为 Heading；留空可避免 TOC 出现“标题行 + 符号行”重复
                    renderer.AddValue(value, "", symbol);
                    renderer.set_Label(value, value);
                }

                IGeoFeatureLayer geoLayer = _featureLayer as IGeoFeatureLayer;
                if (geoLayer == null)
                {
                    MessageBox.Show("无法设置图层渲染器。", "提示");
                    return;
                }

                geoLayer.Renderer = (IFeatureRenderer)renderer;
                Applied = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("唯一值渲染失败：\n" + ex.Message, "错误");
            }
        }

        private List<string> ReadUniqueValues(string fieldName)
        {
            Dictionary<string, bool> dict = new Dictionary<string, bool>();
            IFeatureClass fc = _featureLayer.FeatureClass;
            int fieldIndex = fc.FindField(fieldName);
            if (fieldIndex < 0)
            {
                throw new Exception("未找到字段：" + fieldName);
            }

            IFeatureCursor cursor = fc.Search(null, true);
            try
            {
                IFeature feature = cursor.NextFeature();
                int count = 0;
                const int maxSample = 80000;
                while (feature != null && count < maxSample)
                {
                    object raw = feature.get_Value(fieldIndex);
                    string key;
                    if (raw == null || raw == DBNull.Value)
                    {
                        key = "<空>";
                    }
                    else
                    {
                        key = Convert.ToString(raw);
                        if (string.IsNullOrEmpty(key))
                        {
                            key = "<空>";
                        }
                    }

                    if (!dict.ContainsKey(key))
                    {
                        dict[key] = true;
                    }

                    count++;
                    feature = cursor.NextFeature();
                }
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
            }

            List<string> list = new List<string>(dict.Keys);
            list.Sort(StringComparer.OrdinalIgnoreCase);
            return list;
        }

        private ISymbol CreateSymbol(Color color, int index, int total)
        {
            if (_shapeType == esriGeometryType.esriGeometryPoint ||
                _shapeType == esriGeometryType.esriGeometryMultipoint)
            {
                ISimpleMarkerSymbol marker = new SimpleMarkerSymbolClass();
                marker.Style = esriSimpleMarkerStyle.esriSMSCircle;
                marker.Size = 8;
                marker.Color = ToRgbColor(color);
                marker.Outline = true;
                marker.OutlineSize = 0.5;
                marker.OutlineColor = ToRgbColor(Color.FromArgb(60, 60, 60));
                return (ISymbol)marker;
            }

            if (_shapeType == esriGeometryType.esriGeometryPolyline ||
                _shapeType == esriGeometryType.esriGeometryLine)
            {
                ISimpleLineSymbol line = new SimpleLineSymbolClass();
                line.Style = esriSimpleLineStyle.esriSLSSolid;
                line.Width = 1.5;
                line.Color = ToRgbColor(color);
                return (ISymbol)line;
            }

            ISimpleFillSymbol fill = new SimpleFillSymbolClass();
            fill.Style = esriSimpleFillStyle.esriSFSSolid;
            fill.Color = ToRgbColor(color);

            ISimpleLineSymbol outline = new SimpleLineSymbolClass();
            if (chkDrawOutline.Checked)
            {
                outline.Style = esriSimpleLineStyle.esriSLSSolid;
                outline.Width = 0.4;
                outline.Color = ToRgbColor(Color.FromArgb(80, 80, 80));
            }
            else
            {
                outline.Style = esriSimpleLineStyle.esriSLSNull;
                outline.Width = 0;
            }
            fill.Outline = outline;
            return (ISymbol)fill;
        }

        private static Color ColorFromIndex(int index, int total)
        {
            if (total <= 1)
            {
                return Color.FromArgb(49, 163, 84);
            }

            // HSV 色相均匀分布，饱和度/亮度适中
            double hue = (index * 360.0 / total) % 360.0;
            return ColorFromHsv(hue, 0.65, 0.90);
        }

        private static Color ColorFromHsv(double h, double s, double v)
        {
            double c = v * s;
            double x = c * (1 - Math.Abs((h / 60.0) % 2 - 1));
            double m = v - c;
            double r = 0, g = 0, b = 0;

            if (h < 60) { r = c; g = x; b = 0; }
            else if (h < 120) { r = x; g = c; b = 0; }
            else if (h < 180) { r = 0; g = c; b = x; }
            else if (h < 240) { r = 0; g = x; b = c; }
            else if (h < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            return Color.FromArgb(
                (int)Math.Round((r + m) * 255),
                (int)Math.Round((g + m) * 255),
                (int)Math.Round((b + m) * 255));
        }

        private static IColor ToRgbColor(Color color)
        {
            IRgbColor rgb = new RgbColorClass();
            rgb.Red = color.R;
            rgb.Green = color.G;
            rgb.Blue = color.B;
            return (IColor)rgb;
        }

        private static string GeometryText(esriGeometryType type)
        {
            if (type == esriGeometryType.esriGeometryPoint || type == esriGeometryType.esriGeometryMultipoint)
            {
                return "点图层";
            }
            if (type == esriGeometryType.esriGeometryPolyline || type == esriGeometryType.esriGeometryLine)
            {
                return "线图层";
            }
            if (type == esriGeometryType.esriGeometryPolygon)
            {
                return "面图层";
            }
            return "未知类型";
        }
    }
}

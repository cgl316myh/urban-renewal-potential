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
    /// <summary>面图层分段渲染。</summary>
    public partial class PolygonClassRenderForm : Form
    {
        private readonly IFeatureLayer _featureLayer;

        public bool Applied { get; private set; }

        public PolygonClassRenderForm(IFeatureLayer featureLayer)
        {
            InitializeComponent();
            _featureLayer = featureLayer;
            lblLayer.Text = featureLayer != null ? featureLayer.Name : "";
            LoadNumericFields();
        }

        private void LoadNumericFields()
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
                    field.Type == esriFieldType.esriFieldTypeXML)
                {
                    continue;
                }

                if (IsNumericField(field.Type))
                {
                    cboField.Items.Add(field.Name);
                }
            }

            if (cboField.Items.Count > 0)
            {
                cboField.SelectedIndex = 0;
            }

            cboMethod.Items.Clear();
            cboMethod.Items.Add("等间距");
            cboMethod.Items.Add("自然间断(Jenks)");
            cboMethod.SelectedIndex = 0;

            numClasses.Minimum = 2;
            numClasses.Maximum = 12;
            numClasses.Value = 5;

            btnColorLow.BackColor = Color.FromArgb(255, 255, 178);
            btnColorHigh.BackColor = Color.FromArgb(0, 104, 55);
        }

        private static bool IsNumericField(esriFieldType type)
        {
            return type == esriFieldType.esriFieldTypeSmallInteger ||
                   type == esriFieldType.esriFieldTypeInteger ||
                   type == esriFieldType.esriFieldTypeSingle ||
                   type == esriFieldType.esriFieldTypeDouble;
        }

        private void btnColorLow_Click(object sender, EventArgs e)
        {
            using (ColorDialog dlg = new ColorDialog())
            {
                dlg.Color = btnColorLow.BackColor;
                dlg.FullOpen = true;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    btnColorLow.BackColor = dlg.Color;
                }
            }
        }

        private void btnColorHigh_Click(object sender, EventArgs e)
        {
            using (ColorDialog dlg = new ColorDialog())
            {
                dlg.Color = btnColorHigh.BackColor;
                dlg.FullOpen = true;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    btnColorHigh.BackColor = dlg.Color;
                }
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

                if (_featureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolygon &&
                    _featureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryEnvelope)
                {
                    MessageBox.Show("仅支持面图层分段渲染。", "提示");
                    return;
                }

                if (cboField.SelectedItem == null)
                {
                    MessageBox.Show("请选择数值字段。", "提示");
                    return;
                }

                string fieldName = cboField.SelectedItem.ToString();
                int classCount = (int)numClasses.Value;
                bool useJenks = cboMethod.SelectedIndex == 1;

                List<double> values = ReadFieldValues(fieldName);
                if (values.Count == 0)
                {
                    MessageBox.Show("字段中没有有效数值。", "提示");
                    return;
                }

                double[] breaks = useJenks
                    ? JenksClassifier.ComputeBreaks(values, classCount)
                    : JenksClassifier.ComputeEqualIntervalBreaks(values, classCount);

                // Jenks/等间距返回的断点数量可能因数据被裁剪
                classCount = breaks.Length - 1;
                if (classCount < 1)
                {
                    MessageBox.Show("无法计算有效分级断点。", "提示");
                    return;
                }

                IClassBreaksRenderer renderer = new ClassBreaksRendererClass();
                renderer.Field = fieldName;
                renderer.BreakCount = classCount;
                renderer.MinimumBreak = breaks[0];

                Color low = btnColorLow.BackColor;
                Color high = btnColorHigh.BackColor;

                for (int i = 0; i < classCount; i++)
                {
                    double upper = breaks[i + 1];
                    double lower = breaks[i];
                    renderer.set_Break(i, upper);

                    ISimpleFillSymbol fill = new SimpleFillSymbolClass();
                    fill.Style = esriSimpleFillStyle.esriSFSSolid;
                    fill.Color = ToRgbColor(Interpolate(low, high, classCount <= 1 ? 0 : (double)i / (classCount - 1)));

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

                    renderer.set_Symbol(i, (ISymbol)fill);
                    renderer.set_Label(i, string.Format("{0:0.##} ~ {1:0.##}", lower, upper));
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
                MessageBox.Show("分段渲染失败：\n" + ex.Message, "错误");
            }
        }

        private List<double> ReadFieldValues(string fieldName)
        {
            List<double> values = new List<double>();
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
                const int maxSample = 50000;
                while (feature != null && count < maxSample)
                {
                    object raw = feature.get_Value(fieldIndex);
                    if (raw != null && raw != DBNull.Value)
                    {
                        double v;
                        if (TryToDouble(raw, out v))
                        {
                            values.Add(v);
                        }
                    }
                    count++;
                    feature = cursor.NextFeature();
                }
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
            }

            return values;
        }

        private static bool TryToDouble(object raw, out double value)
        {
            value = 0;
            try
            {
                value = Convert.ToDouble(raw);
                return !double.IsNaN(value) && !double.IsInfinity(value);
            }
            catch
            {
                return false;
            }
        }

        private static Color Interpolate(Color low, Color high, double t)
        {
            if (t < 0) t = 0;
            if (t > 1) t = 1;
            int r = (int)(low.R + (high.R - low.R) * t);
            int g = (int)(low.G + (high.G - low.G) * t);
            int b = (int)(low.B + (high.B - low.B) * t);
            return Color.FromArgb(r, g, b);
        }

        private static IColor ToRgbColor(Color color)
        {
            IRgbColor rgb = new RgbColorClass();
            rgb.Red = color.R;
            rgb.Green = color.G;
            rgb.Blue = color.B;
            return (IColor)rgb;
        }
    }
}

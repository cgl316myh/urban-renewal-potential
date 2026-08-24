using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using UrbanRenewal.GIS;
using Cursor = System.Windows.Forms.Cursor;

namespace UrbanRenewal.Host
{
    public partial class RasterRenderForm : Form
    {
        private const int ModeStretch = 0;
        private const int ModeClassify = 1;
        private const int ModeUnique = 2;
        private const int MaxUniqueValues = 64;
        private const int MaxSamplePixels = 20000;

        private readonly IRasterLayer _rasterLayer;
        private readonly IMap _map;

        public bool Applied { get; private set; }

        public RasterRenderForm(IRasterLayer rasterLayer)
            : this(rasterLayer, null)
        {
        }

        public RasterRenderForm(IRasterLayer rasterLayer, IMap map)
        {
            InitializeComponent();
            _rasterLayer = rasterLayer;
            _map = map;
            lblLayer.Text = rasterLayer != null ? rasterLayer.Name : "";
            InitDefaults();
        }

        private void InitDefaults()
        {
            cboMode.Items.Clear();
            cboMode.Items.Add("拉伸");
            cboMode.Items.Add("分级");
            cboMode.Items.Add("唯一值");

            cboMethod.Items.Clear();
            cboMethod.Items.Add("等间距");
            cboMethod.Items.Add("自然间断(Jenks)");
            cboMethod.SelectedIndex = 0;

            numClasses.Minimum = 2;
            numClasses.Maximum = 12;
            numClasses.Value = 5;

            btnColorLow.BackColor = Color.FromArgb(255, 255, 178);
            btnColorHigh.BackColor = Color.FromArgb(0, 104, 55);
            btnOutlineColor.BackColor = Color.FromArgb(40, 40, 40);
            numOutlineWidth.Minimum = 0.5m;
            numOutlineWidth.Maximum = 10m;
            numOutlineWidth.Increment = 0.5m;
            numOutlineWidth.DecimalPlaces = 1;
            numOutlineWidth.Value = 1.0m;
            chkDrawOutline.Checked = false;
            chkReverseRamp.Checked = false;

            string name = _rasterLayer != null ? (_rasterLayer.Name ?? "") : "";
            string lower = name.ToLowerInvariant();
            if (ContainsAny(lower, "等级", "level", "pot_level"))
            {
                cboMode.SelectedIndex = ModeUnique;
            }
            else if (ContainsAny(lower, "得分", "score", "动力", "可行", "潜力", "mot_", "fea_", "pot_"))
            {
                cboMode.SelectedIndex = ModeStretch;
            }
            else
            {
                cboMode.SelectedIndex = ModeClassify;
            }

            UpdateModeUi();
        }

        private static bool ContainsAny(string text, params string[] keys)
        {
            if (string.IsNullOrEmpty(text) || keys == null)
            {
                return false;
            }
            for (int i = 0; i < keys.Length; i++)
            {
                if (text.IndexOf(keys[i], StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void cboMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateModeUi();
        }

        private void UpdateModeUi()
        {
            int mode = cboMode.SelectedIndex;
            bool classify = mode == ModeClassify;
            bool canOutline = mode == ModeClassify || mode == ModeUnique;

            lblClasses.Enabled = classify;
            numClasses.Enabled = classify;
            lblMethod.Enabled = classify;
            cboMethod.Enabled = classify;
            chkDrawOutline.Enabled = canOutline;
            if (!canOutline)
            {
                chkDrawOutline.Checked = false;
            }
            UpdateOutlineControlsEnabled();
        }

        private void chkDrawOutline_CheckedChanged(object sender, EventArgs e)
        {
            UpdateOutlineControlsEnabled();
        }

        private void UpdateOutlineControlsEnabled()
        {
            bool enabled = chkDrawOutline.Enabled && chkDrawOutline.Checked;
            lblOutlineColor.Enabled = enabled;
            btnOutlineColor.Enabled = enabled;
            lblOutlineWidth.Enabled = enabled;
            numOutlineWidth.Enabled = enabled;
        }

        private void btnColorLow_Click(object sender, EventArgs e)
        {
            PickColor(btnColorLow);
        }

        private void btnColorHigh_Click(object sender, EventArgs e)
        {
            PickColor(btnColorHigh);
        }

        private void btnOutlineColor_Click(object sender, EventArgs e)
        {
            PickColor(btnOutlineColor);
        }

        private static void PickColor(Button button)
        {
            using (ColorDialog dlg = new ColorDialog())
            {
                dlg.Color = button.BackColor;
                dlg.FullOpen = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    button.BackColor = dlg.Color;
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (_rasterLayer == null || _rasterLayer.Raster == null)
                {
                    MessageBox.Show(this, "栅格图层无效。", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Color low = btnColorLow.BackColor;
                Color high = btnColorHigh.BackColor;
                if (chkReverseRamp.Checked)
                {
                    Color tmp = low;
                    low = high;
                    high = tmp;
                }

                int mode = cboMode.SelectedIndex;
                if (mode == ModeStretch)
                {
                    ApplyStretch(low, high);
                }
                else if (mode == ModeClassify)
                {
                    ApplyClassify(low, high, chkDrawOutline.Checked);
                }
                else
                {
                    ApplyUnique(low, high, chkDrawOutline.Checked);
                }

                try
                {
                    SyncOutlineOverlay(mode == ModeClassify || mode == ModeUnique);
                }
                catch (Exception outlineEx)
                {
                    MessageBox.Show(this,
                        "颜色渲染已应用，但边线生成失败：\r\n" + outlineEx.Message,
                        "边线",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                Applied = true;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "栅格渲染失败：\r\n" + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 勾选边线时转面叠加空心面图层（栅格渲染器本身不画 Outline）
        private void SyncOutlineOverlay(bool modeSupportsOutline)
        {
            if (_map == null || _rasterLayer == null)
            {
                return;
            }

            bool draw = modeSupportsOutline && chkDrawOutline.Checked;
            Cursor old = Cursor.Current;
            try
            {
                if (draw)
                {
                    Cursor.Current = Cursors.WaitCursor;
                }

                string msg;
                RasterOutlineHelper.SyncOutlineLayer(
                    _map,
                    _rasterLayer,
                    draw,
                    btnOutlineColor.BackColor,
                    (double)numOutlineWidth.Value,
                    out msg);
            }
            finally
            {
                Cursor.Current = old;
            }
        }

        // Update → BandIndex/ColorRamp/StretchType → Update → 赋图层
        private void ApplyStretch(Color low, Color high)
        {
            IColorRamp colorRamp = CreateColorRamp(low, high, 255);

            IRasterStretchColorRampRenderer stretch = new RasterStretchColorRampRendererClass();
            IRasterRenderer renderer = (IRasterRenderer)stretch;
            renderer.Raster = _rasterLayer.Raster;
            renderer.Update();

            stretch.BandIndex = 0;
            stretch.ColorRamp = colorRamp;

            IRasterStretch stretchType = renderer as IRasterStretch;
            if (stretchType != null)
            {
                stretchType.StretchType = esriRasterStretchTypesEnum.esriRasterStretch_MinimumMaximum;
            }

            renderer.Update();
            AssignRenderer(renderer);
        }

        /// <summary>
        /// 关键必须用 set_Symbol 逐级赋填充色；仅设 ColorRamp 不会改地图颜色。
        /// </summary>
        private void ApplyClassify(Color low, Color high, bool drawOutline)
        {
            int classCount = (int)numClasses.Value;
            bool useJenks = cboMethod.SelectedIndex == 1;

            List<double> samples = SamplePixelValues();
            if (samples.Count == 0)
            {
                double min;
                double max;
                if (!TryGetBandMinMax(out min, out max))
                {
                    throw new Exception("未能从栅格读取有效像元值。");
                }
                samples.Add(min);
                samples.Add(max);
            }

            double[] breaks = useJenks
                ? JenksClassifier.ComputeBreaks(samples, classCount)
                : JenksClassifier.ComputeEqualIntervalBreaks(samples, classCount);

            classCount = breaks.Length - 1;
            if (classCount < 1)
            {
                throw new Exception("无法计算有效分级断点。");
            }

            IRasterClassifyColorRampRenderer classify = new RasterClassifyColorRampRendererClass();
            IRasterRenderer renderer = (IRasterRenderer)classify;
            renderer.Raster = _rasterLayer.Raster;
            classify.ClassCount = classCount;
            try
            {
                classify.ClassField = "Value";
            }
            catch
            {
                // 部分栅格无 Value 字段时忽略
            }
            renderer.Update();

            IColorRamp colorRamp = CreateColorRamp(low, high, classCount);
            for (int i = 0; i < classCount; i++)
            {
                classify.set_Break(i, breaks[i + 1]);

                ISimpleFillSymbol fill = new SimpleFillSymbolClass();
                fill.Style = esriSimpleFillStyle.esriSFSSolid;
                fill.Color = colorRamp.get_Color(i);
                ApplyOutlineToFill(fill, drawOutline);

                classify.set_Symbol(i, (ISymbol)fill);
                classify.set_Label(i, string.Format("{0:0.##} ~ {1:0.##}", breaks[i], breaks[i + 1]));
            }

            renderer.Update();
            // Update 后可能重置符号轮廓，再刷一遍边线
            ReapplyOutlineToClassify(classify, classCount, drawOutline);
            AssignRenderer(renderer);
        }

        /// <summary>
        /// 唯一值用分级渲染器「一类一值」。
        /// RasterUniqueValueRenderer 在 AE 下易因类型/Update 匹配失败变成灰或透明；
        /// 分级路径已验证可正确 set_Symbol 上色。
        /// </summary>
        private void ApplyUnique(Color low, Color high, bool drawOutline)
        {
            IRaster raster = _rasterLayer.Raster;
            string fieldName;
            IUniqueValues engineUniques = TryCalcEngineUniqueValues(raster);
            List<UniqueValueItem> uniques = BuildUniqueItems(engineUniques, out fieldName);

            if (uniques.Count == 0)
            {
                throw new Exception("未能读取唯一值。");
            }

            if (uniques.Count > MaxUniqueValues)
            {
                throw new Exception(string.Format(
                    "唯一值过多（{0} 个，上限 {1}）。请改用「拉伸」或「分级」。",
                    uniques.Count, MaxUniqueValues));
            }

            // 相邻唯一值中点分隔，使每级独占一类
            double[] breaks = new double[uniques.Count + 1];
            breaks[0] = uniques[0].SortKey - 0.5;
            for (int i = 0; i < uniques.Count - 1; i++)
            {
                breaks[i + 1] = (uniques[i].SortKey + uniques[i + 1].SortKey) / 2.0;
            }
            breaks[uniques.Count] = uniques[uniques.Count - 1].SortKey + 0.5;

            int classCount = uniques.Count;
            IRasterClassifyColorRampRenderer classify = new RasterClassifyColorRampRendererClass();
            IRasterRenderer renderer = (IRasterRenderer)classify;
            renderer.Raster = raster;
            classify.ClassCount = classCount;
            try
            {
                classify.ClassField = string.IsNullOrEmpty(fieldName) ? "Value" : fieldName;
            }
            catch
            {
            }
            renderer.Update();

            IColorRamp colorRamp = CreateColorRamp(low, high, classCount);
            for (int i = 0; i < classCount; i++)
            {
                classify.set_Break(i, breaks[i + 1]);

                ISimpleFillSymbol fill = new SimpleFillSymbolClass();
                fill.Style = esriSimpleFillStyle.esriSFSSolid;
                fill.Color = colorRamp.get_Color(i);
                ApplyOutlineToFill(fill, drawOutline);

                classify.set_Symbol(i, (ISymbol)fill);
                classify.set_Label(i, uniques[i].Label);
            }

            renderer.Update();
            ReapplyOutlineToClassify(classify, classCount, drawOutline);
            AssignRenderer(renderer);
        }

        private sealed class UniqueValueItem
        {
            public object Value;
            public string Label;
            public double SortKey;
        }

        private void AssignRenderer(IRasterRenderer renderer)
        {
            if (renderer == null)
            {
                throw new Exception("渲染器无效。");
            }

            _rasterLayer.Renderer = renderer;
        }

        private static IUniqueValues TryCalcEngineUniqueValues(IRaster raster)
        {
            if (raster == null)
            {
                return null;
            }
            try
            {
                IUniqueValues uvs = new UniqueValuesClass();
                IRasterCalcUniqueValues calc = new RasterCalcUniqueValuesClass();
                calc.AddFromRaster(raster, 0, uvs);
                if (uvs.Count <= 0)
                {
                    return null;
                }
                return uvs;
            }
            catch
            {
                return null;
            }
        }

        private List<UniqueValueItem> BuildUniqueItems(IUniqueValues engineUniques, out string fieldName)
        {
            fieldName = "Value";

            // 1) 引擎原生唯一值（像元类型一致）
            if (engineUniques != null && engineUniques.Count > 0)
            {
                List<UniqueValueItem> fromEngine = new List<UniqueValueItem>();
                Dictionary<string, bool> seen = new Dictionary<string, bool>();
                for (int i = 0; i < engineUniques.Count && fromEngine.Count <= MaxUniqueValues; i++)
                {
                    object raw = null;
                    try
                    {
                        raw = engineUniques.get_UniqueValue(i);
                    }
                    catch
                    {
                        continue;
                    }
                    if (raw == null || raw is DBNull)
                    {
                        continue;
                    }
                    double sortKey;
                    if (!TryToDouble(raw, out sortKey))
                    {
                        continue;
                    }
                    string label = FormatUniqueLabel(sortKey);
                    if (seen.ContainsKey(label))
                    {
                        continue;
                    }
                    seen.Add(label, true);
                    UniqueValueItem item = new UniqueValueItem();
                    item.Value = raw;
                    item.Label = label;
                    item.SortKey = sortKey;
                    fromEngine.Add(item);
                }
                if (fromEngine.Count > 0)
                {
                    fromEngine.Sort(CompareUniqueBySortKey);
                    return fromEngine;
                }
            }

            // 2) VAT / 采样回退（VAT 用原始 Variant）
            return CollectUniqueValueItems(out fieldName);
        }

        private static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private static string FormatUniqueLabel(double value)
        {
            if (Math.Abs(value - Math.Round(value)) < 1e-9)
            {
                return ((long)Math.Round(value)).ToString();
            }
            return string.Format("{0:0.##}", value);
        }

        private static IColorRamp CreateColorRamp(Color low, Color high, int size)
        {
            IAlgorithmicColorRamp ramp = new AlgorithmicColorRampClass();
            ramp.FromColor = ToRgbColor(low);
            ramp.ToColor = ToRgbColor(high);
            ramp.Algorithm = esriColorRampAlgorithm.esriCIELabAlgorithm;
            ramp.Size = size < 2 ? 2 : size;
            bool ok = false;
            ramp.CreateRamp(out ok);
            if (!ok)
            {
                // CIELab 失败退回 HSV
                ramp.Algorithm = esriColorRampAlgorithm.esriHSVAlgorithm;
                ramp.CreateRamp(out ok);
            }
            if (!ok)
            {
                throw new Exception("创建色带失败。");
            }
            return (IColorRamp)ramp;
        }

        /// <summary>
        /// 栅格分级/唯一值边线：画的是像元格网轮廓（非整块图斑外轮廓）。
        /// Update 后需重新赋值，否则地图上看不见。
        /// </summary>
        private void ApplyOutlineToFill(ISimpleFillSymbol fill, bool drawOutline)
        {
            if (fill == null)
            {
                return;
            }

            Color outlineColor = btnOutlineColor.BackColor;
            double outlineWidth = (double)numOutlineWidth.Value;
            if (outlineWidth < 0.5)
            {
                outlineWidth = 0.5;
            }

            ISimpleLineSymbol outline = new SimpleLineSymbolClass();
            if (drawOutline)
            {
                outline.Style = esriSimpleLineStyle.esriSLSSolid;
                outline.Width = outlineWidth;
                IRgbColor rgb = new RgbColorClass();
                rgb.Red = outlineColor.R;
                rgb.Green = outlineColor.G;
                rgb.Blue = outlineColor.B;
                rgb.Transparency = 0;
                rgb.UseWindowsDithering = true;
                outline.Color = (IColor)rgb;
            }
            else
            {
                outline.Style = esriSimpleLineStyle.esriSLSNull;
                outline.Width = 0;
                IRgbColor rgb = new RgbColorClass();
                rgb.NullColor = true;
                outline.Color = (IColor)rgb;
            }

            fill.Outline = (ILineSymbol)outline;
        }

        private void ReapplyOutlineToClassify(
            IRasterClassifyColorRampRenderer classify,
            int classCount,
            bool drawOutline)
        {
            if (classify == null || classCount <= 0)
            {
                return;
            }

            for (int i = 0; i < classCount; i++)
            {
                try
                {
                    ISimpleFillSymbol fill = classify.get_Symbol(i) as ISimpleFillSymbol;
                    if (fill == null)
                    {
                        continue;
                    }
                    ApplyOutlineToFill(fill, drawOutline);
                    classify.set_Symbol(i, (ISymbol)fill);
                }
                catch
                {
                }
            }
        }

        private List<double> SamplePixelValues()
        {
            List<double> values = new List<double>();
            IRaster raster = _rasterLayer.Raster;
            IRasterProps props = raster as IRasterProps;
            if (props == null)
            {
                return values;
            }

            int width = props.Width;
            int height = props.Height;
            if (width <= 0 || height <= 0)
            {
                return values;
            }

            long total = (long)width * height;
            int step = 1;
            if (total > MaxSamplePixels)
            {
                step = (int)Math.Ceiling(Math.Sqrt((double)total / MaxSamplePixels));
                if (step < 1)
                {
                    step = 1;
                }
            }

            int blockW = Math.Min(256, width);
            int blockH = Math.Min(256, height);

            for (int y = 0; y < height && values.Count < MaxSamplePixels; y += blockH)
            {
                for (int x = 0; x < width && values.Count < MaxSamplePixels; x += blockW)
                {
                    int w = Math.Min(blockW, width - x);
                    int h = Math.Min(blockH, height - y);
                    try
                    {
                        IPixelBlock pb = raster.CreatePixelBlock(CreatePoint(w, h));
                        raster.Read(CreatePoint(x, y), pb);
                        AppendPixelValues(pb, values, step, MaxSamplePixels);
                    }
                    catch
                    {
                    }
                }
            }

            return values;
        }

        /// <summary>
        /// VAT 优先（原始 Variant）；无表时再采样。采样时按像元类型装箱。
        /// </summary>
        private List<UniqueValueItem> CollectUniqueValueItems(out string fieldName)
        {
            fieldName = "Value";
            List<UniqueValueItem> fromTable = TryCollectUniqueFromAttributeTable(out fieldName);
            if (fromTable != null && fromTable.Count > 0)
            {
                return fromTable;
            }

            fieldName = "Value";
            rstPixelType pixelType = GetPixelType();

            object noData = null;
            try
            {
                IRasterProps props = _rasterLayer.Raster as IRasterProps;
                if (props != null)
                {
                    noData = props.NoDataValue;
                }
            }
            catch
            {
                noData = null;
            }

            List<double> samples = SamplePixelValues();
            if (samples.Count == 0)
            {
                double min;
                double max;
                if (TryGetBandMinMax(out min, out max) && Math.Abs(max - min) < 1e-9)
                {
                    samples.Add(min);
                }
            }

            Dictionary<string, UniqueValueItem> map = new Dictionary<string, UniqueValueItem>();
            for (int i = 0; i < samples.Count; i++)
            {
                double v = samples[i];
                if (IsNoDataValue(v, noData))
                {
                    continue;
                }

                string key = FormatUniqueLabel(v);
                if (!map.ContainsKey(key))
                {
                    UniqueValueItem item = new UniqueValueItem();
                    item.Value = BoxPixelValue(v, pixelType);
                    item.Label = key;
                    item.SortKey = v;
                    map.Add(key, item);
                    if (map.Count > MaxUniqueValues)
                    {
                        break;
                    }
                }
            }

            List<UniqueValueItem> list = new List<UniqueValueItem>(map.Values);
            list.Sort(CompareUniqueBySortKey);
            return list;
        }

        private List<UniqueValueItem> TryCollectUniqueFromAttributeTable(out string fieldName)
        {
            fieldName = "Value";
            try
            {
                ITable table = GetRasterAttributeTable();
                if (table == null)
                {
                    return null;
                }

                int valueIdx = table.FindField("Value");
                if (valueIdx < 0)
                {
                    valueIdx = table.FindField("VALUE");
                }
                if (valueIdx < 0)
                {
                    return null;
                }

                try
                {
                    fieldName = table.Fields.get_Field(valueIdx).Name;
                }
                catch
                {
                    fieldName = "Value";
                }

                int rowCount = table.RowCount(null);
                if (rowCount <= 0)
                {
                    return null;
                }

                List<UniqueValueItem> list = new List<UniqueValueItem>();
                Dictionary<string, bool> seen = new Dictionary<string, bool>();

                // 与 VBA 示例一致：优先 GetRow；失败再 Search
                bool usedGetRow = false;
                try
                {
                    for (int i = 0; i < rowCount && list.Count <= MaxUniqueValues; i++)
                    {
                        IRow row = table.GetRow(i);
                        if (row == null)
                        {
                            continue;
                        }
                        if (TryAddUniqueFromRow(row, valueIdx, seen, list))
                        {
                            usedGetRow = true;
                        }
                    }
                }
                catch
                {
                    usedGetRow = false;
                    list.Clear();
                    seen.Clear();
                }

                if (!usedGetRow || list.Count == 0)
                {
                    list.Clear();
                    seen.Clear();
                    ICursor cursor = table.Search(null, false);
                    try
                    {
                        IRow row = cursor != null ? cursor.NextRow() : null;
                        while (row != null && list.Count <= MaxUniqueValues)
                        {
                            TryAddUniqueFromRow(row, valueIdx, seen, list);
                            row = cursor.NextRow();
                        }
                    }
                    finally
                    {
                        if (cursor != null)
                        {
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
                        }
                    }
                }

                list.Sort(CompareUniqueBySortKey);
                return list;
            }
            catch
            {
                return null;
            }
        }

        private static bool TryAddUniqueFromRow(
            IRow row,
            int valueIdx,
            Dictionary<string, bool> seen,
            List<UniqueValueItem> list)
        {
            object raw = row.get_Value(valueIdx);
            if (raw == null || raw is DBNull)
            {
                return false;
            }

            double sortKey;
            if (!TryToDouble(raw, out sortKey))
            {
                return false;
            }

            string label = FormatUniqueLabel(sortKey);
            if (seen.ContainsKey(label))
            {
                return false;
            }

            seen.Add(label, true);
            UniqueValueItem item = new UniqueValueItem();
            // VBA：直接用表字段原始值，不强制转 Byte
            item.Value = raw;
            item.Label = label;
            item.SortKey = sortKey;
            list.Add(item);
            return true;
        }

        private static object BoxPixelValue(double value, rstPixelType pixelType)
        {
            switch (pixelType)
            {
                case rstPixelType.PT_UCHAR:
                    return (byte)Clamp(Math.Round(value), 0, 255);
                case rstPixelType.PT_CHAR:
                    return (sbyte)Clamp(Math.Round(value), sbyte.MinValue, sbyte.MaxValue);
                case rstPixelType.PT_USHORT:
                    return (ushort)Clamp(Math.Round(value), ushort.MinValue, ushort.MaxValue);
                case rstPixelType.PT_SHORT:
                    return (short)Clamp(Math.Round(value), short.MinValue, short.MaxValue);
                case rstPixelType.PT_ULONG:
                    return (uint)Clamp(Math.Round(value), uint.MinValue, uint.MaxValue);
                case rstPixelType.PT_LONG:
                    return (int)Clamp(Math.Round(value), int.MinValue, int.MaxValue);
                case rstPixelType.PT_FLOAT:
                    return (float)value;
                case rstPixelType.PT_DOUBLE:
                    return value;
                default:
                    if (Math.Abs(value - Math.Round(value)) < 1e-9)
                    {
                        return (int)Math.Round(value);
                    }
                    return value;
            }
        }

        private rstPixelType GetPixelType()
        {
            IRasterProps props = _rasterLayer.Raster as IRasterProps;
            if (props != null)
            {
                return props.PixelType;
            }
            return rstPixelType.PT_UNKNOWN;
        }

        private ITable GetRasterAttributeTable()
        {
            // VBA 示例路径：波段 AttributeTable
            try
            {
                IRasterBandCollection bands = _rasterLayer.Raster as IRasterBandCollection;
                if (bands != null && bands.Count >= 1)
                {
                    ITable t = bands.Item(0).AttributeTable;
                    if (t != null)
                    {
                        return t;
                    }
                }
            }
            catch
            {
            }

            IRaster2 raster2 = _rasterLayer.Raster as IRaster2;
            if (raster2 != null)
            {
                try
                {
                    ITable t = raster2.AttributeTable;
                    if (t != null)
                    {
                        return t;
                    }
                }
                catch
                {
                }
            }

            try
            {
                IDataset ds = _rasterLayer as IDataset;
                IRasterBandCollection bands2 = ds as IRasterBandCollection;
                if (bands2 != null && bands2.Count >= 1)
                {
                    return bands2.Item(0).AttributeTable;
                }
            }
            catch
            {
            }

            return null;
        }

        private static int CompareUniqueBySortKey(UniqueValueItem a, UniqueValueItem b)
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;
            return a.SortKey.CompareTo(b.SortKey);
        }

        private static bool IsNoDataValue(double value, object noData)
        {
            if (noData == null || noData is DBNull)
            {
                return false;
            }

            try
            {
                Array arr = noData as Array;
                if (arr != null && arr.Length > 0)
                {
                    double nd;
                    if (TryToDouble(arr.GetValue(0), out nd))
                    {
                        return Math.Abs(value - nd) < 1e-9;
                    }
                    return false;
                }

                double single;
                if (TryToDouble(noData, out single))
                {
                    return Math.Abs(value - single) < 1e-9;
                }
            }
            catch
            {
            }
            return false;
        }

        private bool TryGetBandMinMax(out double min, out double max)
        {
            min = 0;
            max = 0;
            try
            {
                IRasterBandCollection bands = _rasterLayer.Raster as IRasterBandCollection;
                if (bands == null || bands.Count < 1)
                {
                    IDataset ds = _rasterLayer as IDataset;
                    if (ds != null)
                    {
                        bands = ds as IRasterBandCollection;
                    }
                }

                if (bands == null || bands.Count < 1)
                {
                    return false;
                }

                IRasterBand band = bands.Item(0);
                IRasterStatistics stats = null;
                try
                {
                    stats = band.Statistics;
                }
                catch
                {
                    stats = null;
                }

                if (stats == null)
                {
                    try
                    {
                        band.ComputeStatsAndHist();
                        stats = band.Statistics;
                    }
                    catch
                    {
                        return false;
                    }
                }

                if (stats == null)
                {
                    return false;
                }

                min = stats.Minimum;
                max = stats.Maximum;
                return max >= min;
            }
            catch
            {
                return false;
            }
        }

        private static void AppendPixelValues(IPixelBlock pixelBlock, List<double> values, int step, int maxCount)
        {
            if (pixelBlock == null || values.Count >= maxCount)
            {
                return;
            }

            IPixelBlock3 block3 = pixelBlock as IPixelBlock3;
            if (block3 == null)
            {
                return;
            }

            int w = pixelBlock.Width;
            int h = pixelBlock.Height;
            object raw = block3.get_PixelData(0);
            Array data = raw as Array;
            if (data == null)
            {
                return;
            }

            int rank = data.Rank;
            for (int row = 0; row < h && values.Count < maxCount; row += step)
            {
                for (int col = 0; col < w && values.Count < maxCount; col += step)
                {
                    object cell = GetArrayValue(data, rank, col, row);
                    double v;
                    if (TryToDouble(cell, out v))
                    {
                        values.Add(v);
                    }
                }
            }
        }

        private static object GetArrayValue(Array data, int rank, int col, int row)
        {
            if (rank >= 2)
            {
                try
                {
                    return data.GetValue(col, row);
                }
                catch
                {
                    try
                    {
                        return data.GetValue(row, col);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
            if (rank == 1)
            {
                int len = data.GetLength(0);
                if (len <= 0)
                {
                    return null;
                }
                int index = col;
                if (index < 0)
                {
                    index = 0;
                }
                if (index >= len)
                {
                    index = len - 1;
                }
                return data.GetValue(index);
            }
            return null;
        }

        private static IPnt CreatePoint(int x, int y)
        {
            IPnt pnt = new PntClass();
            pnt.SetCoords(x, y);
            return pnt;
        }

        private static bool TryToDouble(object raw, out double value)
        {
            value = 0;
            if (raw == null || raw is DBNull)
            {
                return false;
            }
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

        private static IColor ToRgbColor(Color color)
        {
            IRgbColor rgb = new RgbColorClass();
            rgb.Red = color.R;
            rgb.Green = color.G;
            rgb.Blue = color.B;
            rgb.UseWindowsDithering = true;
            return (IColor)rgb;
        }
    }
}

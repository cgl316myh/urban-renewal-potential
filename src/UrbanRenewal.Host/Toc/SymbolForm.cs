using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;

namespace UrbanRenewal.Host
{
    /// <summary>
    /// 自定义矢量符号设置窗体（点/线/面）。
    /// </summary>
    public partial class SymbolForm : Form
    {
        private enum SymbolKind
        {
            Marker,
            Line,
            Fill,
            Unknown
        }

        private SymbolKind _kind = SymbolKind.Unknown;
        private Color _fillColor = Color.Green;
        private Color _outlineColor = Color.Black;
        private double _size = 8;
        private double _outlineWidth = 1;

        public ISymbol ResultSymbol { get; private set; }

        public SymbolForm(ISymbol symbol, string layerName)
        {
            InitializeComponent();
            lblLayer.Text = string.IsNullOrEmpty(layerName) ? "当前图层" : layerName;
            LoadFromSymbol(symbol);
            UpdatePreview();
        }

        private void LoadFromSymbol(ISymbol symbol)
        {
            if (symbol is IMarkerSymbol)
            {
                _kind = SymbolKind.Marker;
                IMarkerSymbol marker = (IMarkerSymbol)symbol;
                _fillColor = ToSystemColor(marker.Color);
                _size = marker.Size > 0 ? marker.Size : 8;

                cboStyle.Items.Clear();
                cboStyle.Items.AddRange(new object[] { "圆形", "方形", "十字", "菱形", "X形" });
                cboStyle.SelectedIndex = 0;

                ISimpleMarkerSymbol simpleMarker = symbol as ISimpleMarkerSymbol;
                if (simpleMarker != null)
                {
                    cboStyle.SelectedIndex = MarkerStyleToIndex(simpleMarker.Style);
                }

                lblSize.Text = "大小：";
                numSize.Minimum = 1;
                numSize.Maximum = 50;
                numSize.DecimalPlaces = 1;
                numSize.Value = ClampDecimal((decimal)_size, numSize.Minimum, numSize.Maximum);

                lblOutline.Visible = false;
                btnOutlineColor.Visible = false;
                lblOutlineWidth.Visible = false;
                numOutlineWidth.Visible = false;
                chkDrawOutline.Visible = false;
            }
            else if (symbol is ILineSymbol)
            {
                _kind = SymbolKind.Line;
                ILineSymbol line = (ILineSymbol)symbol;
                _fillColor = ToSystemColor(line.Color);
                _size = line.Width > 0 ? line.Width : 1;

                cboStyle.Items.Clear();
                cboStyle.Items.AddRange(new object[] { "实线", "虚线", "点线", "点划线", "双点划线" });
                cboStyle.SelectedIndex = 0;

                ISimpleLineSymbol simpleLine = symbol as ISimpleLineSymbol;
                if (simpleLine != null)
                {
                    cboStyle.SelectedIndex = LineStyleToIndex(simpleLine.Style);
                }

                lblSize.Text = "线宽：";
                numSize.Minimum = 0.5M;
                numSize.Maximum = 20;
                numSize.DecimalPlaces = 1;
                numSize.Value = ClampDecimal((decimal)_size, numSize.Minimum, numSize.Maximum);

                lblOutline.Visible = false;
                btnOutlineColor.Visible = false;
                lblOutlineWidth.Visible = false;
                numOutlineWidth.Visible = false;
                chkDrawOutline.Visible = false;
            }
            else if (symbol is IFillSymbol)
            {
                _kind = SymbolKind.Fill;
                IFillSymbol fill = (IFillSymbol)symbol;
                _fillColor = ToSystemColor(fill.Color);

                cboStyle.Items.Clear();
                cboStyle.Items.AddRange(new object[] { "实心", "空心", "水平线", "垂直线", "正斜线", "反斜线", "十字线", "交叉线" });
                cboStyle.SelectedIndex = 0;

                bool hasOutline = true;
                ISimpleFillSymbol simpleFill = symbol as ISimpleFillSymbol;
                if (simpleFill != null)
                {
                    cboStyle.SelectedIndex = FillStyleToIndex(simpleFill.Style);
                    if (simpleFill.Outline != null)
                    {
                        ISimpleLineSymbol simpleOutline = simpleFill.Outline as ISimpleLineSymbol;
                        if (simpleOutline != null && simpleOutline.Style == esriSimpleLineStyle.esriSLSNull)
                        {
                            hasOutline = false;
                        }
                        else
                        {
                            _outlineColor = ToSystemColor(simpleFill.Outline.Color);
                            _outlineWidth = simpleFill.Outline.Width > 0 ? simpleFill.Outline.Width : 1;
                        }
                    }
                    else
                    {
                        hasOutline = false;
                    }
                }

                lblSize.Text = "填充：";
                numSize.Visible = false;
                lblSize.Visible = false;

                chkDrawOutline.Visible = true;
                chkDrawOutline.Checked = hasOutline;
                lblOutline.Visible = true;
                btnOutlineColor.Visible = true;
                lblOutlineWidth.Visible = true;
                numOutlineWidth.Visible = true;
                numOutlineWidth.Value = ClampDecimal((decimal)_outlineWidth, numOutlineWidth.Minimum, numOutlineWidth.Maximum);
                UpdateOutlineControlsEnabled();
            }
            else
            {
                _kind = SymbolKind.Unknown;
                MessageBox.Show("当前图例符号类型暂不支持自定义编辑。", "提示");
            }

            btnColor.BackColor = _fillColor;
            btnOutlineColor.BackColor = _outlineColor;
            lblType.Text = SymbolKindText(_kind);
        }

        private void btnColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog dlg = new ColorDialog())
            {
                dlg.Color = _fillColor;
                dlg.FullOpen = true;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _fillColor = dlg.Color;
                    btnColor.BackColor = _fillColor;
                    UpdatePreview();
                }
            }
        }

        private void btnOutlineColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog dlg = new ColorDialog())
            {
                dlg.Color = _outlineColor;
                dlg.FullOpen = true;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _outlineColor = dlg.Color;
                    btnOutlineColor.BackColor = _outlineColor;
                    UpdatePreview();
                }
            }
        }

        private void numSize_ValueChanged(object sender, EventArgs e)
        {
            _size = (double)numSize.Value;
            UpdatePreview();
        }

        private void numOutlineWidth_ValueChanged(object sender, EventArgs e)
        {
            _outlineWidth = (double)numOutlineWidth.Value;
            UpdatePreview();
        }

        private void cboStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePreview();
        }

        private void panelPreview_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            Rectangle bounds = panelPreview.ClientRectangle;
            bounds.Inflate(-12, -12);

            if (_kind == SymbolKind.Marker)
            {
                float s = Math.Max(6f, (float)_size * 2f);
                float cx = bounds.Left + bounds.Width / 2f;
                float cy = bounds.Top + bounds.Height / 2f;
                RectangleF r = new RectangleF(cx - s / 2f, cy - s / 2f, s, s);
                using (Brush brush = new SolidBrush(_fillColor))
                using (Pen pen = new Pen(Color.Black, 1f))
                {
                    DrawMarkerPreview(g, brush, pen, r, cboStyle.SelectedIndex);
                }
            }
            else if (_kind == SymbolKind.Line)
            {
                using (Pen pen = CreateLinePreviewPen())
                {
                    int y = bounds.Top + bounds.Height / 2;
                    g.DrawLine(pen, bounds.Left, y, bounds.Right, y);
                }
            }
            else if (_kind == SymbolKind.Fill)
            {
                using (Brush brush = CreateFillPreviewBrush(bounds))
                {
                    g.FillRectangle(brush, bounds);
                    if (chkDrawOutline.Checked)
                    {
                        using (Pen pen = new Pen(_outlineColor, Math.Max(1f, (float)_outlineWidth)))
                        {
                            g.DrawRectangle(pen, bounds);
                        }
                    }
                }
            }
        }

        private void UpdatePreview()
        {
            panelPreview.Invalidate();
        }

        private void chkDrawOutline_CheckedChanged(object sender, EventArgs e)
        {
            UpdateOutlineControlsEnabled();
            UpdatePreview();
        }

        private void UpdateOutlineControlsEnabled()
        {
            bool enabled = chkDrawOutline.Visible && chkDrawOutline.Checked;
            lblOutline.Enabled = enabled;
            btnOutlineColor.Enabled = enabled;
            lblOutlineWidth.Enabled = enabled;
            numOutlineWidth.Enabled = enabled;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (_kind == SymbolKind.Unknown)
            {
                DialogResult = DialogResult.Cancel;
                return;
            }

            try
            {
                ResultSymbol = BuildSymbol();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("生成符号失败：\n" + ex.Message, "错误");
            }
        }

        private ISymbol BuildSymbol()
        {
            if (_kind == SymbolKind.Marker)
            {
                ISimpleMarkerSymbol marker = new SimpleMarkerSymbolClass();
                marker.Style = IndexToMarkerStyle(cboStyle.SelectedIndex);
                marker.Color = ToRgbColor(_fillColor);
                marker.Size = (double)numSize.Value;
                marker.Outline = true;
                marker.OutlineColor = ToRgbColor(Color.Black);
                marker.OutlineSize = 1;
                return (ISymbol)marker;
            }

            if (_kind == SymbolKind.Line)
            {
                ISimpleLineSymbol line = new SimpleLineSymbolClass();
                line.Style = IndexToLineStyle(cboStyle.SelectedIndex);
                line.Color = ToRgbColor(_fillColor);
                line.Width = (double)numSize.Value;
                return (ISymbol)line;
            }

            ISimpleFillSymbol fill = new SimpleFillSymbolClass();
            fill.Style = IndexToFillStyle(cboStyle.SelectedIndex);
            fill.Color = ToRgbColor(_fillColor);

            ISimpleLineSymbol outline = new SimpleLineSymbolClass();
            if (chkDrawOutline.Checked)
            {
                outline.Style = esriSimpleLineStyle.esriSLSSolid;
                outline.Color = ToRgbColor(_outlineColor);
                outline.Width = (double)numOutlineWidth.Value;
            }
            else
            {
                outline.Style = esriSimpleLineStyle.esriSLSNull;
                outline.Width = 0;
            }
            fill.Outline = outline;
            return (ISymbol)fill;
        }

        private Pen CreateLinePreviewPen()
        {
            Pen pen = new Pen(_fillColor, Math.Max(1f, (float)_size));
            switch (cboStyle.SelectedIndex)
            {
                case 1:
                    pen.DashStyle = DashStyle.Dash;
                    break;
                case 2:
                    pen.DashStyle = DashStyle.Dot;
                    break;
                case 3:
                    pen.DashStyle = DashStyle.DashDot;
                    break;
                case 4:
                    pen.DashStyle = DashStyle.DashDotDot;
                    break;
                default:
                    pen.DashStyle = DashStyle.Solid;
                    break;
            }
            return pen;
        }

        private Brush CreateFillPreviewBrush(Rectangle bounds)
        {
            switch (cboStyle.SelectedIndex)
            {
                case 1:
                    return new SolidBrush(Color.Transparent);
                case 2:
                    return new HatchBrush(HatchStyle.Horizontal, _fillColor, Color.Transparent);
                case 3:
                    return new HatchBrush(HatchStyle.Vertical, _fillColor, Color.Transparent);
                case 4:
                    return new HatchBrush(HatchStyle.ForwardDiagonal, _fillColor, Color.Transparent);
                case 5:
                    return new HatchBrush(HatchStyle.BackwardDiagonal, _fillColor, Color.Transparent);
                case 6:
                    return new HatchBrush(HatchStyle.Cross, _fillColor, Color.Transparent);
                case 7:
                    return new HatchBrush(HatchStyle.DiagonalCross, _fillColor, Color.Transparent);
                default:
                    return new SolidBrush(_fillColor);
            }
        }

        private static void DrawMarkerPreview(Graphics g, Brush brush, Pen pen, RectangleF r, int styleIndex)
        {
            switch (styleIndex)
            {
                case 1:
                    g.FillRectangle(brush, r);
                    g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
                    break;
                case 2:
                    float cx = r.Left + r.Width / 2f;
                    float cy = r.Top + r.Height / 2f;
                    using (Pen p = new Pen(((SolidBrush)brush).Color, 2f))
                    {
                        g.DrawLine(p, cx, r.Top, cx, r.Bottom);
                        g.DrawLine(p, r.Left, cy, r.Right, cy);
                    }
                    break;
                case 3:
                    PointF[] dia = new PointF[]
                    {
                        new PointF(r.Left + r.Width / 2f, r.Top),
                        new PointF(r.Right, r.Top + r.Height / 2f),
                        new PointF(r.Left + r.Width / 2f, r.Bottom),
                        new PointF(r.Left, r.Top + r.Height / 2f)
                    };
                    g.FillPolygon(brush, dia);
                    g.DrawPolygon(pen, dia);
                    break;
                case 4:
                    using (Pen p = new Pen(((SolidBrush)brush).Color, 2f))
                    {
                        g.DrawLine(p, r.Left, r.Top, r.Right, r.Bottom);
                        g.DrawLine(p, r.Right, r.Top, r.Left, r.Bottom);
                    }
                    break;
                default:
                    g.FillEllipse(brush, r);
                    g.DrawEllipse(pen, r);
                    break;
            }
        }

        private static string SymbolKindText(SymbolKind kind)
        {
            switch (kind)
            {
                case SymbolKind.Marker:
                    return "点符号";
                case SymbolKind.Line:
                    return "线符号";
                case SymbolKind.Fill:
                    return "面符号";
                default:
                    return "未知符号";
            }
        }

        private static decimal ClampDecimal(decimal value, decimal min, decimal max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private static Color ToSystemColor(IColor color)
        {
            if (color == null)
            {
                return Color.Black;
            }

            IRgbColor rgb = color as IRgbColor;
            if (rgb == null)
            {
                rgb = new RgbColorClass();
                rgb.RGB = color.RGB;
            }

            return Color.FromArgb(rgb.Red, rgb.Green, rgb.Blue);
        }

        private static IColor ToRgbColor(Color color)
        {
            IRgbColor rgb = new RgbColorClass();
            rgb.Red = color.R;
            rgb.Green = color.G;
            rgb.Blue = color.B;
            return (IColor)rgb;
        }

        private static int MarkerStyleToIndex(esriSimpleMarkerStyle style)
        {
            if (style == esriSimpleMarkerStyle.esriSMSSquare) return 1;
            if (style == esriSimpleMarkerStyle.esriSMSCross) return 2;
            if (style == esriSimpleMarkerStyle.esriSMSDiamond) return 3;
            if (style == esriSimpleMarkerStyle.esriSMSX) return 4;
            return 0;
        }

        private static esriSimpleMarkerStyle IndexToMarkerStyle(int index)
        {
            switch (index)
            {
                case 1: return esriSimpleMarkerStyle.esriSMSSquare;
                case 2: return esriSimpleMarkerStyle.esriSMSCross;
                case 3: return esriSimpleMarkerStyle.esriSMSDiamond;
                case 4: return esriSimpleMarkerStyle.esriSMSX;
                default: return esriSimpleMarkerStyle.esriSMSCircle;
            }
        }

        private static int LineStyleToIndex(esriSimpleLineStyle style)
        {
            if (style == esriSimpleLineStyle.esriSLSDash) return 1;
            if (style == esriSimpleLineStyle.esriSLSDot) return 2;
            if (style == esriSimpleLineStyle.esriSLSDashDot) return 3;
            if (style == esriSimpleLineStyle.esriSLSDashDotDot) return 4;
            return 0;
        }

        private static esriSimpleLineStyle IndexToLineStyle(int index)
        {
            switch (index)
            {
                case 1: return esriSimpleLineStyle.esriSLSDash;
                case 2: return esriSimpleLineStyle.esriSLSDot;
                case 3: return esriSimpleLineStyle.esriSLSDashDot;
                case 4: return esriSimpleLineStyle.esriSLSDashDotDot;
                default: return esriSimpleLineStyle.esriSLSSolid;
            }
        }

        private static int FillStyleToIndex(esriSimpleFillStyle style)
        {
            if (style == esriSimpleFillStyle.esriSFSNull) return 1;
            if (style == esriSimpleFillStyle.esriSFSHorizontal) return 2;
            if (style == esriSimpleFillStyle.esriSFSVertical) return 3;
            if (style == esriSimpleFillStyle.esriSFSForwardDiagonal) return 4;
            if (style == esriSimpleFillStyle.esriSFSBackwardDiagonal) return 5;
            if (style == esriSimpleFillStyle.esriSFSCross) return 6;
            if (style == esriSimpleFillStyle.esriSFSDiagonalCross) return 7;
            return 0;
        }

        private static esriSimpleFillStyle IndexToFillStyle(int index)
        {
            switch (index)
            {
                case 1: return esriSimpleFillStyle.esriSFSNull;
                case 2: return esriSimpleFillStyle.esriSFSHorizontal;
                case 3: return esriSimpleFillStyle.esriSFSVertical;
                case 4: return esriSimpleFillStyle.esriSFSForwardDiagonal;
                case 5: return esriSimpleFillStyle.esriSFSBackwardDiagonal;
                case 6: return esriSimpleFillStyle.esriSFSCross;
                case 7: return esriSimpleFillStyle.esriSFSDiagonalCross;
                default: return esriSimpleFillStyle.esriSFSSolid;
            }
        }
    }
}

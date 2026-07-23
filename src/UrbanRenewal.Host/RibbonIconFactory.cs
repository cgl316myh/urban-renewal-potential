using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace UrbanRenewal.Host
{
    /// <summary>
    /// Ribbon 大图标（32×32），按按钮标题或键名匹配。
    /// </summary>
    internal static class RibbonIconFactory
    {
        private static readonly Dictionary<string, Image> Cache = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        public static Image GetLargeByCaption(string caption)
        {
            if (string.IsNullOrEmpty(caption))
            {
                return GetLarge("default");
            }
            if (caption.IndexOf("全图", StringComparison.Ordinal) >= 0)
            {
                return GetLarge("map_fit");
            }
            if (caption.IndexOf("漫游", StringComparison.Ordinal) >= 0)
            {
                return GetLarge("map_pan");
            }
            if (caption.IndexOf("放大", StringComparison.Ordinal) >= 0)
            {
                return GetLarge("map_zoomin");
            }
            if (caption.IndexOf("打开 GDB", StringComparison.Ordinal) >= 0 || caption.IndexOf("打开GDB", StringComparison.Ordinal) >= 0)
            {
                return GetLarge("open_gdb");
            }
            if (caption.IndexOf("完整性", StringComparison.Ordinal) >= 0 || caption.IndexOf("检查", StringComparison.Ordinal) >= 0)
            {
                return GetLarge("validate");
            }
            if (caption.IndexOf("预处理", StringComparison.Ordinal) >= 0 || caption.IndexOf("投影", StringComparison.Ordinal) >= 0)
            {
                return GetLarge("preprocess");
            }
            if (caption.IndexOf("动力性", StringComparison.Ordinal) >= 0 || caption.IndexOf("运行", StringComparison.Ordinal) >= 0)
            {
                return GetLarge("run_motivation");
            }
            return GetLarge("default");
        }

        public static Image GetLarge(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                key = "default";
            }
            Image img;
            if (Cache.TryGetValue(key, out img))
            {
                return img;
            }
            img = CreateIcon(key);
            Cache[key] = img;
            return img;
        }

        private static Image CreateIcon(string key)
        {
            Bitmap bmp = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                Rectangle bounds = new Rectangle(1, 1, 30, 30);

                switch (key.ToLowerInvariant())
                {
                    case "map_fit":
                        DrawRoundRect(g, bounds, Color.FromArgb(52, 120, 183), Color.FromArgb(90, 160, 220));
                        using (Pen p = new Pen(Color.White, 2f))
                        {
                            g.DrawRectangle(p, 8, 8, 16, 16);
                            g.DrawLine(p, 8, 8, 12, 12);
                            g.DrawLine(p, 24, 8, 20, 12);
                            g.DrawLine(p, 8, 24, 12, 20);
                            g.DrawLine(p, 24, 24, 20, 20);
                        }
                        break;
                    case "map_pan":
                        DrawRoundRect(g, bounds, Color.FromArgb(70, 130, 80), Color.FromArgb(110, 170, 120));
                        using (Pen p = new Pen(Color.White, 2.5f))
                        {
                            g.DrawLine(p, 16, 6, 16, 26);
                            g.DrawLine(p, 6, 16, 26, 16);
                            g.DrawLine(p, 16, 6, 12, 11);
                            g.DrawLine(p, 16, 6, 20, 11);
                            g.DrawLine(p, 16, 26, 12, 21);
                            g.DrawLine(p, 16, 26, 20, 21);
                            g.DrawLine(p, 6, 16, 11, 12);
                            g.DrawLine(p, 6, 16, 11, 20);
                            g.DrawLine(p, 26, 16, 21, 12);
                            g.DrawLine(p, 26, 16, 21, 20);
                        }
                        break;
                    case "map_zoomin":
                        DrawRoundRect(g, bounds, Color.FromArgb(120, 80, 160), Color.FromArgb(160, 120, 200));
                        using (Pen p = new Pen(Color.White, 2f))
                        {
                            g.DrawEllipse(p, 6, 6, 14, 14);
                            g.DrawLine(p, 18, 18, 26, 26);
                            g.DrawLine(p, 10, 13, 16, 13);
                            g.DrawLine(p, 13, 10, 13, 16);
                        }
                        break;
                    case "open_gdb":
                        DrawRoundRect(g, bounds, Color.FromArgb(180, 110, 40), Color.FromArgb(220, 150, 70));
                        using (Brush b = new SolidBrush(Color.FromArgb(240, 230, 200)))
                        {
                            g.FillRectangle(b, 7, 10, 18, 14);
                        }
                        using (Pen p = new Pen(Color.White, 1.5f))
                        {
                            g.DrawRectangle(p, 7, 10, 18, 14);
                            g.DrawLine(p, 7, 15, 25, 15);
                            g.DrawLine(p, 7, 20, 25, 20);
                        }
                        break;
                    case "validate":
                        DrawRoundRect(g, bounds, Color.FromArgb(40, 140, 100), Color.FromArgb(80, 180, 140));
                        using (Pen p = new Pen(Color.White, 3f))
                        {
                            p.StartCap = LineCap.Round;
                            p.EndCap = LineCap.Round;
                            g.DrawLines(p, new Point[] { new Point(8, 16), new Point(13, 22), new Point(24, 10) });
                        }
                        break;
                    case "preprocess":
                        DrawRoundRect(g, bounds, Color.FromArgb(90, 100, 140), Color.FromArgb(130, 140, 180));
                        using (Pen p = new Pen(Color.White, 2f))
                        {
                            g.DrawEllipse(p, 7, 7, 10, 10);
                            g.DrawEllipse(p, 15, 15, 10, 10);
                            g.DrawLine(p, 16, 12, 20, 16);
                        }
                        break;
                    case "run_motivation":
                        DrawRoundRect(g, bounds, Color.FromArgb(190, 60, 60), Color.FromArgb(230, 100, 90));
                        using (Brush b = new SolidBrush(Color.White))
                        {
                            g.FillPolygon(b, new Point[] { new Point(11, 8), new Point(24, 16), new Point(11, 24) });
                        }
                        break;
                    default:
                        DrawRoundRect(g, bounds, Color.FromArgb(100, 100, 100), Color.FromArgb(150, 150, 150));
                        using (Pen p = new Pen(Color.White, 2f))
                        {
                            g.DrawEllipse(p, 10, 10, 12, 12);
                        }
                        break;
                }
            }
            return bmp;
        }

        private static void DrawRoundRect(Graphics g, Rectangle bounds, Color c1, Color c2)
        {
            using (GraphicsPath path = CreateRoundRect(bounds, 6))
            using (LinearGradientBrush brush = new LinearGradientBrush(bounds, c1, c2, 90f))
            {
                g.FillPath(brush, path);
            }
        }

        private static GraphicsPath CreateRoundRect(Rectangle r, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

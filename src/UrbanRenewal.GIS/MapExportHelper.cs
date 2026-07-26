using System;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Output;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// 将当前地图视图导出为 PDF / TIFF（专题图近似输出）。
    /// </summary>
    public static class MapExportHelper
    {
        public static bool ExportToPdf(object mapControl, string filePath, out string message)
        {
            return Export(mapControl, filePath, true, out message);
        }

        public static bool ExportToTiff(object mapControl, string filePath, out string message)
        {
            return Export(mapControl, filePath, false, out message);
        }

        private static bool Export(object mapControl, string filePath, bool pdf, out string message)
        {
            message = null;
            IMapControl3 map = ResolveMap(mapControl);
            if (map == null || map.ActiveView == null)
            {
                message = "地图控件未就绪。";
                return false;
            }
            if (string.IsNullOrEmpty(filePath))
            {
                message = "导出路径无效。";
                return false;
            }

            try
            {
                string dir = System.IO.Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }

                IActiveView activeView = map.ActiveView;
                IExport export;
                if (pdf)
                {
                    export = new ExportPDFClass();
                }
                else
                {
                    export = new ExportTIFFClass();
                }
                export.ExportFileName = filePath;

                tagRECT exportRECT = activeView.ExportFrame;
                IEnvelope envelope = activeView.Extent;
                export.PixelBounds = RectToEnvelope(exportRECT);

                int hDC = export.StartExporting();
                activeView.Output(hDC, (int)export.Resolution, ref exportRECT, envelope, null);
                export.FinishExporting();
                export.Cleanup();

                message = (pdf ? "已导出 PDF: " : "已导出 TIFF: ") + filePath;
                return true;
            }
            catch (Exception ex)
            {
                message = "地图导出失败: " + ex.Message;
                return false;
            }
        }

        private static IEnvelope RectToEnvelope(tagRECT rect)
        {
            IEnvelope env = new EnvelopeClass();
            env.PutCoords(rect.left, rect.bottom, rect.right, rect.top);
            return env;
        }

        private static IMapControl3 ResolveMap(object mapControl)
        {
            IMapControl3 map = mapControl as IMapControl3;
            if (map != null)
            {
                return map;
            }
            if (mapControl == null)
            {
                return null;
            }
            System.Reflection.PropertyInfo prop = mapControl.GetType().GetProperty("Object");
            if (prop != null)
            {
                return prop.GetValue(mapControl, null) as IMapControl3;
            }
            return null;
        }
    }
}

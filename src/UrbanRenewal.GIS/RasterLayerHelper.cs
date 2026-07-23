using System;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geodatabase;

namespace UrbanRenewal.GIS
{
    public static class RasterLayerHelper
    {
        public static bool AddRasterToMap(IMapControl3 mapControl, string rasterPath, string layerName, out string message)
        {
            message = null;
            if (mapControl == null || string.IsNullOrEmpty(rasterPath))
            {
                message = "地图或栅格路径无效。";
                return false;
            }

            try
            {
                IRasterDataset dataset = OpenRasterDataset(rasterPath);
                if (dataset == null)
                {
                    message = "无法打开栅格: " + rasterPath;
                    return false;
                }

                IRasterLayer layer = new RasterLayerClass();
                layer.CreateFromDataset(dataset);
                if (!string.IsNullOrEmpty(layerName))
                {
                    layer.Name = layerName;
                }

                mapControl.AddLayer((ILayer)layer, 0);
                mapControl.Refresh();
                message = "已加载栅格图层: " + layer.Name;
                return true;
            }
            catch (Exception ex)
            {
                message = "加载栅格失败: " + ex.Message;
                return false;
            }
        }

        private static IRasterDataset OpenRasterDataset(string rasterPath)
        {
            string folder = System.IO.Path.GetDirectoryName(rasterPath);
            string name = System.IO.Path.GetFileName(rasterPath);

            // File GDB 内栅格：...\xxx.gdb\rasterName
            if (!string.IsNullOrEmpty(folder) && folder.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
            {
                IWorkspaceFactory gwf = new FileGDBWorkspaceFactoryClass();
                IWorkspace ws = gwf.OpenFromFile(folder, 0);
                IRasterWorkspaceEx rws = ws as IRasterWorkspaceEx;
                if (rws != null)
                {
                    return rws.OpenRasterDataset(name);
                }
            }

            IWorkspaceFactory factory = new RasterWorkspaceFactoryClass();
            IRasterWorkspace rasterWs = (IRasterWorkspace)factory.OpenFromFile(folder, 0);
            return rasterWs.OpenRasterDataset(name);
        }
    }
}

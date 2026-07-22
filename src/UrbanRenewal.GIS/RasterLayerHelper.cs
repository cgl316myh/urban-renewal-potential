using System;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
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
                string folder = System.IO.Path.GetDirectoryName(rasterPath);
                string name = System.IO.Path.GetFileName(rasterPath);

                IWorkspaceFactory factory = new RasterWorkspaceFactoryClass();
                IRasterWorkspace rasterWs = (IRasterWorkspace)factory.OpenFromFile(folder, 0);
                IRasterDataset dataset = rasterWs.OpenRasterDataset(name);

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
    }
}

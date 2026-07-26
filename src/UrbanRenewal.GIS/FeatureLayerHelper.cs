using System;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;

namespace UrbanRenewal.GIS
{
    public static class FeatureLayerHelper
    {
        public static bool AddFeatureClassToMap(
            IMapControl3 mapControl,
            string featureClassPath,
            string layerName,
            out string message)
        {
            message = null;
            if (mapControl == null || string.IsNullOrEmpty(featureClassPath))
            {
                message = "地图或要素类路径无效。";
                return false;
            }

            try
            {
                string gdb = System.IO.Path.GetDirectoryName(featureClassPath);
                string name = System.IO.Path.GetFileName(featureClassPath);
                if (string.IsNullOrEmpty(gdb) || string.IsNullOrEmpty(name))
                {
                    message = "无法解析要素类路径: " + featureClassPath;
                    return false;
                }

                IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
                IFeatureWorkspace fw = factory.OpenFromFile(gdb, 0) as IFeatureWorkspace;
                if (fw == null)
                {
                    message = "无法打开工作空间: " + gdb;
                    return false;
                }

                IFeatureClass fc = fw.OpenFeatureClass(name);
                IFeatureLayer layer = new FeatureLayerClass();
                layer.FeatureClass = fc;
                layer.Name = string.IsNullOrEmpty(layerName) ? name : layerName;

                mapControl.AddLayer((ILayer)layer, 0);
                mapControl.Refresh();
                message = "已加载要素图层: " + layer.Name;
                return true;
            }
            catch (Exception ex)
            {
                message = "加载要素图层失败: " + ex.Message;
                return false;
            }
        }
    }
}

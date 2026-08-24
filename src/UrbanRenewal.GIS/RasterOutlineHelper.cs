using System;
using System.Drawing;
using System.IO;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.ConversionTools;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.SpatialAnalystTools;

namespace UrbanRenewal.GIS
{
    /// <summary>栅格边线：渲染器忽略 Outline，需 RasterToPolygon；GP 中文路径易 000864/000367，浮点栅格需先 Int。</summary>
    public static class RasterOutlineHelper
    {
        public const string OutlineSuffix = "_边线";

        public static string GetOutlineLayerName(string rasterLayerName)
        {
            string baseName = string.IsNullOrEmpty(rasterLayerName) ? "栅格" : rasterLayerName.Trim();
            if (baseName.EndsWith(OutlineSuffix, StringComparison.Ordinal))
            {
                return baseName;
            }
            return baseName + OutlineSuffix;
        }

        public static void SyncOutlineLayer(
            IMap map,
            IRasterLayer rasterLayer,
            bool drawOutline,
            Color outlineColor,
            double outlineWidth,
            out string message)
        {
            message = null;
            if (map == null || rasterLayer == null)
            {
                message = "地图或栅格图层无效。";
                return;
            }

            string outlineName = GetOutlineLayerName(rasterLayer.Name);
            RemoveLayersByName(map, outlineName);

            if (!drawOutline)
            {
                message = "已清除边线图层。";
                return;
            }

            if (rasterLayer.Raster == null)
            {
                throw new Exception("栅格图层无有效数据。");
            }

            string tempDir = Path.Combine(Path.GetTempPath(), "UrbanRenewalRasterOutline");
            Directory.CreateDirectory(tempDir);
            string id = Guid.NewGuid().ToString("N").Substring(0, 8);

            // 另存英文路径，避免 GP 读中文路径失败
            string asciiTiff = Path.Combine(tempDir, "src_" + id + ".tif");
            TryDeleteFile(asciiTiff);
            SaveRasterAsTiff(rasterLayer.Raster, tempDir, "src_" + id + ".tif");
            if (!File.Exists(asciiTiff))
            {
                throw new Exception("无法将栅格导出到临时目录（ASCII 路径）。");
            }

            GeoprocessorHelper gp = new GeoprocessorHelper();

            string polygonInput = asciiTiff;
            if (IsFloatingPixelType(rasterLayer.Raster))
            {
                string intRaster = Path.Combine(tempDir, "int_" + id + ".tif");
                TryDeleteFile(intRaster);
                Int intTool = new Int();
                intTool.in_raster_or_constant = asciiTiff;
                intTool.out_raster = intRaster;
                gp.Execute(intTool, "Int-Outline");
                if (!File.Exists(intRaster))
                {
                    string alt = Path.Combine(tempDir, "int_" + id);
                    if (Directory.Exists(alt) || File.Exists(alt))
                    {
                        intRaster = alt;
                    }
                    else
                    {
                        throw new Exception("浮点栅格转整型失败，无法生成边线。建议对等级栅格（如 pot_level）使用边线。");
                    }
                }
                polygonInput = intRaster;
            }

            string shpBase = "ol_" + id;
            string outShp = Path.Combine(tempDir, shpBase + ".shp");
            TryDeleteShapefile(outShp);

            RasterToPolygon tool = new RasterToPolygon();
            tool.in_raster = polygonInput;
            tool.out_polygon_features = outShp;
            tool.simplify = "NO_SIMPLIFY";
            // 显式清掉高版本工具的无效默认参数（日志里曾出现 SINGLE OUTER PART #）
            TryClearRasterToPolygonExtras(tool);

            gp.Execute(tool, "RasterToPolygon-Outline");

            if (!File.Exists(outShp))
            {
                throw new Exception("栅格转面未生成输出文件。");
            }

            IFeatureClass fc = OpenShapefile(outShp);
            if (fc == null)
            {
                throw new Exception("无法打开边线要素类。");
            }

            IFeatureLayer featureLayer = new FeatureLayerClass();
            featureLayer.FeatureClass = fc;
            featureLayer.Name = outlineName;
            ((ILayer)featureLayer).Visible = true;

            ApplyHollowOutlineRenderer(featureLayer, outlineColor, outlineWidth);

            int rasterIndex = FindLayerIndex(map, rasterLayer);
            map.AddLayer((ILayer)featureLayer);
            if (rasterIndex >= 0)
            {
                try
                {
                    map.MoveLayer((ILayer)featureLayer, rasterIndex);
                }
                catch
                {
                }
            }

            message = "已生成边线图层: " + outlineName;
        }

        private static void TryClearRasterToPolygonExtras(RasterToPolygon tool)
        {
            if (tool == null)
            {
                return;
            }
            try
            {
                // 10.3+/Pro 参数；避免默认 max_vertices=# 导致参数域错误
                tool.create_multipart_features = "MULTIPLE_OUTER_PART";
            }
            catch
            {
            }
            try
            {
                tool.max_vertices_per_feature = 1;
            }
            catch
            {
            }
        }

        private static void SaveRasterAsTiff(IRaster raster, string folder, string fileName)
        {
            if (raster == null)
            {
                throw new Exception("栅格无效。");
            }

            IWorkspaceFactory factory = new RasterWorkspaceFactoryClass();
            IWorkspace ws = factory.OpenFromFile(folder, 0);

            ISaveAs saveAs = raster as ISaveAs;
            if (saveAs == null)
            {
                throw new Exception("当前栅格不支持另存为 TIFF。");
            }

            string full = Path.Combine(folder, fileName);
            TryDeleteFile(full);
            object result = saveAs.SaveAs(fileName, ws, "TIFF");
            if (result == null && !File.Exists(full))
            {
                throw new Exception("另存 TIFF 未生成文件: " + full);
            }
        }

        private static bool IsFloatingPixelType(IRaster raster)
        {
            IRasterProps props = raster as IRasterProps;
            if (props == null)
            {
                return false;
            }
            rstPixelType pt = props.PixelType;
            return pt == rstPixelType.PT_FLOAT || pt == rstPixelType.PT_DOUBLE;
        }

        private static void ApplyHollowOutlineRenderer(
            IFeatureLayer featureLayer,
            Color outlineColor,
            double outlineWidth)
        {
            if (outlineWidth < 0.5)
            {
                outlineWidth = 0.5;
            }

            ISimpleFillSymbol fill = new SimpleFillSymbolClass();
            fill.Style = esriSimpleFillStyle.esriSFSNull;

            IRgbColor fillColor = new RgbColorClass();
            fillColor.NullColor = true;
            fill.Color = (IColor)fillColor;

            ISimpleLineSymbol outline = new SimpleLineSymbolClass();
            outline.Style = esriSimpleLineStyle.esriSLSSolid;
            outline.Width = outlineWidth;
            IRgbColor lineColor = new RgbColorClass();
            lineColor.Red = outlineColor.R;
            lineColor.Green = outlineColor.G;
            lineColor.Blue = outlineColor.B;
            lineColor.Transparency = 0;
            lineColor.UseWindowsDithering = true;
            outline.Color = (IColor)lineColor;
            fill.Outline = (ILineSymbol)outline;

            ISimpleRenderer renderer = new SimpleRendererClass();
            renderer.Symbol = (ISymbol)fill;
            renderer.Label = "边线";

            IGeoFeatureLayer geo = featureLayer as IGeoFeatureLayer;
            if (geo != null)
            {
                geo.Renderer = (IFeatureRenderer)renderer;
            }
        }

        private static int FindLayerIndex(IMap map, ILayer target)
        {
            if (map == null || target == null)
            {
                return -1;
            }
            for (int i = 0; i < map.LayerCount; i++)
            {
                ILayer layer = map.get_Layer(i);
                if (object.ReferenceEquals(layer, target))
                {
                    return i;
                }
                if (layer != null && target.Name != null
                    && string.Equals(layer.Name, target.Name, StringComparison.OrdinalIgnoreCase)
                    && layer is IRasterLayer)
                {
                    return i;
                }
            }
            return -1;
        }

        private static void RemoveLayersByName(IMap map, string layerName)
        {
            if (map == null || string.IsNullOrEmpty(layerName))
            {
                return;
            }

            for (int i = map.LayerCount - 1; i >= 0; i--)
            {
                ILayer layer = map.get_Layer(i);
                if (layer != null
                    && string.Equals(layer.Name, layerName, StringComparison.OrdinalIgnoreCase))
                {
                    map.DeleteLayer(layer);
                }
            }
        }

        public static string TryGetRasterPath(IRasterLayer rasterLayer)
        {
            if (rasterLayer == null)
            {
                return null;
            }

            try
            {
                IDataLayer dataLayer = rasterLayer as IDataLayer;
                if (dataLayer != null)
                {
                    IDatasetName dsName = dataLayer.DataSourceName as IDatasetName;
                    if (dsName != null && dsName.WorkspaceName != null)
                    {
                        string wsPath = dsName.WorkspaceName.PathName;
                        string name = dsName.Name;
                        if (!string.IsNullOrEmpty(wsPath) && !string.IsNullOrEmpty(name))
                        {
                            return Path.Combine(wsPath, name);
                        }
                    }
                }
            }
            catch
            {
            }

            try
            {
                IDataset ds = rasterLayer as IDataset;
                if (ds != null && ds.Workspace != null)
                {
                    string wsPath = ds.Workspace.PathName;
                    string name = ds.BrowseName;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = ds.Name;
                    }
                    if (!string.IsNullOrEmpty(wsPath) && !string.IsNullOrEmpty(name))
                    {
                        return Path.Combine(wsPath, name);
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private static IFeatureClass OpenShapefile(string shpPath)
        {
            string folder = Path.GetDirectoryName(shpPath);
            string name = Path.GetFileNameWithoutExtension(shpPath);
            IWorkspaceFactory factory = new ShapefileWorkspaceFactoryClass();
            IFeatureWorkspace fws = (IFeatureWorkspace)factory.OpenFromFile(folder, 0);
            return fws.OpenFeatureClass(name);
        }

        private static void TryDeleteShapefile(string shpPath)
        {
            if (string.IsNullOrEmpty(shpPath))
            {
                return;
            }
            string dir = Path.GetDirectoryName(shpPath);
            string name = Path.GetFileNameWithoutExtension(shpPath);
            if (string.IsNullOrEmpty(dir) || string.IsNullOrEmpty(name))
            {
                return;
            }
            string[] exts = new string[] { ".shp", ".shx", ".dbf", ".prj", ".sbn", ".sbx", ".cpg", ".xml" };
            for (int i = 0; i < exts.Length; i++)
            {
                TryDeleteFile(Path.Combine(dir, name + exts[i]));
            }
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
            }
        }
    }
}

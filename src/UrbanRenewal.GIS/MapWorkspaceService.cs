using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// 地图工作空间加载与常用视图命令。
    /// </summary>
    public static class MapWorkspaceService
    {
        public static int LoadFileGdb(IMapControl3 mapControl, string gdbPath, out string message)
        {
            message = null;
            if (mapControl == null)
            {
                message = "地图控件无效。";
                return 0;
            }
            if (string.IsNullOrEmpty(gdbPath) || !System.IO.Directory.Exists(gdbPath))
            {
                message = "GDB 路径无效: " + gdbPath;
                return 0;
            }

            try
            {
                mapControl.ClearLayers();

                IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
                IWorkspace workspace = factory.OpenFromFile(gdbPath, 0);
                IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;

                List<string> classNames = new List<string>();
                CollectFeatureClassNames(workspace, classNames);

                int loaded = 0;
                for (int i = 0; i < classNames.Count; i++)
                {
                    string name = classNames[i];
                    try
                    {
                        IFeatureClass fc = featureWorkspace.OpenFeatureClass(name);
                        IFeatureLayer layer = new FeatureLayerClass();
                        layer.FeatureClass = fc;
                        layer.Name = name;
                        mapControl.AddLayer((ILayer)layer, 0);
                        loaded++;
                    }
                    catch (Exception exLayer)
                    {
                        // 跳过无法打开的要素类，继续其余
                        System.Diagnostics.Debug.WriteLine("跳过图层 " + name + ": " + exLayer.Message);
                    }
                }

                if (loaded > 0)
                {
                    mapControl.Extent = mapControl.FullExtent;
                    mapControl.Refresh();
                }

                message = "已从 GDB 加载 " + loaded + " 个要素图层。";
                return loaded;
            }
            catch (Exception ex)
            {
                message = "打开 GDB 失败: " + ex.Message;
                return 0;
            }
        }

        public static string CheckIntegrity(string gdbPath)
        {
            StringBuilder sb = new StringBuilder();
            if (string.IsNullOrEmpty(gdbPath) || !System.IO.Directory.Exists(gdbPath))
            {
                return "失败：未选择有效的 File GDB。";
            }

            try
            {
                IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
                IWorkspace workspace = factory.OpenFromFile(gdbPath, 0);
                sb.AppendLine("GDB 可打开: " + gdbPath);

                List<string> classNames = new List<string>();
                CollectFeatureClassNames(workspace, classNames);
                sb.AppendLine("要素类数量: " + classNames.Count);

                bool hasBuiltUp = ContainsKeyword(classNames, new string[] { "建成区", "城区", "BuiltUp", "builtup", "CityArea" });
                bool hasParcel = ContainsKeyword(classNames, new string[] { "宗地", "地块", "Parcel", "parcel", "LandParcel" });
                sb.AppendLine(hasBuiltUp ? "[通过] 疑似建成区/城区范围图层" : "[警告] 未匹配到建成区/城区范围（必选）");
                sb.AppendLine(hasParcel ? "[通过] 疑似宗地/地块图层" : "[警告] 未匹配到宗地/地块（必选）");

                IFeatureWorkspace fws = (IFeatureWorkspace)workspace;
                ISpatialReference firstSr = null;
                for (int i = 0; i < classNames.Count; i++)
                {
                    try
                    {
                        IFeatureClass fc = fws.OpenFeatureClass(classNames[i]);
                        IGeoDataset geo = fc as IGeoDataset;
                        if (geo != null && geo.SpatialReference != null)
                        {
                            firstSr = geo.SpatialReference;
                            break;
                        }
                    }
                    catch
                    {
                    }
                }

                if (firstSr == null)
                {
                    sb.AppendLine("[警告] 无法读取空间参考。");
                }
                else
                {
                    sb.AppendLine("空间参考: " + firstSr.Name);
                    IProjectedCoordinateSystem pcs = firstSr as IProjectedCoordinateSystem;
                    if (pcs != null)
                    {
                        sb.AppendLine("[通过] 投影坐标系（可用于距离缓冲分析）");
                    }
                    else
                    {
                        sb.AppendLine("[警告] 非投影坐标系，后续缓冲/距离分析前需投影转换");
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return "完整性检查失败: " + ex.Message;
            }
        }

        public static void ZoomToFullExtent(IMapControl3 mapControl)
        {
            if (mapControl == null || mapControl.LayerCount == 0)
            {
                return;
            }
            ICommand cmd = new ControlsMapFullExtentCommandClass();
            cmd.OnCreate(mapControl.Object);
            cmd.OnClick();
        }

        public static void ActivatePan(IMapControl3 mapControl)
        {
            if (mapControl == null)
            {
                return;
            }
            ICommand cmd = new ControlsMapPanToolClass();
            cmd.OnCreate(mapControl.Object);
            mapControl.CurrentTool = cmd as ITool;
        }

        public static void ActivateZoomIn(IMapControl3 mapControl)
        {
            if (mapControl == null)
            {
                return;
            }
            ICommand cmd = new ControlsMapZoomInToolClass();
            cmd.OnCreate(mapControl.Object);
            mapControl.CurrentTool = cmd as ITool;
        }

        private static void CollectFeatureClassNames(IWorkspace workspace, List<string> names)
        {
            IEnumDataset enumFc = workspace.get_Datasets(esriDatasetType.esriDTFeatureClass);
            if (enumFc != null)
            {
                enumFc.Reset();
                IDataset ds = enumFc.Next();
                while (ds != null)
                {
                    names.Add(ds.Name);
                    ds = enumFc.Next();
                }
            }

            IEnumDataset enumFd = workspace.get_Datasets(esriDatasetType.esriDTFeatureDataset);
            if (enumFd != null)
            {
                enumFd.Reset();
                IDataset fd = enumFd.Next();
                while (fd != null)
                {
                    IFeatureDataset featureDataset = fd as IFeatureDataset;
                    if (featureDataset != null)
                    {
                        IEnumDataset subsets = featureDataset.Subsets;
                        if (subsets != null)
                        {
                            subsets.Reset();
                            IDataset child = subsets.Next();
                            while (child != null)
                            {
                                if (child.Type == esriDatasetType.esriDTFeatureClass)
                                {
                                    names.Add(child.Name);
                                }
                                child = subsets.Next();
                            }
                        }
                    }
                    fd = enumFd.Next();
                }
            }
        }

        private static bool ContainsKeyword(List<string> names, string[] keywords)
        {
            for (int i = 0; i < names.Count; i++)
            {
                string n = names[i];
                for (int k = 0; k < keywords.Length; k++)
                {
                    if (n.IndexOf(keywords[k], StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

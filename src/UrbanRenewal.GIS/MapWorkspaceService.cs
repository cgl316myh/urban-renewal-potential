using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
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
                ClearLayers(mapControl);

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

        /// <summary>清空地图图层，释放对 File GDB 要素类的占用（替换图层前建议调用）。</summary>
        public static void ClearLayers(IMapControl3 mapControl)
        {
            if (mapControl == null)
            {
                return;
            }
            try
            {
                // 先断开 FeatureClass 引用，再删层，避免 ClearLayers 后 RCW 仍占 File GDB 锁
                for (int i = mapControl.LayerCount - 1; i >= 0; i--)
                {
                    try
                    {
                        ILayer layer = mapControl.get_Layer(i);
                        IFeatureLayer fl = layer as IFeatureLayer;
                        if (fl != null)
                        {
                            try { fl.FeatureClass = null; }
                            catch { }
                        }
                        // IMapControl3.DeleteLayer 参数为图层索引（int），不是 ILayer
                        mapControl.DeleteLayer(i);
                    }
                    catch
                    {
                    }
                }
                try { mapControl.ClearLayers(); }
                catch { }
                try
                {
                    if (mapControl.ActiveView != null)
                    {
                        mapControl.ActiveView.ContentsChanged();
                        mapControl.ActiveView.Refresh();
                    }
                    else
                    {
                        mapControl.Refresh();
                    }
                }
                catch
                {
                    try { mapControl.Refresh(); }
                    catch { }
                }
            }
            catch
            {
            }

            FileGdbLockHelper.ForceComRelease();
        }

        /// <summary>
        /// 仅移除引用指定 File GDB 的图层（含结果栅格），保留其它图层。
        /// 再分析前调用，避免 traf10mx 等 VAT schema lock 导致 CellStatistics 000871。
        /// </summary>
        public static int RemoveLayersReferencingGdb(IMapControl3 mapControl, string gdbPath)
        {
            if (mapControl == null || string.IsNullOrEmpty(gdbPath))
            {
                return 0;
            }

            string target;
            try
            {
                target = System.IO.Path.GetFullPath(gdbPath).TrimEnd('\\', '/');
            }
            catch
            {
                return 0;
            }

            int removed = 0;
            try
            {
                for (int i = mapControl.LayerCount - 1; i >= 0; i--)
                {
                    ILayer layer = null;
                    try
                    {
                        layer = mapControl.get_Layer(i);
                        if (!LayerReferencesGdb(layer, target))
                        {
                            continue;
                        }

                        IFeatureLayer fl = layer as IFeatureLayer;
                        if (fl != null)
                        {
                            try { fl.FeatureClass = null; }
                            catch { }
                        }
                        mapControl.DeleteLayer(i);
                        removed++;
                    }
                    catch
                    {
                    }
                }

                try
                {
                    if (mapControl.ActiveView != null)
                    {
                        mapControl.ActiveView.ContentsChanged();
                        mapControl.ActiveView.Refresh();
                    }
                    else
                    {
                        mapControl.Refresh();
                    }
                }
                catch
                {
                }
            }
            catch
            {
            }

            FileGdbLockHelper.ForceComRelease();
            return removed;
        }

        private static bool LayerReferencesGdb(ILayer layer, string gdbFullPath)
        {
            if (layer == null || string.IsNullOrEmpty(gdbFullPath))
            {
                return false;
            }

            try
            {
                IFeatureLayer fl = layer as IFeatureLayer;
                if (fl != null && fl.FeatureClass != null)
                {
                    return WorkspacePathEquals(((IDataset)fl.FeatureClass).Workspace, gdbFullPath);
                }
            }
            catch
            {
            }

            try
            {
                IRasterLayer rl = layer as IRasterLayer;
                if (rl != null && rl.Raster != null)
                {
                    IRasterBandCollection bands = rl.Raster as IRasterBandCollection;
                    if (bands != null && bands.Count > 0)
                    {
                        IRasterBand band = bands.Item(0);
                        IRasterDataset rds = band.RasterDataset;
                        IDataset ds = rds as IDataset;
                        if (ds != null)
                        {
                            return WorkspacePathEquals(ds.Workspace, gdbFullPath);
                        }
                    }
                }
            }
            catch
            {
            }

            try
            {
                IDataLayer dataLayer = layer as IDataLayer;
                if (dataLayer != null)
                {
                    IName n = dataLayer.DataSourceName;
                    IDatasetName dn = n as IDatasetName;
                    if (dn != null && dn.WorkspaceName != null)
                    {
                        string path = dn.WorkspaceName.PathName;
                        return PathsEqual(path, gdbFullPath);
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        private static bool WorkspacePathEquals(IWorkspace workspace, string gdbFullPath)
        {
            if (workspace == null)
            {
                return false;
            }
            try
            {
                return PathsEqual(workspace.PathName, gdbFullPath);
            }
            catch
            {
                return false;
            }
        }

        private static bool PathsEqual(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            {
                return false;
            }
            try
            {
                string fa = System.IO.Path.GetFullPath(a).TrimEnd('\\', '/');
                string fb = System.IO.Path.GetFullPath(b).TrimEnd('\\', '/');
                return string.Equals(fa, fb, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 预处理写回输入库前：清空地图并尽量释放 GDB 占用。
        /// </summary>
        public static void ReleaseGdbLocksForReplace(object mapControlOrAx)
        {
            ClearLayersFromObject(mapControlOrAx);
            FileGdbLockHelper.ForceComRelease();
        }

        /// <summary>支持传入 AxMapControl 或 IMapControl3。</summary>
        public static void ClearLayersFromObject(object mapControlOrAx)
        {
            if (mapControlOrAx == null)
            {
                return;
            }
            IMapControl3 map = mapControlOrAx as IMapControl3;
            if (map == null)
            {
                try
                {
                    System.Reflection.PropertyInfo pi = mapControlOrAx.GetType().GetProperty("Object");
                    if (pi != null)
                    {
                        map = pi.GetValue(mapControlOrAx, null) as IMapControl3;
                    }
                }
                catch
                {
                    map = null;
                }
            }
            ClearLayers(map);
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

                List<string> rasterNames = new List<string>();
                CollectRasterDatasetNames(workspace, rasterNames);
                sb.AppendLine("栅格数据集数量: " + rasterNames.Count);
                bool hasDem = ContainsKeyword(rasterNames, new string[] { "DEM", "dem", "高程", "Elevation" });
                bool hasPop = ContainsKeyword(rasterNames, new string[] { "人口", "人口密度", "population", "pop", "PopDensity" });
                sb.AppendLine(hasDem ? "[通过] 疑似 DEM 高程栅格（可行度）" : "[警告] 未匹配到 DEM 高程栅格（可行度推荐）");
                sb.AppendLine(hasPop ? "[通过] 疑似人口密度栅格（可行度）" : "[警告] 未匹配到人口密度栅格（可行度推荐）");

                // 详细空间参考一致性（不一致图层逐条警告）
                sb.Append(SpatialReferenceAudit.Audit(gdbPath).ToCheckReport());

                // 预建路网（路网可达性前置条件）
                sb.Append(NetworkDatasetHelper.BuildIntegrityReport(gdbPath));

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

        public static void ActivateZoomOut(IMapControl3 mapControl)
        {
            if (mapControl == null)
            {
                return;
            }
            ICommand cmd = new ControlsMapZoomOutToolClass();
            cmd.OnCreate(mapControl.Object);
            mapControl.CurrentTool = cmd as ITool;
        }

        public static void ActivateSelectFeatures(IMapControl3 mapControl)
        {
            if (mapControl == null)
            {
                return;
            }
            ICommand cmd = new ControlsSelectFeaturesToolClass();
            cmd.OnCreate(mapControl.Object);
            mapControl.CurrentTool = cmd as ITool;
        }

        public static void ClearSelection(IMapControl3 mapControl)
        {
            if (mapControl == null)
            {
                return;
            }
            ICommand cmd = new ControlsClearSelectionCommandClass();
            cmd.OnCreate(mapControl.Object);
            cmd.OnClick();
            try
            {
                if (mapControl.ActiveView != null)
                {
                    mapControl.ActiveView.PartialRefresh(
                        esriViewDrawPhase.esriViewGeoSelection, null, null);
                }
            }
            catch
            {
                try { mapControl.Refresh(); }
                catch { }
            }
        }

        public static void ActivateIdentify(IMapControl3 mapControl)
        {
            if (mapControl == null)
            {
                return;
            }
            ICommand cmd = new ControlsMapIdentifyToolClass();
            cmd.OnCreate(mapControl.Object);
            mapControl.CurrentTool = cmd as ITool;
        }

        /// <summary>单击加点，双击结束；结果通过 onResult 回传。</summary>
        public static void ActivateMeasureLength(IMapControl3 mapControl, Action<string> onResult)
        {
            if (mapControl == null)
            {
                return;
            }
            ICommand cmd = new MeasureLengthTool(onResult);
            cmd.OnCreate(mapControl.Object);
            mapControl.CurrentTool = cmd as ITool;
        }

        /// <summary>单击加点，双击结束；结果通过 onResult 回传。</summary>
        public static void ActivateMeasureArea(IMapControl3 mapControl, Action<string> onResult)
        {
            if (mapControl == null)
            {
                return;
            }
            ICommand cmd = new MeasureAreaTool(onResult);
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

        private static void CollectRasterDatasetNames(IWorkspace workspace, List<string> names)
        {
            IEnumDataset enumRs = workspace.get_Datasets(esriDatasetType.esriDTRasterDataset);
            if (enumRs != null)
            {
                enumRs.Reset();
                IDataset ds = enumRs.Next();
                while (ds != null)
                {
                    names.Add(ds.Name);
                    ds = enumRs.Next();
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
                                if (child.Type == esriDatasetType.esriDTRasterDataset)
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

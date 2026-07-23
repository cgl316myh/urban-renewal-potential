using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// 单图层空间参考检查项。
    /// </summary>
    public sealed class SpatialReferenceLayerInfo
    {
        public string LayerName { get; set; }
        public string SpatialReferenceName { get; set; }
        public int FactoryCode { get; set; }
        public bool MatchesReference { get; set; }
        public bool IsProjected { get; set; }
    }

    /// <summary>
    /// GDB 空间参考一致性审计结果。
    /// </summary>
    public sealed class SpatialReferenceAuditResult
    {
        public SpatialReferenceAuditResult()
        {
            Layers = new List<SpatialReferenceLayerInfo>();
            MismatchedLayers = new List<SpatialReferenceLayerInfo>();
        }

        public bool Success { get; set; }

        public bool IsUnified { get; set; }

        public string ReferenceLayerName { get; set; }

        public string ReferenceSpatialReferenceName { get; set; }

        public bool ReferenceIsProjected { get; set; }

        public List<SpatialReferenceLayerInfo> Layers { get; private set; }

        public List<SpatialReferenceLayerInfo> MismatchedLayers { get; private set; }

        public string ErrorMessage { get; set; }

        /// <summary>
        /// 生成完整性检查用的警告/通过文本。
        /// </summary>
        public string ToCheckReport()
        {
            StringBuilder sb = new StringBuilder();
            if (!Success)
            {
                sb.AppendLine("[警告] 空间参考检查失败: " + (ErrorMessage ?? "未知错误"));
                return sb.ToString();
            }

            if (Layers.Count == 0)
            {
                sb.AppendLine("[警告] 未找到要素类，无法检查空间参考。");
                return sb.ToString();
            }

            if (string.IsNullOrEmpty(ReferenceSpatialReferenceName))
            {
                sb.AppendLine("[警告] 无法确定基准空间参考。");
                return sb.ToString();
            }

            sb.AppendLine("空间参考基准: " + ReferenceSpatialReferenceName
                + (string.IsNullOrEmpty(ReferenceLayerName) ? string.Empty : "（取自: " + ReferenceLayerName + "）"));

            if (ReferenceIsProjected)
            {
                sb.AppendLine("[通过] 基准为投影坐标系（可用于距离缓冲分析）");
            }
            else
            {
                sb.AppendLine("[警告] 基准为非投影坐标系，缓冲/距离分析前需先投影到平面坐标系");
            }

            if (IsUnified)
            {
                sb.AppendLine("[通过] 全部 " + Layers.Count + " 个要素类空间参考一致");
            }
            else
            {
                sb.AppendLine("[警告] 空间参考不统一：共 " + MismatchedLayers.Count
                    + " 个图层与基准不一致（动力性分析仅校验所用图层；未用图层如宗地可后续再统一）");
                for (int i = 0; i < MismatchedLayers.Count; i++)
                {
                    SpatialReferenceLayerInfo info = MismatchedLayers[i];
                    sb.AppendLine("  [警告] " + info.LayerName + " → "
                        + (string.IsNullOrEmpty(info.SpatialReferenceName) ? "(无空间参考)" : info.SpatialReferenceName));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 生成动力性分析阻断说明。
        /// </summary>
        public string ToBlockMessage()
        {
            if (Success && IsUnified)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("空间参考不统一，已取消动力性分析。");
            sb.AppendLine("请将本次分析用到的图层投影到同一坐标系后再执行（未参与分析的图层可暂不处理）。");
            sb.AppendLine("可在「数据管理 → 数据完整性检查」中查看全库详情。");
            if (!string.IsNullOrEmpty(ReferenceSpatialReferenceName))
            {
                sb.AppendLine("建议基准: " + ReferenceSpatialReferenceName
                    + (string.IsNullOrEmpty(ReferenceLayerName) ? string.Empty : "（" + ReferenceLayerName + "）"));
            }
            if (MismatchedLayers != null)
            {
                for (int i = 0; i < MismatchedLayers.Count; i++)
                {
                    SpatialReferenceLayerInfo info = MismatchedLayers[i];
                    sb.AppendLine("- " + info.LayerName + " → "
                        + (string.IsNullOrEmpty(info.SpatialReferenceName) ? "(无空间参考)" : info.SpatialReferenceName));
                }
            }
            if (!Success && !string.IsNullOrEmpty(ErrorMessage))
            {
                sb.AppendLine(ErrorMessage);
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// 扫描 File GDB 要素类空间参考一致性。
    /// </summary>
    public static class SpatialReferenceAudit
    {
        /// <summary>
        /// 审计 GDB 内全部要素类。基准优先：中心城区/分析范围 → 首个投影坐标系 → 首个可读坐标系。
        /// </summary>
        public static SpatialReferenceAuditResult Audit(string gdbPath)
        {
            return Audit(gdbPath, null);
        }

        /// <summary>
        /// 审计指定要素类；onlyLayerNames 为空则审计全部。
        /// 动力性分析应只传入本次将用到的图层，避免未用图层（如宗地）阻断运行。
        /// </summary>
        public static SpatialReferenceAuditResult Audit(string gdbPath, IList<string> onlyLayerNames)
        {
            SpatialReferenceAuditResult result = new SpatialReferenceAuditResult();
            if (string.IsNullOrEmpty(gdbPath) || !System.IO.Directory.Exists(gdbPath))
            {
                result.ErrorMessage = "GDB 路径无效。";
                return result;
            }

            try
            {
                List<string> allNames = WorkspaceCatalog.ListFeatureClassNames(gdbPath);
                List<string> names = FilterLayerNames(allNames, onlyLayerNames);
                IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
                IWorkspace workspace = factory.OpenFromFile(gdbPath, 0);
                IFeatureWorkspace fws = (IFeatureWorkspace)workspace;

                ISpatialReference referenceSr = null;
                string referenceLayer = null;

                // 1) 优先分析范围
                string preferred = WorkspaceCatalog.FindByKeywords(names, "中心城区", "分析范围");
                if (!string.IsNullOrEmpty(preferred))
                {
                    TryOpenSr(fws, preferred, out referenceSr);
                    if (referenceSr != null)
                    {
                        referenceLayer = preferred;
                    }
                }

                // 2) 首个投影坐标系
                if (referenceSr == null)
                {
                    for (int i = 0; i < names.Count; i++)
                    {
                        ISpatialReference sr;
                        if (TryOpenSr(fws, names[i], out sr) && sr is IProjectedCoordinateSystem)
                        {
                            referenceSr = sr;
                            referenceLayer = names[i];
                            break;
                        }
                    }
                }

                // 3) 任意可读
                if (referenceSr == null)
                {
                    for (int i = 0; i < names.Count; i++)
                    {
                        ISpatialReference sr;
                        if (TryOpenSr(fws, names[i], out sr) && sr != null)
                        {
                            referenceSr = sr;
                            referenceLayer = names[i];
                            break;
                        }
                    }
                }

                result.ReferenceLayerName = referenceLayer;
                if (referenceSr != null)
                {
                    result.ReferenceSpatialReferenceName = referenceSr.Name;
                    result.ReferenceIsProjected = referenceSr is IProjectedCoordinateSystem;
                }

                for (int i = 0; i < names.Count; i++)
                {
                    SpatialReferenceLayerInfo info = new SpatialReferenceLayerInfo();
                    info.LayerName = names[i];
                    ISpatialReference sr;
                    if (!TryOpenSr(fws, names[i], out sr) || sr == null)
                    {
                        info.SpatialReferenceName = null;
                        info.MatchesReference = false;
                        info.IsProjected = false;
                    }
                    else
                    {
                        info.SpatialReferenceName = sr.Name;
                        try { info.FactoryCode = sr.FactoryCode; }
                        catch { info.FactoryCode = 0; }
                        info.IsProjected = sr is IProjectedCoordinateSystem;
                        info.MatchesReference = referenceSr != null
                            && FeatureProjectionHelper.IsSameSpatialReference(sr, referenceSr);
                    }

                    result.Layers.Add(info);
                    if (!info.MatchesReference)
                    {
                        result.MismatchedLayers.Add(info);
                    }
                }

                result.Success = true;
                result.IsUnified = result.MismatchedLayers.Count == 0 && result.Layers.Count > 0;
                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// 从动力性作业 LayerHints 收集待校验图层名（含路网边要素）。
        /// </summary>
        public static List<string> CollectMotivationLayerNames(
            IDictionary<string, string> layerHints,
            IList<string> gdbFeatureClassNames)
        {
            List<string> list = new List<string>();
            if (layerHints == null)
            {
                return list;
            }

            foreach (KeyValuePair<string, string> kv in layerHints)
            {
                if (string.IsNullOrEmpty(kv.Key) || string.IsNullOrEmpty(kv.Value))
                {
                    continue;
                }
                if (string.Equals(kv.Key, "RoadFeatureDataset", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(kv.Key, "RoadNetwork", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(kv.Key, "RoadImpedance", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                AddIfPresent(list, gdbFeatureClassNames, kv.Value);
            }

            // 预建路网边要素（通常名为 road）
            if (layerHints.ContainsKey("RoadFeatureDataset")
                || layerHints.ContainsKey("RoadNetwork"))
            {
                AddIfPresent(list, gdbFeatureClassNames, "road");
            }

            return list;
        }

        private static List<string> FilterLayerNames(List<string> allNames, IList<string> onlyLayerNames)
        {
            if (onlyLayerNames == null || onlyLayerNames.Count == 0)
            {
                return allNames;
            }

            List<string> filtered = new List<string>();
            for (int i = 0; i < onlyLayerNames.Count; i++)
            {
                string want = onlyLayerNames[i];
                if (string.IsNullOrEmpty(want))
                {
                    continue;
                }
                for (int j = 0; j < allNames.Count; j++)
                {
                    if (string.Equals(allNames[j], want, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!ContainsIgnoreCase(filtered, allNames[j]))
                        {
                            filtered.Add(allNames[j]);
                        }
                        break;
                    }
                }
            }
            return filtered;
        }

        private static void AddIfPresent(List<string> target, IList<string> gdbNames, string name)
        {
            if (string.IsNullOrEmpty(name) || gdbNames == null)
            {
                return;
            }
            for (int i = 0; i < gdbNames.Count; i++)
            {
                if (string.Equals(gdbNames[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    if (!ContainsIgnoreCase(target, gdbNames[i]))
                    {
                        target.Add(gdbNames[i]);
                    }
                    return;
                }
            }
        }

        private static bool ContainsIgnoreCase(List<string> list, string name)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (string.Equals(list[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool TryOpenSr(IFeatureWorkspace fws, string name, out ISpatialReference sr)
        {
            sr = null;
            try
            {
                IFeatureClass fc = fws.OpenFeatureClass(name);
                IGeoDataset geo = fc as IGeoDataset;
                if (geo != null)
                {
                    sr = geo.SpatialReference;
                    return sr != null;
                }
            }
            catch
            {
            }
            return false;
        }
    }
}

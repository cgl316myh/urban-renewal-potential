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
                    + " 个图层与基准不一致（动力性分析将拒绝执行，请先统一投影）");
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
            sb.AppendLine("请先在「数据管理 → 数据完整性检查」中查看详情，并将全部相关图层投影到同一坐标系后再执行。");
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
            SpatialReferenceAuditResult result = new SpatialReferenceAuditResult();
            if (string.IsNullOrEmpty(gdbPath) || !System.IO.Directory.Exists(gdbPath))
            {
                result.ErrorMessage = "GDB 路径无效。";
                return result;
            }

            try
            {
                List<string> names = WorkspaceCatalog.ListFeatureClassNames(gdbPath);
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

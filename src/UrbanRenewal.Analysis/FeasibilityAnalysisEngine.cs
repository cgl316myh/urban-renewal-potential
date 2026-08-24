using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Geometry;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Analysis
{
    /// <summary>可行度：宗地/DEM/人口加性合成，标准化 0–100。</summary>
    public class FeasibilityAnalysisEngine
    {
        private GeoprocessorHelper _gp;
        private FeasibilityJob _job;
        private Action<string, int> _progress;
        private int _progressPercent;

        public FeasibilityResult Run(FeasibilityJob job, Action<string, int> progress)
        {
            _progress = progress;
            _progressPercent = 0;
            FeasibilityResult result = new FeasibilityResult();
            if (job == null || string.IsNullOrEmpty(job.GdbPath))
            {
                Note(result, "作业参数无效：缺少 GDB 路径。");
                return result;
            }

            if (string.IsNullOrEmpty(job.OutputGdbPath)
                && !string.IsNullOrEmpty(job.WorkDirectory)
                && OutputGdbHelper.IsFileGdbPath(job.WorkDirectory))
            {
                job.OutputGdbPath = job.WorkDirectory;
            }
            if (string.IsNullOrEmpty(job.OutputGdbPath))
            {
                Note(result, "作业参数无效：请指定输出 File GDB（*.gdb）。");
                return result;
            }
            if (!OutputGdbHelper.IsFileGdbPath(job.OutputGdbPath))
            {
                Note(result, "输出路径必须是 File GDB（以 .gdb 结尾的文件夹）: " + job.OutputGdbPath);
                return result;
            }

            _job = job;
            BufferScoreRasterBuilder.ResetNameSequence();
            FeasibilityRasterBuilder.ResetNameSequence();

            Report(progress, result, "枚举 GDB 图层...", 5);
            List<string> featureNames = WorkspaceCatalog.ListFeatureClassNames(job.GdbPath);
            List<string> rasterNames = WorkspaceCatalog.ListRasterDatasetNames(job.GdbPath);
            Note(result, "GDB 要素类数量: " + featureNames.Count + "；栅格数量: " + rasterNames.Count);

            Report(progress, result, "检查空间参考一致性...", 8);
            List<string> usedLayers = SpatialReferenceAudit.CollectFeasibilityLayerNames(job.LayerHints, featureNames);
            SpatialReferenceAuditResult srAudit = usedLayers.Count > 0
                ? SpatialReferenceAudit.Audit(job.GdbPath, usedLayers)
                : SpatialReferenceAudit.Audit(job.GdbPath);
            if (!srAudit.Success || !srAudit.IsUnified)
            {
                string block = srAudit.ToBlockMessage();
                Note(result, block);
                result.Success = false;
                Report(progress, result, "空间参考不统一，已取消", 100);
                return result;
            }
            Note(result, "空间参考一致: " + srAudit.ReferenceSpatialReferenceName
                + "（校验 " + srAudit.Layers.Count + " 个分析图层）");

            _gp = new GeoprocessorHelper();
            _gp.BindToProgress(_progress, delegate { return _progressPercent; });
            Report(progress, result, "准备输出 GDB...", 10);
            string outGdb = OutputGdbHelper.EnsureExists(_gp, job.OutputGdbPath);
            job.OutputGdbPath = outGdb;
            job.WorkDirectory = outGdb;
            result.OutputGdbPath = outGdb;
            Note(result, "输出 GDB: " + outGdb);

            string studyLayer = ResolveFeature(job, featureNames, "StudyArea", "中心城区", "分析范围", "建成区");
            string extentPath = null;
            ISpatialReference targetSr = null;
            if (!string.IsNullOrEmpty(studyLayer))
            {
                extentPath = WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, studyLayer);
                targetSr = FeatureProjectionHelper.GetSpatialReference(extentPath);
                Note(result, "分析范围: " + studyLayer
                    + (targetSr != null ? " [" + targetSr.Name + "]" : string.Empty));
            }

            _gp.ConfigureAnalysis(outGdb, null, job.CellSize, targetSr);
            if (!string.IsNullOrEmpty(extentPath))
            {
                _gp.SetExtent(extentPath);
            }

            List<string> parts = new List<string>();
            IList<string> live = LiveMsgs(result);

            // 轨道 A · 宗地
            string parcel = ResolveFeature(job, featureNames, "Parcel",
                "宗地", "地块", "土地利用", "LandParcel", "parcel");
            if (!string.IsNullOrEmpty(parcel))
            {
                string parcelPath = WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, parcel);
                Note(result, "宗地斑块: " + parcel);
                try
                {
                    Report(progress, result, "宗地·斑块破碎度 PD...", 22);
                    string pdRaster = ParcelAnalyzer.BuildFragmentationScoreRaster(
                        _gp, parcelPath, outGdb, "pd", job.CellSize, live);
                    parts.Add(pdRaster);
                    result.FactorRasters["斑块破碎度PD"] = pdRaster;

                    Report(progress, result, "宗地·形状指数 SI...", 35);
                    string siRaster = ParcelAnalyzer.BuildSiScoreRaster(
                        _gp, parcelPath, outGdb, "si", job.CellSize, live);
                    parts.Add(siRaster);
                    result.FactorRasters["形状指数SI"] = siRaster;
                }
                catch (Exception ex)
                {
                    Note(result, "宗地因子失败: " + ex.Message);
                }
            }
            else
            {
                Note(result, "宗地准则：未匹配到土地利用/宗地斑块，已跳过。");
            }

            // 轨道 B · 地形
            string dem = ResolveRaster(job, rasterNames, "DEM", "DEM", "dem", "高程", "Elevation");
            if (!string.IsNullOrEmpty(dem))
            {
                string demPath = WorkspaceCatalog.ToRasterPath(job.GdbPath, dem);
                Note(result, "DEM: " + dem);
                try
                {
                    Report(progress, result, "地形·高程阈值重分类...", 48);
                    double elevThr = job.ElevationThreshold > 0 ? job.ElevationThreshold : 50;
                    string elevScore = FeasibilityRasterBuilder.ReclassifyAboveThreshold(
                        _gp, demPath, elevThr, -1, 0, outGdb, "elev");
                    parts.Add(elevScore);
                    result.FactorRasters["高程限制"] = elevScore;
                    Note(result, "高程阈值: >" + elevThr + "m → -1");

                    string slopeSrc = ResolveRaster(job, rasterNames, "Slope", "坡度", "Slope", "slope");
                    string slopeRaster;
                    if (!string.IsNullOrEmpty(slopeSrc))
                    {
                        slopeRaster = WorkspaceCatalog.ToRasterPath(job.GdbPath, slopeSrc);
                        Note(result, "使用已有坡度栅格: " + slopeSrc);
                    }
                    else
                    {
                        Report(progress, result, "地形·由 DEM 生成坡度...", 55);
                        slopeRaster = FeasibilityRasterBuilder.BuildSlope(_gp, demPath, outGdb, "slp");
                        Note(result, "已由 DEM 生成坡度栅格。");
                    }

                    Report(progress, result, "地形·坡度阈值重分类...", 60);
                    double slopeThr = job.SlopeThresholdDegrees > 0 ? job.SlopeThresholdDegrees : 15;
                    string slopeScore = FeasibilityRasterBuilder.ReclassifyAboveThreshold(
                        _gp, slopeRaster, slopeThr, -1, 0, outGdb, "slps");
                    parts.Add(slopeScore);
                    result.FactorRasters["坡度限制"] = slopeScore;
                    Note(result, "坡度阈值: >" + slopeThr + "° → -1");
                }
                catch (Exception ex)
                {
                    Note(result, "地形因子失败: " + ex.Message);
                }
            }
            else
            {
                Note(result, "地形准则：未匹配到 DEM 栅格，已跳过。");
            }

            // 轨道 C · 人口
            string pop = ResolveRaster(job, rasterNames, "Population",
                "人口", "人口密度", "population", "pop", "PopDensity");
            if (!string.IsNullOrEmpty(pop))
            {
                string popPath = WorkspaceCatalog.ToRasterPath(job.GdbPath, pop);
                Note(result, "人口栅格: " + pop);
                try
                {
                    Report(progress, result, "人口·读取值域并重分类...", 70);
                    double minV, maxV;
                    if (!WorkspaceCatalog.TryGetRasterMinMax(popPath, out minV, out maxV))
                    {
                        minV = 0;
                        maxV = 0;
                        Note(result, "人口栅格统计读取失败，使用保守分级。");
                    }
                    else
                    {
                        Note(result, "人口值域: ["
                            + minV.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
                            + ", "
                            + maxV.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)
                            + "]");
                    }

                    string popScore = FeasibilityRasterBuilder.BuildPopulationScore(
                        _gp, popPath, outGdb, "pop", minV, maxV);
                    parts.Add(popScore);
                    result.FactorRasters["人口密度"] = popScore;
                }
                catch (Exception ex)
                {
                    Note(result, "人口因子失败: " + ex.Message);
                }
            }
            else
            {
                Note(result, "人口准则：未匹配到人口密度栅格，已跳过。");
            }

            if (parts.Count == 0)
            {
                Note(result, "未生成任何可行度因子栅格，请检查 GDB 是否包含宗地/DEM/人口数据。");
                Report(progress, result, "失败", 100);
                return result;
            }

            Report(progress, result, "可行度加性合成...", 85);
            string rawPath = OutputGdbHelper.DatasetPath(outGdb, "fea_raw");
            FeasibilityRasterBuilder.SumCombine(_gp, parts, rawPath);
            result.FeasibilityRawRasterPath = rawPath;
            Note(result, "可行度原始得分栅格: " + rawPath
                + "（理论值域约 "
                + FeasibilityScoreScale.TheoreticalMin + "～"
                + FeasibilityScoreScale.TheoreticalMax + "）");

            Report(progress, result, "标准化到 0–100...", 92);
            string normPath = FeasibilityRasterBuilder.NormalizeRangeTo100(
                _gp,
                rawPath,
                FeasibilityScoreScale.TheoreticalMin,
                FeasibilityScoreScale.TheoreticalMax,
                outGdb,
                "feas");
            string finalPath = OutputGdbHelper.DatasetPath(outGdb, "fea_score");
            try
            {
                result.FeasibilityRasterPath = FeasibilityRasterBuilder.SaveAs(_gp, normPath, finalPath);
            }
            catch
            {
                result.FeasibilityRasterPath = normPath;
            }

            if (ResultMaskHelper.IsMaskEnabled())
            {
                if (string.IsNullOrEmpty(extentPath))
                {
                    Note(result, "已启用成果掩膜，但未找到分析范围图层 StudyArea。");
                    Report(progress, result, "失败", 100);
                    return result;
                }
                Report(progress, result, "按中心城区掩膜可行度成果...", 96);
                result.FeasibilityRasterPath = ResultMaskHelper.MaskAndReplace(
                    _gp, result.FeasibilityRasterPath, extentPath, finalPath);
                Note(result, "已按全局设置掩膜可行度成果: " + result.FeasibilityRasterPath);
            }

            result.OutputGdbPath = outGdb;
            result.Success = true;
            Note(result, "可行度栅格已生成（0–100 标准化）: " + result.FeasibilityRasterPath);
            Report(progress, result, "完成", 100);
            return result;
        }

        private static string ResolveFeature(
            FeasibilityJob job,
            List<string> names,
            string hintKey,
            params string[] keywords)
        {
            string hinted = ResolveHint(job, hintKey);
            if (!string.IsNullOrEmpty(hinted))
            {
                // 配置名可能不在 GDB 列表中
                for (int i = 0; i < names.Count; i++)
                {
                    if (string.Equals(names[i], hinted, StringComparison.OrdinalIgnoreCase))
                    {
                        return names[i];
                    }
                }
                for (int i = 0; i < names.Count; i++)
                {
                    if (names[i].IndexOf(hinted, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return names[i];
                    }
                }
                return hinted;
            }
            return WorkspaceCatalog.FindByKeywords(names, keywords);
        }

        private static string ResolveRaster(
            FeasibilityJob job,
            List<string> names,
            string hintKey,
            params string[] keywords)
        {
            return ResolveFeature(job, names, hintKey, keywords);
        }

        private static string ResolveHint(FeasibilityJob job, string hintKey)
        {
            if (job == null || job.LayerHints == null || string.IsNullOrEmpty(hintKey))
            {
                return null;
            }
            if (!job.LayerHints.ContainsKey(hintKey))
            {
                return null;
            }
            string v = job.LayerHints[hintKey];
            return string.IsNullOrEmpty(v) ? null : v;
        }

        private void Note(FeasibilityResult result, string text)
        {
            if (result != null)
            {
                result.Messages.Add(text);
            }
            if (_progress != null)
            {
                _progress(text, _progressPercent);
            }
        }

        private IList<string> LiveMsgs(FeasibilityResult result)
        {
            return new LiveMessageList(result.Messages, delegate(string t)
            {
                if (_progress != null)
                {
                    _progress(t, _progressPercent);
                }
            });
        }

        private void Report(Action<string, int> progress, FeasibilityResult result, string text, int percent)
        {
            _progressPercent = percent;
            Note(result, text);
        }
    }
}

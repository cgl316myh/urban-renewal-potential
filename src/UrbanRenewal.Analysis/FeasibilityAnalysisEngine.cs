using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Geometry;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Analysis
{
    /// <summary>
    /// 可行度分析引擎：宗地 SI/PD + DEM 高程/坡度 + 人口，加性合成后标准化到 0–100。
    /// </summary>
    public class FeasibilityAnalysisEngine
    {
        private GeoprocessorHelper _gp;
        private FeasibilityJob _job;

        public FeasibilityResult Run(FeasibilityJob job, Action<string, int> progress)
        {
            FeasibilityResult result = new FeasibilityResult();
            if (job == null || string.IsNullOrEmpty(job.GdbPath))
            {
                result.Messages.Add("作业参数无效：缺少 GDB 路径。");
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
                result.Messages.Add("作业参数无效：请指定输出 File GDB（*.gdb）。");
                return result;
            }
            if (!OutputGdbHelper.IsFileGdbPath(job.OutputGdbPath))
            {
                result.Messages.Add("输出路径必须是 File GDB（以 .gdb 结尾的文件夹）: " + job.OutputGdbPath);
                return result;
            }

            _job = job;
            BufferScoreRasterBuilder.ResetNameSequence();
            FeasibilityRasterBuilder.ResetNameSequence();

            Report(progress, result, "枚举 GDB 图层...", 5);
            List<string> featureNames = WorkspaceCatalog.ListFeatureClassNames(job.GdbPath);
            List<string> rasterNames = WorkspaceCatalog.ListRasterDatasetNames(job.GdbPath);
            result.Messages.Add("GDB 要素类数量: " + featureNames.Count + "；栅格数量: " + rasterNames.Count);

            Report(progress, result, "检查空间参考一致性...", 8);
            List<string> usedLayers = SpatialReferenceAudit.CollectFeasibilityLayerNames(job.LayerHints, featureNames);
            SpatialReferenceAuditResult srAudit = usedLayers.Count > 0
                ? SpatialReferenceAudit.Audit(job.GdbPath, usedLayers)
                : SpatialReferenceAudit.Audit(job.GdbPath);
            if (!srAudit.Success || !srAudit.IsUnified)
            {
                string block = srAudit.ToBlockMessage();
                result.Messages.Add(block);
                result.Success = false;
                Report(progress, result, "空间参考不统一，已取消", 100);
                return result;
            }
            result.Messages.Add("空间参考一致: " + srAudit.ReferenceSpatialReferenceName
                + "（校验 " + srAudit.Layers.Count + " 个分析图层）");

            _gp = new GeoprocessorHelper();
            Report(progress, result, "准备输出 GDB...", 10);
            string outGdb = OutputGdbHelper.EnsureExists(_gp, job.OutputGdbPath);
            job.OutputGdbPath = outGdb;
            job.WorkDirectory = outGdb;
            result.OutputGdbPath = outGdb;
            result.Messages.Add("输出 GDB: " + outGdb);

            string studyLayer = ResolveFeature(job, featureNames, "StudyArea", "中心城区", "分析范围", "建成区");
            string extentPath = null;
            ISpatialReference targetSr = null;
            if (!string.IsNullOrEmpty(studyLayer))
            {
                extentPath = WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, studyLayer);
                targetSr = FeatureProjectionHelper.GetSpatialReference(extentPath);
                result.Messages.Add("分析范围: " + studyLayer
                    + (targetSr != null ? " [" + targetSr.Name + "]" : string.Empty));
            }

            _gp.ConfigureAnalysis(outGdb, null, job.CellSize, targetSr);
            if (!string.IsNullOrEmpty(extentPath))
            {
                _gp.SetExtent(extentPath);
            }

            List<string> parts = new List<string>();

            // 轨道 A · 宗地
            Report(progress, result, "宗地破碎度 / 形状指数...", 25);
            string parcel = ResolveFeature(job, featureNames, "Parcel",
                "宗地", "地块", "土地利用", "LandParcel", "parcel");
            if (!string.IsNullOrEmpty(parcel))
            {
                string parcelPath = WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, parcel);
                result.Messages.Add("宗地斑块: " + parcel);
                try
                {
                    string pdRaster = ParcelAnalyzer.BuildFragmentationScoreRaster(
                        _gp, parcelPath, outGdb, "pd", job.CellSize, result.Messages);
                    parts.Add(pdRaster);
                    result.FactorRasters["斑块破碎度PD"] = pdRaster;

                    string siRaster = ParcelAnalyzer.BuildSiScoreRaster(
                        _gp, parcelPath, outGdb, "si", job.CellSize, result.Messages);
                    parts.Add(siRaster);
                    result.FactorRasters["形状指数SI"] = siRaster;
                }
                catch (Exception ex)
                {
                    result.Messages.Add("宗地因子失败: " + ex.Message);
                }
            }
            else
            {
                result.Messages.Add("宗地准则：未匹配到土地利用/宗地斑块，已跳过。");
            }

            // 轨道 B · 地形
            Report(progress, result, "DEM 高程 / 坡度重分类...", 50);
            string dem = ResolveRaster(job, rasterNames, "DEM", "DEM", "dem", "高程", "Elevation");
            if (!string.IsNullOrEmpty(dem))
            {
                string demPath = WorkspaceCatalog.ToRasterPath(job.GdbPath, dem);
                result.Messages.Add("DEM: " + dem);
                try
                {
                    double elevThr = job.ElevationThreshold > 0 ? job.ElevationThreshold : 50;
                    string elevScore = FeasibilityRasterBuilder.ReclassifyAboveThreshold(
                        _gp, demPath, elevThr, -1, 0, outGdb, "elev");
                    parts.Add(elevScore);
                    result.FactorRasters["高程限制"] = elevScore;
                    result.Messages.Add("高程阈值: >" + elevThr + "m → -1");

                    string slopeSrc = ResolveRaster(job, rasterNames, "Slope", "坡度", "Slope", "slope");
                    string slopeRaster;
                    if (!string.IsNullOrEmpty(slopeSrc))
                    {
                        slopeRaster = WorkspaceCatalog.ToRasterPath(job.GdbPath, slopeSrc);
                        result.Messages.Add("使用已有坡度栅格: " + slopeSrc);
                    }
                    else
                    {
                        slopeRaster = FeasibilityRasterBuilder.BuildSlope(_gp, demPath, outGdb, "slp");
                        result.Messages.Add("已由 DEM 生成坡度栅格。");
                    }

                    double slopeThr = job.SlopeThresholdDegrees > 0 ? job.SlopeThresholdDegrees : 15;
                    string slopeScore = FeasibilityRasterBuilder.ReclassifyAboveThreshold(
                        _gp, slopeRaster, slopeThr, -1, 0, outGdb, "slps");
                    parts.Add(slopeScore);
                    result.FactorRasters["坡度限制"] = slopeScore;
                    result.Messages.Add("坡度阈值: >" + slopeThr + "° → -1");
                }
                catch (Exception ex)
                {
                    result.Messages.Add("地形因子失败: " + ex.Message);
                }
            }
            else
            {
                result.Messages.Add("地形准则：未匹配到 DEM 栅格，已跳过。");
            }

            // 轨道 C · 人口
            Report(progress, result, "人口密度重分类...", 70);
            string pop = ResolveRaster(job, rasterNames, "Population",
                "人口", "人口密度", "population", "pop", "PopDensity");
            if (!string.IsNullOrEmpty(pop))
            {
                string popPath = WorkspaceCatalog.ToRasterPath(job.GdbPath, pop);
                result.Messages.Add("人口栅格: " + pop);
                try
                {
                    double minV, maxV;
                    if (!WorkspaceCatalog.TryGetRasterMinMax(popPath, out minV, out maxV))
                    {
                        minV = 0;
                        maxV = 0;
                        result.Messages.Add("人口栅格统计读取失败，使用保守分级。");
                    }
                    else
                    {
                        result.Messages.Add("人口值域: ["
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
                    result.Messages.Add("人口因子失败: " + ex.Message);
                }
            }
            else
            {
                result.Messages.Add("人口准则：未匹配到人口密度栅格，已跳过。");
            }

            if (parts.Count == 0)
            {
                result.Messages.Add("未生成任何可行度因子栅格，请检查 GDB 是否包含宗地/DEM/人口数据。");
                Report(progress, result, "失败", 100);
                return result;
            }

            Report(progress, result, "可行度加性合成...", 85);
            string rawPath = OutputGdbHelper.DatasetPath(outGdb, "fea_raw");
            FeasibilityRasterBuilder.SumCombine(_gp, parts, rawPath);
            result.FeasibilityRawRasterPath = rawPath;
            result.Messages.Add("可行度原始得分栅格: " + rawPath
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

            result.OutputGdbPath = outGdb;
            result.Success = true;
            result.Messages.Add("可行度栅格已生成（0–100 标准化）: " + result.FeasibilityRasterPath);
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
                // 提示可能来自配置名，确认在列表中或直接使用
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

        private static void Report(Action<string, int> progress, FeasibilityResult result, string text, int percent)
        {
            result.Messages.Add(text);
            if (progress != null)
            {
                progress(text, percent);
            }
        }
    }
}

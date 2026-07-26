using System;
using System.Collections.Generic;
using System.IO;
using ESRI.ArcGIS.Geometry;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;
using IoPath = System.IO.Path;

namespace UrbanRenewal.Analysis
{
    /// <summary>
    /// 综合潜力叠置引擎：mot_score × W动力 + fea_score × W可行 → pot_score + 五级分类。
    /// </summary>
    public class OverlayAnalysisEngine
    {
        public OverlayResult Run(OverlayJob job, Action<string, int> progress)
        {
            OverlayResult result = new OverlayResult();
            if (job == null || string.IsNullOrEmpty(job.OutputGdbPath))
            {
                result.Messages.Add("作业参数无效：请指定输出 File GDB。");
                return result;
            }
            if (!OutputGdbHelper.IsFileGdbPath(job.OutputGdbPath))
            {
                result.Messages.Add("输出路径必须是 File GDB（*.gdb）: " + job.OutputGdbPath);
                return result;
            }

            PotentialOverlayBuilder.ResetNameSequence();
            Report(progress, result, "准备输出 GDB...", 5);

            GeoprocessorHelper gp = new GeoprocessorHelper();
            string outGdb = OutputGdbHelper.EnsureExists(gp, job.OutputGdbPath);
            job.OutputGdbPath = outGdb;
            result.OutputGdbPath = outGdb;

            string studyPath = null;
            ISpatialReference targetSr = null;
            if (!string.IsNullOrEmpty(job.GdbPath) && Directory.Exists(job.GdbPath))
            {
                List<string> names = WorkspaceCatalog.ListFeatureClassNames(job.GdbPath);
                string study = ResolveHintOrKeyword(job, names, "StudyArea", "中心城区", "分析范围", "建成区");
                if (!string.IsNullOrEmpty(study))
                {
                    studyPath = WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, study);
                    targetSr = FeatureProjectionHelper.GetSpatialReference(studyPath);
                    result.Messages.Add("分析范围: " + study);
                }
            }

            gp.ConfigureAnalysis(outGdb, null, job.CellSize, targetSr);
            if (!string.IsNullOrEmpty(studyPath))
            {
                gp.SetExtent(studyPath);
            }

            Report(progress, result, "解析动力性/可行度栅格...", 15);
            string motRaster = ResolveRasterPath(outGdb, job.GdbPath, job.MotivationRasterName, "mot_score");
            string feaRaster = ResolveRasterPath(outGdb, job.GdbPath, job.FeasibilityRasterName, "fea_score");
            if (string.IsNullOrEmpty(motRaster) || !RasterExists(motRaster))
            {
                result.Messages.Add("未找到动力性栅格 mot_score，请先运行动力性分析。");
                Report(progress, result, "失败", 100);
                return result;
            }
            if (string.IsNullOrEmpty(feaRaster) || !RasterExists(feaRaster))
            {
                result.Messages.Add("未找到可行度栅格 fea_score，请先运行可行度分析。");
                Report(progress, result, "失败", 100);
                return result;
            }
            result.Messages.Add("动力性栅格: " + motRaster);
            result.Messages.Add("可行度栅格: " + feaRaster);
            result.Messages.Add("权重: 动力=" + job.MotivationWeight.ToString("0.###")
                + "，可行=" + job.FeasibilityWeight.ToString("0.###"));

            Report(progress, result, "加权叠置综合潜力...", 45);
            string rawPath = OutputGdbHelper.DatasetPath(outGdb, "pot_raw");
            PotentialOverlayBuilder.WeightedOverlay(
                gp, motRaster, feaRaster, job.MotivationWeight, job.FeasibilityWeight, rawPath);

            string finalPath = OutputGdbHelper.DatasetPath(outGdb, "pot_score");
            try
            {
                result.PotentialRasterPath = PotentialOverlayBuilder.SaveAs(gp, rawPath, finalPath);
            }
            catch
            {
                result.PotentialRasterPath = rawPath;
            }
            result.Messages.Add("综合潜力栅格: " + result.PotentialRasterPath);

            Report(progress, result, "五级分类...", 70);
            string levelTmp = PotentialOverlayBuilder.ClassifyLevels(gp, result.PotentialRasterPath, outGdb, "lvl");
            string levelFinal = OutputGdbHelper.DatasetPath(outGdb, "pot_level");
            try
            {
                result.LevelRasterPath = PotentialOverlayBuilder.SaveAs(gp, levelTmp, levelFinal);
            }
            catch
            {
                result.LevelRasterPath = levelTmp;
            }
            result.Messages.Add("潜力等级栅格: " + result.LevelRasterPath + "（1偏低～5极高）");

            Report(progress, result, "统计各等级面积...", 88);
            result.LevelAreas = PotentialOverlayBuilder.ComputeLevelAreas(
                gp, result.LevelRasterPath, job.CellSize, result.Messages);
            for (int i = 0; i < result.LevelAreas.Count; i++)
            {
                LevelAreaStat st = result.LevelAreas[i];
                double ha = st.AreaSqMeters / 10000.0;
                result.Messages.Add("等级[" + st.LevelName + "] 像元=" + st.CellCount
                    + " 面积≈" + ha.ToString("0.###") + " ha 占比=" + st.Percent.ToString("0.##") + "%");
            }

            result.Success = true;
            Report(progress, result, "完成", 100);
            return result;
        }

        private static string ResolveRasterPath(string outGdb, string inputGdb, string nameOrPath, string defaultName)
        {
            if (!string.IsNullOrEmpty(nameOrPath))
            {
                if (nameOrPath.IndexOf(".gdb", StringComparison.OrdinalIgnoreCase) >= 0
                    || nameOrPath.IndexOf('\\') >= 0 || nameOrPath.IndexOf('/') >= 0)
                {
                    return nameOrPath;
                }
                string inOut = OutputGdbHelper.DatasetPath(outGdb, nameOrPath);
                if (RasterExists(inOut))
                {
                    return inOut;
                }
                if (!string.IsNullOrEmpty(inputGdb))
                {
                    string inIn = OutputGdbHelper.DatasetPath(inputGdb, nameOrPath);
                    if (RasterExists(inIn))
                    {
                        return inIn;
                    }
                }
            }
            string defOut = OutputGdbHelper.DatasetPath(outGdb, defaultName);
            if (RasterExists(defOut))
            {
                return defOut;
            }
            if (!string.IsNullOrEmpty(inputGdb))
            {
                string defIn = OutputGdbHelper.DatasetPath(inputGdb, defaultName);
                if (RasterExists(defIn))
                {
                    return defIn;
                }
            }
            return defOut;
        }

        private static bool RasterExists(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            string gdb = IoPath.GetDirectoryName(path);
            string name = IoPath.GetFileName(path);
            if (string.IsNullOrEmpty(gdb) || !Directory.Exists(gdb))
            {
                return false;
            }
            try
            {
                List<string> rasters = WorkspaceCatalog.ListRasterDatasetNames(gdb);
                for (int i = 0; i < rasters.Count; i++)
                {
                    if (string.Equals(rasters[i], name, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        private static string ResolveHintOrKeyword(
            OverlayJob job,
            List<string> names,
            string hintKey,
            params string[] keywords)
        {
            if (job != null && job.LayerHints != null && job.LayerHints.ContainsKey(hintKey))
            {
                string hinted = job.LayerHints[hintKey];
                if (!string.IsNullOrEmpty(hinted))
                {
                    return hinted;
                }
            }
            return WorkspaceCatalog.FindByKeywords(names, keywords);
        }

        private static void Report(Action<string, int> progress, OverlayResult result, string text, int percent)
        {
            result.Messages.Add(text);
            if (progress != null)
            {
                progress(text, percent);
            }
        }
    }
}

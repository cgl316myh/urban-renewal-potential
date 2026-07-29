using System;
using System.Collections.Generic;
using System.IO;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Analysis
{
    /// <summary>
    /// 验证校核引擎。
    /// </summary>
    public class ValidationAnalysisEngine
    {
        private Action<string, int> _progress;
        private int _progressPercent;

        public ValidationResult Run(ValidationJob job, Action<string, int> progress)
        {
            _progress = progress;
            _progressPercent = 0;
            ValidationResult result = new ValidationResult();
            if (job == null || string.IsNullOrEmpty(job.GdbPath) || string.IsNullOrEmpty(job.OutputGdbPath))
            {
                Note(result, "作业参数无效：需要输入 GDB 与输出 File GDB。");
                return result;
            }

            Report(progress, result, "准备...", 5);
            GeoprocessorHelper gp = new GeoprocessorHelper();
            gp.BindToProgress(_progress, delegate { return _progressPercent; });
            string outGdb = OutputGdbHelper.EnsureExists(gp, job.OutputGdbPath);
            job.OutputGdbPath = outGdb;
            result.OutputGdbPath = outGdb;
            gp.ConfigureAnalysis(outGdb, null, 0, null);

            Report(progress, result, "解析已更新宗地...", 20);
            List<string> names = WorkspaceCatalog.ListFeatureClassNames(job.GdbPath);
            string updated = ResolveFeature(job, names, "UpdatedParcel",
                "已更新", "已改造", "更新宗地", "更新地块", "UpdatedParcel");
            if (string.IsNullOrEmpty(updated))
            {
                Note(result, "未匹配到「已更新宗地」图层。请在城市配置中设置 UpdatedParcel，或确保图层名含「已更新」。");
                Report(progress, result, "失败", 100);
                return result;
            }
            string updatedPath = WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, updated);
            Note(result, "已更新宗地: " + updated);

            Report(progress, result, "解析评价宗地...", 35);
            string scoredName = string.IsNullOrEmpty(job.ScoredParcelName) ? "parcel_pot" : job.ScoredParcelName;
            string scoredPath = OutputGdbHelper.DatasetPath(outGdb, scoredName);
            if (!FeatureExists(outGdb, scoredName))
            {
                // 回退：输入库中带潜力字段的宗地
                string parcel = ResolveFeature(job, names, "Parcel", "宗地", "地块", "土地利用");
                if (!string.IsNullOrEmpty(parcel))
                {
                    scoredPath = WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, parcel);
                    Note(result, "输出库无 parcel_pot，改用输入宗地: " + parcel);
                }
                else
                {
                    Note(result, "未找到评价宗地 parcel_pot，请先运行宗地关联。");
                    Report(progress, result, "失败", 100);
                    return result;
                }
            }
            else
            {
                Note(result, "评价宗地: " + scoredPath);
            }

            Report(progress, result, "对标统计与报告...", 55);
            ValidationResult core = ValidationAnalyzer.Run(
                gp,
                updatedPath,
                scoredPath,
                outGdb,
                job.DiffFeatureClassName,
                job.HighLevelThreshold > 0 ? job.HighLevelThreshold : 60,
                job.PassHighRatio > 0 ? job.PassHighRatio : 0.6,
                job.ReviewComment,
                LiveMsgs(result));

            result.Success = core.Success;
            result.Passed = core.Passed;
            result.ReportPath = core.ReportPath;
            result.DiffFeatureClassPath = core.DiffFeatureClassPath;
            result.UpdatedCount = core.UpdatedCount;
            result.HighCount = core.HighCount;
            result.HighRatio = core.HighRatio;
            result.LevelCounts = core.LevelCounts;
            Report(progress, result, "完成", 100);
            return result;
        }

        private static string ResolveFeature(
            ValidationJob job,
            List<string> names,
            string hintKey,
            params string[] keywords)
        {
            if (job != null && job.LayerHints != null && job.LayerHints.ContainsKey(hintKey))
            {
                string hinted = job.LayerHints[hintKey];
                if (!string.IsNullOrEmpty(hinted))
                {
                    for (int i = 0; i < names.Count; i++)
                    {
                        if (string.Equals(names[i], hinted, StringComparison.OrdinalIgnoreCase)
                            || names[i].IndexOf(hinted, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            return names[i];
                        }
                    }
                    return hinted;
                }
            }
            return WorkspaceCatalog.FindByKeywords(names, keywords);
        }

        private static bool FeatureExists(string gdb, string name)
        {
            if (string.IsNullOrEmpty(gdb) || !Directory.Exists(gdb))
            {
                return false;
            }
            try
            {
                List<string> list = WorkspaceCatalog.ListFeatureClassNames(gdb);
                for (int i = 0; i < list.Count; i++)
                {
                    if (string.Equals(list[i], name, StringComparison.OrdinalIgnoreCase))
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

        private void Note(ValidationResult result, string text)
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

        private IList<string> LiveMsgs(ValidationResult result)
        {
            return new LiveMessageList(result.Messages, delegate(string t)
            {
                if (_progress != null)
                {
                    _progress(t, _progressPercent);
                }
            });
        }

        private void Report(Action<string, int> progress, ValidationResult result, string text, int percent)
        {
            _progressPercent = percent;
            Note(result, text);
        }
    }
}

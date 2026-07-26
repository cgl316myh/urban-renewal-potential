using System;
using System.Collections.Generic;
using System.IO;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Analysis
{
    /// <summary>
    /// 宗地关联引擎：Zonal Statistics → 宗地字段。
    /// </summary>
    public class ParcelLinkEngine
    {
        private Action<string, int> _progress;
        private int _progressPercent;

        public ParcelLinkResult Run(ParcelLinkJob job, Action<string, int> progress)
        {
            _progress = progress;
            _progressPercent = 0;
            ParcelLinkResult result = new ParcelLinkResult();
            if (job == null || string.IsNullOrEmpty(job.GdbPath) || string.IsNullOrEmpty(job.OutputGdbPath))
            {
                Note(result, "作业参数无效：需要输入 GDB 与输出 File GDB。");
                return result;
            }
            if (!Directory.Exists(job.GdbPath))
            {
                Note(result, "输入 GDB 不存在: " + job.GdbPath);
                return result;
            }
            if (!OutputGdbHelper.IsFileGdbPath(job.OutputGdbPath))
            {
                Note(result, "输出路径必须是 File GDB（*.gdb）: " + job.OutputGdbPath);
                return result;
            }

            Report(progress, result, "准备输出 GDB...", 5);
            GeoprocessorHelper gp = new GeoprocessorHelper();
            string outGdb = OutputGdbHelper.EnsureExists(gp, job.OutputGdbPath);
            job.OutputGdbPath = outGdb;
            result.OutputGdbPath = outGdb;
            gp.ConfigureAnalysis(outGdb, null, 0, null);

            Report(progress, result, "解析宗地图层...", 15);
            List<string> names = WorkspaceCatalog.ListFeatureClassNames(job.GdbPath);
            string parcel = ResolveFeature(job, names, "Parcel", "宗地", "地块", "土地利用", "LandParcel", "parcel");
            if (string.IsNullOrEmpty(parcel))
            {
                Note(result, "未匹配到宗地/土地利用斑块图层。");
                Report(progress, result, "失败", 100);
                return result;
            }
            string parcelPath = WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, parcel);
            Note(result, "宗地图层: " + parcel);

            Report(progress, result, "解析评价栅格...", 30);
            string pot = ResolveRaster(outGdb, job.GdbPath, job.PotentialRasterName, "pot_score");
            string mot = ResolveRaster(outGdb, job.GdbPath, job.MotivationRasterName, "mot_score");
            string fea = ResolveRaster(outGdb, job.GdbPath, job.FeasibilityRasterName, "fea_score");
            if (string.IsNullOrEmpty(pot) || !RasterExists(pot))
            {
                Note(result, "未找到综合潜力栅格 pot_score，请先运行叠置评价。");
                Report(progress, result, "失败", 100);
                return result;
            }
            Note(result, "潜力栅格: " + pot);
            if (!string.IsNullOrEmpty(mot) && RasterExists(mot))
            {
                Note(result, "动力栅格: " + mot);
            }
            else
            {
                mot = null;
                Note(result, "动力栅格缺失，MOTIV_SCORE 将写 0。");
            }
            if (!string.IsNullOrEmpty(fea) && RasterExists(fea))
            {
                Note(result, "可行栅格: " + fea);
            }
            else
            {
                fea = null;
                Note(result, "可行栅格缺失，FEASIB_SCORE 将写 0。");
            }

            Report(progress, result, "区统计并写入宗地字段...", 55);
            int count;
            string outFc = ParcelZonalLinker.Link(
                gp,
                parcelPath,
                outGdb,
                job.OutputFeatureClassName,
                pot,
                mot,
                fea,
                job.StatisticType,
                result.Messages,
                out count);

            result.ParcelFeatureClassPath = outFc;
            result.ParcelCount = count;
            result.Success = true;
            Note(result, "宗地关联完成: " + outFc);
            Report(progress, result, "完成", 100);
            return result;
        }

        private static string ResolveFeature(
            ParcelLinkJob job,
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

        private static string ResolveRaster(string outGdb, string inputGdb, string nameOrPath, string defaultName)
        {
            if (!string.IsNullOrEmpty(nameOrPath)
                && (nameOrPath.IndexOf(".gdb", StringComparison.OrdinalIgnoreCase) >= 0
                    || nameOrPath.IndexOf('\\') >= 0 || nameOrPath.IndexOf('/') >= 0))
            {
                return nameOrPath;
            }
            string n = string.IsNullOrEmpty(nameOrPath) ? defaultName : nameOrPath;
            string p1 = OutputGdbHelper.DatasetPath(outGdb, n);
            if (RasterExists(p1))
            {
                return p1;
            }
            if (!string.IsNullOrEmpty(inputGdb))
            {
                string p2 = OutputGdbHelper.DatasetPath(inputGdb, n);
                if (RasterExists(p2))
                {
                    return p2;
                }
            }
            return p1;
        }

        private static bool RasterExists(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            string gdb = Path.GetDirectoryName(path);
            string name = Path.GetFileName(path);
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

        private void Note(ParcelLinkResult result, string text)
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

        private void Report(Action<string, int> progress, ParcelLinkResult result, string text, int percent)
        {
            _progressPercent = percent;
            Note(result, text);
        }
    }
}

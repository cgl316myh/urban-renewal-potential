using System;
using System.Collections.Generic;
using System.IO;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Analysis
{
    /// <summary>
    /// 动力性分析引擎：四准则层缓冲赋分 + 加权叠置。
    /// </summary>
    public class MotivationAnalysisEngine
    {
        public MotivationResult Run(MotivationJob job, Action<string, int> progress)
        {
            MotivationResult result = new MotivationResult();
            if (job == null || string.IsNullOrEmpty(job.GdbPath))
            {
                result.Messages.Add("作业参数无效：缺少 GDB 路径。");
                return result;
            }

            if (string.IsNullOrEmpty(job.WorkDirectory))
            {
                job.WorkDirectory = Path.Combine(Path.GetTempPath(), "UrbanRenewal_Motivation_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            }
            Directory.CreateDirectory(job.WorkDirectory);

            Report(progress, result, "枚举 GDB 图层...", 5);
            List<string> names = WorkspaceCatalog.ListFeatureClassNames(job.GdbPath);
            result.Messages.Add("GDB 要素类数量: " + names.Count);

            GeoprocessorHelper gp = new GeoprocessorHelper();
            List<string> criterionRasters = new List<string>();
            List<double> weights = new List<double>();

            // 交通 30%
            Report(progress, result, "交通便捷度分析...", 20);
            string traffic = BuildTraffic(gp, job, names, result);
            if (!string.IsNullOrEmpty(traffic))
            {
                criterionRasters.Add(traffic);
                weights.Add(job.TrafficWeight);
                result.CriterionRasters["交通便捷度"] = traffic;
            }

            // 环境 20%
            Report(progress, result, "环境舒适度分析...", 40);
            string env = BuildEnvironment(gp, job, names, result);
            if (!string.IsNullOrEmpty(env))
            {
                criterionRasters.Add(env);
                weights.Add(job.EnvironmentWeight);
                result.CriterionRasters["环境舒适度"] = env;
            }

            // 设施 25%
            Report(progress, result, "设施完善度分析...", 60);
            string facility = BuildFacility(gp, job, names, result);
            if (!string.IsNullOrEmpty(facility))
            {
                criterionRasters.Add(facility);
                weights.Add(job.FacilityWeight);
                result.CriterionRasters["设施完善度"] = facility;
            }

            // 政策 25%
            Report(progress, result, "政策支持度分析...", 75);
            string policy = BuildPolicy(gp, job, names, result);
            if (!string.IsNullOrEmpty(policy))
            {
                criterionRasters.Add(policy);
                weights.Add(job.PolicyWeight);
                result.CriterionRasters["政策支持度"] = policy;
            }

            if (criterionRasters.Count == 0)
            {
                result.Messages.Add("未生成任何准则层栅格，请检查 GDB 是否包含可匹配的动力性图层。");
                Report(progress, result, "失败", 100);
                return result;
            }

            Report(progress, result, "准则层加权叠置...", 90);
            string outRaster = Path.Combine(job.WorkDirectory, "motivation_score");
            BufferScoreRasterBuilder.WeightedSum(gp, criterionRasters, weights, outRaster);
            result.MotivationRasterPath = outRaster;
            result.Success = true;
            result.Messages.Add("动力性栅格已生成: " + outRaster);
            Report(progress, result, "完成", 100);
            return result;
        }

        private static string BuildTraffic(GeoprocessorHelper gp, MotivationJob job, List<string> names, MotivationResult result)
        {
            List<string> parts = new List<string>();
            double cell = job.CellSize;

            string metroMulti = Resolve(job, names, "MetroMulti", "两线地铁", "两线", "换乘", "多线", "枢纽站");
            string metro = Resolve(job, names, "Metro", "一线地铁", "一线", "单线地铁");
            string cbd = Resolve(job, names, "CBD", "开发强度高", "CBD", "中心区", "高强度");
            string trafficFac = Resolve(job, names, "TrafficFacility", "交通设施", "交通枢纽", "高铁", "机场", "客运");

            if (!string.IsNullOrEmpty(metroMulti))
            {
                string path = WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, metroMulti);
                result.Messages.Add("多线地铁: " + metroMulti);
                parts.Add(BufferScoreRasterBuilder.BuildMultiRingMax(
                    gp, path,
                    new double[] { 300, 600, 1000 },
                    new int[] { 4, 3, 2 },
                    job.WorkDirectory, "metro_multi", cell));
            }

            if (!string.IsNullOrEmpty(metro) && !string.Equals(metro, metroMulti, StringComparison.OrdinalIgnoreCase))
            {
                string path = WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, metro);
                result.Messages.Add("地铁站点: " + metro);
                parts.Add(BufferScoreRasterBuilder.BuildMultiRingMax(
                    gp, path,
                    new double[] { 300, 600, 1000 },
                    new int[] { 3, 2, 1 },
                    job.WorkDirectory, "metro", cell));
            }

            if (!string.IsNullOrEmpty(cbd))
            {
                string path = WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, cbd);
                result.Messages.Add("CBD: " + cbd);
                parts.Add(BufferScoreRasterBuilder.BuildSingle(gp, path, 1000, 3, job.WorkDirectory, "cbd", cell));
            }

            if (!string.IsNullOrEmpty(trafficFac))
            {
                string path = WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, trafficFac);
                result.Messages.Add("交通设施: " + trafficFac);
                parts.Add(BufferScoreRasterBuilder.BuildSingle(gp, path, 300, 1, job.WorkDirectory, "traf_fac", cell));
            }

            if (parts.Count == 0)
            {
                result.Messages.Add("交通准则：未匹配到可用图层，已跳过。");
                return null;
            }

            return BufferScoreRasterBuilder.MaxCombine(gp, parts, job.WorkDirectory, "traffic");
        }

        private static string BuildEnvironment(GeoprocessorHelper gp, MotivationJob job, List<string> names, MotivationResult result)
        {
            List<string> parts = new List<string>();
            double cell = job.CellSize;

            string eco = Resolve(job, names, "EcoCorridor", "重要生态廊道", "生态廊道", "水系", "河道", "绿廊");
            string openSpace = Resolve(job, names, "OpenSpace", "大型开敞空间", "开敞空间", "湖泊");
            string green = Resolve(job, names, "Green", "城市公园绿地", "公园绿地", "现状绿地", "绿地");

            if (!string.IsNullOrEmpty(eco))
            {
                result.Messages.Add("生态廊道: " + eco);
                parts.Add(BufferScoreRasterBuilder.BuildSingle(
                    gp, WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, eco), 500, 2, job.WorkDirectory, "eco", cell));
            }
            if (!string.IsNullOrEmpty(openSpace))
            {
                result.Messages.Add("开敞空间: " + openSpace);
                parts.Add(BufferScoreRasterBuilder.BuildSingle(
                    gp, WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, openSpace), 500, 2, job.WorkDirectory, "open", cell));
            }
            if (!string.IsNullOrEmpty(green) && !string.Equals(green, openSpace, StringComparison.OrdinalIgnoreCase))
            {
                result.Messages.Add("现状绿地: " + green);
                parts.Add(BufferScoreRasterBuilder.BuildSingle(
                    gp, WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, green), 300, 1, job.WorkDirectory, "green", cell));
            }

            if (parts.Count == 0)
            {
                result.Messages.Add("环境准则：未匹配到可用图层，已跳过。");
                return null;
            }
            return BufferScoreRasterBuilder.MaxCombine(gp, parts, job.WorkDirectory, "environment");
        }

        private static string BuildFacility(GeoprocessorHelper gp, MotivationJob job, List<string> names, MotivationResult result)
        {
            List<string> parts = new List<string>();
            double cell = job.CellSize;

            string pub = Resolve(job, names, "PublicService", "市级医院", "高校学院", "文体设施", "公共服务", "公服", "医院", "学校");
            string conv = Resolve(job, names, "Convenience", "便民", "文体");
            string shop = Resolve(job, names, "Commercial", "商业", "商场");

            if (!string.IsNullOrEmpty(pub))
            {
                result.Messages.Add("市级公服: " + pub);
                parts.Add(BufferScoreRasterBuilder.BuildSingle(
                    gp, WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, pub), 1000, 2, job.WorkDirectory, "pub", cell));
            }
            if (!string.IsNullOrEmpty(conv))
            {
                result.Messages.Add("便民设施: " + conv);
                parts.Add(BufferScoreRasterBuilder.BuildSingle(
                    gp, WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, conv), 300, 1, job.WorkDirectory, "conv", cell));
            }
            if (!string.IsNullOrEmpty(shop))
            {
                result.Messages.Add("商业设施: " + shop);
                parts.Add(BufferScoreRasterBuilder.BuildSingle(
                    gp, WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, shop), 1000, 1, job.WorkDirectory, "shop", cell));
            }

            if (parts.Count == 0)
            {
                result.Messages.Add("设施准则：未匹配到可用图层，已跳过。");
                return null;
            }
            return BufferScoreRasterBuilder.MaxCombine(gp, parts, job.WorkDirectory, "facility");
        }

        private static string BuildPolicy(GeoprocessorHelper gp, MotivationJob job, List<string> names, MotivationResult result)
        {
            List<string> parts = new List<string>();
            double cell = job.CellSize;

            string belt = Resolve(job, names, "PolicyBelt", "战略圈层", "发展带", "发展圈", "圈带", "片区");
            string strategy = Resolve(job, names, "PolicyStrategy", "战略片区", "战略区");
            string key = Resolve(job, names, "PolicyKey", "近期重点发展", "近期重点", "重点发展");

            if (!string.IsNullOrEmpty(belt))
            {
                result.Messages.Add("发展圈带: " + belt);
                parts.Add(BufferScoreRasterBuilder.BuildPolygonScore(
                    gp, WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, belt), 1, job.WorkDirectory, "belt", cell));
            }
            if (!string.IsNullOrEmpty(strategy))
            {
                result.Messages.Add("战略片区: " + strategy);
                parts.Add(BufferScoreRasterBuilder.BuildPolygonScore(
                    gp, WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, strategy), 1, job.WorkDirectory, "strategy", cell));
            }
            if (!string.IsNullOrEmpty(key))
            {
                result.Messages.Add("近期重点区: " + key);
                parts.Add(BufferScoreRasterBuilder.BuildPolygonScore(
                    gp, WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, key), 2, job.WorkDirectory, "keyzone", cell));
            }

            if (parts.Count == 0)
            {
                result.Messages.Add("政策准则：未匹配到可用图层，已跳过。");
                return null;
            }
            return BufferScoreRasterBuilder.MaxCombine(gp, parts, job.WorkDirectory, "policy");
        }

        private static string Resolve(MotivationJob job, List<string> names, string hintKey, params string[] keywords)
        {
            if (job.LayerHints != null && job.LayerHints.ContainsKey(hintKey))
            {
                string hint = job.LayerHints[hintKey];
                if (!string.IsNullOrEmpty(hint))
                {
                    return hint;
                }
            }
            return WorkspaceCatalog.FindByKeywords(names, keywords);
        }

        private static void Report(Action<string, int> progress, MotivationResult result, string text, int percent)
        {
            result.Messages.Add(text);
            if (progress != null)
            {
                progress(text, percent);
            }
        }
    }
}

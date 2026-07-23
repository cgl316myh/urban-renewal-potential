using System;
using System.Collections.Generic;
using ESRI.ArcGIS.Geometry;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Analysis
{
    /// <summary>
    /// 动力性分析引擎：四准则层缓冲赋分 + 加权叠置。
    /// </summary>
    public class MotivationAnalysisEngine
    {
        private GeoprocessorHelper _gp;
        private MotivationJob _job;
        private ISpatialReference _targetSr;
        private string _extentPath;
        private readonly Dictionary<string, string> _preparedPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public MotivationResult Run(MotivationJob job, Action<string, int> progress)
        {
            MotivationResult result = new MotivationResult();
            if (job == null || string.IsNullOrEmpty(job.GdbPath))
            {
                result.Messages.Add("作业参数无效：缺少 GDB 路径。");
                return result;
            }

            // 解析输出 GDB：优先 OutputGdbPath；兼容旧 WorkDirectory（若为 *.gdb）
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
            _preparedPaths.Clear();
            BufferScoreRasterBuilder.ResetNameSequence();

            Report(progress, result, "枚举 GDB 图层...", 5);
            List<string> names = WorkspaceCatalog.ListFeatureClassNames(job.GdbPath);
            result.Messages.Add("GDB 要素类数量: " + names.Count);

            // 空间参考必须统一，否则拒绝执行（不做隐式重投影）
            Report(progress, result, "检查空间参考一致性...", 8);
            SpatialReferenceAuditResult srAudit = SpatialReferenceAudit.Audit(job.GdbPath);
            if (!srAudit.Success || !srAudit.IsUnified)
            {
                string block = srAudit.ToBlockMessage();
                result.Messages.Add(block);
                result.Success = false;
                Report(progress, result, "空间参考不统一，已取消", 100);
                return result;
            }
            result.Messages.Add("空间参考一致: " + srAudit.ReferenceSpatialReferenceName
                + "（" + srAudit.Layers.Count + " 个要素类）");

            _gp = new GeoprocessorHelper();
            Report(progress, result, "准备输出 GDB...", 10);
            string outGdb = OutputGdbHelper.EnsureExists(_gp, job.OutputGdbPath);
            job.OutputGdbPath = outGdb;
            job.WorkDirectory = outGdb;
            result.OutputGdbPath = outGdb;
            result.Messages.Add("输出 GDB: " + outGdb);
            OutputGdbHelper.Remember(outGdb);

            string studyLayer = Resolve(job, names, "StudyArea", "中心城区", "分析范围");
            string extentPath = null;
            if (!string.IsNullOrEmpty(studyLayer))
            {
                extentPath = WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, studyLayer);
                _targetSr = FeatureProjectionHelper.GetSpatialReference(extentPath);
                result.Messages.Add("分析范围: " + studyLayer + (_targetSr != null ? " [" + _targetSr.Name + "]" : string.Empty));
            }
            if (_targetSr == null)
            {
                // 回退：优先投影坐标系图层（如 CBD）
                string fallback = Resolve(job, names, "CBD", "开发强度高", "CBD", "宗地", "公园绿地");
                if (!string.IsNullOrEmpty(fallback))
                {
                    string fbPath = WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, fallback);
                    _targetSr = FeatureProjectionHelper.GetSpatialReference(fbPath);
                    if (string.IsNullOrEmpty(extentPath))
                    {
                        extentPath = fbPath;
                    }
                    result.Messages.Add("目标坐标系取自: " + fallback + (_targetSr != null ? " [" + _targetSr.Name + "]" : string.Empty));
                }
            }

            _gp.ConfigureAnalysis(outGdb, null, job.CellSize, _targetSr);
            if (!string.IsNullOrEmpty(extentPath))
            {
                _gp.SetExtent(extentPath);
            }
            _extentPath = extentPath;

            List<string> criterionRasters = new List<string>();
            List<double> weights = new List<double>();

            // 交通 30%
            Report(progress, result, "交通便捷度分析...", 20);
            string traffic = BuildTraffic(job, names, result);
            if (!string.IsNullOrEmpty(traffic))
            {
                criterionRasters.Add(traffic);
                weights.Add(job.TrafficWeight);
                result.CriterionRasters["交通便捷度"] = traffic;
            }

            // 环境 20%
            Report(progress, result, "环境舒适度分析...", 40);
            string env = BuildEnvironment(job, names, result);
            if (!string.IsNullOrEmpty(env))
            {
                criterionRasters.Add(env);
                weights.Add(job.EnvironmentWeight);
                result.CriterionRasters["环境舒适度"] = env;
            }

            // 设施 25%
            Report(progress, result, "设施完善度分析...", 60);
            string facility = BuildFacility(job, names, result);
            if (!string.IsNullOrEmpty(facility))
            {
                criterionRasters.Add(facility);
                weights.Add(job.FacilityWeight);
                result.CriterionRasters["设施完善度"] = facility;
            }

            // 政策 25%
            Report(progress, result, "政策支持度分析...", 75);
            string policy = BuildPolicy(job, names, result);
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
            string outRaster = OutputGdbHelper.DatasetPath(job.OutputGdbPath, "mot_score");
            BufferScoreRasterBuilder.WeightedSum(_gp, criterionRasters, weights, outRaster);
            result.MotivationRasterPath = outRaster;
            result.OutputGdbPath = job.OutputGdbPath;
            result.Success = true;
            result.Messages.Add("动力性栅格已生成: " + outRaster);
            Report(progress, result, "完成", 100);
            return result;
        }

        private string OutGdb
        {
            get { return _job != null ? _job.OutputGdbPath : null; }
        }

        private string Prepared(string layerName, MotivationResult result)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                return null;
            }
            if (_preparedPaths.ContainsKey(layerName))
            {
                return _preparedPaths[layerName];
            }

            // 空间参考已在 Run 入口校验统一，直接使用源图层路径
            string src = WorkspaceCatalog.ToFeatureClassPath(_job.GdbPath, layerName);
            _preparedPaths[layerName] = src;
            return src;
        }

        private string BuildTraffic(MotivationJob job, List<string> names, MotivationResult result)
        {
            List<string> parts = new List<string>();
            double cell = job.CellSize;

            string metroMulti = Resolve(job, names, "MetroMulti", "两线地铁", "两线", "换乘", "多线", "枢纽站");
            string metro = Resolve(job, names, "Metro", "一线地铁", "一线", "单线地铁");
            string cbd = Resolve(job, names, "CBD", "开发强度高", "CBD", "中心区", "高强度");
            string trafficFac = Resolve(job, names, "TrafficFacility", "交通设施", "交通枢纽", "高铁", "机场", "客运");

            if (!string.IsNullOrEmpty(metroMulti))
            {
                result.Messages.Add("多线地铁: " + metroMulti);
                parts.Add(BufferScoreRasterBuilder.BuildMultiRingMax(
                    _gp, Prepared(metroMulti, result),
                    new double[] { 300, 600, 1000 },
                    new int[] { 4, 3, 2 },
                    OutGdb, "metro_multi", cell));
            }

            if (!string.IsNullOrEmpty(metro) && !string.Equals(metro, metroMulti, StringComparison.OrdinalIgnoreCase))
            {
                result.Messages.Add("地铁站点: " + metro);
                parts.Add(BufferScoreRasterBuilder.BuildMultiRingMax(
                    _gp, Prepared(metro, result),
                    new double[] { 300, 600, 1000 },
                    new int[] { 3, 2, 1 },
                    OutGdb, "metro", cell));
            }

            if (!string.IsNullOrEmpty(cbd))
            {
                result.Messages.Add("CBD: " + cbd);
                parts.Add(BufferScoreRasterBuilder.BuildSingle(_gp, Prepared(cbd, result), 1000, 3, OutGdb, "cbd", cell));
            }

            if (!string.IsNullOrEmpty(trafficFac))
            {
                result.Messages.Add("交通设施: " + trafficFac);
                parts.Add(BufferScoreRasterBuilder.BuildSingle(_gp, Prepared(trafficFac, result), 300, 1, OutGdb, "traf_fac", cell));
            }

            if (parts.Count == 0)
            {
                result.Messages.Add("交通准则：未匹配到可用图层，已跳过。");
                return null;
            }

            return BufferScoreRasterBuilder.MaxCombine(_gp, parts, OutGdb, "traffic");
        }

        private string BuildEnvironment(MotivationJob job, List<string> names, MotivationResult result)
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
                    _gp, Prepared(eco, result), 500, 2, OutGdb, "eco", cell));
            }
            if (!string.IsNullOrEmpty(openSpace))
            {
                result.Messages.Add("开敞空间: " + openSpace);
                parts.Add(BufferScoreRasterBuilder.BuildSingle(
                    _gp, Prepared(openSpace, result), 500, 2, OutGdb, "open", cell));
            }
            if (!string.IsNullOrEmpty(green) && !string.Equals(green, openSpace, StringComparison.OrdinalIgnoreCase))
            {
                result.Messages.Add("现状绿地: " + green);
                parts.Add(BufferScoreRasterBuilder.BuildSingle(
                    _gp, Prepared(green, result), 300, 1, OutGdb, "green", cell));
            }

            if (parts.Count == 0)
            {
                result.Messages.Add("环境准则：未匹配到可用图层，已跳过。");
                return null;
            }
            return BufferScoreRasterBuilder.MaxCombine(_gp, parts, OutGdb, "environment");
        }

        private string BuildFacility(MotivationJob job, List<string> names, MotivationResult result)
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
                    _gp, Prepared(pub, result), 1000, 2, OutGdb, "pub", cell));
            }
            if (!string.IsNullOrEmpty(conv))
            {
                result.Messages.Add("便民设施: " + conv);
                parts.Add(BufferScoreRasterBuilder.BuildSingle(
                    _gp, Prepared(conv, result), 300, 1, OutGdb, "conv", cell));
            }
            if (!string.IsNullOrEmpty(shop))
            {
                result.Messages.Add("商业设施: " + shop);
                parts.Add(BufferScoreRasterBuilder.BuildSingle(
                    _gp, Prepared(shop, result), 1000, 1, OutGdb, "shop", cell));
            }

            if (parts.Count == 0)
            {
                result.Messages.Add("设施准则：未匹配到可用图层，已跳过。");
                return null;
            }
            return BufferScoreRasterBuilder.MaxCombine(_gp, parts, OutGdb, "facility");
        }

        private string BuildPolicy(MotivationJob job, List<string> names, MotivationResult result)
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
                    _gp, Prepared(belt, result), 1, OutGdb, "belt", cell));
            }
            if (!string.IsNullOrEmpty(strategy))
            {
                result.Messages.Add("战略片区: " + strategy);
                parts.Add(BufferScoreRasterBuilder.BuildPolygonScore(
                    _gp, Prepared(strategy, result), 1, OutGdb, "strategy", cell));
            }
            if (!string.IsNullOrEmpty(key))
            {
                result.Messages.Add("近期重点区: " + key);
                parts.Add(BufferScoreRasterBuilder.BuildPolygonScore(
                    _gp, Prepared(key, result), 2, OutGdb, "keyzone", cell));
            }

            if (parts.Count == 0)
            {
                result.Messages.Add("政策准则：未匹配到可用图层，已跳过。");
                return null;
            }
            return BufferScoreRasterBuilder.MaxCombine(_gp, parts, OutGdb, "policy");
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

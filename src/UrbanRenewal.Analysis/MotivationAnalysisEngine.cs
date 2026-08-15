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
        private Action<string, int> _progress;
        private int _progressPercent;

        public MotivationResult Run(MotivationJob job, Action<string, int> progress)
        {
            _progress = progress;
            _progressPercent = 0;
            MotivationResult result = new MotivationResult();
            if (job == null || string.IsNullOrEmpty(job.GdbPath))
            {
                Note(result, "作业参数无效：缺少 GDB 路径。");
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
                Note(result, "作业参数无效：请指定输出 File GDB（*.gdb）。");
                return result;
            }
            if (!OutputGdbHelper.IsFileGdbPath(job.OutputGdbPath))
            {
                Note(result, "输出路径必须是 File GDB（以 .gdb 结尾的文件夹）: " + job.OutputGdbPath);
                return result;
            }

            _job = job;
            _preparedPaths.Clear();
            BufferScoreRasterBuilder.ResetNameSequence();

            Report(progress, result, "枚举 GDB 图层...", 5);
            List<string> names = WorkspaceCatalog.ListFeatureClassNames(job.GdbPath);
            Note(result, "GDB 要素类数量: " + names.Count);

            // 空间参考：仅校验本次分析用到的图层（避免未用宗地等阻断）
            Report(progress, result, "检查空间参考一致性...", 8);
            List<string> usedLayers = SpatialReferenceAudit.CollectMotivationLayerNames(job.LayerHints, names);
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
                + "（校验 " + srAudit.Layers.Count + " 个分析图层"
                + (usedLayers.Count > 0 ? "，未用图层已忽略" : string.Empty) + "）");

            _gp = new GeoprocessorHelper();
            _gp.BindToProgress(_progress, delegate { return _progressPercent; });
            Report(progress, result, "准备输出 GDB...", 10);
            string outGdb = OutputGdbHelper.EnsureExists(_gp, job.OutputGdbPath);
            job.OutputGdbPath = outGdb;
            job.WorkDirectory = outGdb;
            result.OutputGdbPath = outGdb;
            Note(result, "输出 GDB: " + outGdb);
            // 全局路径记忆由宿主 SaveGlobalSettings 统一负责

            string studyLayer = Resolve(job, names, "StudyArea", "中心城区", "分析范围");
            string extentPath = null;
            if (!string.IsNullOrEmpty(studyLayer))
            {
                extentPath = WorkspaceCatalog.ToFeatureClassPath(job.GdbPath, studyLayer);
                _targetSr = FeatureProjectionHelper.GetSpatialReference(extentPath);
                Note(result, "分析范围: " + studyLayer + (_targetSr != null ? " [" + _targetSr.Name + "]" : string.Empty));
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
                    Note(result, "目标坐标系取自: " + fallback + (_targetSr != null ? " [" + _targetSr.Name + "]" : string.Empty));
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
            List<double> scoreMaxes = new List<double>();

            // 交通 30%
            Report(progress, result, "交通便捷度分析...", 20);
            string traffic = BuildTraffic(job, names, result);
            if (!string.IsNullOrEmpty(traffic))
            {
                criterionRasters.Add(traffic);
                weights.Add(job.TrafficWeight);
                scoreMaxes.Add(MotivationScoreScale.TrafficMax);
                result.CriterionRasters["交通便捷度"] = traffic;
            }

            // 环境 20%
            Report(progress, result, "环境舒适度分析...", 40);
            string env = BuildEnvironment(job, names, result);
            if (!string.IsNullOrEmpty(env))
            {
                criterionRasters.Add(env);
                weights.Add(job.EnvironmentWeight);
                scoreMaxes.Add(MotivationScoreScale.EnvironmentMax);
                result.CriterionRasters["环境舒适度"] = env;
            }

            // 设施 25%
            Report(progress, result, "设施完善度分析...", 60);
            string facility = BuildFacility(job, names, result);
            if (!string.IsNullOrEmpty(facility))
            {
                criterionRasters.Add(facility);
                weights.Add(job.FacilityWeight);
                scoreMaxes.Add(MotivationScoreScale.FacilityMax);
                result.CriterionRasters["设施完善度"] = facility;
            }

            // 政策 25%
            Report(progress, result, "政策支持度分析...", 75);
            string policy = BuildPolicy(job, names, result);
            if (!string.IsNullOrEmpty(policy))
            {
                criterionRasters.Add(policy);
                weights.Add(job.PolicyWeight);
                scoreMaxes.Add(MotivationScoreScale.PolicyMax);
                result.CriterionRasters["政策支持度"] = policy;
            }

            if (criterionRasters.Count == 0)
            {
                Note(result, "未生成任何准则层栅格，请检查 GDB 是否包含可匹配的动力性图层。");
                Report(progress, result, "失败", 100);
                return result;
            }

            Report(progress, result, "准则层标准化到 0–100...", 85);
            List<string> normalized = new List<string>();
            List<string> criterionLabels = new List<string>();
            List<string> normPrefixes = new List<string>();
            if (!string.IsNullOrEmpty(traffic)) { criterionLabels.Add("交通"); normPrefixes.Add("ntraf"); }
            if (!string.IsNullOrEmpty(env)) { criterionLabels.Add("环境"); normPrefixes.Add("nenv"); }
            if (!string.IsNullOrEmpty(facility)) { criterionLabels.Add("设施"); normPrefixes.Add("nfac"); }
            if (!string.IsNullOrEmpty(policy)) { criterionLabels.Add("政策"); normPrefixes.Add("npol"); }

            for (int i = 0; i < criterionRasters.Count; i++)
            {
                string label = i < criterionLabels.Count ? criterionLabels[i] : ("c" + i.ToString());
                string prefix = i < normPrefixes.Count ? normPrefixes[i] : ("nc" + i.ToString());
                string n100 = BufferScoreRasterBuilder.NormalizeTo100(
                    _gp, criterionRasters[i], scoreMaxes[i], OutGdb, prefix);
                normalized.Add(n100);
                Note(result, "准则「" + label + "」标准化 0–100（理论满分="
                    + scoreMaxes[i].ToString(System.Globalization.CultureInfo.InvariantCulture)
                    + "）: " + n100);
            }

            Report(progress, result, "准则层加权叠置...", 90);
            string outRaster = OutputGdbHelper.DatasetPath(job.OutputGdbPath, "mot_score");
            BufferScoreRasterBuilder.WeightedSum(_gp, normalized, weights, outRaster);
            result.MotivationRasterPath = outRaster;
            result.OutputGdbPath = job.OutputGdbPath;
            result.Success = true;
            Note(result, "动力性栅格已生成（0–100 标准化）: " + outRaster);
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
            BufferScoreRules rules = job.BufferScoreRules ?? BufferScoreRules.CreateOriginal();
            Note(result, "缓冲赋分规则: " + rules.DescribeMetro());

            string metroMulti = Resolve(job, names, "MetroMulti", "两线地铁", "两线", "换乘", "多线", "枢纽站");
            string metro = Resolve(job, names, "Metro", "一线地铁", "一线", "单线地铁");
            string cbd = Resolve(job, names, "CBD", "开发强度高", "CBD", "中心区", "高强度");
            string trafficFac = Resolve(job, names, "TrafficFacility", "交通设施", "交通枢纽", "高铁", "机场", "客运");
            string study = Resolve(job, names, "StudyArea", "中心城区", "分析范围");

            if (!string.IsNullOrEmpty(metroMulti))
            {
                double[] dMulti;
                int[] sMulti;
                GetActiveRingsSafe(rules.MetroMulti, out dMulti, out sMulti);
                if (dMulti.Length > 0)
                {
                    Report(_progress, result, "交通·多线地铁多环缓冲(" + rules.MetroMulti.ToDisplay() + ")...", 21);
                    Note(result, "多线地铁: " + metroMulti);
                    parts.Add(BufferScoreRasterBuilder.BuildMultiRingMax(
                        _gp, Prepared(metroMulti, result),
                        dMulti, sMulti,
                        OutGdb, "metro_multi", cell,
                        delegate(string t) { Note(result, "  · " + t); }));
                    Note(result, "交通·多线地铁缓冲完成");
                }
                else
                {
                    Note(result, "多线地铁规则无有效环，已跳过。");
                }
            }

            if (!string.IsNullOrEmpty(metro) && !string.Equals(metro, metroMulti, StringComparison.OrdinalIgnoreCase))
            {
                double[] dSingle;
                int[] sSingle;
                GetActiveRingsSafe(rules.MetroSingle, out dSingle, out sSingle);
                if (dSingle.Length > 0)
                {
                    Report(_progress, result, "交通·一线地铁多环缓冲(" + rules.MetroSingle.ToDisplay() + ")...", 24);
                    Note(result, "地铁站点: " + metro);
                    parts.Add(BufferScoreRasterBuilder.BuildMultiRingMax(
                        _gp, Prepared(metro, result),
                        dSingle, sSingle,
                        OutGdb, "metro", cell,
                        delegate(string t) { Note(result, "  · " + t); }));
                    Note(result, "交通·一线地铁缓冲完成");
                }
                else
                {
                    Note(result, "单线地铁规则无有效环，已跳过。");
                }
            }

            // 路网可达性（须预先构建 Network Dataset，如 roadNet\roadNet_ND）
            Report(_progress, result, "交通·路网可达性（服务区求解）...", 27);
            string roadAccess = BuildRoadAccessibility(job, cbd, study, metro, result);
            if (!string.IsNullOrEmpty(roadAccess))
            {
                parts.Add(roadAccess);
                Note(result, "交通·路网可达性完成");
            }

            if (rules.Cbd != null && rules.Cbd.IsActive && !string.IsNullOrEmpty(cbd))
            {
                Report(_progress, result, "交通·CBD 缓冲(" + rules.Cbd.ToDisplay() + ")...", 33);
                Note(result, "CBD: " + cbd);
                parts.Add(BufferScoreRasterBuilder.BuildSingle(
                    _gp, Prepared(cbd, result), rules.Cbd.Distance, rules.Cbd.Score, OutGdb, "cbd", cell));
                Note(result, "交通·CBD 缓冲完成");
            }

            if (rules.TrafficFacility != null && rules.TrafficFacility.IsActive && !string.IsNullOrEmpty(trafficFac))
            {
                Report(_progress, result, "交通·交通设施缓冲(" + rules.TrafficFacility.ToDisplay() + ")...", 35);
                Note(result, "交通设施: " + trafficFac);
                parts.Add(BufferScoreRasterBuilder.BuildSingle(
                    _gp, Prepared(trafficFac, result),
                    rules.TrafficFacility.Distance, rules.TrafficFacility.Score,
                    OutGdb, "traf_fac", cell));
                Note(result, "交通·交通设施缓冲完成");
            }

            if (parts.Count == 0)
            {
                Note(result, "交通准则：未匹配到可用图层，已跳过。");
                return null;
            }

            Report(_progress, result, "交通·准则层 MAX 合并...", 37);
            return BufferScoreRasterBuilder.MaxCombine(_gp, parts, OutGdb, "traffic");
        }

        private static void GetActiveRingsSafe(MultiRingRule rule, out double[] distances, out int[] scores)
        {
            if (rule == null)
            {
                distances = new double[0];
                scores = new int[0];
                return;
            }
            rule.GetActiveRings(out distances, out scores);
        }

        /// <summary>
        /// 到城市中心的路网可达性（1–5 分）。路网数据集须事先建好。
        /// </summary>
        private string BuildRoadAccessibility(
            MotivationJob job,
            string cbdLayer,
            string studyLayer,
            string metroLayer,
            MotivationResult result)
        {
            string facilityLayer = cbdLayer;
            if (string.IsNullOrEmpty(facilityLayer))
            {
                facilityLayer = studyLayer;
            }
            if (string.IsNullOrEmpty(facilityLayer))
            {
                facilityLayer = metroLayer;
            }
            if (string.IsNullOrEmpty(facilityLayer))
            {
                Note(result, "路网可达性：无 CBD/分析范围/地铁作为中心设施，已跳过。");
                return null;
            }

            string fdName = ResolveHint(job, "RoadFeatureDataset") ?? NetworkDatasetHelper.DefaultFeatureDataset;
            string ndName = ResolveHint(job, "RoadNetwork") ?? NetworkDatasetHelper.DefaultNetworkName;
            string impedance = ResolveHint(job, "RoadImpedance") ?? NetworkDatasetHelper.DefaultImpedance;

            Note(result, "路网可达性：中心设施=" + facilityLayer
                + "；网络=" + fdName + "\\" + ndName + "（须预先构建）");

            // AddMsg 已写入 Messages；此处仅推进度，避免重复入列表
            Action<string> live = delegate(string t)
            {
                if (_progress != null)
                {
                    _progress(t, _progressPercent);
                }
            };

            return RoadNetworkAccessibilityBuilder.Build(
                _gp,
                job.GdbPath,
                OutGdb,
                Prepared(facilityLayer, result),
                fdName,
                ndName,
                impedance,
                job.CellSize,
                result.Messages,
                live);
        }

        private static string ResolveHint(MotivationJob job, string hintKey)
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

        private string BuildEnvironment(MotivationJob job, List<string> names, MotivationResult result)
        {
            List<string> parts = new List<string>();
            double cell = job.CellSize;
            BufferScoreRules rules = job.BufferScoreRules ?? BufferScoreRules.CreateOriginal();

            string eco = Resolve(job, names, "EcoCorridor", "重要生态廊道", "生态廊道", "水系", "河道", "绿廊");
            string openSpace = Resolve(job, names, "OpenSpace", "大型开敞空间", "开敞空间", "湖泊");
            string green = Resolve(job, names, "Green", "城市公园绿地", "公园绿地", "现状绿地", "绿地");

            if (!string.IsNullOrEmpty(eco))
            {
                SingleRingRule ecoRule = rules.EcoCorridor ?? SingleRingRule.Create(500, 2);
                if (ecoRule.IsActive)
                {
                    Report(_progress, result, "环境·生态廊道缓冲(" + ecoRule.ToDisplay() + ")...", 42);
                    Note(result, "生态廊道: " + eco);
                    parts.Add(BufferScoreRasterBuilder.BuildSingle(
                        _gp, Prepared(eco, result), ecoRule.Distance, ecoRule.Score, OutGdb, "eco", cell));
                }
            }
            if (!string.IsNullOrEmpty(openSpace))
            {
                SingleRingRule openRule = rules.OpenSpace ?? SingleRingRule.Create(500, 2);
                if (openRule.IsActive)
                {
                    Report(_progress, result, "环境·开敞空间缓冲(" + openRule.ToDisplay() + ")...", 45);
                    Note(result, "开敞空间: " + openSpace);
                    parts.Add(BufferScoreRasterBuilder.BuildSingle(
                        _gp, Prepared(openSpace, result), openRule.Distance, openRule.Score, OutGdb, "open", cell));
                }
            }
            if (!string.IsNullOrEmpty(green) && !string.Equals(green, openSpace, StringComparison.OrdinalIgnoreCase))
            {
                SingleRingRule greenRule = rules.Green ?? SingleRingRule.Create(300, 1);
                if (greenRule.IsActive)
                {
                    Report(_progress, result, "环境·现状绿地缓冲(" + greenRule.ToDisplay() + ")...", 48);
                    Note(result, "现状绿地: " + green);
                    parts.Add(BufferScoreRasterBuilder.BuildSingle(
                        _gp, Prepared(green, result), greenRule.Distance, greenRule.Score, OutGdb, "green", cell));
                }
            }

            if (parts.Count == 0)
            {
                Note(result, "环境准则：未匹配到可用图层，已跳过。");
                return null;
            }
            Report(_progress, result, "环境·准则层 MAX 合并...", 50);
            return BufferScoreRasterBuilder.MaxCombine(_gp, parts, OutGdb, "environment");
        }

        private string BuildFacility(MotivationJob job, List<string> names, MotivationResult result)
        {
            List<string> parts = new List<string>();
            double cell = job.CellSize;
            BufferScoreRules rules = job.BufferScoreRules ?? BufferScoreRules.CreateOriginal();

            string pub = Resolve(job, names, "PublicService", "市级医院", "高校学院", "文体设施", "公共服务", "公服", "医院", "学校");
            string conv = Resolve(job, names, "Convenience", "便民", "文体");
            string shop = Resolve(job, names, "Commercial", "商业", "商场");

            if (!string.IsNullOrEmpty(pub))
            {
                SingleRingRule pubRule = rules.PublicService ?? SingleRingRule.Create(1000, 2);
                if (pubRule.IsActive)
                {
                    Report(_progress, result, "设施·市级公服缓冲(" + pubRule.ToDisplay() + ")...", 62);
                    Note(result, "市级公服: " + pub);
                    parts.Add(BufferScoreRasterBuilder.BuildSingle(
                        _gp, Prepared(pub, result), pubRule.Distance, pubRule.Score, OutGdb, "pub", cell));
                }
            }
            if (!string.IsNullOrEmpty(conv))
            {
                SingleRingRule convRule = rules.Convenience ?? SingleRingRule.Create(300, 1);
                if (convRule.IsActive)
                {
                    Report(_progress, result, "设施·便民设施缓冲(" + convRule.ToDisplay() + ")...", 66);
                    Note(result, "便民设施: " + conv);
                    parts.Add(BufferScoreRasterBuilder.BuildSingle(
                        _gp, Prepared(conv, result), convRule.Distance, convRule.Score, OutGdb, "conv", cell));
                }
            }
            if (!string.IsNullOrEmpty(shop))
            {
                SingleRingRule shopRule = rules.Commercial ?? SingleRingRule.Create(1000, 1);
                if (shopRule.IsActive)
                {
                    Report(_progress, result, "设施·商业设施缓冲(" + shopRule.ToDisplay() + ")...", 70);
                    Note(result, "商业设施: " + shop);
                    parts.Add(BufferScoreRasterBuilder.BuildSingle(
                        _gp, Prepared(shop, result), shopRule.Distance, shopRule.Score, OutGdb, "shop", cell));
                }
            }

            if (parts.Count == 0)
            {
                Note(result, "设施准则：未匹配到可用图层，已跳过。");
                return null;
            }
            Report(_progress, result, "设施·准则层 MAX 合并...", 72);
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
                Report(_progress, result, "政策·发展圈带栅格化...", 76);
                Note(result, "发展圈带: " + belt);
                parts.Add(BufferScoreRasterBuilder.BuildPolygonScore(
                    _gp, Prepared(belt, result), 1, OutGdb, "belt", cell));
            }
            if (!string.IsNullOrEmpty(strategy))
            {
                Report(_progress, result, "政策·战略片区栅格化...", 79);
                Note(result, "战略片区: " + strategy);
                parts.Add(BufferScoreRasterBuilder.BuildPolygonScore(
                    _gp, Prepared(strategy, result), 1, OutGdb, "strategy", cell));
            }
            if (!string.IsNullOrEmpty(key))
            {
                Report(_progress, result, "政策·近期重点区栅格化...", 82);
                Note(result, "近期重点区: " + key);
                parts.Add(BufferScoreRasterBuilder.BuildPolygonScore(
                    _gp, Prepared(key, result), 2, OutGdb, "keyzone", cell));
            }

            if (parts.Count == 0)
            {
                Note(result, "政策准则：未匹配到可用图层，已跳过。");
                return null;
            }
            Report(_progress, result, "政策·准则层 MAX 合并...", 84);
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

        private void Note(MotivationResult result, string text)
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

        private void Report(Action<string, int> progress, MotivationResult result, string text, int percent)
        {
            _progressPercent = percent;
            Note(result, text);
        }
    }
}

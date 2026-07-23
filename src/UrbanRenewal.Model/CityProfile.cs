using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace UrbanRenewal.Model
{
    /// <summary>
    /// 城市分析配置：换城市时复制模板改图层名即可。
    /// </summary>
    [XmlRoot("CityProfile")]
    public class CityProfile
    {
        public CityProfile()
        {
            Layers = new List<CityLayerMapping>();
            CellSize = 30;
            TrafficWeight = 0.30;
            EnvironmentWeight = 0.20;
            FacilityWeight = 0.25;
            PolicyWeight = 0.25;
        }

        [XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("displayName")]
        public string DisplayName { get; set; }

        [XmlAttribute("isDefault")]
        public bool IsDefault { get; set; }

        [XmlAttribute("cellSize")]
        public double CellSize { get; set; }

        [XmlAttribute("preferredCrsName")]
        public string PreferredCrsName { get; set; }

        [XmlAttribute("trafficWeight")]
        public double TrafficWeight { get; set; }

        [XmlAttribute("environmentWeight")]
        public double EnvironmentWeight { get; set; }

        [XmlAttribute("facilityWeight")]
        public double FacilityWeight { get; set; }

        [XmlAttribute("policyWeight")]
        public double PolicyWeight { get; set; }

        [XmlElement("Layer")]
        public List<CityLayerMapping> Layers { get; set; }

        /// <summary>预建路网（Network Dataset）；分析不会自动构建。</summary>
        [XmlElement("NetworkDataset")]
        public CityNetworkDataset NetworkDataset { get; set; }

        /// <summary>配置文件完整路径（加载后填充，不序列化）。</summary>
        [XmlIgnore]
        public string SourcePath { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(DisplayName))
            {
                return DisplayName;
            }
            return Id ?? base.ToString();
        }

        /// <summary>
        /// 将配置写入作业：权重、像元、图层提示（仅当 GDB 中能匹配到时写入）。
        /// </summary>
        public void ApplyToJob(MotivationJob job, IList<string> featureClassNames, IList<string> messages)
        {
            if (job == null)
            {
                return;
            }

            if (CellSize > 0)
            {
                job.CellSize = CellSize;
            }
            job.TrafficWeight = TrafficWeight;
            job.EnvironmentWeight = EnvironmentWeight;
            job.FacilityWeight = FacilityWeight;
            job.PolicyWeight = PolicyWeight;

            if (job.LayerHints == null)
            {
                job.LayerHints = new Dictionary<string, string>();
            }

            if (Layers == null)
            {
                return;
            }

            for (int i = 0; i < Layers.Count; i++)
            {
                CityLayerMapping map = Layers[i];
                if (map == null || string.IsNullOrEmpty(map.Role))
                {
                    continue;
                }

                string resolved = ResolveLayerName(map, featureClassNames);
                if (!string.IsNullOrEmpty(resolved))
                {
                    job.LayerHints[map.Role] = resolved;
                    if (messages != null)
                    {
                        messages.Add("配置映射 " + map.Role + " → " + resolved);
                    }
                }
                else if (messages != null)
                {
                    messages.Add("配置未匹配 " + map.Role
                        + (string.IsNullOrEmpty(map.Name) ? string.Empty : "（期望: " + map.Name + "）"));
                }
            }

            if (NetworkDataset != null)
            {
                if (!string.IsNullOrEmpty(NetworkDataset.FeatureDataset))
                {
                    job.LayerHints["RoadFeatureDataset"] = NetworkDataset.FeatureDataset;
                }
                if (!string.IsNullOrEmpty(NetworkDataset.Name))
                {
                    job.LayerHints["RoadNetwork"] = NetworkDataset.Name;
                }
                if (!string.IsNullOrEmpty(NetworkDataset.ImpedanceAttribute))
                {
                    job.LayerHints["RoadImpedance"] = NetworkDataset.ImpedanceAttribute;
                }
                if (messages != null)
                {
                    messages.Add("配置路网 "
                        + (NetworkDataset.FeatureDataset ?? "")
                        + "\\"
                        + (NetworkDataset.Name ?? "")
                        + "（须预先构建 Network Dataset）");
                }
            }
        }

        public string BuildLayerPresenceReport(IList<string> featureClassNames)
        {
            StringBuilder sb = new StringBuilder();
            string title = string.IsNullOrEmpty(DisplayName) ? (Id ?? "未命名") : DisplayName;
            sb.AppendLine("城市配置: " + title
                + (string.IsNullOrEmpty(SourcePath) ? string.Empty : " [" + Path.GetFileName(SourcePath) + "]"));

            if (!string.IsNullOrEmpty(PreferredCrsName))
            {
                sb.AppendLine("建议坐标系: " + PreferredCrsName);
            }

            if (Layers == null || Layers.Count == 0)
            {
                sb.AppendLine("[警告] 城市配置未定义任何图层角色。");
                return sb.ToString();
            }

            int ok = 0;
            int miss = 0;
            for (int i = 0; i < Layers.Count; i++)
            {
                CityLayerMapping map = Layers[i];
                if (map == null || string.IsNullOrEmpty(map.Role))
                {
                    continue;
                }
                string resolved = ResolveLayerName(map, featureClassNames);
                if (!string.IsNullOrEmpty(resolved))
                {
                    ok++;
                    sb.AppendLine("[通过] " + map.Role + " → " + resolved);
                }
                else
                {
                    miss++;
                    sb.AppendLine("[警告] 缺少角色 " + map.Role
                        + (string.IsNullOrEmpty(map.Name) ? string.Empty : "（配置名: " + map.Name + "）"
                        + (string.IsNullOrEmpty(map.Keywords) ? string.Empty : "；关键词: " + map.Keywords)));
                }
            }
            sb.AppendLine("图层角色匹配: 通过 " + ok + " / 缺失 " + miss);
            return sb.ToString();
        }

        /// <summary>
        /// 检查 required=true 的角色是否都能在 GDB 中解析到。
        /// </summary>
        public bool ValidateRequired(IList<string> featureClassNames, out string message)
        {
            StringBuilder sb = new StringBuilder();
            bool ok = true;
            if (Layers == null)
            {
                message = null;
                return true;
            }
            for (int i = 0; i < Layers.Count; i++)
            {
                CityLayerMapping map = Layers[i];
                if (map == null || !map.Required || string.IsNullOrEmpty(map.Role))
                {
                    continue;
                }
                string resolved = ResolveLayerName(map, featureClassNames);
                if (string.IsNullOrEmpty(resolved))
                {
                    ok = false;
                    sb.AppendLine("- 必选角色缺失: " + map.Role
                        + (string.IsNullOrEmpty(map.Name) ? string.Empty : "（配置名: " + map.Name + "）"));
                }
            }
            message = ok ? null : sb.ToString();
            return ok;
        }

        /// <summary>
        /// 按通用角色关键词，从 GDB 图层名自动草拟城市配置（换城时少改即可）。
        /// </summary>
        public static CityProfile CreateDraft(string id, string displayName, IList<string> featureClassNames)
        {
            CityProfile profile = new CityProfile();
            profile.Id = string.IsNullOrEmpty(id) ? "NewCity" : id;
            profile.DisplayName = string.IsNullOrEmpty(displayName) ? profile.Id : displayName;
            profile.IsDefault = false;
            profile.CellSize = 30;
            profile.TrafficWeight = 0.30;
            profile.EnvironmentWeight = 0.20;
            profile.FacilityWeight = 0.25;
            profile.PolicyWeight = 0.25;

            AddDraftLayer(profile, featureClassNames, "StudyArea", true, "中心城区", "分析范围", "建成区", "城区范围");
            AddDraftLayer(profile, featureClassNames, "MetroMulti", false, "两线地铁", "换乘", "多线");
            AddDraftLayer(profile, featureClassNames, "Metro", false, "一线地铁", "地铁站", "地铁");
            AddDraftLayer(profile, featureClassNames, "CBD", false, "CBD", "开发强度", "中心区");
            AddDraftLayer(profile, featureClassNames, "TrafficFacility", false, "交通枢纽", "高铁", "机场", "客运");
            AddDraftLayer(profile, featureClassNames, "EcoCorridor", false, "生态廊道", "水系", "绿廊");
            AddDraftLayer(profile, featureClassNames, "OpenSpace", false, "开敞空间", "湖泊");
            AddDraftLayer(profile, featureClassNames, "Green", false, "公园绿地", "绿地");
            AddDraftLayer(profile, featureClassNames, "PublicService", false, "医院", "高校", "公服");
            AddDraftLayer(profile, featureClassNames, "Convenience", false, "文体", "便民");
            AddDraftLayer(profile, featureClassNames, "Commercial", false, "商业", "商场");
            AddDraftLayer(profile, featureClassNames, "PolicyBelt", false, "战略圈层", "发展带", "片区");
            AddDraftLayer(profile, featureClassNames, "PolicyStrategy", false, "战略片区", "战略区");
            AddDraftLayer(profile, featureClassNames, "PolicyKey", false, "近期重点", "重点发展");

            // 默认路网命名（须用户预先在 GDB 中构建；分析不会自动创建）
            profile.NetworkDataset = new CityNetworkDataset();
            profile.NetworkDataset.FeatureDataset = "roadNet";
            profile.NetworkDataset.Name = "roadNet_ND";
            profile.NetworkDataset.ImpedanceAttribute = "Length";

            // 建议坐标系：优先分析范围图层的名称提示，留给用户填写 preferredCrsName
            return profile;
        }

        private static void AddDraftLayer(CityProfile profile, IList<string> names, string role, bool required, params string[] keywords)
        {
            CityLayerMapping map = new CityLayerMapping();
            map.Role = role;
            map.Required = required;
            map.Keywords = string.Join(",", keywords);
            string resolved = null;
            CityLayerMapping probe = new CityLayerMapping();
            probe.Role = role;
            probe.Keywords = map.Keywords;
            resolved = ResolveLayerName(probe, names);
            if (!string.IsNullOrEmpty(resolved))
            {
                map.Name = resolved;
            }
            profile.Layers.Add(map);
        }

        public static string ResolveLayerName(CityLayerMapping map, IList<string> featureClassNames)
        {
            if (map == null || featureClassNames == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(map.Name))
            {
                for (int i = 0; i < featureClassNames.Count; i++)
                {
                    if (string.Equals(featureClassNames[i], map.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return featureClassNames[i];
                    }
                }
                for (int i = 0; i < featureClassNames.Count; i++)
                {
                    if (featureClassNames[i].IndexOf(map.Name, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return featureClassNames[i];
                    }
                }
            }

            if (!string.IsNullOrEmpty(map.Keywords))
            {
                string[] parts = map.Keywords.Split(new char[] { ',', ';', '|', '，', '；' }, StringSplitOptions.RemoveEmptyEntries);
                for (int p = 0; p < parts.Length; p++)
                {
                    string kw = parts[p].Trim();
                    if (kw.Length == 0)
                    {
                        continue;
                    }
                    for (int i = 0; i < featureClassNames.Count; i++)
                    {
                        if (featureClassNames[i].IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            return featureClassNames[i];
                        }
                    }
                }
            }

            return null;
        }
    }

    public class CityLayerMapping
    {
        /// <summary>角色键，对应 MotivationJob.LayerHints，如 StudyArea / Metro / CBD。</summary>
        [XmlAttribute("role")]
        public string Role { get; set; }

        /// <summary>要素类精确或包含匹配名。</summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>备用关键词，逗号分隔。</summary>
        [XmlAttribute("keywords")]
        public string Keywords { get; set; }

        [XmlAttribute("required")]
        public bool Required { get; set; }
    }

    /// <summary>
    /// 城市预建路网配置（分析程序只打开、不构建）。
    /// </summary>
    public class CityNetworkDataset
    {
        [XmlAttribute("featureDataset")]
        public string FeatureDataset { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("impedanceAttribute")]
        public string ImpedanceAttribute { get; set; }
    }

    /// <summary>
    /// 从 Config/Cities/*.xml 加载城市配置。
    /// </summary>
    public static class CityProfileStore
    {
        public static string GetCitiesDirectory()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string dir = Path.Combine(baseDir, "Config", "Cities");
            if (Directory.Exists(dir))
            {
                return dir;
            }
            // 开发时：从 Host 输出目录向上找解决方案 Config
            string alt = Path.Combine(baseDir, "..", "..", "..", "Config", "Cities");
            alt = Path.GetFullPath(alt);
            if (Directory.Exists(alt))
            {
                return alt;
            }
            return dir;
        }

        public static List<CityProfile> LoadAll()
        {
            List<CityProfile> list = new List<CityProfile>();
            string dir = GetCitiesDirectory();
            if (!Directory.Exists(dir))
            {
                return list;
            }

            string[] files = Directory.GetFiles(dir, "*.xml");
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < files.Length; i++)
            {
                string name = Path.GetFileName(files[i]);
                if (name != null && name.StartsWith("_", StringComparison.Ordinal))
                {
                    // _Template.xml 等以下划线开头的不作为可选城市
                    continue;
                }
                try
                {
                    CityProfile profile = LoadFile(files[i]);
                    if (profile != null)
                    {
                        list.Add(profile);
                    }
                }
                catch
                {
                }
            }
            return list;
        }

        public static CityProfile LoadFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return null;
            }
            XmlSerializer xs = new XmlSerializer(typeof(CityProfile));
            using (FileStream fs = File.OpenRead(path))
            {
                CityProfile profile = xs.Deserialize(fs) as CityProfile;
                if (profile != null)
                {
                    profile.SourcePath = path;
                    if (string.IsNullOrEmpty(profile.Id))
                    {
                        profile.Id = Path.GetFileNameWithoutExtension(path);
                    }
                }
                return profile;
            }
        }

        public static CityProfile GetDefault()
        {
            List<CityProfile> all = LoadAll();
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].IsDefault)
                {
                    return all[i];
                }
            }
            if (all.Count > 0)
            {
                return all[0];
            }
            return null;
        }

        public static CityProfile FindById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            List<CityProfile> all = LoadAll();
            for (int i = 0; i < all.Count; i++)
            {
                if (string.Equals(all[i].Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    return all[i];
                }
            }
            return null;
        }

        /// <summary>解析当前生效配置：指定 Id → 记住的 Id → isDefault。</summary>
        public static CityProfile ResolveActive(string preferredId)
        {
            CityProfile p = FindById(preferredId);
            if (p != null)
            {
                return p;
            }
            p = FindById(LoadRememberedId());
            if (p != null)
            {
                return p;
            }
            return GetDefault();
        }

        public static string GetActiveIdFilePath()
        {
            return Path.Combine(GetCitiesDirectory(), "_active_city.txt");
        }

        public static string LoadRememberedId()
        {
            try
            {
                string path = GetActiveIdFilePath();
                if (File.Exists(path))
                {
                    return File.ReadAllText(path).Trim();
                }
            }
            catch
            {
            }
            return null;
        }

        public static void RememberId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }
            try
            {
                string dir = GetCitiesDirectory();
                Directory.CreateDirectory(dir);
                File.WriteAllText(GetActiveIdFilePath(), id.Trim(), Encoding.UTF8);
            }
            catch
            {
            }
        }

        public static void Save(CityProfile profile, string path)
        {
            if (profile == null || string.IsNullOrEmpty(path))
            {
                return;
            }
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }
            XmlSerializer xs = new XmlSerializer(typeof(CityProfile));
            using (FileStream fs = File.Create(path))
            {
                xs.Serialize(fs, profile);
            }
        }

        /// <summary>权重属性若以百分数写在 XML（如 30），规范化为 0~1。</summary>
        public static void NormalizeWeights(CityProfile profile)
        {
            if (profile == null)
            {
                return;
            }
            profile.TrafficWeight = NormalizeOne(profile.TrafficWeight);
            profile.EnvironmentWeight = NormalizeOne(profile.EnvironmentWeight);
            profile.FacilityWeight = NormalizeOne(profile.FacilityWeight);
            profile.PolicyWeight = NormalizeOne(profile.PolicyWeight);
        }

        private static double NormalizeOne(double w)
        {
            if (w > 1.0)
            {
                return w / 100.0;
            }
            return w;
        }
    }
}

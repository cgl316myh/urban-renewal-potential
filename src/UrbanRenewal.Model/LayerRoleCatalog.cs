using System.Collections.Generic;

namespace UrbanRenewal.Model
{
    /// <summary>
    /// 系统标准图层角色目录（数据配置界面与城市模板共用）。
    /// </summary>
    public sealed class LayerRoleDefinition
    {
        public string Role { get; set; }

        /// <summary>界面中文名。</summary>
        public string DisplayNameZh { get; set; }

        /// <summary>期望几何：Point / Polyline / Polygon / Raster / 任意。</summary>
        public string GeometryType { get; set; }

        /// <summary>数据配置中是否必填。</summary>
        public bool Required { get; set; }

        public bool IsRaster { get; set; }

        public string DefaultKeywords { get; set; }
    }

    public static class LayerRoleCatalog
    {
        private static readonly LayerRoleDefinition[] Definitions = new LayerRoleDefinition[]
        {
            Def("StudyArea", "分析范围/建成区", "Polygon", true, false, "中心城区,分析范围,建成区"),
            Def("MetroMulti", "两线及以上地铁站", "Point", false, false, "两线地铁,换乘,多线"),
            Def("Metro", "单线地铁站", "Point", false, false, "一线地铁,地铁站,地铁"),
            Def("CBD", "CBD/高强度建设区", "Polygon", false, false, "CBD,开发强度,中心区"),
            Def("TrafficFacility", "大型交通设施", "Point", false, false, "交通枢纽,高铁,机场"),
            Def("EcoCorridor", "重要生态廊道/水系", "Polyline", false, false, "生态廊道,水系,绿廊"),
            Def("OpenSpace", "大型开敞空间/湖泊", "Polygon", false, false, "开敞空间,湖泊"),
            Def("Green", "现状绿地/公园", "Polygon", false, false, "公园绿地,绿地"),
            Def("PublicService", "市级公共服务设施", "Point", false, false, "医院,高校,公服"),
            Def("Convenience", "便民/文体设施", "Point", false, false, "文体,便民"),
            Def("Commercial", "市级商业设施", "Point", false, false, "商业,商场"),
            Def("PolicyBelt", "城市发展带/战略圈层", "Polygon", false, false, "战略圈层,发展带,片区"),
            Def("PolicyStrategy", "战略片区", "Polygon", false, false, "战略片区,战略区"),
            Def("PolicyKey", "近期重点发展区", "Polygon", false, false, "近期重点,重点发展"),
            Def("Parcel", "宗地/土地利用斑块", "Polygon", true, false, "宗地,地块,土地利用"),
            Def("UpdatedParcel", "已更新宗地", "Polygon", false, false, "已更新,已改造,更新宗地"),
            Def("DEM", "DEM 高程栅格", "Raster", false, true, "DEM,高程,Elevation"),
            Def("Slope", "坡度栅格（可选）", "Raster", false, true, "坡度,Slope"),
            Def("Population", "人口密度栅格", "Raster", false, true, "人口,人口密度,population")
        };

        public static IList<LayerRoleDefinition> GetAll()
        {
            return Definitions;
        }

        public static LayerRoleDefinition Find(string role)
        {
            if (string.IsNullOrEmpty(role))
            {
                return null;
            }
            for (int i = 0; i < Definitions.Length; i++)
            {
                if (string.Equals(Definitions[i].Role, role, System.StringComparison.OrdinalIgnoreCase))
                {
                    return Definitions[i];
                }
            }
            return null;
        }

        /// <summary>确保城市配置包含全部标准角色（不覆盖已有 name）。</summary>
        public static void EnsureRoles(CityProfile profile)
        {
            if (profile == null)
            {
                return;
            }
            if (profile.Layers == null)
            {
                profile.Layers = new List<CityLayerMapping>();
            }
            for (int i = 0; i < Definitions.Length; i++)
            {
                LayerRoleDefinition def = Definitions[i];
                CityLayerMapping existing = null;
                for (int j = 0; j < profile.Layers.Count; j++)
                {
                    if (profile.Layers[j] != null
                        && string.Equals(profile.Layers[j].Role, def.Role, System.StringComparison.OrdinalIgnoreCase))
                    {
                        existing = profile.Layers[j];
                        break;
                    }
                }
                if (existing == null)
                {
                    CityLayerMapping map = new CityLayerMapping();
                    map.Role = def.Role;
                    map.Name = string.Empty;
                    map.Keywords = def.DefaultKeywords;
                    map.Required = def.Required;
                    profile.Layers.Add(map);
                }
                else
                {
                    existing.Required = def.Required;
                    if (string.IsNullOrEmpty(existing.Keywords))
                    {
                        existing.Keywords = def.DefaultKeywords;
                    }
                }
            }
        }

        private static LayerRoleDefinition Def(
            string role,
            string zh,
            string geom,
            bool required,
            bool raster,
            string keywords)
        {
            LayerRoleDefinition d = new LayerRoleDefinition();
            d.Role = role;
            d.DisplayNameZh = zh;
            d.GeometryType = geom;
            d.Required = required;
            d.IsRaster = raster;
            d.DefaultKeywords = keywords;
            return d;
        }
    }
}

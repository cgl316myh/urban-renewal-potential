using System;
using System.Runtime.InteropServices;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;

namespace UrbanRenewal.GIS
{
    /// <summary>读取地图选中要素的潜力相关字段摘要。</summary>
    public static class ParcelSelectionHelper
    {
        public static bool TryDescribeSelection(object mapControl, out string text)
        {
            text = null;
            IMapControl3 map = mapControl as IMapControl3;
            if (map == null && mapControl != null)
            {
                System.Reflection.PropertyInfo prop = mapControl.GetType().GetProperty("Object");
                if (prop != null)
                {
                    map = prop.GetValue(mapControl, null) as IMapControl3;
                }
            }

            if (map == null || map.Map == null)
            {
                text = "地图控件未就绪。";
                return false;
            }

            IMap mapObj = map.Map;
            StringBuilder sb = new StringBuilder();
            int found = 0;
            string[] fields = new string[]
            {
                ParcelZonalLinker.FieldPotentialScore,
                ParcelZonalLinker.FieldMotivScore,
                ParcelZonalLinker.FieldFeasibScore,
                ParcelZonalLinker.FieldPotentialLevel
            };

            for (int i = 0; i < mapObj.LayerCount; i++)
            {
                IFeatureLayer fl = mapObj.get_Layer(i) as IFeatureLayer;
                if (fl == null || fl.FeatureClass == null)
                {
                    continue;
                }
                IFeatureSelection sel = fl as IFeatureSelection;
                if (sel == null || sel.SelectionSet == null || sel.SelectionSet.Count == 0)
                {
                    continue;
                }

                IEnumIDs ids = sel.SelectionSet.IDs;
                ids.Reset();
                int oid = ids.Next();
                while (oid != -1)
                {
                    IFeature feature = null;
                    try
                    {
                        feature = fl.FeatureClass.GetFeature(oid);
                        if (feature != null)
                        {
                            found++;
                            sb.AppendLine("图层: " + fl.Name + "  OID=" + oid);
                            for (int f = 0; f < fields.Length; f++)
                            {
                                AppendField(sb, feature, fields[f]);
                            }
                            sb.AppendLine();
                        }
                    }
                    finally
                    {
                        if (feature != null)
                        {
                            Marshal.FinalReleaseComObject(feature);
                        }
                    }
                    if (found >= 20)
                    {
                        sb.AppendLine("（仅显示前 20 条选中要素）");
                        break;
                    }
                    oid = ids.Next();
                }
                if (found >= 20)
                {
                    break;
                }
            }

            if (found == 0)
            {
                text = "当前地图没有选中要素。\r\n请先选中「宗地潜力」图层中的宗地，再查看详情。";
                return false;
            }
            text = sb.ToString();
            return true;
        }

        private static void AppendField(StringBuilder sb, IFeature feature, string fieldName)
        {
            int idx = feature.Fields.FindField(fieldName);
            if (idx < 0)
            {
                sb.AppendLine("  " + fieldName + ": （无此字段）");
                return;
            }
            object v = feature.get_Value(idx);
            sb.AppendLine("  " + fieldName + ": "
                + (v == null || v == DBNull.Value ? "(空)" : Convert.ToString(v)));
        }
    }
}

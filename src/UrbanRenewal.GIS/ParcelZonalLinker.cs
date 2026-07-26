using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.SpatialAnalystTools;
using UrbanRenewal.Model;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// 将潜力/动力/可行栅格通过 Zonal Statistics 写入宗地面字段。
    /// </summary>
    public static class ParcelZonalLinker
    {
        public const string FieldPotentialScore = "POTENTIAL_SCORE";
        public const string FieldMotivScore = "MOTIV_SCORE";
        public const string FieldFeasibScore = "FEASIB_SCORE";
        public const string FieldPotentialLevel = "POTENTIAL_LEVEL";
        public const string FieldZoneId = "ZONE_ID";

        public static string Link(
            GeoprocessorHelper gp,
            string parcelFeatureClass,
            string outputGdb,
            string outputFcName,
            string potentialRaster,
            string motivationRaster,
            string feasibilityRaster,
            string statisticType,
            IList<string> messages,
            out int parcelCount)
        {
            parcelCount = 0;
            if (gp == null || string.IsNullOrEmpty(parcelFeatureClass) || string.IsNullOrEmpty(outputGdb))
            {
                throw new ArgumentException("宗地关联输入无效。");
            }

            string outName = string.IsNullOrEmpty(outputFcName) ? "parcel_pot" : outputFcName;
            string outFc = OutputGdbHelper.DatasetPath(outputGdb, outName);
            OutputGdbHelper.TryDeleteDataset(gp, outFc);

            CopyFeatures copy = new CopyFeatures();
            copy.in_features = parcelFeatureClass;
            copy.out_feature_class = outFc;
            gp.Execute(copy, "CopyFeatures-ParcelLink");
            if (messages != null)
            {
                messages.Add("已复制宗地到输出库: " + outName);
            }

            EnsureZoneId(gp, outFc, messages);
            EnsureDoubleField(gp, outFc, FieldPotentialScore);
            EnsureDoubleField(gp, outFc, FieldMotivScore);
            EnsureDoubleField(gp, outFc, FieldFeasibScore);
            EnsureTextField(gp, outFc, FieldPotentialLevel, 20);

            string stats = string.IsNullOrEmpty(statisticType) ? "MEAN" : statisticType.ToUpperInvariant();
            if (stats != "MEAN" && stats != "MAXIMUM" && stats != "MAX")
            {
                stats = "MEAN";
            }
            if (stats == "MAX")
            {
                stats = "MAXIMUM";
            }

            Dictionary<int, double> potMap = RunZonalMeanMap(gp, outFc, potentialRaster, stats, outputGdb, "zpot", messages);
            Dictionary<int, double> motMap = string.IsNullOrEmpty(motivationRaster)
                ? new Dictionary<int, double>()
                : RunZonalMeanMap(gp, outFc, motivationRaster, stats, outputGdb, "zmot", messages);
            Dictionary<int, double> feaMap = string.IsNullOrEmpty(feasibilityRaster)
                ? new Dictionary<int, double>()
                : RunZonalMeanMap(gp, outFc, feasibilityRaster, stats, outputGdb, "zfea", messages);

            parcelCount = WriteScores(outFc, potMap, motMap, feaMap, messages);
            return outFc;
        }

        private static Dictionary<int, double> RunZonalMeanMap(
            GeoprocessorHelper gp,
            string zoneFc,
            string valueRaster,
            string statisticsType,
            string outputGdb,
            string tablePrefix,
            IList<string> messages)
        {
            Dictionary<int, double> map = new Dictionary<int, double>();
            if (string.IsNullOrEmpty(valueRaster))
            {
                return map;
            }

            string tablePath = OutputGdbHelper.DatasetPath(outputGdb, tablePrefix);
            OutputGdbHelper.TryDeleteDataset(gp, tablePath);

            try
            {
                ZonalStatisticsAsTable zonal = new ZonalStatisticsAsTable();
                zonal.in_zone_data = zoneFc;
                zonal.zone_field = FieldZoneId;
                zonal.in_value_raster = valueRaster;
                zonal.out_table = tablePath;
                zonal.statistics_type = statisticsType;
                zonal.ignore_nodata = "DATA";
                gp.Execute(zonal, "ZonalStatisticsAsTable-" + tablePrefix);
            }
            catch (Exception ex)
            {
                if (messages != null)
                {
                    messages.Add("区统计失败[" + tablePrefix + "]: " + ex.Message);
                }
                return map;
            }

            map = ReadZoneStatTable(tablePath, statisticsType);
            if (messages != null)
            {
                messages.Add("区统计完成[" + tablePrefix + "]: " + map.Count + " 个区");
            }
            return map;
        }

        private static Dictionary<int, double> ReadZoneStatTable(string tablePath, string statisticsType)
        {
            Dictionary<int, double> map = new Dictionary<int, double>();
            ITable table = null;
            ICursor cursor = null;
            try
            {
                table = OpenTable(tablePath);
                if (table == null)
                {
                    return map;
                }

                int zoneIdx = table.FindField(FieldZoneId);
                if (zoneIdx < 0)
                {
                    zoneIdx = table.FindField("ZONE_ID");
                }
                string valueField = ResolveStatField(table, statisticsType);
                int valueIdx = string.IsNullOrEmpty(valueField) ? -1 : table.FindField(valueField);
                if (zoneIdx < 0 || valueIdx < 0)
                {
                    return map;
                }

                cursor = table.Search(null, false);
                IRow row = cursor.NextRow();
                while (row != null)
                {
                    object zObj = row.get_Value(zoneIdx);
                    object vObj = row.get_Value(valueIdx);
                    if (zObj != null && zObj != DBNull.Value && vObj != null && vObj != DBNull.Value)
                    {
                        int z = Convert.ToInt32(zObj, CultureInfo.InvariantCulture);
                        double v = Convert.ToDouble(vObj, CultureInfo.InvariantCulture);
                        map[z] = v;
                    }
                    row = cursor.NextRow();
                }
            }
            catch
            {
            }
            finally
            {
                if (cursor != null)
                {
                    Marshal.FinalReleaseComObject(cursor);
                }
                if (table != null)
                {
                    Marshal.FinalReleaseComObject(table);
                }
            }
            return map;
        }

        private static string ResolveStatField(ITable table, string statisticsType)
        {
            string[] candidates;
            if (string.Equals(statisticsType, "MAXIMUM", StringComparison.OrdinalIgnoreCase))
            {
                candidates = new string[] { "MAX", "MAXIMUM" };
            }
            else
            {
                candidates = new string[] { "MEAN", "MEAN_" };
            }
            for (int i = 0; i < candidates.Length; i++)
            {
                if (table.FindField(candidates[i]) >= 0)
                {
                    return candidates[i];
                }
            }
            // 回退：扫描含 MEAN/MAX 的字段
            for (int i = 0; i < table.Fields.FieldCount; i++)
            {
                string n = table.Fields.get_Field(i).Name;
                if (n != null)
                {
                    string u = n.ToUpperInvariant();
                    if (u.Contains("MEAN") || u == "MAX" || u.Contains("MAXIMUM"))
                    {
                        return n;
                    }
                }
            }
            return null;
        }

        private static int WriteScores(
            string featureClassPath,
            Dictionary<int, double> potMap,
            Dictionary<int, double> motMap,
            Dictionary<int, double> feaMap,
            IList<string> messages)
        {
            int count = 0;
            IFeatureClass fc = null;
            IFeatureCursor cursor = null;
            try
            {
                fc = OpenFeatureClass(featureClassPath);
                if (fc == null)
                {
                    throw new InvalidOperationException("无法打开输出宗地: " + featureClassPath);
                }

                int zoneIdx = fc.FindField(FieldZoneId);
                int potIdx = fc.FindField(FieldPotentialScore);
                int motIdx = fc.FindField(FieldMotivScore);
                int feaIdx = fc.FindField(FieldFeasibScore);
                int lvlIdx = fc.FindField(FieldPotentialLevel);
                if (zoneIdx < 0 || potIdx < 0 || motIdx < 0 || feaIdx < 0 || lvlIdx < 0)
                {
                    throw new InvalidOperationException("宗地得分字段缺失。");
                }

                cursor = fc.Update(null, false);
                IFeature feature = cursor.NextFeature();
                while (feature != null)
                {
                    int zone = Convert.ToInt32(feature.get_Value(zoneIdx), CultureInfo.InvariantCulture);
                    double pot = 0;
                    double mot = 0;
                    double fea = 0;
                    potMap.TryGetValue(zone, out pot);
                    motMap.TryGetValue(zone, out mot);
                    feaMap.TryGetValue(zone, out fea);

                    feature.set_Value(potIdx, pot);
                    feature.set_Value(motIdx, mot);
                    feature.set_Value(feaIdx, fea);
                    feature.set_Value(lvlIdx, PotentialLevel.ToName(pot));
                    cursor.UpdateFeature(feature);
                    count++;
                    feature = cursor.NextFeature();
                }
                cursor.Flush();
            }
            finally
            {
                if (cursor != null)
                {
                    Marshal.FinalReleaseComObject(cursor);
                }
                if (fc != null)
                {
                    Marshal.FinalReleaseComObject(fc);
                }
            }

            if (messages != null)
            {
                messages.Add("已写入宗地得分字段: " + count + " 条（"
                    + FieldPotentialScore + "/" + FieldMotivScore + "/"
                    + FieldFeasibScore + "/" + FieldPotentialLevel + "）");
            }
            return count;
        }

        private static void EnsureZoneId(GeoprocessorHelper gp, string featureClass, IList<string> messages)
        {
            EnsureLongField(gp, featureClass, FieldZoneId);

            IFeatureClass fc = null;
            IFeatureCursor cursor = null;
            try
            {
                fc = OpenFeatureClass(featureClass);
                int zoneIdx = fc.FindField(FieldZoneId);
                int oidIdx = fc.FindField(fc.OIDFieldName);
                if (zoneIdx < 0 || oidIdx < 0)
                {
                    throw new InvalidOperationException("无法写入 ZONE_ID。");
                }
                cursor = fc.Update(null, false);
                IFeature feature = cursor.NextFeature();
                while (feature != null)
                {
                    feature.set_Value(zoneIdx, feature.OID);
                    cursor.UpdateFeature(feature);
                    feature = cursor.NextFeature();
                }
                cursor.Flush();
            }
            finally
            {
                if (cursor != null)
                {
                    Marshal.FinalReleaseComObject(cursor);
                }
                if (fc != null)
                {
                    Marshal.FinalReleaseComObject(fc);
                }
            }
            if (messages != null)
            {
                messages.Add("已生成分区字段 " + FieldZoneId + "（=OBJECTID）。");
            }
        }

        private static void EnsureDoubleField(GeoprocessorHelper gp, string table, string fieldName)
        {
            try
            {
                AddField add = new AddField();
                add.in_table = table;
                add.field_name = fieldName;
                add.field_type = "DOUBLE";
                gp.Execute(add, "AddField-" + fieldName);
            }
            catch
            {
            }
        }

        private static void EnsureLongField(GeoprocessorHelper gp, string table, string fieldName)
        {
            try
            {
                AddField add = new AddField();
                add.in_table = table;
                add.field_name = fieldName;
                add.field_type = "LONG";
                gp.Execute(add, "AddField-" + fieldName);
            }
            catch
            {
            }
        }

        private static void EnsureTextField(GeoprocessorHelper gp, string table, string fieldName, int length)
        {
            try
            {
                AddField add = new AddField();
                add.in_table = table;
                add.field_name = fieldName;
                add.field_type = "TEXT";
                add.field_length = length;
                gp.Execute(add, "AddField-" + fieldName);
            }
            catch
            {
            }
        }

        private static IFeatureClass OpenFeatureClass(string path)
        {
            string gdb = System.IO.Path.GetDirectoryName(path);
            string name = System.IO.Path.GetFileName(path);
            if (string.IsNullOrEmpty(gdb) || string.IsNullOrEmpty(name))
            {
                return null;
            }
            IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
            IFeatureWorkspace fw = factory.OpenFromFile(gdb, 0) as IFeatureWorkspace;
            if (fw == null)
            {
                return null;
            }
            try
            {
                return fw.OpenFeatureClass(name);
            }
            catch
            {
                return null;
            }
        }

        private static ITable OpenTable(string path)
        {
            string gdb = System.IO.Path.GetDirectoryName(path);
            string name = System.IO.Path.GetFileName(path);
            if (string.IsNullOrEmpty(gdb) || string.IsNullOrEmpty(name))
            {
                return null;
            }
            IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
            IFeatureWorkspace fw = factory.OpenFromFile(gdb, 0) as IFeatureWorkspace;
            if (fw == null)
            {
                return null;
            }
            try
            {
                return fw.OpenTable(name);
            }
            catch
            {
                return null;
            }
        }
    }
}

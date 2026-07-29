using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ConversionTools;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// 宗地斑块形状指数 SI / 破碎度 FS→PD 得分，并转栅格。
    /// </summary>
    public static class ParcelAnalyzer
    {
        private static readonly string[] LandcodeCandidates = new string[]
        {
            "landcode", "LANDCODE", "LandCode", "DLBM", "dlbm", "地类编码", "YDYHFLDM", "ydyhfldm"
        };

        /// <summary>
        /// 计算 SI 并映射得分：SI≤1.3 → 0；SI≥3.0 → -1；中间约半阈值起扣。面转栅格。
        /// </summary>
        public static string BuildSiScoreRaster(
            GeoprocessorHelper gp,
            string inFeatureClass,
            string outputGdb,
            string namePrefix,
            double cellSize,
            IList<string> messages)
        {
            if (gp == null || string.IsNullOrEmpty(inFeatureClass))
            {
                throw new ArgumentException("宗地要素类无效。");
            }

            string workFc = OutputGdbHelper.DatasetPath(outputGdb, ShortName(namePrefix) + "_si");
            OutputGdbHelper.TryDeleteDataset(gp, workFc);

            if (messages != null)
            {
                messages.Add("SI：复制宗地并计算形状指数（斑块逐条，可能较久）...");
            }
            CopyFeatures copy = new CopyFeatures();
            copy.in_features = inFeatureClass;
            copy.out_feature_class = workFc;
            gp.Execute(copy, "CopyFeatures-SI-" + namePrefix);

            EnsureDoubleField(gp, workFc, "SI");
            EnsureShortField(gp, workFc, "SCORE");

            int count = 0;
            double sumSi = 0;
            IFeatureClass fc = null;
            IFeatureCursor cursor = null;
            try
            {
                fc = OpenFeatureClass(workFc);
                if (fc == null)
                {
                    throw new InvalidOperationException("无法打开宗地要素类: " + workFc);
                }

                int siIdx = fc.FindField("SI");
                int scoreIdx = fc.FindField("SCORE");
                if (siIdx < 0 || scoreIdx < 0)
                {
                    throw new InvalidOperationException("SI/SCORE 字段创建失败。");
                }

                cursor = fc.Update(null, false);
                IFeature feature = cursor.NextFeature();
                while (feature != null)
                {
                    double si = ComputeShapeIndex(feature.Shape);
                    int score = MapSiToScore(si);
                    feature.set_Value(siIdx, si);
                    feature.set_Value(scoreIdx, score);
                    cursor.UpdateFeature(feature);
                    count++;
                    sumSi += si;
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
                messages.Add("SI 计算完成: 斑块数=" + count
                    + (count > 0 ? ("，平均 SI=" + (sumSi / count).ToString("0.000", CultureInfo.InvariantCulture)) : string.Empty));
                messages.Add("SI：面转栅格...");
            }

            string raster = OutputGdbHelper.DatasetPath(outputGdb, ShortName(namePrefix));
            OutputGdbHelper.TryDeleteDataset(gp, raster);
            FeatureToRasterScore(gp, workFc, raster, cellSize);
            return raster;
        }

        /// <summary>
        /// 按地类（landcode）汇总 ASI/FS，映射为 PD 破碎度得分（0～-2），写回斑块后转栅格。
        /// 无地类字段时按全局一套 FS。
        /// </summary>
        public static string BuildFragmentationScoreRaster(
            GeoprocessorHelper gp,
            string inFeatureClass,
            string outputGdb,
            string namePrefix,
            double cellSize,
            IList<string> messages)
        {
            if (gp == null || string.IsNullOrEmpty(inFeatureClass))
            {
                throw new ArgumentException("宗地要素类无效。");
            }

            string workFc = OutputGdbHelper.DatasetPath(outputGdb, ShortName(namePrefix) + "_pd");
            OutputGdbHelper.TryDeleteDataset(gp, workFc);

            string landcodeField = DetectLandcodeField(inFeatureClass);
            string sourceForMetrics = inFeatureClass;
            if (!string.IsNullOrEmpty(landcodeField))
            {
                if (messages != null)
                {
                    messages.Add("PD：按 " + landcodeField + " Dissolve（SINGLE_PART）...");
                }
                string dissolved = OutputGdbHelper.DatasetPath(outputGdb, ShortName(namePrefix) + "_d");
                OutputGdbHelper.TryDeleteDataset(gp, dissolved);
                try
                {
                    Dissolve dissolve = new Dissolve();
                    dissolve.in_features = inFeatureClass;
                    dissolve.out_feature_class = dissolved;
                    dissolve.dissolve_field = landcodeField;
                    dissolve.multi_part = "SINGLE_PART";
                    gp.Execute(dissolve, "Dissolve-Parcel-" + namePrefix);
                    sourceForMetrics = dissolved;
                    if (messages != null)
                    {
                        messages.Add("宗地按 " + landcodeField + " Dissolve（SINGLE_PART）完成。");
                    }
                }
                catch (Exception ex)
                {
                    if (messages != null)
                    {
                        messages.Add("Dissolve 跳过（使用原斑块）: " + ex.Message);
                    }
                    sourceForMetrics = inFeatureClass;
                }
            }
            else if (messages != null)
            {
                messages.Add("未检测到 landcode 字段，破碎度按全局斑块统计。");
            }

            if (messages != null)
            {
                messages.Add("PD：复制斑块并计算破碎度得分...");
            }
            CopyFeatures copy = new CopyFeatures();
            copy.in_features = sourceForMetrics;
            copy.out_feature_class = workFc;
            gp.Execute(copy, "CopyFeatures-PD-" + namePrefix);

            EnsureDoubleField(gp, workFc, "SI");
            EnsureDoubleField(gp, workFc, "FS");
            EnsureShortField(gp, workFc, "SCORE");

            string workLandcode = DetectLandcodeField(workFc);
            Dictionary<string, ClassAccum> groups = new Dictionary<string, ClassAccum>(StringComparer.OrdinalIgnoreCase);

            IFeatureClass fcScan = null;
            IFeatureCursor curScan = null;
            try
            {
                fcScan = OpenFeatureClass(workFc);
                if (fcScan == null)
                {
                    throw new InvalidOperationException("无法打开宗地要素类: " + workFc);
                }

                int landIdx = string.IsNullOrEmpty(workLandcode) ? -1 : fcScan.FindField(workLandcode);
                curScan = fcScan.Search(null, false);
                IFeature feature = curScan.NextFeature();
                while (feature != null)
                {
                    double area = GetArea(feature.Shape);
                    double si = ComputeShapeIndex(feature.Shape);
                    string key = ResolveClassKey(feature, landIdx);
                    ClassAccum acc;
                    if (!groups.TryGetValue(key, out acc))
                    {
                        acc = new ClassAccum();
                        groups[key] = acc;
                    }
                    acc.Count++;
                    acc.TotalArea += area;
                    acc.WeightedSi += si * area;
                    feature = curScan.NextFeature();
                }
            }
            finally
            {
                if (curScan != null)
                {
                    Marshal.FinalReleaseComObject(curScan);
                }
                if (fcScan != null)
                {
                    Marshal.FinalReleaseComObject(fcScan);
                }
            }

            Dictionary<string, double> fsByClass = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, int> scoreByClass = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, ClassAccum> kv in groups)
            {
                ClassAccum acc = kv.Value;
                double asi = acc.TotalArea > 0 ? acc.WeightedSi / acc.TotalArea : 1.0;
                if (asi < 1.0)
                {
                    asi = 1.0;
                }
                double fs = 1.0 - (1.0 / asi);
                int score = MapFsToScore(fs);
                fsByClass[kv.Key] = fs;
                scoreByClass[kv.Key] = score;
                if (messages != null)
                {
                    double taHa = acc.TotalArea / 10000.0;
                    double pd = taHa > 0 ? acc.Count / taHa : 0;
                    messages.Add("地类[" + kv.Key + "] NP=" + acc.Count
                        + " TA(ha)=" + taHa.ToString("0.###", CultureInfo.InvariantCulture)
                        + " PD=" + pd.ToString("0.####", CultureInfo.InvariantCulture)
                        + " ASI=" + asi.ToString("0.000", CultureInfo.InvariantCulture)
                        + " FS=" + fs.ToString("0.000", CultureInfo.InvariantCulture)
                        + " → SCORE=" + score);
                }
            }

            IFeatureClass fcUpd = null;
            IFeatureCursor curUpd = null;
            try
            {
                fcUpd = OpenFeatureClass(workFc);
                int siIdx = fcUpd.FindField("SI");
                int fsIdx = fcUpd.FindField("FS");
                int scoreIdx = fcUpd.FindField("SCORE");
                int landIdx = string.IsNullOrEmpty(workLandcode) ? -1 : fcUpd.FindField(workLandcode);

                curUpd = fcUpd.Update(null, false);
                IFeature feature = curUpd.NextFeature();
                while (feature != null)
                {
                    double si = ComputeShapeIndex(feature.Shape);
                    string key = ResolveClassKey(feature, landIdx);
                    double fs = 0;
                    int score = 0;
                    fsByClass.TryGetValue(key, out fs);
                    scoreByClass.TryGetValue(key, out score);

                    feature.set_Value(siIdx, si);
                    feature.set_Value(fsIdx, fs);
                    feature.set_Value(scoreIdx, score);
                    curUpd.UpdateFeature(feature);
                    feature = curUpd.NextFeature();
                }
                curUpd.Flush();
            }
            finally
            {
                if (curUpd != null)
                {
                    Marshal.FinalReleaseComObject(curUpd);
                }
                if (fcUpd != null)
                {
                    Marshal.FinalReleaseComObject(fcUpd);
                }
            }

            string raster = OutputGdbHelper.DatasetPath(outputGdb, ShortName(namePrefix));
            OutputGdbHelper.TryDeleteDataset(gp, raster);
            if (messages != null)
            {
                messages.Add("PD：面转栅格...");
            }
            FeatureToRasterScore(gp, workFc, raster, cellSize);
            return raster;
        }

        public static string DetectLandcodeField(string featureClassPath)
        {
            IFeatureClass fc = null;
            try
            {
                fc = OpenFeatureClass(featureClassPath);
                if (fc == null)
                {
                    return null;
                }
                for (int i = 0; i < LandcodeCandidates.Length; i++)
                {
                    if (fc.FindField(LandcodeCandidates[i]) >= 0)
                    {
                        return LandcodeCandidates[i];
                    }
                }
            }
            catch
            {
            }
            finally
            {
                if (fc != null)
                {
                    Marshal.FinalReleaseComObject(fc);
                }
            }
            return null;
        }

        public static double ComputeShapeIndex(IGeometry shape)
        {
            double area = GetArea(shape);
            double peri = GetPerimeter(shape);
            if (area <= 0 || peri <= 0)
            {
                return 0;
            }
            return peri / (2.0 * Math.Sqrt(Math.PI * area));
        }

        /// <summary>SI 1.0–1.3 不扣分；&gt;3.0 扣 -1；中间过半则扣。</summary>
        public static int MapSiToScore(double si)
        {
            if (si <= 1.3)
            {
                return 0;
            }
            if (si >= 3.0)
            {
                return -1;
            }
            double t = (si - 1.3) / (3.0 - 1.3);
            return t >= 0.5 ? -1 : 0;
        }

        /// <summary>FS≈0 不扣；FS≥0.5 → -2；中间分档。</summary>
        public static int MapFsToScore(double fs)
        {
            if (fs <= 0.2)
            {
                return 0;
            }
            if (fs >= 0.5)
            {
                return -2;
            }
            double t = (fs - 0.2) / (0.5 - 0.2);
            if (t < 0.33)
            {
                return 0;
            }
            if (t < 0.66)
            {
                return -1;
            }
            return -2;
        }

        private static string ResolveClassKey(IFeature feature, int landIdx)
        {
            if (landIdx < 0 || feature == null)
            {
                return "_ALL_";
            }
            object v = feature.get_Value(landIdx);
            if (v == null || v == DBNull.Value)
            {
                return "_ALL_";
            }
            string s = Convert.ToString(v, CultureInfo.InvariantCulture);
            return string.IsNullOrEmpty(s) ? "_ALL_" : s;
        }

        private static double GetArea(IGeometry shape)
        {
            IArea area = shape as IArea;
            if (area == null)
            {
                return 0;
            }
            return Math.Abs(area.Area);
        }

        private static double GetPerimeter(IGeometry shape)
        {
            ICurve curve = shape as ICurve;
            if (curve == null)
            {
                return 0;
            }
            return Math.Abs(curve.Length);
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

        private static void EnsureShortField(GeoprocessorHelper gp, string table, string fieldName)
        {
            try
            {
                AddField add = new AddField();
                add.in_table = table;
                add.field_name = fieldName;
                add.field_type = "SHORT";
                gp.Execute(add, "AddField-" + fieldName);
            }
            catch
            {
            }
        }

        private static void FeatureToRasterScore(
            GeoprocessorHelper gp,
            string inFeatures,
            string outRaster,
            double cellSize)
        {
            FeatureToRaster f2r = new FeatureToRaster();
            f2r.in_features = inFeatures;
            f2r.field = "SCORE";
            f2r.out_raster = outRaster;
            f2r.cell_size = cellSize.ToString(CultureInfo.InvariantCulture);
            gp.Execute(f2r, "FeatureToRaster-ParcelScore");
        }

        private static IFeatureClass OpenFeatureClass(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
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

        private static int _nameSeq;

        private static string ShortName(string namePrefix)
        {
            _nameSeq++;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(namePrefix))
            {
                for (int i = 0; i < namePrefix.Length && sb.Length < 4; i++)
                {
                    char c = namePrefix[i];
                    if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                    {
                        sb.Append(char.ToLowerInvariant(c));
                    }
                }
            }
            if (sb.Length == 0)
            {
                sb.Append('p');
            }
            if (sb[0] >= '0' && sb[0] <= '9')
            {
                sb.Insert(0, 'p');
            }
            sb.Append(_nameSeq.ToString("00"));
            return sb.ToString();
        }

        private sealed class ClassAccum
        {
            public int Count;
            public double TotalArea;
            public double WeightedSi;
        }
    }
}

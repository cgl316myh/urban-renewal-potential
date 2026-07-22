using System;
using System.Collections.Generic;
using System.IO;
using ESRI.ArcGIS.AnalysisTools;
using ESRI.ArcGIS.ConversionTools;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.SpatialAnalystTools;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// 缓冲区赋分 → 转栅格 → MAX 合并。
    /// </summary>
    public static class BufferScoreRasterBuilder
    {
        /// <summary>
        /// 单环缓冲固定得分。
        /// </summary>
        public static string BuildSingle(
            GeoprocessorHelper gp,
            string inFeatureClass,
            double distanceMeters,
            int score,
            string workDir,
            string namePrefix,
            double cellSize)
        {
            string buf = Path.Combine(workDir, namePrefix + "_buf.shp");
            string raster = Path.Combine(workDir, namePrefix + "_r");

            ESRI.ArcGIS.AnalysisTools.Buffer buffer = new ESRI.ArcGIS.AnalysisTools.Buffer();
            buffer.in_features = inFeatureClass;
            buffer.out_feature_class = buf;
            buffer.buffer_distance_or_field = distanceMeters.ToString(System.Globalization.CultureInfo.InvariantCulture) + " Meters";
            buffer.dissolve_option = "ALL";
            gp.Execute(buffer, "Buffer-" + namePrefix);

            EnsureScoreField(gp, buf, score);
            FeatureToRaster(gp, buf, raster, cellSize);
            return raster;
        }

        /// <summary>
        /// 多环缓冲，按距离赋分后取 MAX。
        /// </summary>
        public static string BuildMultiRingMax(
            GeoprocessorHelper gp,
            string inFeatureClass,
            double[] distancesMeters,
            int[] scores,
            string workDir,
            string namePrefix,
            double cellSize)
        {
            if (distancesMeters == null || scores == null || distancesMeters.Length != scores.Length || distancesMeters.Length == 0)
            {
                throw new ArgumentException("距离与得分数组无效。");
            }

            List<string> rasters = new List<string>();
            for (int i = 0; i < distancesMeters.Length; i++)
            {
                string r = BuildSingle(gp, inFeatureClass, distancesMeters[i], scores[i], workDir, namePrefix + "_r" + i, cellSize);
                rasters.Add(r);
            }

            if (rasters.Count == 1)
            {
                return rasters[0];
            }

            string outRaster = Path.Combine(workDir, namePrefix + "_max");
            CellStatistics stats = new CellStatistics();
            stats.in_rasters_or_constants = string.Join(";", rasters.ToArray());
            stats.out_raster = outRaster;
            stats.statistics_type = "MAXIMUM";
            stats.ignore_nodata = "DATA";
            gp.Execute(stats, "CellStatistics-MAX-" + namePrefix);
            return outRaster;
        }

        /// <summary>
        /// 面要素直接赋固定分后转栅格（政策区等）。
        /// </summary>
        public static string BuildPolygonScore(
            GeoprocessorHelper gp,
            string inFeatureClass,
            int score,
            string workDir,
            string namePrefix,
            double cellSize)
        {
            string scored = Path.Combine(workDir, namePrefix + "_scored.shp");
            CopyFeatures copy = new CopyFeatures();
            copy.in_features = inFeatureClass;
            copy.out_feature_class = scored;
            gp.Execute(copy, "CopyFeatures-" + namePrefix);

            EnsureScoreField(gp, scored, score);
            string raster = Path.Combine(workDir, namePrefix + "_r");
            FeatureToRaster(gp, scored, raster, cellSize);
            return raster;
        }

        public static string MaxCombine(GeoprocessorHelper gp, IList<string> rasters, string workDir, string namePrefix)
        {
            if (rasters == null || rasters.Count == 0)
            {
                return null;
            }
            if (rasters.Count == 1)
            {
                return rasters[0];
            }

            string outRaster = Path.Combine(workDir, namePrefix + "_max");
            CellStatistics stats = new CellStatistics();
            stats.in_rasters_or_constants = string.Join(";", ToArray(rasters));
            stats.out_raster = outRaster;
            stats.statistics_type = "MAXIMUM";
            stats.ignore_nodata = "DATA";
            gp.Execute(stats, "CellStatistics-MAX-" + namePrefix);
            return outRaster;
        }

        public static string WeightedSum(
            GeoprocessorHelper gp,
            IList<string> rasters,
            IList<double> weights,
            string outRaster)
        {
            if (rasters == null || weights == null || rasters.Count == 0 || rasters.Count != weights.Count)
            {
                throw new ArgumentException("加权叠置输入无效。");
            }

            // 归一化权重
            double sumW = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                sumW += weights[i];
            }
            if (sumW <= 0)
            {
                throw new ArgumentException("权重之和必须大于 0。");
            }

            System.Text.StringBuilder expr = new System.Text.StringBuilder();
            for (int i = 0; i < rasters.Count; i++)
            {
                double w = weights[i] / sumW;
                if (i > 0)
                {
                    expr.Append(" + ");
                }
                expr.Append("(\"");
                expr.Append(rasters[i]);
                expr.Append("\" * ");
                expr.Append(w.ToString(System.Globalization.CultureInfo.InvariantCulture));
                expr.Append(")");
            }

            // Con 处理 NoData：用 Float(IsNull()) 较复杂；此处要求各准则已对齐，缺侧已跳过
            RasterCalculator calc = new RasterCalculator();
            calc.expression = expr.ToString();
            calc.output_raster = outRaster;
            gp.Execute(calc, "RasterCalculator-WeightedSum");
            return outRaster;
        }

        private static void EnsureScoreField(GeoprocessorHelper gp, string featureClass, int score)
        {
            try
            {
                AddField add = new AddField();
                add.in_table = featureClass;
                add.field_name = "SCORE";
                add.field_type = "SHORT";
                gp.Execute(add, "AddField-SCORE");
            }
            catch
            {
                // 字段可能已存在
            }

            CalculateField calc = new CalculateField();
            calc.in_table = featureClass;
            calc.field = "SCORE";
            calc.expression = score.ToString();
            calc.expression_type = "VB";
            gp.Execute(calc, "CalculateField-SCORE");
        }

        private static void FeatureToRaster(GeoprocessorHelper gp, string inFeatures, string outRaster, double cellSize)
        {
            FeatureToRaster f2r = new FeatureToRaster();
            f2r.in_features = inFeatures;
            f2r.field = "SCORE";
            f2r.out_raster = outRaster;
            f2r.cell_size = cellSize.ToString(System.Globalization.CultureInfo.InvariantCulture);
            gp.Execute(f2r, "FeatureToRaster");
        }

        private static string[] ToArray(IList<string> list)
        {
            string[] arr = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                arr[i] = list[i];
            }
            return arr;
        }
    }
}

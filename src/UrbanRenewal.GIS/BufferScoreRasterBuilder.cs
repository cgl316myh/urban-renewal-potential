using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ESRI.ArcGIS.AnalysisTools;
using ESRI.ArcGIS.ConversionTools;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.SpatialAnalystTools;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// 缓冲区赋分 → 转栅格 → MAX 合并（中间/结果均写入输出 File GDB）。
    /// </summary>
    public static class BufferScoreRasterBuilder
    {
        private static int _nameSeq;

        public static void ResetNameSequence()
        {
            _nameSeq = 0;
        }

        /// <summary>
        /// 单环缓冲固定得分。
        /// </summary>
        public static string BuildSingle(
            GeoprocessorHelper gp,
            string inFeatureClass,
            double distanceMeters,
            int score,
            string outputGdb,
            string namePrefix,
            double cellSize)
        {
            string shortName = ShortName(namePrefix);
            string buf = OutputGdbHelper.DatasetPath(outputGdb, shortName + "_b");
            string raster = OutputGdbHelper.DatasetPath(outputGdb, shortName);

            ESRI.ArcGIS.AnalysisTools.Buffer buffer = new ESRI.ArcGIS.AnalysisTools.Buffer();
            buffer.in_features = inFeatureClass;
            buffer.out_feature_class = buf;
            buffer.buffer_distance_or_field = distanceMeters.ToString(CultureInfo.InvariantCulture) + " Meters";
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
            string outputGdb,
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
                string r = BuildSingle(gp, inFeatureClass, distancesMeters[i], scores[i], outputGdb, namePrefix + i.ToString(), cellSize);
                rasters.Add(r);
            }

            if (rasters.Count == 1)
            {
                return rasters[0];
            }

            string outRaster = OutputGdbHelper.DatasetPath(outputGdb, ShortName(namePrefix) + "mx");
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
            string outputGdb,
            string namePrefix,
            double cellSize)
        {
            string shortName = ShortName(namePrefix);
            string scored = OutputGdbHelper.DatasetPath(outputGdb, shortName + "_s");
            CopyFeatures copy = new CopyFeatures();
            copy.in_features = inFeatureClass;
            copy.out_feature_class = scored;
            gp.Execute(copy, "CopyFeatures-" + namePrefix);

            EnsureScoreField(gp, scored, score);
            string raster = OutputGdbHelper.DatasetPath(outputGdb, shortName);
            FeatureToRaster(gp, scored, raster, cellSize);
            return raster;
        }

        public static string MaxCombine(GeoprocessorHelper gp, IList<string> rasters, string outputGdb, string namePrefix)
        {
            if (rasters == null || rasters.Count == 0)
            {
                return null;
            }
            if (rasters.Count == 1)
            {
                return rasters[0];
            }

            string outRaster = OutputGdbHelper.DatasetPath(outputGdb, ShortName(namePrefix) + "mx");
            CellStatistics stats = new CellStatistics();
            stats.in_rasters_or_constants = string.Join(";", ToArray(rasters));
            stats.out_raster = outRaster;
            stats.statistics_type = "MAXIMUM";
            stats.ignore_nodata = "DATA";
            gp.Execute(stats, "CellStatistics-MAX-" + namePrefix);
            return outRaster;
        }

        /// <summary>
        /// 将准则层原始得分缩放到 0–100：score/theoreticalMax×100；NoData 视为 0。
        /// </summary>
        public static string NormalizeTo100(
            GeoprocessorHelper gp,
            string inRaster,
            double theoreticalMax,
            string outputGdb,
            string namePrefix)
        {
            if (gp == null || string.IsNullOrEmpty(inRaster))
            {
                throw new ArgumentException("标准化输入栅格无效。");
            }
            if (theoreticalMax <= 0)
            {
                throw new ArgumentException("理论满分必须大于 0。");
            }

            string outRaster = OutputGdbHelper.DatasetPath(outputGdb, ShortName(namePrefix) + "n");
            OutputGdbHelper.TryDeleteDataset(gp, outRaster);

            // Con(IsNull(r),0,r) / max * 100
            string expr = "(Float(Con(IsNull(\"" + inRaster + "\"),0,\"" + inRaster
                + "\")) / " + theoreticalMax.ToString(CultureInfo.InvariantCulture) + ") * 100";

            RasterCalculator calc = new RasterCalculator();
            calc.expression = expr;
            calc.output_raster = outRaster;
            gp.Execute(calc, "RasterCalculator-Normalize100-" + namePrefix);
            return outRaster;
        }

        /// <summary>
        /// 对已标准化为 0–100 的准则栅格做加权求和（权重自动归一化），输出约 0–100。
        /// </summary>
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
                // 再保险：加权前空值填 0，避免某一准则 NoData 污染整幅
                expr.Append("(Con(IsNull(\"");
                expr.Append(rasters[i]);
                expr.Append("\"),0,\"");
                expr.Append(rasters[i]);
                expr.Append("\") * ");
                expr.Append(w.ToString(CultureInfo.InvariantCulture));
                expr.Append(")");
            }

            OutputGdbHelper.TryDeleteDataset(gp, outRaster);
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
            f2r.cell_size = cellSize.ToString(CultureInfo.InvariantCulture);
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
                sb.Append('r');
            }
            // File GDB 栅格名不能以数字开头
            if (sb[0] >= '0' && sb[0] <= '9')
            {
                sb.Insert(0, 'r');
            }
            sb.Append(_nameSeq.ToString("00"));
            return sb.ToString();
        }
    }
}

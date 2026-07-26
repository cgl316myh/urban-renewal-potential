using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.SpatialAnalystTools;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// 可行度相关栅格工具：坡度、阈值重分类、人口分级、加性叠置与区间标准化。
    /// </summary>
    public static class FeasibilityRasterBuilder
    {
        /// <summary>由 DEM 生成坡度栅格（度）。</summary>
        public static string BuildSlope(
            GeoprocessorHelper gp,
            string demRaster,
            string outputGdb,
            string namePrefix)
        {
            if (gp == null || string.IsNullOrEmpty(demRaster))
            {
                throw new ArgumentException("DEM 栅格无效。");
            }

            string outSlope = OutputGdbHelper.DatasetPath(outputGdb, ShortName(namePrefix));
            OutputGdbHelper.TryDeleteDataset(gp, outSlope);

            Slope slope = new Slope();
            slope.in_raster = demRaster;
            slope.out_raster = outSlope;
            slope.output_measurement = "DEGREE";
            gp.Execute(slope, "Slope-" + namePrefix);
            return outSlope;
        }

        /// <summary>
        /// 阈值重分类：值 &gt; threshold → aboveScore，否则 → belowScore。
        /// </summary>
        public static string ReclassifyAboveThreshold(
            GeoprocessorHelper gp,
            string inRaster,
            double threshold,
            int aboveScore,
            int belowScore,
            string outputGdb,
            string namePrefix)
        {
            if (gp == null || string.IsNullOrEmpty(inRaster))
            {
                throw new ArgumentException("重分类输入栅格无效。");
            }

            string outRaster = OutputGdbHelper.DatasetPath(outputGdb, ShortName(namePrefix));
            OutputGdbHelper.TryDeleteDataset(gp, outRaster);

            // Remap: (-inf, thr] → below; (thr, +inf) → above
            string remap = string.Format(
                CultureInfo.InvariantCulture,
                "-100000000 {0} {1};{0} 100000000 {2}",
                threshold,
                belowScore,
                aboveScore);

            Reclassify reclass = new Reclassify();
            reclass.in_raster = inRaster;
            reclass.reclass_field = "VALUE";
            reclass.remap = remap;
            reclass.out_raster = outRaster;
            reclass.missing_values = "DATA";
            gp.Execute(reclass, "Reclassify-" + namePrefix);
            return outRaster;
        }

        /// <summary>
        /// 按区间重分类（breaks 升序，scores.Length = breaks.Length + 1）。
        /// 区间：(-inf,b0]、 (b0,b1]、…、(bLast,+inf)。
        /// </summary>
        public static string ReclassifyByBreaks(
            GeoprocessorHelper gp,
            string inRaster,
            double[] breaks,
            int[] scores,
            string outputGdb,
            string namePrefix)
        {
            if (gp == null || string.IsNullOrEmpty(inRaster)
                || breaks == null || scores == null
                || scores.Length != breaks.Length + 1)
            {
                throw new ArgumentException("分级重分类参数无效。");
            }

            string outRaster = OutputGdbHelper.DatasetPath(outputGdb, ShortName(namePrefix));
            OutputGdbHelper.TryDeleteDataset(gp, outRaster);

            StringBuilder sb = new StringBuilder();
            double prev = -100000000;
            for (int i = 0; i < breaks.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(";");
                }
                sb.Append(prev.ToString(CultureInfo.InvariantCulture));
                sb.Append(" ");
                sb.Append(breaks[i].ToString(CultureInfo.InvariantCulture));
                sb.Append(" ");
                sb.Append(scores[i].ToString(CultureInfo.InvariantCulture));
                prev = breaks[i];
            }
            sb.Append(";");
            sb.Append(prev.ToString(CultureInfo.InvariantCulture));
            sb.Append(" 100000000 ");
            sb.Append(scores[scores.Length - 1].ToString(CultureInfo.InvariantCulture));

            Reclassify reclass = new Reclassify();
            reclass.in_raster = inRaster;
            reclass.reclass_field = "VALUE";
            reclass.remap = sb.ToString();
            reclass.out_raster = outRaster;
            reclass.missing_values = "DATA";
            gp.Execute(reclass, "ReclassifyBreaks-" + namePrefix);
            return outRaster;
        }

        /// <summary>
        /// 人口密度五级等分：最高级 +2，次高 +1，其余 0。
        /// 若无法读取统计则按相对高值区（假定值域）回退。
        /// </summary>
        public static string BuildPopulationScore(
            GeoprocessorHelper gp,
            string populationRaster,
            string outputGdb,
            string namePrefix,
            double minValue,
            double maxValue)
        {
            if (maxValue <= minValue)
            {
                // 无法分级时：有值区域给 +1
                return ReclassifyAboveThreshold(gp, populationRaster, minValue, 1, 0, outputGdb, namePrefix);
            }

            double span = maxValue - minValue;
            double b1 = minValue + span * 0.2;
            double b2 = minValue + span * 0.4;
            double b3 = minValue + span * 0.6;
            double b4 = minValue + span * 0.8;
            // 低→高：0,0,1,1,2
            return ReclassifyByBreaks(
                gp,
                populationRaster,
                new double[] { b1, b2, b3, b4 },
                new int[] { 0, 0, 1, 1, 2 },
                outputGdb,
                namePrefix);
        }

        /// <summary>多栅格加性叠置（NoData→0），输出原始可行度得分。</summary>
        public static string SumCombine(
            GeoprocessorHelper gp,
            IList<string> rasters,
            string outRaster)
        {
            if (gp == null || rasters == null || rasters.Count == 0 || string.IsNullOrEmpty(outRaster))
            {
                throw new ArgumentException("加性叠置输入无效。");
            }

            StringBuilder expr = new StringBuilder();
            for (int i = 0; i < rasters.Count; i++)
            {
                if (i > 0)
                {
                    expr.Append(" + ");
                }
                expr.Append("Con(IsNull(\"");
                expr.Append(rasters[i]);
                expr.Append("\"),0,\"");
                expr.Append(rasters[i]);
                expr.Append("\")");
            }

            OutputGdbHelper.TryDeleteDataset(gp, outRaster);
            RasterCalculator calc = new RasterCalculator();
            calc.expression = expr.ToString();
            calc.output_raster = outRaster;
            gp.Execute(calc, "RasterCalculator-FeasibSum");
            return outRaster;
        }

        /// <summary>
        /// 将 [rawMin, rawMax] 线性映射到 0–100；NoData 视为 rawMin 对应 0。
        /// </summary>
        public static string NormalizeRangeTo100(
            GeoprocessorHelper gp,
            string inRaster,
            double rawMin,
            double rawMax,
            string outputGdb,
            string namePrefix)
        {
            if (gp == null || string.IsNullOrEmpty(inRaster))
            {
                throw new ArgumentException("标准化输入栅格无效。");
            }
            if (rawMax <= rawMin)
            {
                throw new ArgumentException("标准化值域无效。");
            }

            string outRaster = OutputGdbHelper.DatasetPath(outputGdb, ShortName(namePrefix) + "n");
            OutputGdbHelper.TryDeleteDataset(gp, outRaster);

            string span = (rawMax - rawMin).ToString(CultureInfo.InvariantCulture);
            string minS = rawMin.ToString(CultureInfo.InvariantCulture);
            // (Con(IsNull(r), rawMin, r) - rawMin) / span * 100
            string expr = "((Float(Con(IsNull(\"" + inRaster + "\")," + minS + ",\"" + inRaster
                + "\")) - " + minS + ") / " + span + ") * 100";

            RasterCalculator calc = new RasterCalculator();
            calc.expression = expr;
            calc.output_raster = outRaster;
            gp.Execute(calc, "RasterCalculator-FeasibNorm-" + namePrefix);
            return outRaster;
        }

        /// <summary>复制栅格到输出 GDB（便于统一路径与后续处理）。</summary>
        public static string CopyRasterToGdb(
            GeoprocessorHelper gp,
            string inRaster,
            string outputGdb,
            string namePrefix)
        {
            string outRaster = OutputGdbHelper.DatasetPath(outputGdb, ShortName(namePrefix));
            OutputGdbHelper.TryDeleteDataset(gp, outRaster);

            CopyRaster copy = new CopyRaster();
            copy.in_raster = inRaster;
            copy.out_rasterdataset = outRaster;
            gp.Execute(copy, "CopyRaster-" + namePrefix);
            return outRaster;
        }

        /// <summary>将栅格另存为固定数据集名（如 fea_score）。</summary>
        public static string SaveAs(
            GeoprocessorHelper gp,
            string inRaster,
            string outRaster)
        {
            OutputGdbHelper.TryDeleteDataset(gp, outRaster);
            RasterCalculator calc = new RasterCalculator();
            calc.expression = "\"" + inRaster + "\"";
            calc.output_raster = outRaster;
            gp.Execute(calc, "RasterCalculator-SaveAs");
            return outRaster;
        }

        private static int _nameSeq;

        public static void ResetNameSequence()
        {
            _nameSeq = 0;
        }

        private static string ShortName(string namePrefix)
        {
            _nameSeq++;
            StringBuilder sb = new StringBuilder();
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
                sb.Append('f');
            }
            if (sb[0] >= '0' && sb[0] <= '9')
            {
                sb.Insert(0, 'f');
            }
            sb.Append(_nameSeq.ToString("00"));
            return sb.ToString();
        }
    }
}

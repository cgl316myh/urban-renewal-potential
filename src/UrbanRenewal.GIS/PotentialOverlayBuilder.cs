using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.SpatialAnalystTools;
using UrbanRenewal.Model;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// 动力性 + 可行度加权叠置、五级重分类与面积统计。
    /// </summary>
    public static class PotentialOverlayBuilder
    {
        public static string WeightedOverlay(
            GeoprocessorHelper gp,
            string motivationRaster,
            string feasibilityRaster,
            double motivationWeight,
            double feasibilityWeight,
            string outRaster)
        {
            if (gp == null || string.IsNullOrEmpty(motivationRaster) || string.IsNullOrEmpty(feasibilityRaster))
            {
                throw new ArgumentException("叠置输入栅格无效。");
            }

            double sumW = motivationWeight + feasibilityWeight;
            if (sumW <= 0)
            {
                throw new ArgumentException("动力性/可行度权重之和须大于 0。");
            }
            double wm = motivationWeight / sumW;
            double wf = feasibilityWeight / sumW;

            string wmS = wm.ToString(CultureInfo.InvariantCulture);
            string wfS = wf.ToString(CultureInfo.InvariantCulture);
            // Con(IsNull(m),0,m)*wm + Con(IsNull(f),0,f)*wf
            string expr = "Con(IsNull(\"" + motivationRaster + "\"),0,\"" + motivationRaster + "\") * " + wmS
                + " + Con(IsNull(\"" + feasibilityRaster + "\"),0,\"" + feasibilityRaster + "\") * " + wfS;

            OutputGdbHelper.TryDeleteDataset(gp, outRaster);
            RasterCalculator calc = new RasterCalculator();
            calc.expression = expr;
            calc.output_raster = outRaster;
            gp.Execute(calc, "RasterCalculator-PotentialOverlay");
            return outRaster;
        }

        public static string ClassifyLevels(
            GeoprocessorHelper gp,
            string potentialRaster,
            string outputGdb,
            string namePrefix)
        {
            if (gp == null || string.IsNullOrEmpty(potentialRaster))
            {
                throw new ArgumentException("分级输入栅格无效。");
            }

            string outRaster = OutputGdbHelper.DatasetPath(outputGdb, ShortName(namePrefix));
            OutputGdbHelper.TryDeleteDataset(gp, outRaster);

            Reclassify reclass = new Reclassify();
            reclass.in_raster = potentialRaster;
            reclass.reclass_field = "VALUE";
            reclass.remap = PotentialLevel.BuildLevelRemap();
            reclass.out_raster = outRaster;
            reclass.missing_values = "DATA";
            gp.Execute(reclass, "Reclassify-PotentialLevel");
            return outRaster;
        }

        public static string SaveAs(GeoprocessorHelper gp, string inRaster, string outRaster)
        {
            OutputGdbHelper.TryDeleteDataset(gp, outRaster);
            RasterCalculator calc = new RasterCalculator();
            calc.expression = "\"" + inRaster + "\"";
            calc.output_raster = outRaster;
            gp.Execute(calc, "RasterCalculator-SaveAs-Potential");
            return outRaster;
        }

        /// <summary>
        /// 基于分级栅格 VAT 统计各等级像元数与面积占比。
        /// </summary>
        public static List<LevelAreaStat> ComputeLevelAreas(
            GeoprocessorHelper gp,
            string levelRaster,
            double cellSize,
            IList<string> messages)
        {
            List<LevelAreaStat> list = new List<LevelAreaStat>();
            if (string.IsNullOrEmpty(levelRaster))
            {
                return list;
            }

            try
            {
                if (messages != null)
                {
                    messages.Add("构建潜力等级栅格属性表并统计面积...");
                }
                BuildRasterAttributeTable build = new BuildRasterAttributeTable();
                build.in_raster = levelRaster;
                build.overwrite = "OVERWRITE";
                gp.Execute(build, "BuildRasterAttributeTable-Level");
            }
            catch (Exception ex)
            {
                if (messages != null)
                {
                    messages.Add("构建分级栅格属性表跳过/失败: " + ex.Message);
                }
            }

            double cellArea = cellSize > 0 ? cellSize * cellSize : 0;
            Dictionary<int, long> counts = ReadValueCounts(levelRaster);
            long total = 0;
            foreach (KeyValuePair<int, long> kv in counts)
            {
                total += kv.Value;
            }

            int[] codes = new int[] { 5, 4, 3, 2, 1 };
            for (int i = 0; i < codes.Length; i++)
            {
                int code = codes[i];
                long cnt = 0;
                counts.TryGetValue(code, out cnt);
                LevelAreaStat st = new LevelAreaStat();
                st.LevelCode = code;
                st.LevelName = PotentialLevel.ToNameFromCode(code);
                st.CellCount = cnt;
                st.AreaSqMeters = cellArea > 0 ? cnt * cellArea : 0;
                st.Percent = total > 0 ? (100.0 * cnt / total) : 0;
                list.Add(st);
            }
            return list;
        }

        private static Dictionary<int, long> ReadValueCounts(string rasterPath)
        {
            Dictionary<int, long> map = new Dictionary<int, long>();
            IRasterDataset dataset = null;
            ITable table = null;
            ICursor cursor = null;
            try
            {
                dataset = OpenRasterDataset(rasterPath);
                if (dataset == null)
                {
                    return map;
                }
                IRasterBandCollection bands = dataset as IRasterBandCollection;
                if (bands == null || bands.Count < 1)
                {
                    return map;
                }
                IRasterBand band = bands.Item(0);
                table = band.AttributeTable;
                if (table == null)
                {
                    return map;
                }
                int valueIdx = table.FindField("VALUE");
                int countIdx = table.FindField("COUNT");
                if (valueIdx < 0 || countIdx < 0)
                {
                    return map;
                }
                cursor = table.Search(null, false);
                IRow row = cursor.NextRow();
                while (row != null)
                {
                    object vObj = row.get_Value(valueIdx);
                    object cObj = row.get_Value(countIdx);
                    int v = Convert.ToInt32(vObj, CultureInfo.InvariantCulture);
                    long c = Convert.ToInt64(cObj, CultureInfo.InvariantCulture);
                    map[v] = c;
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
                if (dataset != null)
                {
                    Marshal.FinalReleaseComObject(dataset);
                }
            }
            return map;
        }

        private static IRasterDataset OpenRasterDataset(string rasterPath)
        {
            string folder = System.IO.Path.GetDirectoryName(rasterPath);
            string name = System.IO.Path.GetFileName(rasterPath);
            if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(name))
            {
                return null;
            }
            if (folder.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
            {
                IWorkspaceFactory gwf = new FileGDBWorkspaceFactoryClass();
                IWorkspace ws = gwf.OpenFromFile(folder, 0);
                IRasterWorkspaceEx rws = ws as IRasterWorkspaceEx;
                if (rws != null)
                {
                    return rws.OpenRasterDataset(name);
                }
            }
            IWorkspaceFactory factory = new RasterWorkspaceFactoryClass();
            IRasterWorkspace rasterWs = (IRasterWorkspace)factory.OpenFromFile(folder, 0);
            return rasterWs.OpenRasterDataset(name);
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
                sb.Append('o');
            }
            if (sb[0] >= '0' && sb[0] <= '9')
            {
                sb.Insert(0, 'o');
            }
            sb.Append(_nameSeq.ToString("00"));
            return sb.ToString();
        }
    }
}

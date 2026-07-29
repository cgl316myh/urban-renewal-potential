using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ESRI.ArcGIS.ConversionTools;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using UrbanRenewal.Model;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// 成果导出：TIFF 栅格、SHP、CSV 报表。
    /// </summary>
    public static class ResultsExporter
    {
        public static OutputResult Export(
            GeoprocessorHelper gp,
            OutputJob job,
            IList<string> messages)
        {
            OutputResult result = new OutputResult();
            if (job == null || string.IsNullOrEmpty(job.OutputGdbPath))
            {
                if (messages != null)
                {
                    messages.Add("输出作业无效。");
                }
                return result;
            }

            string folder = job.ExportFolder;
            if (string.IsNullOrEmpty(folder))
            {
                string parent = Path.GetDirectoryName(job.OutputGdbPath);
                folder = Path.Combine(string.IsNullOrEmpty(parent) ? job.OutputGdbPath : parent, "Export");
            }
            Directory.CreateDirectory(folder);
            result.ExportFolder = folder;

            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            if (job.ExportTiff)
            {
                if (messages != null)
                {
                    messages.Add("导出 TIFF：综合潜力 / 潜力等级...");
                }
                TryExportRasterTiff(gp, job.OutputGdbPath, job.PotentialRasterName,
                    Path.Combine(folder, "pot_score_" + stamp + ".tif"), result, messages);
                TryExportRasterTiff(gp, job.OutputGdbPath, job.LevelRasterName,
                    Path.Combine(folder, "pot_level_" + stamp + ".tif"), result, messages);
            }

            if (job.ExportShp)
            {
                if (messages != null)
                {
                    messages.Add("导出 SHP：宗地潜力...");
                }
                TryExportShapefile(gp, job.OutputGdbPath, job.ParcelFeatureName, folder, result, messages);
            }

            if (job.ExportCsv)
            {
                if (messages != null)
                {
                    messages.Add("导出 CSV：宗地报表...");
                }
                string csv = Path.Combine(folder, "parcel_potential_" + stamp + ".csv");
                if (TryExportParcelCsv(job.OutputGdbPath, job.ParcelFeatureName, csv, messages))
                {
                    result.ExportedFiles.Add(csv);
                }
            }

            // 简单 HTML 说明（两套专题图清单）
            string indexHtml = Path.Combine(folder, "export_index_" + stamp + ".html");
            File.WriteAllText(indexHtml, BuildIndexHtml(result), Encoding.UTF8);
            result.ExportedFiles.Add(indexHtml);
            if (messages != null)
            {
                messages.Add("导出清单: " + indexHtml);
            }

            result.Success = result.ExportedFiles.Count > 0;
            return result;
        }

        private static void TryExportRasterTiff(
            GeoprocessorHelper gp,
            string gdb,
            string rasterName,
            string outTiff,
            OutputResult result,
            IList<string> messages)
        {
            if (string.IsNullOrEmpty(rasterName))
            {
                return;
            }
            string inRaster = OutputGdbHelper.DatasetPath(gdb, rasterName);
            if (!RasterExists(gdb, rasterName))
            {
                if (messages != null)
                {
                    messages.Add("跳过 TIFF（栅格不存在）: " + rasterName);
                }
                return;
            }
            try
            {
                if (File.Exists(outTiff))
                {
                    File.Delete(outTiff);
                }
                CopyRaster copy = new CopyRaster();
                copy.in_raster = inRaster;
                copy.out_rasterdataset = outTiff;
                gp.Execute(copy, "CopyRaster-Tiff-" + rasterName);
                result.ExportedFiles.Add(outTiff);
                if (messages != null)
                {
                    messages.Add("已导出 TIFF: " + outTiff);
                }
            }
            catch (Exception ex)
            {
                if (messages != null)
                {
                    messages.Add("导出 TIFF 失败[" + rasterName + "]: " + ex.Message);
                }
            }
        }

        private static void TryExportShapefile(
            GeoprocessorHelper gp,
            string gdb,
            string fcName,
            string folder,
            OutputResult result,
            IList<string> messages)
        {
            if (string.IsNullOrEmpty(fcName) || !FeatureExists(gdb, fcName))
            {
                if (messages != null)
                {
                    messages.Add("跳过 SHP（要素不存在）: " + fcName);
                }
                return;
            }
            try
            {
                string inFc = OutputGdbHelper.DatasetPath(gdb, fcName);
                FeatureClassToShapefile tool = new FeatureClassToShapefile();
                tool.Input_Features = inFc;
                tool.Output_Folder = folder;
                gp.Execute(tool, "FeatureClassToShapefile-" + fcName);
                string shp = Path.Combine(folder, fcName + ".shp");
                if (File.Exists(shp))
                {
                    result.ExportedFiles.Add(shp);
                }
                if (messages != null)
                {
                    messages.Add("已导出 SHP 到: " + folder + "\\" + fcName + ".shp");
                }
            }
            catch (Exception ex)
            {
                if (messages != null)
                {
                    messages.Add("导出 SHP 失败: " + ex.Message);
                }
            }
        }

        public static bool TryExportParcelCsv(string gdb, string fcName, string csvPath, IList<string> messages)
        {
            if (!FeatureExists(gdb, fcName))
            {
                if (messages != null)
                {
                    messages.Add("跳过 CSV（要素不存在）: " + fcName);
                }
                return false;
            }

            IFeatureClass fc = null;
            IFeatureCursor cursor = null;
            StreamWriter sw = null;
            try
            {
                fc = OpenFeatureClass(OutputGdbHelper.DatasetPath(gdb, fcName));
                if (fc == null)
                {
                    return false;
                }
                int oidIdx = fc.FindField(fc.OIDFieldName);
                int potIdx = FindField(fc, ParcelZonalLinker.FieldPotentialScore);
                int motIdx = FindField(fc, ParcelZonalLinker.FieldMotivScore);
                int feaIdx = FindField(fc, ParcelZonalLinker.FieldFeasibScore);
                int lvlIdx = FindField(fc, ParcelZonalLinker.FieldPotentialLevel);

                sw = new StreamWriter(csvPath, false, new UTF8Encoding(true));
                sw.WriteLine("OID,POTENTIAL_SCORE,MOTIV_SCORE,FEASIB_SCORE,POTENTIAL_LEVEL");
                cursor = fc.Search(null, false);
                IFeature f = cursor.NextFeature();
                int n = 0;
                while (f != null)
                {
                    sw.Write(GetVal(f, oidIdx));
                    sw.Write(",");
                    sw.Write(GetVal(f, potIdx));
                    sw.Write(",");
                    sw.Write(GetVal(f, motIdx));
                    sw.Write(",");
                    sw.Write(GetVal(f, feaIdx));
                    sw.Write(",");
                    sw.Write(CsvEscape(GetVal(f, lvlIdx)));
                    sw.WriteLine();
                    n++;
                    f = cursor.NextFeature();
                }
                if (messages != null)
                {
                    messages.Add("已导出 CSV 报表: " + csvPath + "（" + n + " 行）");
                }
                return true;
            }
            catch (Exception ex)
            {
                if (messages != null)
                {
                    messages.Add("导出 CSV 失败: " + ex.Message);
                }
                return false;
            }
            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                }
                if (cursor != null)
                {
                    Marshal.FinalReleaseComObject(cursor);
                }
                if (fc != null)
                {
                    Marshal.FinalReleaseComObject(fc);
                }
            }
        }

        private static string BuildIndexHtml(OutputResult result)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"/><title>成果导出清单</title></head><body>");
            sb.AppendLine("<h1>城市更新潜力评价 — 成果导出</h1>");
            sb.AppendLine("<p>目录: " + (result.ExportFolder ?? "") + "</p><ul>");
            for (int i = 0; i < result.ExportedFiles.Count; i++)
            {
                sb.AppendLine("<li>" + result.ExportedFiles[i] + "</li>");
            }
            sb.AppendLine("</ul>");
            sb.AppendLine("<p>专题图说明：请在地图加载 pot_score / parcel_pot 后，使用「导出地图 PDF/TIFF」生成全域潜力图与宗地潜力图。</p>");
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private static string GetVal(IFeature f, int idx)
        {
            if (idx < 0)
            {
                return "";
            }
            object v = f.get_Value(idx);
            if (v == null || v == DBNull.Value)
            {
                return "";
            }
            if (v is double || v is float)
            {
                return Convert.ToDouble(v, CultureInfo.InvariantCulture).ToString("0.####", CultureInfo.InvariantCulture);
            }
            return Convert.ToString(v, CultureInfo.InvariantCulture);
        }

        private static string CsvEscape(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return "";
            }
            if (s.IndexOf(',') >= 0 || s.IndexOf('"') >= 0)
            {
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            }
            return s;
        }

        private static int FindField(IFeatureClass fc, string name)
        {
            int idx = fc.FindField(name);
            if (idx >= 0)
            {
                return idx;
            }
            for (int i = 0; i < fc.Fields.FieldCount; i++)
            {
                if (string.Equals(fc.Fields.get_Field(i).Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

        private static bool RasterExists(string gdb, string name)
        {
            try
            {
                List<string> list = WorkspaceCatalog.ListRasterDatasetNames(gdb);
                for (int i = 0; i < list.Count; i++)
                {
                    if (string.Equals(list[i], name, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        private static bool FeatureExists(string gdb, string name)
        {
            try
            {
                List<string> list = WorkspaceCatalog.ListFeatureClassNames(gdb);
                for (int i = 0; i < list.Count; i++)
                {
                    if (string.Equals(list[i], name, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }
            return false;
        }

        private static IFeatureClass OpenFeatureClass(string path)
        {
            string gdb = Path.GetDirectoryName(path);
            string name = Path.GetFileName(path);
            IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
            IFeatureWorkspace fw = factory.OpenFromFile(gdb, 0) as IFeatureWorkspace;
            return fw != null ? fw.OpenFeatureClass(name) : null;
        }
    }
}

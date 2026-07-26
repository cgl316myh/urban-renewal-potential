using System;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Analysis
{
    /// <summary>
    /// 成果输出引擎（GDB 数据导出）；地图 PDF/TIFF 由插件侧调用 MapExportHelper。
    /// </summary>
    public class OutputExportEngine
    {
        public OutputResult Run(OutputJob job, Action<string, int> progress)
        {
            OutputResult result = new OutputResult();
            if (job == null || string.IsNullOrEmpty(job.OutputGdbPath))
            {
                result.Messages.Add("请指定输出 File GDB。");
                return result;
            }

            if (progress != null)
            {
                progress("导出成果数据...", 30);
            }
            result.Messages.Add("开始导出...");

            GeoprocessorHelper gp = new GeoprocessorHelper();
            gp.ConfigureAnalysis(job.OutputGdbPath, null, 0, null);
            OutputResult exported = ResultsExporter.Export(gp, job, result.Messages);
            result.Success = exported.Success;
            result.ExportFolder = exported.ExportFolder;
            result.ExportedFiles = exported.ExportedFiles;

            if (progress != null)
            {
                progress("完成", 100);
            }
            result.Messages.Add(result.Success ? "数据导出完成。" : "未导出任何文件。");
            return result;
        }
    }
}

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

        private Action<string, int> _progress;

        private int _progressPercent;



        public OutputResult Run(OutputJob job, Action<string, int> progress)

        {

            _progress = progress;

            _progressPercent = 0;

            OutputResult result = new OutputResult();

            if (job == null || string.IsNullOrEmpty(job.OutputGdbPath))

            {

                Note(result, "请指定输出 File GDB。");

                return result;

            }



            Report(result, "导出成果数据...", 30);

            Note(result, "开始导出...");



            GeoprocessorHelper gp = new GeoprocessorHelper();
            gp.BindToProgress(_progress, delegate { return _progressPercent; });
            gp.ConfigureAnalysis(job.OutputGdbPath, null, 0, null);

            OutputResult exported = ResultsExporter.Export(gp, job, LiveMsgs(result));

            result.Success = exported.Success;

            result.ExportFolder = exported.ExportFolder;

            result.ExportedFiles = exported.ExportedFiles;



            Report(result, result.Success ? "数据导出完成。" : "未导出任何文件。", 100);

            return result;

        }



        private void Note(OutputResult result, string text)

        {

            if (result != null)

            {

                result.Messages.Add(text);

            }

            if (_progress != null)

            {

                _progress(text, _progressPercent);

            }

        }



        private System.Collections.Generic.IList<string> LiveMsgs(OutputResult result)

        {

            return new LiveMessageList(result.Messages, delegate(string t)

            {

                if (_progress != null)

                {

                    _progress(t, _progressPercent);

                }

            });

        }



        private void Report(OutputResult result, string text, int percent)

        {

            _progressPercent = percent;

            Note(result, text);

        }

    }

}


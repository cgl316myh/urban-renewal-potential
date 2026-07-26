using System.Collections.Generic;

namespace UrbanRenewal.Model
{
    /// <summary>
    /// 成果输出作业：专题图/TIFF/PDF/SHP/报表。
    /// </summary>
    public class OutputJob
    {
        public OutputJob()
        {
            ExportFolder = null;
            PotentialRasterName = "pot_score";
            LevelRasterName = "pot_level";
            ParcelFeatureName = "parcel_pot";
            ExportTiff = true;
            ExportPdf = true;
            ExportShp = true;
            ExportCsv = true;
            ExportMapPdf = true;
            ExportMapTiff = true;
        }

        public string OutputGdbPath { get; set; }

        /// <summary>导出目录；空则用输出 GDB 旁 Export 文件夹。</summary>
        public string ExportFolder { get; set; }

        public string PotentialRasterName { get; set; }

        public string LevelRasterName { get; set; }

        public string ParcelFeatureName { get; set; }

        public bool ExportTiff { get; set; }

        public bool ExportPdf { get; set; }

        public bool ExportShp { get; set; }

        public bool ExportCsv { get; set; }

        public bool ExportMapPdf { get; set; }

        public bool ExportMapTiff { get; set; }
    }

    public class OutputResult
    {
        public OutputResult()
        {
            Messages = new List<string>();
            ExportedFiles = new List<string>();
        }

        public bool Success { get; set; }

        public string ExportFolder { get; set; }

        public List<string> ExportedFiles { get; set; }

        public List<string> Messages { get; set; }
    }
}

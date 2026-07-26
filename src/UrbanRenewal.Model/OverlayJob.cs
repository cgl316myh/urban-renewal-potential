using System.Collections.Generic;

namespace UrbanRenewal.Model
{
    /// <summary>
    /// 综合潜力叠置作业：动力性 × W动力 + 可行度 × W可行。
    /// </summary>
    public class OverlayJob
    {
        public OverlayJob()
        {
            MotivationWeight = 0.7;
            FeasibilityWeight = 0.3;
            CellSize = 30;
            MotivationRasterName = "mot_score";
            FeasibilityRasterName = "fea_score";
            LayerHints = new Dictionary<string, string>();
        }

        public string GdbPath { get; set; }

        public string OutputGdbPath { get; set; }

        public double CellSize { get; set; }

        public double MotivationWeight { get; set; }

        public double FeasibilityWeight { get; set; }

        /// <summary>动力性栅格名或完整路径；默认输出库内 mot_score。</summary>
        public string MotivationRasterName { get; set; }

        /// <summary>可行度栅格名或完整路径；默认输出库内 fea_score。</summary>
        public string FeasibilityRasterName { get; set; }

        public Dictionary<string, string> LayerHints { get; set; }
    }

    public class OverlayResult
    {
        public OverlayResult()
        {
            Messages = new List<string>();
            LevelAreas = new List<LevelAreaStat>();
        }

        public bool Success { get; set; }

        public string OutputGdbPath { get; set; }

        /// <summary>综合潜力连续分值栅格（0–100）。</summary>
        public string PotentialRasterPath { get; set; }

        /// <summary>五级分类栅格（1偏低～5极高）。</summary>
        public string LevelRasterPath { get; set; }

        public List<LevelAreaStat> LevelAreas { get; set; }

        public List<string> Messages { get; set; }
    }

    public class LevelAreaStat
    {
        public int LevelCode { get; set; }

        public string LevelName { get; set; }

        public long CellCount { get; set; }

        public double AreaSqMeters { get; set; }

        public double Percent { get; set; }
    }
}

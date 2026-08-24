using System.Collections.Generic;

namespace UrbanRenewal.Model
{
    /// <summary>验证校核作业：已更新宗地 vs 评价结果潜力等级分布。</summary>
    public class ValidationJob
    {
        public ValidationJob()
        {
            HighLevelThreshold = 60;
            PassHighRatio = 0.6;
            ScoredParcelName = "parcel_pot";
            DiffFeatureClassName = "valid_diff";
            LayerHints = new Dictionary<string, string>();
        }

        public string GdbPath { get; set; }

        public string OutputGdbPath { get; set; }

        /// <summary>已关联潜力字段的宗地（默认输出库 parcel_pot）。</summary>
        public string ScoredParcelName { get; set; }

        public string DiffFeatureClassName { get; set; }

        /// <summary>得分≥该值视为高/极高等级。</summary>
        public double HighLevelThreshold { get; set; }

        /// <summary>高等级占比达到该值则判定通过。</summary>
        public double PassHighRatio { get; set; }

        public string ReviewComment { get; set; }

        /// <summary>角色：UpdatedParcel。</summary>
        public Dictionary<string, string> LayerHints { get; set; }
    }

    public class ValidationResult
    {
        public ValidationResult()
        {
            Messages = new List<string>();
            LevelCounts = new Dictionary<string, int>();
        }

        public bool Success { get; set; }

        public bool Passed { get; set; }

        public string OutputGdbPath { get; set; }

        public string ReportPath { get; set; }

        public string DiffFeatureClassPath { get; set; }

        public int UpdatedCount { get; set; }

        public int HighCount { get; set; }

        public double HighRatio { get; set; }

        public Dictionary<string, int> LevelCounts { get; set; }

        public List<string> Messages { get; set; }
    }
}

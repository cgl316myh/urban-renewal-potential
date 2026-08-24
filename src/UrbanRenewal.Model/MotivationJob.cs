using System.Collections.Generic;

namespace UrbanRenewal.Model
{
    /// <summary>动力性分析作业参数。</summary>
    public class MotivationJob
    {
        public MotivationJob()
        {
            CellSize = 30;
            TrafficWeight = 0.30;
            EnvironmentWeight = 0.20;
            FacilityWeight = 0.25;
            PolicyWeight = 0.25;
            LayerHints = new Dictionary<string, string>();
            BufferScoreRules = BufferScoreRules.CreateOriginal();
        }

        public string GdbPath { get; set; }

        public string OutputGdbPath { get; set; }

        /// <summary>兼容旧字段；未设 OutputGdbPath 且本字段为 *.gdb 时当作输出库。</summary>
        public string WorkDirectory { get; set; }

        public double CellSize { get; set; }

        public double TrafficWeight { get; set; }

        public double EnvironmentWeight { get; set; }

        public double FacilityWeight { get; set; }

        public double PolicyWeight { get; set; }

        /// <summary>缓冲赋分规则；空则用现状默认。</summary>
        public BufferScoreRules BufferScoreRules { get; set; }

        /// <summary>因子键 → 要素类名（覆盖自动匹配）。</summary>
        public Dictionary<string, string> LayerHints { get; set; }
    }

    public class MotivationResult
    {
        public MotivationResult()
        {
            Messages = new List<string>();
            CriterionRasters = new Dictionary<string, string>();
        }

        public bool Success { get; set; }

        public string MotivationRasterPath { get; set; }

        public string OutputGdbPath { get; set; }

        public Dictionary<string, string> CriterionRasters { get; set; }

        public List<string> Messages { get; set; }
    }
}

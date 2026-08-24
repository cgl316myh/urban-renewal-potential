using System.Collections.Generic;

namespace UrbanRenewal.Model
{
    /// <summary>可行度分析作业参数（宗地 SI/PD + DEM + 人口）。</summary>
    public class FeasibilityJob
    {
        public FeasibilityJob()
        {
            CellSize = 30;
            ElevationThreshold = 50;
            SlopeThresholdDegrees = 15;
            LayerHints = new Dictionary<string, string>();
        }

        public string GdbPath { get; set; }

        public string OutputGdbPath { get; set; }

        /// <summary>兼容旧字段；未设 OutputGdbPath 且本字段为 *.gdb 时当作输出库。</summary>
        public string WorkDirectory { get; set; }

        public double CellSize { get; set; }

        /// <summary>高程超过该阈值（米）扣 -1 分。</summary>
        public double ElevationThreshold { get; set; }

        /// <summary>坡度超过该阈值（度）扣 -1 分。</summary>
        public double SlopeThresholdDegrees { get; set; }

        /// <summary>角色键 → 要素类/栅格名。常用：StudyArea, Parcel, DEM, Population, Slope。</summary>
        public Dictionary<string, string> LayerHints { get; set; }
    }

    public class FeasibilityResult
    {
        public FeasibilityResult()
        {
            Messages = new List<string>();
            FactorRasters = new Dictionary<string, string>();
        }

        public bool Success { get; set; }

        /// <summary>可行度合成栅格（0–100 标准化）。</summary>
        public string FeasibilityRasterPath { get; set; }

        /// <summary>原始加分/扣分合成栅格（可能为负）。</summary>
        public string FeasibilityRawRasterPath { get; set; }

        public string OutputGdbPath { get; set; }

        /// <summary>因子显示名 → 栅格路径。</summary>
        public Dictionary<string, string> FactorRasters { get; set; }

        public List<string> Messages { get; set; }
    }
}

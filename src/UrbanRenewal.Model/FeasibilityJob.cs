using System.Collections.Generic;

namespace UrbanRenewal.Model
{
    /// <summary>
    /// 可行度分析作业参数（宗地 SI/PD + DEM + 人口）。
    /// </summary>
    public class FeasibilityJob
    {
        public FeasibilityJob()
        {
            CellSize = 30;
            ElevationThreshold = 50;
            SlopeThresholdDegrees = 15;
            LayerHints = new Dictionary<string, string>();
        }

        /// <summary>输入工作空间 File GDB。</summary>
        public string GdbPath { get; set; }

        /// <summary>输出 File GDB：中间与结果均写入此库。</summary>
        public string OutputGdbPath { get; set; }

        /// <summary>兼容旧字段；若未设 OutputGdbPath 且本字段为 *.gdb 则当作输出库。</summary>
        public string WorkDirectory { get; set; }

        public double CellSize { get; set; }

        /// <summary>高程超过该阈值（米）扣 -1 分。</summary>
        public double ElevationThreshold { get; set; }

        /// <summary>坡度超过该阈值（度）扣 -1 分。</summary>
        public double SlopeThresholdDegrees { get; set; }

        /// <summary>
        /// 可选：角色键 → 要素类/栅格名。
        /// 常用：StudyArea, Parcel, DEM, Population, Slope（已有坡度栅格时可跳过 Slope 工具）。
        /// </summary>
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

        /// <summary>因子显示名 → 栅格路径（PD/SI/高程/坡度/人口）。</summary>
        public Dictionary<string, string> FactorRasters { get; set; }

        public List<string> Messages { get; set; }
    }
}

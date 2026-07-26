using System.Collections.Generic;

namespace UrbanRenewal.Model
{
    /// <summary>
    /// 宗地关联作业：Zonal Statistics 将潜力/动力/可行栅格均值写入宗地面。
    /// </summary>
    public class ParcelLinkJob
    {
        public ParcelLinkJob()
        {
            StatisticType = "MEAN";
            PotentialRasterName = "pot_score";
            MotivationRasterName = "mot_score";
            FeasibilityRasterName = "fea_score";
            OutputFeatureClassName = "parcel_pot";
            LayerHints = new Dictionary<string, string>();
        }

        public string GdbPath { get; set; }

        public string OutputGdbPath { get; set; }

        /// <summary>MEAN 或 MAX。</summary>
        public string StatisticType { get; set; }

        public string PotentialRasterName { get; set; }

        public string MotivationRasterName { get; set; }

        public string FeasibilityRasterName { get; set; }

        /// <summary>输出宗地要素类名（写入输出 GDB）。</summary>
        public string OutputFeatureClassName { get; set; }

        /// <summary>角色提示：Parcel、StudyArea。</summary>
        public Dictionary<string, string> LayerHints { get; set; }
    }

    public class ParcelLinkResult
    {
        public ParcelLinkResult()
        {
            Messages = new List<string>();
        }

        public bool Success { get; set; }

        public string OutputGdbPath { get; set; }

        /// <summary>已写入潜力字段的宗地面路径。</summary>
        public string ParcelFeatureClassPath { get; set; }

        public int ParcelCount { get; set; }

        public List<string> Messages { get; set; }
    }
}

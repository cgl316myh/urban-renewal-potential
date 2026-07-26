using System.Globalization;

namespace UrbanRenewal.Model
{
    /// <summary>
    /// 更新潜力五级划分（标准化 0–100）。
    /// </summary>
    public static class PotentialLevel
    {
        public const string Extreme = "极高";
        public const string High = "高";
        public const string Medium = "中";
        public const string Low = "低";
        public const string VeryLow = "偏低";

        /// <summary>1偏低 … 5极高。</summary>
        public static int ToCode(double score)
        {
            if (score >= 80)
            {
                return 5;
            }
            if (score >= 60)
            {
                return 4;
            }
            if (score >= 40)
            {
                return 3;
            }
            if (score >= 20)
            {
                return 2;
            }
            return 1;
        }

        public static string ToName(double score)
        {
            return ToNameFromCode(ToCode(score));
        }

        public static string ToNameFromCode(int code)
        {
            switch (code)
            {
                case 5:
                    return Extreme;
                case 4:
                    return High;
                case 3:
                    return Medium;
                case 2:
                    return Low;
                default:
                    return VeryLow;
            }
        }

        /// <summary>Reclassify remap：0–20→1 … 80–100→5。</summary>
        public static string BuildLevelRemap()
        {
            return "0 20 1;20 40 2;40 60 3;60 80 4;80 100.0001 5";
        }

        public static string FormatScore(double score)
        {
            return score.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}

namespace UrbanRenewal.Analysis
{
    /// <summary>动力性准则层理论满分，用于缩放到 0–100。</summary>
    public static class MotivationScoreScale
    {
        /// <summary>交通满分。</summary>
        public const double TrafficMax = 5.0;

        /// <summary>环境满分（SUM：廊道2+开敞2+绿地1）。</summary>
        public const double EnvironmentMax = 5.0;

        /// <summary>设施满分（SUM：公服2+便民1+商业1，沿用现赋分）。</summary>
        public const double FacilityMax = 4.0;

        /// <summary>政策满分（SUM：圈带1+战略1+重点2，沿用现赋分）。</summary>
        public const double PolicyMax = 4.0;
    }
}

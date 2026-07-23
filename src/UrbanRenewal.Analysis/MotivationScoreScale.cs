namespace UrbanRenewal.Analysis
{
    /// <summary>
    /// 动力性准则层原始得分的理论满分（同准则内 MAX 合并后的上限），用于缩放到 0–100。
    /// </summary>
    public static class MotivationScoreScale
    {
        /// <summary>交通：路网可达性最高 5 分。</summary>
        public const double TrafficMax = 5.0;

        /// <summary>环境：生态廊道/开敞空间最高 2 分。</summary>
        public const double EnvironmentMax = 2.0;

        /// <summary>设施：市级公服最高 2 分。</summary>
        public const double FacilityMax = 2.0;

        /// <summary>政策：近期重点区最高 2 分。</summary>
        public const double PolicyMax = 2.0;
    }
}

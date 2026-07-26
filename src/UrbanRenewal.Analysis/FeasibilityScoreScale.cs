namespace UrbanRenewal.Analysis
{
    /// <summary>
    /// 可行度原始得分理论值域（加分/扣分直接叠加），用于映射到 0–100。
    /// 人口 +2，高程 -1，坡度 -1，PD -2，SI -1 → 约 [-5, +2]。
    /// </summary>
    public static class FeasibilityScoreScale
    {
        public const double TheoreticalMin = -5.0;

        public const double TheoreticalMax = 2.0;

        public const double PopulationMax = 2.0;

        public const double ElevationPenalty = -1.0;

        public const double SlopePenalty = -1.0;

        public const double FragmentationPenalty = -2.0;

        public const double ShapeIndexPenalty = -1.0;
    }
}

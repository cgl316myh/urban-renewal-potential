namespace UrbanRenewal.Analysis
{
    /// <summary>可行度原始得分理论值域，用于映射 0–100。</summary>
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

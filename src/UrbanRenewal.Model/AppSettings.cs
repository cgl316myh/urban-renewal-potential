namespace UrbanRenewal.Model
{
    /// <summary>
    /// 应用级配置（P1 最小集，后续扩展权重与缓冲规则）。
    /// </summary>
    public class AppSettings
    {
        public AppSettings()
        {
            SkinName = "Office 2013";
            PluginsDirectoryName = "Plugins";
            MotivationWeight = 0.7;
            FeasibilityWeight = 0.3;
        }

        public string SkinName { get; set; }

        public string PluginsDirectoryName { get; set; }

        public string LastGdbPath { get; set; }

        /// <summary>当前选用的城市配置 Id（对应 Config/Cities/*.xml）。</summary>
        public string ActiveCityProfileId { get; set; }

        public double MotivationWeight { get; set; }

        public double FeasibilityWeight { get; set; }
    }
}

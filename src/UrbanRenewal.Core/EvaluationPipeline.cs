using UrbanRenewal.Model;

namespace UrbanRenewal.Core
{
    /// <summary>评价流程编排（占位）。</summary>
    public class EvaluationPipeline
    {
        private readonly AppSettings _settings;

        public EvaluationPipeline(AppSettings settings)
        {
            _settings = settings ?? new AppSettings();
        }

        public AppSettings Settings
        {
            get { return _settings; }
        }

        public void Run()
        {
            // 动力性 → 可行度 → 叠置 → 宗地关联
        }
    }
}

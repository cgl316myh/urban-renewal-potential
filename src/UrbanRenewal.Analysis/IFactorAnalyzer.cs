namespace UrbanRenewal.Analysis
{
    /// <summary>因子分析器接口。</summary>
    public interface IFactorAnalyzer
    {
        string Name { get; }

        /// <summary>执行分析，返回结果栅格路径。</summary>
        string Analyze(string workGdbPath);
    }
}

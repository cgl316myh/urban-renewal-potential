namespace UrbanRenewal.Analysis
{
    /// <summary>
    /// 因子分析器统一接口（P2+ 各 Analyzer 实现）。
    /// </summary>
    public interface IFactorAnalyzer
    {
        string Name { get; }

        /// <summary>
        /// 执行分析并返回结果栅格路径或标识（P2 再改为 IRaster）。
        /// </summary>
        string Analyze(string workGdbPath);
    }
}

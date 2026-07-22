namespace UrbanRenewal.Contracts
{
    /// <summary>
    /// 业务模块插件统一契约。各插件 DLL 至少实现一个此接口类型。
    /// </summary>
    public interface IModulePlugin
    {
        string Id { get; }

        string Name { get; }

        /// <summary>
        /// 加载顺序，数值越小越先初始化。
        /// </summary>
        int Order { get; }

        void Initialize(IAppContext context);

        void RegisterRibbon(IRibbonHost ribbonHost);

        void Shutdown();
    }
}

using System;

namespace UrbanRenewal.Contracts
{
    /// <summary>
    /// 宿主注入给插件的运行上下文（避免插件直接依赖主窗体类型）。
    /// </summary>
    public interface IAppContext
    {
        object MapControl { get; }

        object TocControl { get; }

        object DockManager { get; }

        /// <summary>输入工作空间 File GDB。</summary>
        string GdbPath { get; set; }

        /// <summary>分析结果输出 File GDB（全局，全模块共用）。</summary>
        string OutputGdbPath { get; set; }

        /// <summary>当前城市配置 Id（全局）。</summary>
        string ActiveCityProfileId { get; set; }

        /// <summary>将当前全局设置写入 Config/app_settings.xml。</summary>
        void SaveGlobalSettings();

        /// <summary>从磁盘重新加载全局设置。</summary>
        void ReloadGlobalSettings();

        /// <summary>
        /// 由数据管理插件注册全局设置窗体；其它模块调用 <see cref="ShowGlobalSettings"/> 打开。
        /// </summary>
        void RegisterGlobalSettingsUI(Action showDialog);

        /// <summary>打开全局设置（输出 GDB、城市配置），设置一次全模块共用。</summary>
        void ShowGlobalSettings();

        bool OpenFileGdb(string gdbPath, out string message);

        string CheckDataIntegrity();

        void ZoomToFullExtent();

        void ActivatePanTool();

        void ActivateZoomInTool();

        /// <summary>
        /// 将栅格结果加载到地图。
        /// </summary>
        bool AddRasterLayer(string rasterPath, string layerName, out string message);

        void LogInfo(string message);

        void LogWarn(string message);

        void LogError(string message);

        void ShowProgress(string caption, int percent);

        void HideProgress();

        void ShowMessage(string caption, string text);
    }
}

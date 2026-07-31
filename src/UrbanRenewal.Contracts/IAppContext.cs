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

        /// <summary>全局基准坐标系来源路径（*.shp 或 *.gdb）。</summary>
        string SpatialRefSourcePath { get; set; }

        /// <summary>来源为 GDB 时的图层名。</summary>
        string SpatialRefLayerName { get; set; }

        /// <summary>已解析的基准坐标系名称。</summary>
        string SpatialRefName { get; set; }

        /// <summary>已解析的基准坐标系 FactoryCode（0=未知）。</summary>
        int SpatialRefFactoryCode { get; set; }

        /// <summary>当前工程 MXD 路径。</summary>
        string ProjectMxdPath { get; set; }

        /// <summary>将当前全局设置写入 Config/app_settings.xml。</summary>
        void SaveGlobalSettings();

        /// <summary>从磁盘重新加载全局设置。</summary>
        void ReloadGlobalSettings();

        /// <summary>新建工程：清空全局工作区配置与地图，并持久化。</summary>
        bool NewProject(out string message);

        /// <summary>保存工程：将当前地图写入本地 MXD，并记住路径以便下次启动加载。</summary>
        bool SaveProject(out string message);

        /// <summary>若存在已保存工程 MXD，则加载到地图。</summary>
        bool TryLoadSavedProject(out string message);

        /// <summary>
        /// 由数据管理插件注册全局设置窗体；其它模块调用 <see cref="ShowGlobalSettings"/> 打开。
        /// </summary>
        void RegisterGlobalSettingsUI(Action showDialog);

        /// <summary>打开全局设置（输出 GDB、城市配置），设置一次全模块共用。</summary>
        void ShowGlobalSettings();

        /// <summary>宿主主窗体（用于非模态分析窗体 Owner，避免锁死主界面）。</summary>
        object MainWindow { get; }

        bool OpenFileGdb(string gdbPath, out string message);

        string CheckDataIntegrity();

        void ZoomToFullExtent();

        void ActivatePanTool();

        void ActivateZoomInTool();

        void ActivateZoomOutTool();

        /// <summary>
        /// 将栅格结果加载到地图。
        /// </summary>
        bool AddRasterLayer(string rasterPath, string layerName, out string message);

        /// <summary>
        /// 将要素类结果加载到地图（File GDB 路径）。
        /// </summary>
        bool AddFeatureLayer(string featureClassPath, string layerName, out string message);

        /// <summary>综合潜力叠置：动力性权重（默认 0.7）。</summary>
        double MotivationWeight { get; set; }

        /// <summary>综合潜力叠置：可行度权重（默认 0.3）。</summary>
        double FeasibilityWeight { get; set; }

        /// <summary>潜力分析统一像元大小（米，默认 30）。</summary>
        double CellSize { get; set; }

        /// <summary>当前 DevExpress 皮肤名。</summary>
        string SkinName { get; set; }

        /// <summary>应用并可选持久化皮肤。</summary>
        void ApplySkin(string skinName);

        /// <summary>获取宿主日志文本（最近条目）。</summary>
        string GetLogText();

        void LogInfo(string message);

        void LogWarn(string message);

        void LogError(string message);

        void ShowProgress(string caption, int percent);

        void HideProgress();

        void ShowMessage(string caption, string text);
    }
}

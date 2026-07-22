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

        string GdbPath { get; set; }

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

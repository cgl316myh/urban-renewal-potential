using System;

namespace UrbanRenewal.Contracts
{
    /// <summary>
    /// 宿主注入给插件的运行上下文（避免插件直接依赖主窗体类型）。
    /// </summary>
    public interface IAppContext
    {
        /// <summary>
        /// ArcEngine MapControl 宿主控件（实际类型为 AxMapControl，此处用 object 解耦）。
        /// </summary>
        object MapControl { get; }

        /// <summary>
        /// ArcEngine TOC 控件（可为 null）。
        /// </summary>
        object TocControl { get; }

        /// <summary>
        /// DevExpress DockManager（可为 null）。
        /// </summary>
        object DockManager { get; }

        string GdbPath { get; set; }

        void LogInfo(string message);

        void LogWarn(string message);

        void LogError(string message);

        void ShowProgress(string caption, int percent);

        void HideProgress();

        void ShowMessage(string caption, string text);
    }
}

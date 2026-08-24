using System;

namespace UrbanRenewal.Contracts
{
    /// <summary>宿主注入给插件的运行上下文。</summary>
    public interface IAppContext
    {
        object MapControl { get; }

        object TocControl { get; }

        object DockManager { get; }

        string GdbPath { get; set; }

        string OutputGdbPath { get; set; }

        string ActiveCityProfileId { get; set; }

        string SpatialRefSourcePath { get; set; }

        string SpatialRefLayerName { get; set; }

        string SpatialRefName { get; set; }

        int SpatialRefFactoryCode { get; set; }

        string ProjectMxdPath { get; set; }

        void SaveGlobalSettings();

        void ReloadGlobalSettings();

        bool NewProject(out string message);

        bool SaveProject(out string message);

        bool TryLoadSavedProject(out string message);

        void RegisterGlobalSettingsUI(Action showDialog);

        void ShowGlobalSettings();

        object MainWindow { get; }

        bool OpenFileGdb(string gdbPath, out string message);

        string CheckDataIntegrity();

        void ZoomToFullExtent();

        void ActivatePanTool();

        void ActivateZoomInTool();

        void ActivateZoomOutTool();

        void ActivateSelectFeaturesTool();

        void ClearMapSelection();

        void ActivateIdentifyTool();

        void ActivateMeasureLengthTool();

        void ActivateMeasureAreaTool();

        bool AddRasterLayer(string rasterPath, string layerName, out string message);

        bool AddFeatureLayer(string featureClassPath, string layerName, out string message);

        /// <summary>
        /// 移除地图中引用指定 File GDB 的图层（分析前释放结果栅格 schema lock）。
        /// </summary>
        int RemoveMapLayersFromGdb(string gdbPath);

        double MotivationWeight { get; set; }

        double FeasibilityWeight { get; set; }

        double CellSize { get; set; }

        string SkinName { get; set; }

        void ApplySkin(string skinName);

        string GetLogText();

        void LogInfo(string message);

        void LogWarn(string message);

        void LogError(string message);

        void ShowProgress(string caption, int percent);

        void HideProgress();

        void ShowMessage(string caption, string text);
    }
}

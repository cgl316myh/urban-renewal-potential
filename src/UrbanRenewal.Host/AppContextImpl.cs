using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using UrbanRenewal.Contracts;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Host
{
    internal sealed class AppContextImpl : IAppContext
    {
        private readonly MainRibbonForm _form;
        private GlobalAppSettings _settings;
        private Action _showGlobalSettings;

        public AppContextImpl(MainRibbonForm form)
        {
            _form = form;
            ReloadGlobalSettings();
        }

        public object MapControl
        {
            get { return _form.MapControl; }
        }

        public object TocControl
        {
            get { return _form.TocControl; }
        }

        public object DockManager
        {
            get { return null; }
        }

        public object MainWindow
        {
            get { return _form; }
        }

        public string GdbPath
        {
            get { return _settings != null ? _settings.InputGdbPath : null; }
            set
            {
                EnsureSettings();
                _settings.InputGdbPath = value;
            }
        }

        public string OutputGdbPath
        {
            get { return _settings != null ? _settings.OutputGdbPath : null; }
            set
            {
                EnsureSettings();
                _settings.OutputGdbPath = value;
            }
        }

        public string ActiveCityProfileId
        {
            get { return _settings != null ? _settings.ActiveCityProfileId : null; }
            set
            {
                EnsureSettings();
                _settings.ActiveCityProfileId = value;
            }
        }

        public string SpatialRefSourcePath
        {
            get { return _settings != null ? _settings.SpatialRefSourcePath : null; }
            set
            {
                EnsureSettings();
                _settings.SpatialRefSourcePath = value;
            }
        }

        public string SpatialRefLayerName
        {
            get { return _settings != null ? _settings.SpatialRefLayerName : null; }
            set
            {
                EnsureSettings();
                _settings.SpatialRefLayerName = value;
            }
        }

        public string SpatialRefName
        {
            get { return _settings != null ? _settings.SpatialRefName : null; }
            set
            {
                EnsureSettings();
                _settings.SpatialRefName = value;
            }
        }

        public int SpatialRefFactoryCode
        {
            get { return _settings != null ? _settings.SpatialRefFactoryCode : 0; }
            set
            {
                EnsureSettings();
                _settings.SpatialRefFactoryCode = value;
            }
        }

        public string ProjectMxdPath
        {
            get { return _settings != null ? _settings.ProjectMxdPath : null; }
            set
            {
                EnsureSettings();
                _settings.ProjectMxdPath = value;
            }
        }

        public double MotivationWeight
        {
            get { return _settings != null ? _settings.MotivationWeight : 0.7; }
            set
            {
                EnsureSettings();
                _settings.MotivationWeight = value;
            }
        }

        public double FeasibilityWeight
        {
            get { return _settings != null ? _settings.FeasibilityWeight : 0.3; }
            set
            {
                EnsureSettings();
                _settings.FeasibilityWeight = value;
            }
        }

        public string SkinName
        {
            get { return _settings != null ? _settings.SkinName : "Office 2013"; }
            set
            {
                EnsureSettings();
                _settings.SkinName = value;
            }
        }

        public void ApplySkin(string skinName)
        {
            if (string.IsNullOrEmpty(skinName))
            {
                return;
            }
            EnsureSettings();
            _settings.SkinName = skinName;
            try
            {
                DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle(skinName);
                LogInfo("已切换皮肤: " + skinName);
            }
            catch (Exception ex)
            {
                LogWarn("切换皮肤失败: " + ex.Message);
            }
        }

        public string GetLogText()
        {
            return _form != null ? _form.GetLogText() : string.Empty;
        }

        public void SaveGlobalSettings()
        {
            EnsureSettings();
            GlobalAppSettingsStore.Save(_settings);
            if (!string.IsNullOrEmpty(_settings.OutputGdbPath))
            {
                OutputGdbHelper.Remember(_settings.OutputGdbPath);
            }
            if (!string.IsNullOrEmpty(_settings.ActiveCityProfileId))
            {
                CityProfileStore.RememberId(_settings.ActiveCityProfileId);
            }
            LogInfo("全局设置已保存: " + GlobalAppSettingsStore.GetSettingsFilePath()
                + "；输出GDB=" + (_settings.OutputGdbPath ?? "(空)")
                + "；输入GDB=" + (_settings.InputGdbPath ?? "(空)")
                + "；城市=" + (_settings.ActiveCityProfileId ?? "(空)")
                + "；SpatialRef=" + (_settings.SpatialRefName ?? "(自动)")
                + "；工程MXD=" + (_settings.ProjectMxdPath ?? "(未保存)"));
            RefreshStatusBar();
        }

        public void ReloadGlobalSettings()
        {
            _settings = GlobalAppSettingsStore.Load() ?? new GlobalAppSettings();
            RefreshStatusBar();
        }

        public bool NewProject(out string message)
        {
            EnsureSettings();
            string oldMxd = _settings.ProjectMxdPath;

            MapWorkspaceService.ClearLayersFromObject(_form.MapControl);

            _settings.ClearWorkspaceSettings();
            GlobalAppSettingsStore.Save(_settings);

            // 清除活动城市记忆文件，避免 ResolveActive 仍读到旧城市
            try
            {
                string activeFile = Path.Combine(CityProfileStore.GetCitiesDirectory(), "_active_city.txt");
                if (System.IO.File.Exists(activeFile))
                {
                    System.IO.File.Delete(activeFile);
                }
            }
            catch
            {
            }

            if (!string.IsNullOrEmpty(oldMxd))
            {
                MapDocumentHelper.TryDeleteMxd(oldMxd);
            }
            MapDocumentHelper.TryDeleteMxd(MapDocumentHelper.GetDefaultProjectMxdPath());

            message = "已新建工程。\r\n"
                + "全局工作区配置已清空并写入：\r\n"
                + GlobalAppSettingsStore.GetSettingsFilePath()
                + "\r\n地图已清空，工程 MXD 已删除。";
            LogInfo(message);
            RefreshStatusBar();
            return true;
        }

        public bool SaveProject(out string message)
        {
            EnsureSettings();
            if (_form.MapControl == null)
            {
                message = "地图控件未就绪，无法保存工程。";
                return false;
            }

            // 先持久化当前全局配置
            GlobalAppSettingsStore.Save(_settings);

            string mxdPath = _settings.ProjectMxdPath;
            if (string.IsNullOrEmpty(mxdPath))
            {
                mxdPath = MapDocumentHelper.GetDefaultProjectMxdPath();
            }

            // 传入 AxMapControl，便于解析 IMxdContents
            string mxdMsg;
            if (!MapDocumentHelper.SaveMapToMxd(_form.MapControl, mxdPath, out mxdMsg))
            {
                message = mxdMsg;
                LogError(mxdMsg);
                return false;
            }

            _settings.ProjectMxdPath = mxdPath;
            GlobalAppSettingsStore.Save(_settings);

            message = "工程已保存（已持久化到本地）。\r\n"
                + "全局配置: " + GlobalAppSettingsStore.GetSettingsFilePath()
                + "\r\n地图文档: " + mxdPath
                + "\r\n下次启动将自动加载该 MXD。";
            LogInfo(mxdMsg);
            RefreshStatusBar();
            return true;
        }

        public bool TryLoadSavedProject(out string message)
        {
            EnsureSettings();
            string mxdPath = _settings.ProjectMxdPath;
            if (string.IsNullOrEmpty(mxdPath) || !System.IO.File.Exists(mxdPath))
            {
                // 兼容：仅有默认 MXD 时也尝试加载
                string def = MapDocumentHelper.GetDefaultProjectMxdPath();
                if (System.IO.File.Exists(def))
                {
                    mxdPath = def;
                    _settings.ProjectMxdPath = def;
                    GlobalAppSettingsStore.Save(_settings);
                }
                else
                {
                    message = null;
                    return false;
                }
            }
            if (_form.MapControl == null)
            {
                message = "地图控件未就绪，无法加载工程 MXD。";
                return false;
            }

            bool ok = MapDocumentHelper.LoadMxdToMap(_form.MapControl, mxdPath, out message);
            if (ok)
            {
                LogInfo(message);
                try
                {
                    if (_form.TocControl != null && _form.TocControl.Object != null)
                    {
                        ESRI.ArcGIS.Controls.ITOCControl2 toc =
                            _form.TocControl.Object as ESRI.ArcGIS.Controls.ITOCControl2;
                        if (toc != null)
                        {
                            toc.Update();
                        }
                    }
                }
                catch
                {
                }
                RefreshStatusBar();
            }
            else
            {
                LogWarn(message);
            }
            return ok;
        }

        public void RegisterGlobalSettingsUI(Action showDialog)
        {
            _showGlobalSettings = showDialog;
        }

        public void ShowGlobalSettings()
        {
            if (_showGlobalSettings != null)
            {
                _showGlobalSettings();
                ReloadGlobalSettings();
                return;
            }
            ShowMessage("全局设置",
                "全局设置界面未就绪。请通过 Ribbon「数据管理 → 全局设置」打开。");
        }

        private void RefreshStatusBar()
        {
            if (_form == null)
            {
                return;
            }
            string city = _settings != null ? _settings.ActiveCityProfileId : null;
            string outGdb = _settings != null ? _settings.OutputGdbPath : null;
            string outShort = string.IsNullOrEmpty(outGdb)
                ? "输出GDB未设"
                : System.IO.Path.GetFileName(outGdb);
            string cityShort = string.IsNullOrEmpty(city) ? "城市未设" : city;
            _form.SetStatus("就绪 | " + cityShort + " | " + outShort);
        }

        private void EnsureSettings()
        {
            if (_settings == null)
            {
                _settings = new GlobalAppSettings();
            }
        }

        public bool OpenFileGdb(string gdbPath, out string message)
        {
            GdbPath = gdbPath;
            if (_form.MapControl == null || _form.MapControl.Object == null)
            {
                message = "地图控件未就绪，无法加载 GDB。";
                return false;
            }

            IMapControl3 map = _form.MapControl.Object as IMapControl3;
            int count = MapWorkspaceService.LoadFileGdb(map, gdbPath, out message);
            if (count > 0)
            {
                if (string.IsNullOrEmpty(OutputGdbPath))
                {
                    OutputGdbPath = OutputGdbHelper.SuggestDefaultBesideInput(gdbPath);
                }
                SaveGlobalSettings();
            }
            return count > 0;
        }

        public string CheckDataIntegrity()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(MapWorkspaceService.CheckIntegrity(GdbPath));

            sb.AppendLine("全局输出 GDB: "
                + (string.IsNullOrEmpty(OutputGdbPath) ? "(未设置 — 请在「数据管理 → 全局设置」中指定)" : OutputGdbPath));

            CityProfile profile = CityProfileStore.ResolveActive(ActiveCityProfileId);
            if (profile != null)
            {
                CityProfileStore.NormalizeWeights(profile);
                System.Collections.Generic.List<string> names = WorkspaceCatalog.ListFeatureClassNames(GdbPath);
                sb.AppendLine();
                sb.Append(profile.BuildLayerPresenceReport(names));

                string reqMsg;
                if (!profile.ValidateRequired(names, out reqMsg) && !string.IsNullOrEmpty(reqMsg))
                {
                    sb.AppendLine("[警告] 必选图层未齐备，动力性分析将被拒绝：");
                    sb.Append(reqMsg);
                }

                if (!string.IsNullOrEmpty(profile.PreferredCrsName))
                {
                    SpatialReferenceAuditResult audit = SpatialReferenceAudit.Audit(
                        GdbPath, null, SpatialRefSourcePath, SpatialRefLayerName);
                    if (audit.Success && !string.IsNullOrEmpty(audit.ReferenceSpatialReferenceName)
                        && audit.ReferenceSpatialReferenceName.IndexOf(profile.PreferredCrsName, StringComparison.OrdinalIgnoreCase) < 0
                        && profile.PreferredCrsName.IndexOf(audit.ReferenceSpatialReferenceName, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        sb.AppendLine("[警告] 当前基准坐标系「" + audit.ReferenceSpatialReferenceName
                            + "」与城市配置建议「" + profile.PreferredCrsName + "」不一致");
                    }
                }
            }
            else
            {
                sb.AppendLine();
                sb.AppendLine("[提示] 未选择城市配置。请打开「数据管理 → 全局设置」选择或生成城市配置。");
            }

            return sb.ToString();
        }

        public void ZoomToFullExtent()
        {
            if (_form.MapControl != null && _form.MapControl.Object != null)
            {
                MapWorkspaceService.ZoomToFullExtent((IMapControl3)_form.MapControl.Object);
            }
        }

        public void ActivatePanTool()
        {
            if (_form.MapControl != null && _form.MapControl.Object != null)
            {
                MapWorkspaceService.ActivatePan((IMapControl3)_form.MapControl.Object);
            }
        }

        public void ActivateZoomInTool()
        {
            if (_form.MapControl != null && _form.MapControl.Object != null)
            {
                MapWorkspaceService.ActivateZoomIn((IMapControl3)_form.MapControl.Object);
            }
        }

        public bool AddRasterLayer(string rasterPath, string layerName, out string message)
        {
            if (_form.MapControl == null || _form.MapControl.Object == null)
            {
                message = "地图控件未就绪。";
                return false;
            }

            return RasterLayerHelper.AddRasterToMap(
                (IMapControl3)_form.MapControl.Object,
                rasterPath,
                layerName,
                out message);
        }

        public bool AddFeatureLayer(string featureClassPath, string layerName, out string message)
        {
            if (_form.MapControl == null || _form.MapControl.Object == null)
            {
                message = "地图控件未就绪。";
                return false;
            }

            return FeatureLayerHelper.AddFeatureClassToMap(
                (IMapControl3)_form.MapControl.Object,
                featureClassPath,
                layerName,
                out message);
        }

        public void LogInfo(string message)
        {
            _form.AppendLog("INFO", message);
        }

        public void LogWarn(string message)
        {
            _form.AppendLog("WARN", message);
        }

        public void LogError(string message)
        {
            _form.AppendLog("ERROR", message);
        }

        public void ShowProgress(string caption, int percent)
        {
            // 仅更新状态栏；明细日志由各窗体 OnProgress → LogInfo，避免重复刷列表卡 UI
            string line = "[" + percent + "%] " + (caption ?? string.Empty);
            _form.SetStatus(line);
            _form.EnsureLogPanelVisible();
        }

        public void HideProgress()
        {
            _form.SetStatus("就绪");
        }

        public void ShowMessage(string caption, string text)
        {
            MessageBox.Show(_form, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

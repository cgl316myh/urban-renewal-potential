using System;
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
            LogInfo("全局设置已保存: 输出GDB=" + (_settings.OutputGdbPath ?? "(空)")
                + "；城市=" + (_settings.ActiveCityProfileId ?? "(空)"));
            RefreshStatusBar();
        }

        public void ReloadGlobalSettings()
        {
            _settings = GlobalAppSettingsStore.Load() ?? new GlobalAppSettings();
            RefreshStatusBar();
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
                    SpatialReferenceAuditResult audit = SpatialReferenceAudit.Audit(GdbPath);
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
            _form.SetStatus(caption + " " + percent + "%");
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

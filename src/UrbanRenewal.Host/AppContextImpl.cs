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

        public AppContextImpl(MainRibbonForm form)
        {
            _form = form;
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

        public string GdbPath { get; set; }

        public string ActiveCityProfileId { get; set; }

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
            return count > 0;
        }

        public string CheckDataIntegrity()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(MapWorkspaceService.CheckIntegrity(GdbPath));

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
                sb.AppendLine("[提示] 未找到城市配置（Config\\Cities\\*.xml）。");
                sb.AppendLine("换城市步骤：动力性分析窗体 →「从 GDB 生成配置」或复制 _Template.xml 填写图层名。");
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

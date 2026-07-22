using System;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using UrbanRenewal.Contracts;
using UrbanRenewal.GIS;

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
            return MapWorkspaceService.CheckIntegrity(GdbPath);
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

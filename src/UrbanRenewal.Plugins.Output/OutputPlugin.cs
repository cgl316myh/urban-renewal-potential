using System;
using System.Windows.Forms;
using UrbanRenewal.Contracts;

namespace UrbanRenewal.Plugins.Output
{
    /// <summary>成果输出插件。</summary>
    public sealed class OutputPlugin : IModulePlugin
    {
        private IAppContext _context;
        private OutputRunForm _openForm;

        public string Id { get { return "Output"; } }
        public string Name { get { return "成果输出"; } }
        public int Order { get { return 60; } }

        public void Initialize(IAppContext context)
        {
            _context = context;
            if (_context != null)
            {
                _context.LogInfo("成果输出插件已初始化。");
            }
        }

        public void RegisterRibbon(IRibbonHost ribbonHost)
        {
            if (ribbonHost == null)
            {
                return;
            }
            object page = ribbonHost.AddPage("成果输出");
            object group = ribbonHost.AddGroup(page, "导出");
            ribbonHost.AddButton(group, "导出评价成果", OnExportData);
            ribbonHost.AddButton(group, "导出地图PDF", OnExportPdf);
            ribbonHost.AddButton(group, "导出地图TIFF", OnExportTiff);
        }

        public void Shutdown()
        {
            _context = null;
        }

        private void OnExportData(object sender, EventArgs e)
        {
            IWin32Window owner = _context != null ? _context.MainWindow as IWin32Window : null;
            ModelessFormHelper.ShowOrActivate(
                ref _openForm,
                delegate { return new OutputRunForm(_context); },
                owner);
        }

        private void OnExportPdf(object sender, EventArgs e)
        {
            ExportMap(true);
        }

        private void OnExportTiff(object sender, EventArgs e)
        {
            ExportMap(false);
        }

        private void ExportMap(bool pdf)
        {
            if (_context == null)
            {
                return;
            }
            using (SaveFileDialog dlg = new SaveFileDialog())
            {
                dlg.Filter = pdf ? "PDF|*.pdf" : "TIFF|*.tif";
                dlg.FileName = pdf ? "potential_map.pdf" : "potential_map.tif";
                if (dlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }
                string msg;
                bool ok = pdf
                    ? UrbanRenewal.GIS.MapExportHelper.ExportToPdf(_context.MapControl, dlg.FileName, out msg)
                    : UrbanRenewal.GIS.MapExportHelper.ExportToTiff(_context.MapControl, dlg.FileName, out msg);
                _context.LogInfo(msg);
                MessageBox.Show(msg, "成果输出", MessageBoxButtons.OK,
                    ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
        }
    }
}

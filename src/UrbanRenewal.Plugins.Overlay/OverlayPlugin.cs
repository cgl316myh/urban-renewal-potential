using System;
using System.Windows.Forms;
using UrbanRenewal.Contracts;

namespace UrbanRenewal.Plugins.Overlay
{
    /// <summary>
    /// M4 叠置评价 + M5 宗地关联（潜力分析流程第 3–4 步）。
    /// </summary>
    public sealed class OverlayPlugin : IModulePlugin
    {
        private IAppContext _context;
        private OverlayRunForm _openOverlay;
        private ParcelLinkRunForm _openParcel;

        public string Id
        {
            get { return "Overlay"; }
        }

        public string Name
        {
            get { return "潜力分析-叠置宗地"; }
        }

        public int Order
        {
            get { return 40; }
        }

        public void Initialize(IAppContext context)
        {
            _context = context;
            if (_context != null)
            {
                _context.LogInfo("叠置与宗地关联插件已初始化。");
            }
        }

        public void RegisterRibbon(IRibbonHost ribbonHost)
        {
            if (ribbonHost == null)
            {
                return;
            }

            object page = ribbonHost.AddPage("潜力分析");
            object group = ribbonHost.AddGroup(page, "分析流程");
            ribbonHost.AddButton(group, "运行综合潜力叠置", OnRunOverlay);
            ribbonHost.AddButton(group, "运行宗地关联", OnRunParcelLink);
            ribbonHost.AddButton(group, "查看选中宗地详情", OnShowParcelDetail);
        }

        public void Shutdown()
        {
            if (_context != null)
            {
                _context.LogInfo("叠置与宗地关联插件已关闭。");
            }
            _context = null;
        }

        private IWin32Window Owner
        {
            get { return _context != null ? _context.MainWindow as IWin32Window : null; }
        }

        private void OnRunOverlay(object sender, EventArgs e)
        {
            ModelessFormHelper.ShowOrActivate(
                ref _openOverlay,
                delegate { return new OverlayRunForm(_context); },
                Owner);
        }

        private void OnRunParcelLink(object sender, EventArgs e)
        {
            ModelessFormHelper.ShowOrActivate(
                ref _openParcel,
                delegate { return new ParcelLinkRunForm(_context); },
                Owner);
        }

        private void OnShowParcelDetail(object sender, EventArgs e)
        {
            string text;
            if (!ParcelDetailHelper.TryDescribeSelection(_context, out text))
            {
                MessageBox.Show(text, "宗地详情", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            MessageBox.Show(text, "选中宗地详情", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

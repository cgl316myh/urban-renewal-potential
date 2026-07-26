using System;
using System.Windows.Forms;
using UrbanRenewal.Contracts;

namespace UrbanRenewal.Plugins.Overlay
{
    /// <summary>
    /// M4 叠置评价 + M5 宗地关联（P4）。
    /// </summary>
    public sealed class OverlayPlugin : IModulePlugin
    {
        private IAppContext _context;

        public string Id
        {
            get { return "Overlay"; }
        }

        public string Name
        {
            get { return "叠置与宗地关联"; }
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

            object pageOverlay = ribbonHost.AddPage("叠置评价");
            object groupOverlay = ribbonHost.AddGroup(pageOverlay, "综合潜力");
            ribbonHost.AddButton(groupOverlay, "运行综合潜力叠置", OnRunOverlay);

            object pageParcel = ribbonHost.AddPage("宗地关联");
            object groupParcel = ribbonHost.AddGroup(pageParcel, "赋值");
            ribbonHost.AddButton(groupParcel, "运行宗地关联", OnRunParcelLink);
            ribbonHost.AddButton(groupParcel, "查看选中宗地详情", OnShowParcelDetail);
        }

        public void Shutdown()
        {
            if (_context != null)
            {
                _context.LogInfo("叠置与宗地关联插件已关闭。");
            }
            _context = null;
        }

        private void OnRunOverlay(object sender, EventArgs e)
        {
            using (OverlayRunForm form = new OverlayRunForm(_context))
            {
                form.ShowDialog();
            }
        }

        private void OnRunParcelLink(object sender, EventArgs e)
        {
            using (ParcelLinkRunForm form = new ParcelLinkRunForm(_context))
            {
                form.ShowDialog();
            }
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

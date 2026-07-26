using System;
using System.Windows.Forms;
using UrbanRenewal.Contracts;

namespace UrbanRenewal.Plugins.Feasibility
{
    /// <summary>
    /// M3 可行度分析插件。
    /// </summary>
    public sealed class FeasibilityPlugin : IModulePlugin
    {
        private IAppContext _context;

        public string Id
        {
            get { return "Feasibility"; }
        }

        public string Name
        {
            get { return "可行度分析"; }
        }

        public int Order
        {
            get { return 30; }
        }

        public void Initialize(IAppContext context)
        {
            _context = context;
            if (_context != null)
            {
                _context.LogInfo("可行度分析插件已初始化。");
            }
        }

        public void RegisterRibbon(IRibbonHost ribbonHost)
        {
            if (ribbonHost == null)
            {
                return;
            }

            object page = ribbonHost.AddPage("可行度分析");
            object group = ribbonHost.AddGroup(page, "评价");
            ribbonHost.AddButton(group, "运行可行度分析", OnRun);
        }

        public void Shutdown()
        {
            if (_context != null)
            {
                _context.LogInfo("可行度分析插件已关闭。");
            }
            _context = null;
        }

        private void OnRun(object sender, EventArgs e)
        {
            using (FeasibilityRunForm form = new FeasibilityRunForm(_context))
            {
                form.ShowDialog();
            }
        }
    }
}

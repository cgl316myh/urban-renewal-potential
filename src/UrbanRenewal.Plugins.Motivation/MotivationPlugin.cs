using System;
using System.Windows.Forms;
using UrbanRenewal.Contracts;

namespace UrbanRenewal.Plugins.Motivation
{
    /// <summary>
    /// M2 动力性分析插件。
    /// </summary>
    public sealed class MotivationPlugin : IModulePlugin
    {
        private IAppContext _context;

        public string Id
        {
            get { return "Motivation"; }
        }

        public string Name
        {
            get { return "动力性分析"; }
        }

        public int Order
        {
            get { return 20; }
        }

        public void Initialize(IAppContext context)
        {
            _context = context;
            if (_context != null)
            {
                _context.LogInfo("动力性分析插件已初始化。");
            }
        }

        public void RegisterRibbon(IRibbonHost ribbonHost)
        {
            if (ribbonHost == null)
            {
                return;
            }

            object page = ribbonHost.AddPage("动力性分析");
            object group = ribbonHost.AddGroup(page, "评价");
            ribbonHost.AddButton(group, "运行动力性分析", OnRun);
        }

        public void Shutdown()
        {
            if (_context != null)
            {
                _context.LogInfo("动力性分析插件已关闭。");
            }
            _context = null;
        }

        private void OnRun(object sender, EventArgs e)
        {
            using (MotivationRunForm form = new MotivationRunForm(_context))
            {
                form.ShowDialog();
            }
        }
    }
}

using System;
using System.Windows.Forms;
using UrbanRenewal.Contracts;

namespace UrbanRenewal.Plugins.Feasibility
{
    /// <summary>
    /// M3 可行度分析（潜力分析流程第 2 步）。
    /// </summary>
    public sealed class FeasibilityPlugin : IModulePlugin
    {
        private IAppContext _context;
        private FeasibilityRunForm _openForm;

        public string Id
        {
            get { return "Feasibility"; }
        }

        public string Name
        {
            get { return "潜力分析-可行度"; }
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

            object page = ribbonHost.AddPage("潜力分析");
            object group = ribbonHost.AddGroup(page, "分析流程");
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
            IWin32Window owner = _context != null ? _context.MainWindow as IWin32Window : null;
            ModelessFormHelper.ShowOrActivate(
                ref _openForm,
                delegate { return new FeasibilityRunForm(_context); },
                owner);
        }
    }
}

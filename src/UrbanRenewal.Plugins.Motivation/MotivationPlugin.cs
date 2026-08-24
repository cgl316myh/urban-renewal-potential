using System;
using System.Windows.Forms;
using UrbanRenewal.Contracts;

namespace UrbanRenewal.Plugins.Motivation
{
    /// <summary>动力性分析插件。</summary>
    public sealed class MotivationPlugin : IModulePlugin
    {
        private IAppContext _context;
        private MotivationRunForm _openForm;

        public string Id
        {
            get { return "Motivation"; }
        }

        public string Name
        {
            get { return "潜力分析-动力性"; }
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

            object page = ribbonHost.AddPage("潜力分析");
            object group = ribbonHost.AddGroup(page, "分析流程");
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
            IWin32Window owner = _context != null ? _context.MainWindow as IWin32Window : null;
            ModelessFormHelper.ShowOrActivate(
                ref _openForm,
                delegate { return new MotivationRunForm(_context); },
                owner);
        }
    }
}

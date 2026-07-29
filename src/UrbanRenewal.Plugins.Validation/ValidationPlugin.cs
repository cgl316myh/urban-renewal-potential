using System;
using System.Windows.Forms;
using UrbanRenewal.Contracts;

namespace UrbanRenewal.Plugins.Validation
{
    /// <summary>
    /// M6 验证校核（潜力分析流程第 5 步）。
    /// </summary>
    public sealed class ValidationPlugin : IModulePlugin
    {
        private IAppContext _context;
        private ValidationRunForm _openForm;

        public string Id { get { return "Validation"; } }
        public string Name { get { return "潜力分析-验证校核"; } }
        public int Order { get { return 50; } }

        public void Initialize(IAppContext context)
        {
            _context = context;
            if (_context != null)
            {
                _context.LogInfo("验证校核插件已初始化。");
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
            ribbonHost.AddButton(group, "运行验证校准", OnRun);
        }

        public void Shutdown()
        {
            _context = null;
        }

        private void OnRun(object sender, EventArgs e)
        {
            IWin32Window owner = _context != null ? _context.MainWindow as IWin32Window : null;
            ModelessFormHelper.ShowOrActivate(
                ref _openForm,
                delegate { return new ValidationRunForm(_context); },
                owner);
        }
    }
}

using System;
using System.Windows.Forms;
using UrbanRenewal.Contracts;

namespace UrbanRenewal.Plugins.Validation
{
    public sealed class ValidationPlugin : IModulePlugin
    {
        private IAppContext _context;

        public string Id { get { return "Validation"; } }
        public string Name { get { return "验证校核"; } }
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
            object page = ribbonHost.AddPage("验证校核");
            object group = ribbonHost.AddGroup(page, "对标");
            ribbonHost.AddButton(group, "运行验证校核", OnRun);
        }

        public void Shutdown()
        {
            _context = null;
        }

        private void OnRun(object sender, EventArgs e)
        {
            using (ValidationRunForm form = new ValidationRunForm(_context))
            {
                form.ShowDialog();
            }
        }
    }
}

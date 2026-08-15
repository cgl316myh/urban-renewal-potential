using System;
using System.Windows.Forms;
using UrbanRenewal.Contracts;

namespace UrbanRenewal.Plugins.Config
{
    public sealed class ConfigPlugin : IModulePlugin
    {
        private IAppContext _context;

        public string Id { get { return "Config"; } }
        public string Name { get { return "系统配置"; } }
        public int Order { get { return 70; } }

        public void Initialize(IAppContext context)
        {
            _context = context;
            if (_context != null)
            {
                _context.LogInfo("系统配置插件已初始化。");
            }
        }

        public void RegisterRibbon(IRibbonHost ribbonHost)
        {
            if (ribbonHost == null)
            {
                return;
            }
            object page = ribbonHost.AddPage("系统配置");
            object group = ribbonHost.AddGroup(page, "参数");
            ribbonHost.AddButton(group, "权重与缓冲赋分", OnOpen);
            ribbonHost.AddButton(group, "查看分析日志", OnLog);
        }

        public void Shutdown()
        {
            _context = null;
        }

        private void OnOpen(object sender, EventArgs e)
        {
            using (SystemConfigForm form = new SystemConfigForm(_context))
            {
                form.ShowDialog();
            }
        }

        private void OnLog(object sender, EventArgs e)
        {
            if (_context == null)
            {
                return;
            }
            string text = _context.GetLogText();
            if (string.IsNullOrEmpty(text))
            {
                text = "（暂无日志）";
            }
            MessageBox.Show(text, "分析日志（最近）", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

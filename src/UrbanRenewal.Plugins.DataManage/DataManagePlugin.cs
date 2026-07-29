using System;
using System.Windows.Forms;
using UrbanRenewal.Contracts;

namespace UrbanRenewal.Plugins.DataManage
{
    /// <summary>
    /// M1 数据管理插件：新建/保存工程、全局设置、数据配置、完整性检查、预处理。
    /// </summary>
    public sealed class DataManagePlugin : IModulePlugin
    {
        private IAppContext _context;
        private PreprocessForm _openPreprocess;

        public string Id
        {
            get { return "DataManage"; }
        }

        public string Name
        {
            get { return "数据管理"; }
        }

        public int Order
        {
            get { return 10; }
        }

        public void Initialize(IAppContext context)
        {
            _context = context;
            if (_context != null)
            {
                _context.LogInfo("数据管理插件已初始化。");
                _context.RegisterGlobalSettingsUI(ShowGlobalSettingsDialog);
            }
        }

        private void ShowGlobalSettingsDialog()
        {
            using (GlobalSettingsForm form = new GlobalSettingsForm(_context))
            {
                form.ShowDialog();
            }
        }

        public void RegisterRibbon(IRibbonHost ribbonHost)
        {
            if (ribbonHost == null)
            {
                return;
            }

            object page = ribbonHost.AddPage("数据管理");
            object group = ribbonHost.AddGroup(page, "工作空间");

            ribbonHost.AddButton(group, "新建工程", OnNewProject);
            ribbonHost.AddButton(group, "保存工程", OnSaveProject);
            ribbonHost.AddButton(group, "全局设置", OnGlobalSettings);
            ribbonHost.AddButton(group, "数据配置", OnDataConfig);
            ribbonHost.AddButton(group, "数据完整性检查", OnValidateData);
            ribbonHost.AddButton(group, "投影/裁剪预处理", OnPreprocess);
        }

        public void Shutdown()
        {
            if (_context != null)
            {
                _context.LogInfo("数据管理插件已关闭。");
            }
            _context = null;
        }

        private void OnNewProject(object sender, EventArgs e)
        {
            if (_context == null)
            {
                return;
            }

            DialogResult dr = MessageBox.Show(
                "新建工程将清空全局设置中的工作区配置（输入/输出 GDB、城市、基准坐标系等），\r\n"
                + "并清空当前地图。此操作会写入本地配置文件。\r\n\r\n是否继续？",
                "新建工程",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (dr != DialogResult.Yes)
            {
                return;
            }

            string message;
            if (_context.NewProject(out message))
            {
                _context.ShowMessage("新建工程", message);
            }
            else
            {
                _context.ShowMessage("新建工程", message ?? "新建失败。");
            }
        }

        private void OnSaveProject(object sender, EventArgs e)
        {
            if (_context == null)
            {
                return;
            }

            // 确保内存中的全局设置已落盘
            _context.SaveGlobalSettings();

            string message;
            if (_context.SaveProject(out message))
            {
                _context.ShowMessage("保存工程", message);
            }
            else
            {
                _context.ShowMessage("保存工程", message ?? "保存失败。");
            }
        }

        private void OnDataConfig(object sender, EventArgs e)
        {
            if (_context == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(_context.GdbPath) || !System.IO.Directory.Exists(_context.GdbPath))
            {
                _context.ShowMessage("数据配置", "请先在「全局设置」中指定并打开输入 GDB，再进行图层角色映射。");
                return;
            }

            using (DataConfigForm form = new DataConfigForm(_context))
            {
                form.ShowDialog();
            }
        }

        private void OnGlobalSettings(object sender, EventArgs e)
        {
            if (_context == null)
            {
                return;
            }
            _context.ShowGlobalSettings();
        }

        private void OnValidateData(object sender, EventArgs e)
        {
            if (_context == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(_context.GdbPath))
            {
                _context.ShowMessage("数据完整性检查", "请先在「全局设置」中指定输入 GDB。");
                return;
            }

            _context.LogInfo("开始数据完整性检查: " + _context.GdbPath);
            using (DataIntegrityForm form = new DataIntegrityForm(_context))
            {
                form.ShowDialog();
            }
        }

        private void OnPreprocess(object sender, EventArgs e)
        {
            if (_context == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(_context.GdbPath))
            {
                _context.ShowMessage("预处理", "请先在「全局设置」中指定输入 GDB。");
                return;
            }

            IWin32Window owner = _context.MainWindow as IWin32Window;
            ModelessFormHelper.ShowOrActivate(
                ref _openPreprocess,
                delegate { return new PreprocessForm(_context); },
                owner);
        }
    }
}

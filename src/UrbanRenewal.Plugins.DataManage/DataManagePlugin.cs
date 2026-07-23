using System;
using System.Windows.Forms;
using UrbanRenewal.Contracts;

namespace UrbanRenewal.Plugins.DataManage
{
    /// <summary>
    /// M1 数据管理插件：打开 GDB、完整性检查、全局设置、预处理入口。
    /// </summary>
    public sealed class DataManagePlugin : IModulePlugin
    {
        private IAppContext _context;

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

            ribbonHost.AddButton(group, "打开 GDB", OnOpenGdb);
            ribbonHost.AddButton(group, "全局设置", OnGlobalSettings);
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

        private void OnOpenGdb(object sender, EventArgs e)
        {
            if (_context == null)
            {
                return;
            }

            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "选择 File Geodatabase 文件夹（*.gdb）";
                if (!string.IsNullOrEmpty(_context.GdbPath))
                {
                    dlg.SelectedPath = _context.GdbPath;
                }

                if (dlg.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                string path = dlg.SelectedPath;
                _context.ShowProgress("加载 GDB", 10);
                string message;
                bool ok = _context.OpenFileGdb(path, out message);
                _context.HideProgress();

                if (ok)
                {
                    _context.LogInfo(message);
                    _context.ZoomToFullExtent();
                    string tip = message;
                    tip += "\r\n\r\n全局输出 GDB:\r\n"
                        + (string.IsNullOrEmpty(_context.OutputGdbPath)
                            ? "(未设置 — 请打开「全局设置」指定；后续所有分析结果均写入该库)"
                            : _context.OutputGdbPath);
                    tip += "\r\n城市配置: " + (_context.ActiveCityProfileId ?? "(未设置)");
                    _context.ShowMessage("打开 GDB", tip);
                }
                else
                {
                    _context.LogError(message);
                    _context.ShowMessage("打开 GDB", message);
                }
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
                _context.ShowMessage("数据完整性检查", "请先打开 GDB 工作空间。");
                return;
            }

            _context.LogInfo("开始数据完整性检查: " + _context.GdbPath);
            string report = _context.CheckDataIntegrity();
            _context.LogInfo(report.Replace("\r\n", " | "));
            _context.ShowMessage("数据完整性检查", report);
        }

        private void OnPreprocess(object sender, EventArgs e)
        {
            if (_context == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(_context.GdbPath))
            {
                _context.ShowMessage("预处理", "请先打开 GDB 工作空间。");
                return;
            }

            _context.LogInfo("投影/裁剪预处理：待接 Geoprocessor（统一 CGCS2000 + 建成区 Clip）。");
            _context.ShowMessage("预处理",
                "当前输入 GDB:\r\n" + _context.GdbPath +
                "\r\n\r\n全局输出 GDB:\r\n" + (_context.OutputGdbPath ?? "(未设置)") +
                "\r\n\r\n下一步将调用投影统一与建成区 Clip（结果写入输出 GDB）。");
        }
    }
}

using System;
using System.Windows.Forms;
using UrbanRenewal.Contracts;

namespace UrbanRenewal.Plugins.DataManage
{
    /// <summary>
    /// M1 数据管理插件样例：动态注册 Ribbon「数据管理」页签。
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
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "选择 File Geodatabase 文件夹（*.gdb）";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if (_context != null)
                    {
                        _context.GdbPath = dlg.SelectedPath;
                        _context.LogInfo("已选择 GDB: " + dlg.SelectedPath);
                        _context.ShowMessage("打开 GDB", "工作空间已设置为:\r\n" + dlg.SelectedPath);
                    }
                }
            }
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

            // P1：命令桩；后续检查建成区/宗地必选图层与坐标系
            _context.LogInfo("开始数据完整性检查: " + _context.GdbPath);
            _context.ShowMessage("数据完整性检查", "检查逻辑将在 P1-6 完善（必选图层、坐标系、字段）。");
        }

        private void OnPreprocess(object sender, EventArgs e)
        {
            if (_context == null)
            {
                return;
            }

            _context.LogInfo("投影/裁剪预处理命令已触发（待接 ArcEngine GP）。");
            _context.ShowMessage("预处理", "将调用投影统一与建成区 Clip（P1-6 / P2 前完善）。");
        }
    }
}

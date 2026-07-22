using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraBars;
using UrbanRenewal.Model;
using UrbanRenewal.PluginLoader;

namespace UrbanRenewal.Host
{
    /// <summary>
    /// DevExpress 13.1 Ribbon 主壳。界面布局在设计器中编辑；业务页签由插件在运行时注册。
    /// </summary>
    public partial class MainRibbonForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private readonly AppSettings _settings = new AppSettings();
        private PluginManager _pluginManager;
        private AppContextImpl _appContext;
        private RibbonHostImpl _ribbonHost;

        public MainRibbonForm()
        {
            InitializeComponent();

            // 设计器打开时不加载插件，避免设计时反射扫描 Plugins 目录
            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                LoadPlugins();
            }
        }

        private void LoadPlugins()
        {
            _appContext = new AppContextImpl(this);
            _ribbonHost = new RibbonHostImpl(this.ribbonControl);
            _pluginManager = new PluginManager(
                delegate(string m) { AppendLog("INFO", m); },
                delegate(string m) { AppendLog("ERROR", m); });

            string pluginsDir = Path.Combine(Application.StartupPath, _settings.PluginsDirectoryName);
            _pluginManager.LoadAll(pluginsDir);
            _pluginManager.InitializeAll(_appContext, _ribbonHost);
            this.barStaticStatus.Caption = "插件已加载: " + _pluginManager.Plugins.Count;
        }

        internal void AppendLog(string level, string message)
        {
            string line = DateTime.Now.ToString("HH:mm:ss") + " [" + level + "] " + message;
            if (InvokeRequired)
            {
                BeginInvoke(new Action(delegate { this.listBoxLog.Items.Insert(0, line); }));
            }
            else
            {
                this.listBoxLog.Items.Insert(0, line);
            }
        }

        internal void SetStatus(string text)
        {
            this.barStaticStatus.Caption = text;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_pluginManager != null)
            {
                _pluginManager.UnloadAll();
            }
            base.OnFormClosing(e);
        }

        private void btnMapFit_ItemClick(object sender, ItemClickEventArgs e)
        {
            AppendLog("INFO", "地图全图命令（待绑定 ESRI ICommand）");
        }

        private void btnMapPan_ItemClick(object sender, ItemClickEventArgs e)
        {
            AppendLog("INFO", "地图漫游命令（待绑定 ESRI ICommand）");
        }

        private void btnMapZoomIn_ItemClick(object sender, ItemClickEventArgs e)
        {
            AppendLog("INFO", "地图放大命令（待绑定 ESRI ICommand）");
        }
    }
}

using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using ESRI.ArcGIS.Controls;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;
using UrbanRenewal.PluginLoader;

namespace UrbanRenewal.Host
{
    /// <summary>
    /// DevExpress 13.1 Ribbon 主壳。界面布局在设计器中编辑；Ax 地图控件运行时嵌入面板。
    /// </summary>
    public partial class MainRibbonForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private readonly AppSettings _settings = new AppSettings();
        private PluginManager _pluginManager;
        private AppContextImpl _appContext;
        private RibbonHostImpl _ribbonHost;

        private AxMapControl _axMapControl;
        private AxTOCControl _axTocControl;

        public MainRibbonForm()
        {
            InitializeComponent();
            ApplyRibbonLargeImages();

            // 设计器打开时不创建 AO 对象、不加载插件
            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                CreateArcEngineControls();
                LoadPlugins();
            }
        }

        /// <summary>
        /// 为主界面 Ribbon 按钮设置 LargeGlyph（大图标）。
        /// </summary>
        private void ApplyRibbonLargeImages()
        {
            RibbonHostImpl.ApplyLargeImage(this.btnMapFit, this.btnMapFit.Caption);
            RibbonHostImpl.ApplyLargeImage(this.btnMapPan, this.btnMapPan.Caption);
            RibbonHostImpl.ApplyLargeImage(this.btnMapZoomIn, this.btnMapZoomIn.Caption);
        }

        internal AxMapControl MapControl
        {
            get { return _axMapControl; }
        }

        internal AxTOCControl TocControl
        {
            get { return _axTocControl; }
        }

        private void CreateArcEngineControls()
        {
            try
            {
                _axMapControl = new AxMapControl();
                ((ISupportInitialize)_axMapControl).BeginInit();
                _axMapControl.Dock = DockStyle.Fill;
                _axMapControl.Name = "axMapControl";
                this.panelMap.Controls.Clear();
                this.panelMap.Controls.Add(_axMapControl);
                ((ISupportInitialize)_axMapControl).EndInit();

                _axTocControl = new AxTOCControl();
                ((ISupportInitialize)_axTocControl).BeginInit();
                _axTocControl.Dock = DockStyle.Fill;
                _axTocControl.Name = "axTocControl";
                this.panelToc.Controls.Clear();
                this.panelToc.Controls.Add(_axTocControl);
                ((ISupportInitialize)_axTocControl).EndInit();

                _axTocControl.SetBuddyControl(_axMapControl);
                AppendLog("INFO", "ArcEngine 地图控件已嵌入。");
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", "创建地图控件失败: " + ex.Message);
                MessageBox.Show(this,
                    "无法创建 ArcEngine 地图控件。\r\n" + ex.Message,
                    "ArcEngine",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
            AppendLog("INFO", "StartupPath=" + Application.StartupPath);
            _pluginManager.LoadAll(pluginsDir);
            _pluginManager.InitializeAll(_appContext, _ribbonHost);
            this.barStaticStatus.Caption = "插件已加载: " + _pluginManager.Plugins.Count;

            if (_pluginManager.Plugins.Count == 0)
            {
                AppendLog("WARN", "未加载到业务插件。请生成整个解决方案，确认 Plugins 下有 UrbanRenewal.Plugins.*.dll");
            }
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
            ArcEngineBootstrap.Shutdown();
            base.OnFormClosing(e);
        }

        private void btnMapFit_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_axMapControl == null)
            {
                AppendLog("WARN", "地图控件未就绪。");
                return;
            }
            MapWorkspaceService.ZoomToFullExtent((IMapControl3)_axMapControl.Object);
            AppendLog("INFO", "已缩放到全图。");
        }

        private void btnMapPan_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_axMapControl == null)
            {
                AppendLog("WARN", "地图控件未就绪。");
                return;
            }
            MapWorkspaceService.ActivatePan((IMapControl3)_axMapControl.Object);
            AppendLog("INFO", "当前工具: 漫游");
        }

        private void btnMapZoomIn_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_axMapControl == null)
            {
                AppendLog("WARN", "地图控件未就绪。");
                return;
            }
            MapWorkspaceService.ActivateZoomIn((IMapControl3)_axMapControl.Object);
            AppendLog("INFO", "当前工具: 放大");
        }
    }
}

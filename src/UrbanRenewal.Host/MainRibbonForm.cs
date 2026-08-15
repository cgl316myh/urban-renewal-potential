using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;
using UrbanRenewal.PluginLoader;
using IOPath = System.IO.Path;

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
        private ILayer _tocContextLayer;

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
            RibbonHostImpl.ApplyLargeImage(this.btnMapZoomOut, this.btnMapZoomOut.Caption);
            RibbonHostImpl.ApplyLargeImage(this.btnMapSelect, this.btnMapSelect.Caption);
            RibbonHostImpl.ApplyLargeImage(this.btnMapClearSelection, this.btnMapClearSelection.Caption);
            RibbonHostImpl.ApplyLargeImage(this.btnMapIdentify, this.btnMapIdentify.Caption);
            RibbonHostImpl.ApplyLargeImage(this.btnMapMeasureLength, this.btnMapMeasureLength.Caption);
            RibbonHostImpl.ApplyLargeImage(this.btnMapMeasureArea, this.btnMapMeasureArea.Caption);
            RibbonHostImpl.ApplyLargeImage(this.btnToggleLog, this.btnToggleLog.Caption);
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
                _axTocControl.OnMouseDown += axTocControl_OnMouseDown;
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
            if (!string.IsNullOrEmpty(_appContext.SkinName))
            {
                _appContext.ApplySkin(_appContext.SkinName);
            }
            _ribbonHost = new RibbonHostImpl(this.ribbonControl);
            _pluginManager = new PluginManager(
                delegate(string m) { AppendLog("INFO", m); },
                delegate(string m) { AppendLog("ERROR", m); });

            string pluginsDir = IOPath.Combine(Application.StartupPath, _settings.PluginsDirectoryName);
            AppendLog("INFO", "StartupPath=" + Application.StartupPath);
            _pluginManager.LoadAll(pluginsDir);
            _pluginManager.InitializeAll(_appContext, _ribbonHost);
            this.barStaticStatus.Caption = "插件已加载: " + _pluginManager.Plugins.Count;

            if (_pluginManager.Plugins.Count == 0)
            {
                AppendLog("WARN", "未加载到业务插件。请生成整个解决方案，确认 Plugins 下有 UrbanRenewal.Plugins.*.dll");
            }

            // 启动时自动加载上次保存的工程 MXD
            string mxdMsg;
            if (_appContext.TryLoadSavedProject(out mxdMsg) && !string.IsNullOrEmpty(mxdMsg))
            {
                AppendLog("INFO", mxdMsg);
            }
        }

        internal void AppendLog(string level, string message)
        {
            string line = DateTime.Now.ToString("HH:mm:ss") + " [" + level + "] " + message;
            if (InvokeRequired)
            {
                BeginInvoke(new Action(delegate { InsertLogLine(line); }));
            }
            else
            {
                InsertLogLine(line);
            }
        }

        private int _logPaintCounter;

        private void InsertLogLine(string line)
        {
            this.listBoxLog.BeginUpdate();
            try
            {
                this.listBoxLog.Items.Insert(0, line);
                // 防止日志无限增长
                while (this.listBoxLog.Items.Count > 2000)
                {
                    this.listBoxLog.Items.RemoveAt(this.listBoxLog.Items.Count - 1);
                }
            }
            finally
            {
                this.listBoxLog.EndUpdate();
            }
            // 频繁 Refresh 会拖死 UI 消息泵；批量后再重绘
            _logPaintCounter++;
            if (_logPaintCounter >= 8)
            {
                _logPaintCounter = 0;
                this.listBoxLog.Invalidate();
            }
        }

        /// <summary>分析运行时若日志面板被隐藏，自动展开以便查看逐步日志。</summary>
        internal void EnsureLogPanelVisible()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(EnsureLogPanelVisible));
                return;
            }
            if (this.splitWorkspace != null && this.splitWorkspace.Panel2Collapsed)
            {
                SetLogPanelVisible(true);
            }
        }

        internal string GetLogText()
        {
            StringBuilder sb = new StringBuilder();
            int max = Math.Min(200, this.listBoxLog.Items.Count);
            for (int i = 0; i < max; i++)
            {
                sb.AppendLine(Convert.ToString(this.listBoxLog.Items[i]));
            }
            return sb.ToString();
        }

        internal void SetStatus(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(delegate { SetStatus(text); }));
                return;
            }
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

        private void btnMapZoomOut_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_axMapControl == null)
            {
                AppendLog("WARN", "地图控件未就绪。");
                return;
            }
            MapWorkspaceService.ActivateZoomOut((IMapControl3)_axMapControl.Object);
            AppendLog("INFO", "当前工具: 缩小");
        }

        private void btnMapSelect_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_axMapControl == null)
            {
                AppendLog("WARN", "地图控件未就绪。");
                return;
            }
            MapWorkspaceService.ActivateSelectFeatures((IMapControl3)_axMapControl.Object);
            AppendLog("INFO", "当前工具: 选择");
        }

        private void btnMapClearSelection_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_axMapControl == null)
            {
                AppendLog("WARN", "地图控件未就绪。");
                return;
            }
            MapWorkspaceService.ClearSelection((IMapControl3)_axMapControl.Object);
            AppendLog("INFO", "已取消选择。");
        }

        private void btnMapIdentify_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_axMapControl == null)
            {
                AppendLog("WARN", "地图控件未就绪。");
                return;
            }
            MapWorkspaceService.ActivateIdentify((IMapControl3)_axMapControl.Object);
            AppendLog("INFO", "当前工具: 识别");
        }

        private void btnMapMeasureLength_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_axMapControl == null)
            {
                AppendLog("WARN", "地图控件未就绪。");
                return;
            }
            MapWorkspaceService.ActivateMeasureLength(
                (IMapControl3)_axMapControl.Object,
                OnMapMeasureResult);
            AppendLog("INFO", "当前工具: 长度测量（单击加点，双击结束，Esc 取消）");
        }

        private void btnMapMeasureArea_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_axMapControl == null)
            {
                AppendLog("WARN", "地图控件未就绪。");
                return;
            }
            MapWorkspaceService.ActivateMeasureArea(
                (IMapControl3)_axMapControl.Object,
                OnMapMeasureResult);
            AppendLog("INFO", "当前工具: 面积测量（单击加点，双击结束，Esc 取消）");
        }

        private void OnMapMeasureResult(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }
            AppendLog("INFO", message);
            SetStatus(message);
        }

        private void btnToggleLog_ItemClick(object sender, ItemClickEventArgs e)
        {
            ToggleLogPanel();
        }

        private void btnHideLog_Click(object sender, EventArgs e)
        {
            SetLogPanelVisible(false);
        }

        private void ToggleLogPanel()
        {
            SetLogPanelVisible(this.splitWorkspace.Panel2Collapsed);
        }

        private void SetLogPanelVisible(bool visible)
        {
            this.splitWorkspace.Panel2Collapsed = !visible;
            this.btnToggleLog.Caption = visible ? "隐藏日志" : "显示日志";
            RibbonHostImpl.ApplyLargeImage(this.btnToggleLog, this.btnToggleLog.Caption);
        }

        private void axTocControl_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            if (_axTocControl == null || _axMapControl == null)
            {
                return;
            }

            esriTOCControlItem itemType = esriTOCControlItem.esriTOCControlItemNone;
            IBasicMap map = null;
            ILayer layer = null;
            object other = null;
            object data = null;

            _axTocControl.HitTest(e.x, e.y, ref itemType, ref map, ref layer, ref other, ref data);

            // 左键：点击矢量图例符号 → 自定义符号窗体
            if (e.button == 1)
            {
                if (layer == null)
                {
                    return;
                }

                if (itemType == esriTOCControlItem.esriTOCControlItemLegendClass)
                {
                    IFeatureLayer featureLayer = layer as IFeatureLayer;
                    if (featureLayer != null)
                    {
                        try
                        {
                            ILegendClass legendClass = ((ILegendGroup)other).get_Class((int)data);
                            if (legendClass == null || legendClass.Symbol == null)
                            {
                                return;
                            }

                            using (SymbolForm form = new SymbolForm(legendClass.Symbol, layer.Name))
                            {
                                if (form.ShowDialog(this) == DialogResult.OK && form.ResultSymbol != null)
                                {
                                    legendClass.Symbol = form.ResultSymbol;

                                    IGeoFeatureLayer geoFeatureLayer = layer as IGeoFeatureLayer;
                                    if (geoFeatureLayer != null)
                                    {
                                        ISimpleRenderer simpleRenderer = geoFeatureLayer.Renderer as ISimpleRenderer;
                                        if (simpleRenderer != null)
                                        {
                                            simpleRenderer.Symbol = form.ResultSymbol;
                                        }
                                    }

                                    _axMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, layer, null);
                                    _axTocControl.Update();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(this, "修改图层符号失败：\r\n" + ex.Message, "错误",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show(this, "仅支持修改矢量图层符号。", "提示",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                return;
            }

            // 右键：图层菜单
            if (e.button != 2)
            {
                return;
            }

            if (layer == null)
            {
                return;
            }

            _tocContextLayer = layer;

            bool isFeature = layer is IFeatureLayer;
            this.menuTocViewTable.Enabled = isFeature;

            bool isPolygon = false;
            bool isVectorGeom = false;
            IFeatureLayer fl = layer as IFeatureLayer;
            if (fl != null && fl.FeatureClass != null)
            {
                esriGeometryType st = fl.FeatureClass.ShapeType;
                isPolygon = st == esriGeometryType.esriGeometryPolygon;
                isVectorGeom =
                    st == esriGeometryType.esriGeometryPoint ||
                    st == esriGeometryType.esriGeometryMultipoint ||
                    st == esriGeometryType.esriGeometryPolyline ||
                    st == esriGeometryType.esriGeometryLine ||
                    st == esriGeometryType.esriGeometryPolygon;
            }
            this.menuTocClassRender.Enabled = isPolygon;
            this.menuTocUniqueRender.Enabled = isVectorGeom;
            this.menuTocRasterRender.Enabled = layer is IRasterLayer;

            this.contextMenuToc.Show(_axTocControl, new System.Drawing.Point(e.x, e.y));
        }

        private void menuTocRasterRender_Click(object sender, EventArgs e)
        {
            if (_tocContextLayer == null)
            {
                return;
            }

            IRasterLayer rasterLayer = _tocContextLayer as IRasterLayer;
            if (rasterLayer == null || rasterLayer.Raster == null)
            {
                MessageBox.Show(this, "仅支持对栅格图层进行渲染。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (RasterRenderForm form = new RasterRenderForm(rasterLayer, _axMapControl.Map))
                {
                    if (form.ShowDialog(this) == DialogResult.OK && form.Applied)
                    {
                        // 栅格渲染器变更后需整图刷新，PartialRefresh 有时不重绘色带
                        _axMapControl.ActiveView.ContentsChanged();
                        _axMapControl.ActiveView.Refresh();
                        _axTocControl.Update();
                        AppendLog("INFO", "已应用栅格渲染: " + _tocContextLayer.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "栅格渲染失败：\r\n" + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuTocUniqueRender_Click(object sender, EventArgs e)
        {
            if (_tocContextLayer == null)
            {
                return;
            }

            IFeatureLayer featureLayer = _tocContextLayer as IFeatureLayer;
            if (featureLayer == null || featureLayer.FeatureClass == null)
            {
                MessageBox.Show(this, "仅支持对点、线、面图层进行唯一值渲染。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            esriGeometryType st = featureLayer.FeatureClass.ShapeType;
            if (st != esriGeometryType.esriGeometryPoint &&
                st != esriGeometryType.esriGeometryMultipoint &&
                st != esriGeometryType.esriGeometryPolyline &&
                st != esriGeometryType.esriGeometryLine &&
                st != esriGeometryType.esriGeometryPolygon)
            {
                MessageBox.Show(this, "仅支持对点、线、面图层进行唯一值渲染。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (UniqueValueRenderForm form = new UniqueValueRenderForm(featureLayer))
                {
                    if (form.ShowDialog(this) == DialogResult.OK && form.Applied)
                    {
                        _axMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, _tocContextLayer, null);
                        _axTocControl.Update();
                        AppendLog("INFO", "已应用唯一值渲染: " + _tocContextLayer.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "唯一值渲染失败：\r\n" + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuTocClassRender_Click(object sender, EventArgs e)
        {
            if (_tocContextLayer == null)
            {
                return;
            }

            IFeatureLayer featureLayer = _tocContextLayer as IFeatureLayer;
            if (featureLayer == null || featureLayer.FeatureClass == null ||
                featureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolygon)
            {
                MessageBox.Show(this, "仅支持对面图层进行分段渲染。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                using (PolygonClassRenderForm form = new PolygonClassRenderForm(featureLayer))
                {
                    if (form.ShowDialog(this) == DialogResult.OK && form.Applied)
                    {
                        _axMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, _tocContextLayer, null);
                        _axTocControl.Update();
                        AppendLog("INFO", "已应用分段渲染: " + _tocContextLayer.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "面图层分段渲染失败：\r\n" + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuTocViewTable_Click(object sender, EventArgs e)
        {
            if (_tocContextLayer == null)
            {
                return;
            }

            IFeatureLayer featureLayer = _tocContextLayer as IFeatureLayer;
            if (featureLayer == null)
            {
                MessageBox.Show(this, "仅支持查看矢量图层属性表。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            AttributeTableForm form = new AttributeTableForm();
            form.LoadFeatureLayer(featureLayer);
            form.Show(this);
        }

        private void menuTocZoomToLayer_Click(object sender, EventArgs e)
        {
            if (_tocContextLayer == null || _axMapControl == null)
            {
                return;
            }

            try
            {
                IEnvelope env = _tocContextLayer.AreaOfInterest;
                if (env == null || env.IsEmpty)
                {
                    MessageBox.Show(this, "无法获取图层范围。", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                _axMapControl.Extent = env;
                _axMapControl.ActiveView.Refresh();
                AppendLog("INFO", "已缩放到图层: " + _tocContextLayer.Name);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "缩放到图层失败：\r\n" + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuTocRemoveLayer_Click(object sender, EventArgs e)
        {
            if (_tocContextLayer == null || _axMapControl == null)
            {
                return;
            }

            string layerName = _tocContextLayer.Name;
            DialogResult result = MessageBox.Show(
                this,
                "确定移除图层“" + layerName + "”吗？",
                "移除图层",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            _axMapControl.Map.DeleteLayer(_tocContextLayer);
            _tocContextLayer = null;
            _axMapControl.ActiveView.Refresh();
            _axTocControl.Update();
            AppendLog("INFO", "已移除图层: " + layerName);
        }
    }
}

namespace UrbanRenewal.Host
{
    partial class MainRibbonForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ribbonControl = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.btnMapFit = new DevExpress.XtraBars.BarButtonItem();
            this.btnMapPan = new DevExpress.XtraBars.BarButtonItem();
            this.btnMapZoomIn = new DevExpress.XtraBars.BarButtonItem();
            this.btnMapZoomOut = new DevExpress.XtraBars.BarButtonItem();
            this.btnMapSelect = new DevExpress.XtraBars.BarButtonItem();
            this.btnMapClearSelection = new DevExpress.XtraBars.BarButtonItem();
            this.btnMapIdentify = new DevExpress.XtraBars.BarButtonItem();
            this.btnMapMeasureLength = new DevExpress.XtraBars.BarButtonItem();
            this.btnMapMeasureArea = new DevExpress.XtraBars.BarButtonItem();
            this.btnToggleLog = new DevExpress.XtraBars.BarButtonItem();
            this.barStaticStatus = new DevExpress.XtraBars.BarStaticItem();
            this.ribbonPageMap = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribbonPageGroupView = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonPageGroupSelect = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonStatusBar = new DevExpress.XtraBars.Ribbon.RibbonStatusBar();
            this.splitWorkspace = new System.Windows.Forms.SplitContainer();
            this.splitMap = new System.Windows.Forms.SplitContainer();
            this.panelToc = new System.Windows.Forms.Panel();
            this.labelTocTip = new System.Windows.Forms.Label();
            this.panelMap = new System.Windows.Forms.Panel();
            this.labelMapTip = new System.Windows.Forms.Label();
            this.panelLog = new System.Windows.Forms.Panel();
            this.listBoxLog = new System.Windows.Forms.ListBox();
            this.panelLogHeader = new System.Windows.Forms.Panel();
            this.btnHideLog = new System.Windows.Forms.Button();
            this.lblLogTitle = new System.Windows.Forms.Label();
            this.contextMenuToc = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuTocViewTable = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTocClassRender = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTocUniqueRender = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTocRasterRender = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTocZoomToLayer = new System.Windows.Forms.ToolStripMenuItem();
            this.menuTocRemoveLayer = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitWorkspace)).BeginInit();
            this.splitWorkspace.Panel1.SuspendLayout();
            this.splitWorkspace.Panel2.SuspendLayout();
            this.splitWorkspace.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMap)).BeginInit();
            this.splitMap.Panel1.SuspendLayout();
            this.splitMap.Panel2.SuspendLayout();
            this.splitMap.SuspendLayout();
            this.panelToc.SuspendLayout();
            this.panelMap.SuspendLayout();
            this.panelLog.SuspendLayout();
            this.panelLogHeader.SuspendLayout();
            this.contextMenuToc.SuspendLayout();
            this.SuspendLayout();
            // 
            // ribbonControl
            // 
            this.ribbonControl.ApplicationButtonText = null;
            this.ribbonControl.ExpandCollapseItem.Id = 0;
            this.ribbonControl.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbonControl.ExpandCollapseItem,
            this.btnMapFit,
            this.btnMapPan,
            this.btnMapZoomIn,
            this.btnMapZoomOut,
            this.btnMapSelect,
            this.btnMapClearSelection,
            this.btnMapIdentify,
            this.btnMapMeasureLength,
            this.btnMapMeasureArea,
            this.btnToggleLog,
            this.barStaticStatus});
            this.ribbonControl.Location = new System.Drawing.Point(0, 0);
            this.ribbonControl.MaxItemId = 12;
            this.ribbonControl.Name = "ribbonControl";
            this.ribbonControl.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
            this.ribbonPageMap});
            this.ribbonControl.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonControlStyle.Office2013;
            this.ribbonControl.Size = new System.Drawing.Size(1493, 147);
            this.ribbonControl.StatusBar = this.ribbonStatusBar;
            // 
            // btnMapFit
            // 
            this.btnMapFit.Caption = "全图";
            this.btnMapFit.Id = 1;
            this.btnMapFit.Name = "btnMapFit";
            this.btnMapFit.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnMapFit_ItemClick);
            // 
            // btnMapPan
            // 
            this.btnMapPan.Caption = "漫游";
            this.btnMapPan.Id = 2;
            this.btnMapPan.Name = "btnMapPan";
            this.btnMapPan.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnMapPan_ItemClick);
            // 
            // btnMapZoomIn
            // 
            this.btnMapZoomIn.Caption = "放大";
            this.btnMapZoomIn.Id = 3;
            this.btnMapZoomIn.Name = "btnMapZoomIn";
            this.btnMapZoomIn.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnMapZoomIn_ItemClick);
            // 
            // btnMapZoomOut
            // 
            this.btnMapZoomOut.Caption = "缩小";
            this.btnMapZoomOut.Id = 6;
            this.btnMapZoomOut.Name = "btnMapZoomOut";
            this.btnMapZoomOut.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnMapZoomOut_ItemClick);
            // 
            // btnMapSelect
            // 
            this.btnMapSelect.Caption = "选择";
            this.btnMapSelect.Id = 7;
            this.btnMapSelect.Name = "btnMapSelect";
            this.btnMapSelect.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnMapSelect_ItemClick);
            // 
            // btnMapClearSelection
            // 
            this.btnMapClearSelection.Caption = "取消选择";
            this.btnMapClearSelection.Id = 8;
            this.btnMapClearSelection.Name = "btnMapClearSelection";
            this.btnMapClearSelection.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnMapClearSelection_ItemClick);
            // 
            // btnMapIdentify
            // 
            this.btnMapIdentify.Caption = "识别";
            this.btnMapIdentify.Id = 9;
            this.btnMapIdentify.Name = "btnMapIdentify";
            this.btnMapIdentify.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnMapIdentify_ItemClick);
            // 
            // btnMapMeasureLength
            // 
            this.btnMapMeasureLength.Caption = "长度测量";
            this.btnMapMeasureLength.Id = 10;
            this.btnMapMeasureLength.Name = "btnMapMeasureLength";
            this.btnMapMeasureLength.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnMapMeasureLength_ItemClick);
            // 
            // btnMapMeasureArea
            // 
            this.btnMapMeasureArea.Caption = "面积测量";
            this.btnMapMeasureArea.Id = 11;
            this.btnMapMeasureArea.Name = "btnMapMeasureArea";
            this.btnMapMeasureArea.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnMapMeasureArea_ItemClick);
            // 
            // btnToggleLog
            // 
            this.btnToggleLog.Caption = "隐藏日志";
            this.btnToggleLog.Id = 5;
            this.btnToggleLog.Name = "btnToggleLog";
            this.btnToggleLog.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnToggleLog_ItemClick);
            // 
            // barStaticStatus
            // 
            this.barStaticStatus.AutoSize = DevExpress.XtraBars.BarStaticItemSize.Spring;
            this.barStaticStatus.Caption = "就绪";
            this.barStaticStatus.Id = 4;
            this.barStaticStatus.Name = "barStaticStatus";
            this.barStaticStatus.TextAlignment = System.Drawing.StringAlignment.Near;
            // 
            // ribbonPageMap
            // 
            this.ribbonPageMap.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.ribbonPageGroupView,
            this.ribbonPageGroupSelect});
            this.ribbonPageMap.Name = "ribbonPageMap";
            this.ribbonPageMap.Text = "地图";
            // 
            // ribbonPageGroupView
            // 
            this.ribbonPageGroupView.ItemLinks.Add(this.btnMapFit);
            this.ribbonPageGroupView.ItemLinks.Add(this.btnMapPan);
            this.ribbonPageGroupView.ItemLinks.Add(this.btnMapZoomIn);
            this.ribbonPageGroupView.ItemLinks.Add(this.btnMapZoomOut);
            this.ribbonPageGroupView.ItemLinks.Add(this.btnToggleLog);
            this.ribbonPageGroupView.Name = "ribbonPageGroupView";
            this.ribbonPageGroupView.Text = "视图";
            // 
            // ribbonPageGroupSelect
            // 
            this.ribbonPageGroupSelect.ItemLinks.Add(this.btnMapSelect);
            this.ribbonPageGroupSelect.ItemLinks.Add(this.btnMapClearSelection);
            this.ribbonPageGroupSelect.ItemLinks.Add(this.btnMapIdentify);
            this.ribbonPageGroupSelect.ItemLinks.Add(this.btnMapMeasureLength);
            this.ribbonPageGroupSelect.ItemLinks.Add(this.btnMapMeasureArea);
            this.ribbonPageGroupSelect.Name = "ribbonPageGroupSelect";
            this.ribbonPageGroupSelect.Text = "选择与量测";
            // 
            // ribbonStatusBar
            // 
            this.ribbonStatusBar.ItemLinks.Add(this.barStaticStatus);
            this.ribbonStatusBar.Location = new System.Drawing.Point(0, 902);
            this.ribbonStatusBar.Name = "ribbonStatusBar";
            this.ribbonStatusBar.Ribbon = this.ribbonControl;
            this.ribbonStatusBar.Size = new System.Drawing.Size(1493, 31);
            // 
            // splitWorkspace
            // 
            this.splitWorkspace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitWorkspace.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitWorkspace.Location = new System.Drawing.Point(0, 147);
            this.splitWorkspace.Name = "splitWorkspace";
            // 
            // splitWorkspace.Panel1
            // 
            this.splitWorkspace.Panel1.Controls.Add(this.splitMap);
            // 
            // splitWorkspace.Panel2
            // 
            this.splitWorkspace.Panel2.Controls.Add(this.panelLog);
            this.splitWorkspace.Panel2MinSize = 120;
            this.splitWorkspace.Size = new System.Drawing.Size(1493, 755);
            this.splitWorkspace.SplitterDistance = 1212;
            this.splitWorkspace.SplitterWidth = 5;
            this.splitWorkspace.TabIndex = 2;
            // 
            // splitMap
            // 
            this.splitMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMap.Location = new System.Drawing.Point(0, 0);
            this.splitMap.Name = "splitMap";
            // 
            // splitMap.Panel1
            // 
            this.splitMap.Panel1.Controls.Add(this.panelToc);
            // 
            // splitMap.Panel2
            // 
            this.splitMap.Panel2.Controls.Add(this.panelMap);
            this.splitMap.Size = new System.Drawing.Size(1212, 755);
            this.splitMap.SplitterDistance = 290;
            this.splitMap.SplitterWidth = 5;
            this.splitMap.TabIndex = 0;
            // 
            // panelToc
            // 
            this.panelToc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.panelToc.Controls.Add(this.labelTocTip);
            this.panelToc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelToc.Location = new System.Drawing.Point(0, 0);
            this.panelToc.Name = "panelToc";
            this.panelToc.Size = new System.Drawing.Size(290, 755);
            this.panelToc.TabIndex = 0;
            // 
            // labelTocTip
            // 
            this.labelTocTip.AutoSize = true;
            this.labelTocTip.Location = new System.Drawing.Point(14, 14);
            this.labelTocTip.Name = "labelTocTip";
            this.labelTocTip.Size = new System.Drawing.Size(156, 14);
            this.labelTocTip.TabIndex = 0;
            this.labelTocTip.Text = "图层目录（AxTOCControl）";
            // 
            // panelMap
            // 
            this.panelMap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.panelMap.Controls.Add(this.labelMapTip);
            this.panelMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMap.Location = new System.Drawing.Point(0, 0);
            this.panelMap.Name = "panelMap";
            this.panelMap.Size = new System.Drawing.Size(917, 755);
            this.panelMap.TabIndex = 0;
            // 
            // labelMapTip
            // 
            this.labelMapTip.AutoSize = true;
            this.labelMapTip.Location = new System.Drawing.Point(14, 14);
            this.labelMapTip.Name = "labelMapTip";
            this.labelMapTip.Size = new System.Drawing.Size(154, 14);
            this.labelMapTip.TabIndex = 0;
            this.labelMapTip.Text = "地图视图（AxMapControl）";
            // 
            // panelLog
            // 
            this.panelLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.panelLog.Controls.Add(this.listBoxLog);
            this.panelLog.Controls.Add(this.panelLogHeader);
            this.panelLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLog.Location = new System.Drawing.Point(0, 0);
            this.panelLog.Name = "panelLog";
            this.panelLog.Size = new System.Drawing.Size(276, 755);
            this.panelLog.TabIndex = 0;
            // 
            // listBoxLog
            // 
            this.listBoxLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxLog.FormattingEnabled = true;
            this.listBoxLog.IntegralHeight = false;
            this.listBoxLog.ItemHeight = 14;
            this.listBoxLog.Location = new System.Drawing.Point(0, 33);
            this.listBoxLog.Name = "listBoxLog";
            this.listBoxLog.Size = new System.Drawing.Size(276, 722);
            this.listBoxLog.TabIndex = 1;
            // 
            // panelLogHeader
            // 
            this.panelLogHeader.Controls.Add(this.btnHideLog);
            this.panelLogHeader.Controls.Add(this.lblLogTitle);
            this.panelLogHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelLogHeader.Location = new System.Drawing.Point(0, 0);
            this.panelLogHeader.Name = "panelLogHeader";
            this.panelLogHeader.Size = new System.Drawing.Size(276, 33);
            this.panelLogHeader.TabIndex = 0;
            // 
            // btnHideLog
            // 
            this.btnHideLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHideLog.Location = new System.Drawing.Point(199, 3);
            this.btnHideLog.Name = "btnHideLog";
            this.btnHideLog.Size = new System.Drawing.Size(70, 26);
            this.btnHideLog.TabIndex = 1;
            this.btnHideLog.Text = "隐藏";
            this.btnHideLog.UseVisualStyleBackColor = true;
            this.btnHideLog.Click += new System.EventHandler(this.btnHideLog_Click);
            // 
            // lblLogTitle
            // 
            this.lblLogTitle.AutoSize = true;
            this.lblLogTitle.Location = new System.Drawing.Point(9, 9);
            this.lblLogTitle.Name = "lblLogTitle";
            this.lblLogTitle.Size = new System.Drawing.Size(55, 14);
            this.lblLogTitle.TabIndex = 0;
            this.lblLogTitle.Text = "运行日志";
            // 
            // contextMenuToc
            // 
            this.contextMenuToc.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuTocViewTable,
            this.menuTocClassRender,
            this.menuTocUniqueRender,
            this.menuTocRasterRender,
            this.menuTocZoomToLayer,
            this.menuTocRemoveLayer});
            this.contextMenuToc.Name = "contextMenuToc";
            this.contextMenuToc.Size = new System.Drawing.Size(137, 136);
            // 
            // menuTocViewTable
            // 
            this.menuTocViewTable.Name = "menuTocViewTable";
            this.menuTocViewTable.Size = new System.Drawing.Size(136, 22);
            this.menuTocViewTable.Text = "查看属性表";
            this.menuTocViewTable.Click += new System.EventHandler(this.menuTocViewTable_Click);
            // 
            // menuTocClassRender
            // 
            this.menuTocClassRender.Name = "menuTocClassRender";
            this.menuTocClassRender.Size = new System.Drawing.Size(136, 22);
            this.menuTocClassRender.Text = "分段渲染";
            this.menuTocClassRender.Click += new System.EventHandler(this.menuTocClassRender_Click);
            // 
            // menuTocUniqueRender
            // 
            this.menuTocUniqueRender.Name = "menuTocUniqueRender";
            this.menuTocUniqueRender.Size = new System.Drawing.Size(136, 22);
            this.menuTocUniqueRender.Text = "唯一值渲染";
            this.menuTocUniqueRender.Click += new System.EventHandler(this.menuTocUniqueRender_Click);
            // 
            // menuTocRasterRender
            // 
            this.menuTocRasterRender.Name = "menuTocRasterRender";
            this.menuTocRasterRender.Size = new System.Drawing.Size(136, 22);
            this.menuTocRasterRender.Text = "栅格渲染";
            this.menuTocRasterRender.Click += new System.EventHandler(this.menuTocRasterRender_Click);
            // 
            // menuTocZoomToLayer
            // 
            this.menuTocZoomToLayer.Name = "menuTocZoomToLayer";
            this.menuTocZoomToLayer.Size = new System.Drawing.Size(136, 22);
            this.menuTocZoomToLayer.Text = "缩放到图层";
            this.menuTocZoomToLayer.Click += new System.EventHandler(this.menuTocZoomToLayer_Click);
            // 
            // menuTocRemoveLayer
            // 
            this.menuTocRemoveLayer.Name = "menuTocRemoveLayer";
            this.menuTocRemoveLayer.Size = new System.Drawing.Size(136, 22);
            this.menuTocRemoveLayer.Text = "移除图层";
            this.menuTocRemoveLayer.Click += new System.EventHandler(this.menuTocRemoveLayer_Click);
            // 
            // MainRibbonForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1493, 933);
            this.Controls.Add(this.splitWorkspace);
            this.Controls.Add(this.ribbonStatusBar);
            this.Controls.Add(this.ribbonControl);
            this.Name = "MainRibbonForm";
            this.Ribbon = this.ribbonControl;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.StatusBar = this.ribbonStatusBar;
            this.Text = "城市更新潜力评价与验证系统";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl)).EndInit();
            this.splitWorkspace.Panel1.ResumeLayout(false);
            this.splitWorkspace.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitWorkspace)).EndInit();
            this.splitWorkspace.ResumeLayout(false);
            this.splitMap.Panel1.ResumeLayout(false);
            this.splitMap.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMap)).EndInit();
            this.splitMap.ResumeLayout(false);
            this.panelToc.ResumeLayout(false);
            this.panelToc.PerformLayout();
            this.panelMap.ResumeLayout(false);
            this.panelMap.PerformLayout();
            this.panelLog.ResumeLayout(false);
            this.panelLogHeader.ResumeLayout(false);
            this.panelLogHeader.PerformLayout();
            this.contextMenuToc.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl;
        private DevExpress.XtraBars.BarButtonItem btnMapFit;
        private DevExpress.XtraBars.BarButtonItem btnMapPan;
        private DevExpress.XtraBars.BarButtonItem btnMapZoomIn;
        private DevExpress.XtraBars.BarButtonItem btnMapZoomOut;
        private DevExpress.XtraBars.BarButtonItem btnMapSelect;
        private DevExpress.XtraBars.BarButtonItem btnMapClearSelection;
        private DevExpress.XtraBars.BarButtonItem btnMapIdentify;
        private DevExpress.XtraBars.BarButtonItem btnMapMeasureLength;
        private DevExpress.XtraBars.BarButtonItem btnMapMeasureArea;
        private DevExpress.XtraBars.BarButtonItem btnToggleLog;
        private DevExpress.XtraBars.BarStaticItem barStaticStatus;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPageMap;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroupView;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroupSelect;
        private DevExpress.XtraBars.Ribbon.RibbonStatusBar ribbonStatusBar;
        private System.Windows.Forms.SplitContainer splitWorkspace;
        private System.Windows.Forms.SplitContainer splitMap;
        private System.Windows.Forms.Panel panelToc;
        private System.Windows.Forms.Label labelTocTip;
        private System.Windows.Forms.Panel panelMap;
        private System.Windows.Forms.Label labelMapTip;
        private System.Windows.Forms.Panel panelLog;
        private System.Windows.Forms.Panel panelLogHeader;
        private System.Windows.Forms.Label lblLogTitle;
        private System.Windows.Forms.Button btnHideLog;
        private System.Windows.Forms.ListBox listBoxLog;
        private System.Windows.Forms.ContextMenuStrip contextMenuToc;
        private System.Windows.Forms.ToolStripMenuItem menuTocViewTable;
        private System.Windows.Forms.ToolStripMenuItem menuTocClassRender;
        private System.Windows.Forms.ToolStripMenuItem menuTocUniqueRender;
        private System.Windows.Forms.ToolStripMenuItem menuTocRasterRender;
        private System.Windows.Forms.ToolStripMenuItem menuTocZoomToLayer;
        private System.Windows.Forms.ToolStripMenuItem menuTocRemoveLayer;
    }
}

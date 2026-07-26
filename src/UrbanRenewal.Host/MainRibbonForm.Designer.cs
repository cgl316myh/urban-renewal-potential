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
            this.btnToggleLog = new DevExpress.XtraBars.BarButtonItem();
            this.barStaticStatus = new DevExpress.XtraBars.BarStaticItem();
            this.ribbonPageMap = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribbonPageGroupView = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
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
            this.lblLogTitle = new System.Windows.Forms.Label();
            this.btnHideLog = new System.Windows.Forms.Button();
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
            this.btnToggleLog,
            this.barStaticStatus});
            this.ribbonControl.Location = new System.Drawing.Point(0, 0);
            this.ribbonControl.MaxItemId = 6;
            this.ribbonControl.Name = "ribbonControl";
            this.ribbonControl.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
            this.ribbonPageMap});
            this.ribbonControl.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonControlStyle.Office2013;
            this.ribbonControl.Size = new System.Drawing.Size(1280, 145);
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
            this.ribbonPageGroupView});
            this.ribbonPageMap.Name = "ribbonPageMap";
            this.ribbonPageMap.Text = "地图";
            // 
            // ribbonPageGroupView
            // 
            this.ribbonPageGroupView.ItemLinks.Add(this.btnMapFit);
            this.ribbonPageGroupView.ItemLinks.Add(this.btnMapPan);
            this.ribbonPageGroupView.ItemLinks.Add(this.btnMapZoomIn);
            this.ribbonPageGroupView.ItemLinks.Add(this.btnToggleLog);
            this.ribbonPageGroupView.Name = "ribbonPageGroupView";
            this.ribbonPageGroupView.Text = "视图";
            // 
            // ribbonStatusBar
            // 
            this.ribbonStatusBar.ItemLinks.Add(this.barStaticStatus);
            this.ribbonStatusBar.Location = new System.Drawing.Point(0, 770);
            this.ribbonStatusBar.Name = "ribbonStatusBar";
            this.ribbonStatusBar.Ribbon = this.ribbonControl;
            this.ribbonStatusBar.Size = new System.Drawing.Size(1280, 30);
            // 
            // splitWorkspace — 左：TOC+地图；右：日志
            // 
            this.splitWorkspace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitWorkspace.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitWorkspace.Location = new System.Drawing.Point(0, 145);
            this.splitWorkspace.Name = "splitWorkspace";
            this.splitWorkspace.Orientation = System.Windows.Forms.Orientation.Vertical;
            // 
            // splitWorkspace.Panel1
            // 
            this.splitWorkspace.Panel1.Controls.Add(this.splitMap);
            // 
            // splitWorkspace.Panel2
            // 
            this.splitWorkspace.Panel2.Controls.Add(this.panelLog);
            this.splitWorkspace.Panel2MinSize = 120;
            this.splitWorkspace.Size = new System.Drawing.Size(1280, 625);
            this.splitWorkspace.SplitterDistance = 1000;
            this.splitWorkspace.TabIndex = 2;
            // 
            // splitMap — 左 TOC，右地图
            // 
            this.splitMap.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMap.Location = new System.Drawing.Point(0, 0);
            this.splitMap.Name = "splitMap";
            this.splitMap.Orientation = System.Windows.Forms.Orientation.Vertical;
            // 
            // splitMap.Panel1
            // 
            this.splitMap.Panel1.Controls.Add(this.panelToc);
            // 
            // splitMap.Panel2
            // 
            this.splitMap.Panel2.Controls.Add(this.panelMap);
            this.splitMap.Size = new System.Drawing.Size(1000, 625);
            this.splitMap.SplitterDistance = 240;
            this.splitMap.TabIndex = 0;
            // 
            // panelToc
            // 
            this.panelToc.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(250)))), ((int)(((byte)(250)))));
            this.panelToc.Controls.Add(this.labelTocTip);
            this.panelToc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelToc.Location = new System.Drawing.Point(0, 0);
            this.panelToc.Name = "panelToc";
            this.panelToc.Size = new System.Drawing.Size(240, 625);
            this.panelToc.TabIndex = 0;
            // 
            // labelTocTip
            // 
            this.labelTocTip.AutoSize = true;
            this.labelTocTip.Location = new System.Drawing.Point(12, 12);
            this.labelTocTip.Name = "labelTocTip";
            this.labelTocTip.Size = new System.Drawing.Size(173, 12);
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
            this.panelMap.Size = new System.Drawing.Size(756, 625);
            this.panelMap.TabIndex = 0;
            // 
            // labelMapTip
            // 
            this.labelMapTip.AutoSize = true;
            this.labelMapTip.Location = new System.Drawing.Point(12, 12);
            this.labelMapTip.Name = "labelMapTip";
            this.labelMapTip.Size = new System.Drawing.Size(173, 12);
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
            this.panelLog.Size = new System.Drawing.Size(276, 625);
            this.panelLog.TabIndex = 0;
            // 
            // panelLogHeader
            // 
            this.panelLogHeader.Controls.Add(this.btnHideLog);
            this.panelLogHeader.Controls.Add(this.lblLogTitle);
            this.panelLogHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelLogHeader.Location = new System.Drawing.Point(0, 0);
            this.panelLogHeader.Name = "panelLogHeader";
            this.panelLogHeader.Size = new System.Drawing.Size(276, 28);
            this.panelLogHeader.TabIndex = 0;
            // 
            // lblLogTitle
            // 
            this.lblLogTitle.AutoSize = true;
            this.lblLogTitle.Location = new System.Drawing.Point(8, 8);
            this.lblLogTitle.Name = "lblLogTitle";
            this.lblLogTitle.Size = new System.Drawing.Size(53, 12);
            this.lblLogTitle.TabIndex = 0;
            this.lblLogTitle.Text = "运行日志";
            // 
            // btnHideLog
            // 
            this.btnHideLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHideLog.Location = new System.Drawing.Point(210, 3);
            this.btnHideLog.Name = "btnHideLog";
            this.btnHideLog.Size = new System.Drawing.Size(60, 22);
            this.btnHideLog.TabIndex = 1;
            this.btnHideLog.Text = "隐藏";
            this.btnHideLog.UseVisualStyleBackColor = true;
            this.btnHideLog.Click += new System.EventHandler(this.btnHideLog_Click);
            // 
            // listBoxLog
            // 
            this.listBoxLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxLog.FormattingEnabled = true;
            this.listBoxLog.IntegralHeight = false;
            this.listBoxLog.ItemHeight = 12;
            this.listBoxLog.Location = new System.Drawing.Point(0, 28);
            this.listBoxLog.Name = "listBoxLog";
            this.listBoxLog.Size = new System.Drawing.Size(276, 597);
            this.listBoxLog.TabIndex = 1;
            // 
            // MainRibbonForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 800);
            this.Controls.Add(this.splitWorkspace);
            this.Controls.Add(this.ribbonStatusBar);
            this.Controls.Add(this.ribbonControl);
            this.Name = "MainRibbonForm";
            this.Ribbon = this.ribbonControl;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.StatusBar = this.ribbonStatusBar;
            this.Text = "城市更新潜力评价与验证系统";
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
            this.panelLogHeader.ResumeLayout(false);
            this.panelLogHeader.PerformLayout();
            this.panelLog.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl;
        private DevExpress.XtraBars.BarButtonItem btnMapFit;
        private DevExpress.XtraBars.BarButtonItem btnMapPan;
        private DevExpress.XtraBars.BarButtonItem btnMapZoomIn;
        private DevExpress.XtraBars.BarButtonItem btnToggleLog;
        private DevExpress.XtraBars.BarStaticItem barStaticStatus;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPageMap;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroupView;
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
    }
}

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
            this.barStaticStatus = new DevExpress.XtraBars.BarStaticItem();
            this.ribbonPageMap = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.ribbonPageGroupView = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonStatusBar = new DevExpress.XtraBars.Ribbon.RibbonStatusBar();
            this.panelMapHost = new System.Windows.Forms.Panel();
            this.labelMapTip = new System.Windows.Forms.Label();
            this.listBoxLog = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl)).BeginInit();
            this.panelMapHost.SuspendLayout();
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
            this.barStaticStatus});
            this.ribbonControl.Location = new System.Drawing.Point(0, 0);
            this.ribbonControl.MaxItemId = 5;
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
            // panelMapHost
            // 
            this.panelMapHost.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.panelMapHost.Controls.Add(this.labelMapTip);
            this.panelMapHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMapHost.Location = new System.Drawing.Point(0, 145);
            this.panelMapHost.Name = "panelMapHost";
            this.panelMapHost.Size = new System.Drawing.Size(1280, 485);
            this.panelMapHost.TabIndex = 2;
            // 
            // labelMapTip
            // 
            this.labelMapTip.AutoSize = true;
            this.labelMapTip.Location = new System.Drawing.Point(24, 24);
            this.labelMapTip.Name = "labelMapTip";
            this.labelMapTip.Size = new System.Drawing.Size(341, 12);
            this.labelMapTip.TabIndex = 0;
            this.labelMapTip.Text = "地图工作区（P1-5 在此嵌入 AxMapControl / AxTOCControl）";
            // 
            // listBoxLog
            // 
            this.listBoxLog.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.listBoxLog.FormattingEnabled = true;
            this.listBoxLog.IntegralHeight = false;
            this.listBoxLog.ItemHeight = 12;
            this.listBoxLog.Location = new System.Drawing.Point(0, 630);
            this.listBoxLog.Name = "listBoxLog";
            this.listBoxLog.Size = new System.Drawing.Size(1280, 140);
            this.listBoxLog.TabIndex = 3;
            // 
            // MainRibbonForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1280, 800);
            this.Controls.Add(this.panelMapHost);
            this.Controls.Add(this.listBoxLog);
            this.Controls.Add(this.ribbonStatusBar);
            this.Controls.Add(this.ribbonControl);
            this.Name = "MainRibbonForm";
            this.Ribbon = this.ribbonControl;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.StatusBar = this.ribbonStatusBar;
            this.Text = "城市更新潜力评价与验证系统";
            ((System.ComponentModel.ISupportInitialize)(this.ribbonControl)).EndInit();
            this.panelMapHost.ResumeLayout(false);
            this.panelMapHost.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraBars.Ribbon.RibbonControl ribbonControl;
        private DevExpress.XtraBars.BarButtonItem btnMapFit;
        private DevExpress.XtraBars.BarButtonItem btnMapPan;
        private DevExpress.XtraBars.BarButtonItem btnMapZoomIn;
        private DevExpress.XtraBars.BarStaticItem barStaticStatus;
        private DevExpress.XtraBars.Ribbon.RibbonPage ribbonPageMap;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup ribbonPageGroupView;
        private DevExpress.XtraBars.Ribbon.RibbonStatusBar ribbonStatusBar;
        private System.Windows.Forms.Panel panelMapHost;
        private System.Windows.Forms.Label labelMapTip;
        private System.Windows.Forms.ListBox listBoxLog;
    }
}

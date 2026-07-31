namespace UrbanRenewal.Host
{
    partial class SymbolForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblLayer = new System.Windows.Forms.Label();
            this.lblType = new System.Windows.Forms.Label();
            this.groupStyle = new System.Windows.Forms.GroupBox();
            this.lblStyle = new System.Windows.Forms.Label();
            this.cboStyle = new System.Windows.Forms.ComboBox();
            this.lblColor = new System.Windows.Forms.Label();
            this.btnColor = new System.Windows.Forms.Button();
            this.lblSize = new System.Windows.Forms.Label();
            this.numSize = new System.Windows.Forms.NumericUpDown();
            this.lblOutline = new System.Windows.Forms.Label();
            this.btnOutlineColor = new System.Windows.Forms.Button();
            this.lblOutlineWidth = new System.Windows.Forms.Label();
            this.numOutlineWidth = new System.Windows.Forms.NumericUpDown();
            this.chkDrawOutline = new System.Windows.Forms.CheckBox();
            this.groupPreview = new System.Windows.Forms.GroupBox();
            this.panelPreview = new System.Windows.Forms.Panel();
            this.lblTip = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupStyle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOutlineWidth)).BeginInit();
            this.groupPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblTitle.Location = new System.Drawing.Point(16, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(448, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "图层符号设置";
            // 
            // lblLayer
            // 
            this.lblLayer.AutoSize = true;
            this.lblLayer.Location = new System.Drawing.Point(16, 48);
            this.lblLayer.Name = "lblLayer";
            this.lblLayer.Size = new System.Drawing.Size(80, 18);
            this.lblLayer.TabIndex = 1;
            this.lblLayer.Text = "图层名称";
            // 
            // lblType
            // 
            this.lblType.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblType.Location = new System.Drawing.Point(280, 48);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(184, 18);
            this.lblType.TabIndex = 2;
            this.lblType.Text = "符号类型";
            this.lblType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupStyle
            // 
            this.groupStyle.Controls.Add(this.lblStyle);
            this.groupStyle.Controls.Add(this.cboStyle);
            this.groupStyle.Controls.Add(this.lblColor);
            this.groupStyle.Controls.Add(this.btnColor);
            this.groupStyle.Controls.Add(this.lblSize);
            this.groupStyle.Controls.Add(this.numSize);
            this.groupStyle.Controls.Add(this.chkDrawOutline);
            this.groupStyle.Controls.Add(this.lblOutline);
            this.groupStyle.Controls.Add(this.btnOutlineColor);
            this.groupStyle.Controls.Add(this.lblOutlineWidth);
            this.groupStyle.Controls.Add(this.numOutlineWidth);
            this.groupStyle.Location = new System.Drawing.Point(16, 80);
            this.groupStyle.Name = "groupStyle";
            this.groupStyle.Size = new System.Drawing.Size(280, 250);
            this.groupStyle.TabIndex = 3;
            this.groupStyle.TabStop = false;
            this.groupStyle.Text = "符号参数";
            // 
            // lblStyle
            // 
            this.lblStyle.AutoSize = true;
            this.lblStyle.Location = new System.Drawing.Point(16, 36);
            this.lblStyle.Name = "lblStyle";
            this.lblStyle.Size = new System.Drawing.Size(62, 18);
            this.lblStyle.TabIndex = 0;
            this.lblStyle.Text = "样式：";
            // 
            // cboStyle
            // 
            this.cboStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStyle.FormattingEnabled = true;
            this.cboStyle.Location = new System.Drawing.Point(100, 32);
            this.cboStyle.Name = "cboStyle";
            this.cboStyle.Size = new System.Drawing.Size(160, 26);
            this.cboStyle.TabIndex = 1;
            this.cboStyle.SelectedIndexChanged += new System.EventHandler(this.cboStyle_SelectedIndexChanged);
            // 
            // lblColor
            // 
            this.lblColor.AutoSize = true;
            this.lblColor.Location = new System.Drawing.Point(16, 76);
            this.lblColor.Name = "lblColor";
            this.lblColor.Size = new System.Drawing.Size(62, 18);
            this.lblColor.TabIndex = 2;
            this.lblColor.Text = "颜色：";
            // 
            // btnColor
            // 
            this.btnColor.Location = new System.Drawing.Point(100, 70);
            this.btnColor.Name = "btnColor";
            this.btnColor.Size = new System.Drawing.Size(160, 30);
            this.btnColor.TabIndex = 3;
            this.btnColor.UseVisualStyleBackColor = false;
            this.btnColor.Click += new System.EventHandler(this.btnColor_Click);
            // 
            // lblSize
            // 
            this.lblSize.AutoSize = true;
            this.lblSize.Location = new System.Drawing.Point(16, 120);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(62, 18);
            this.lblSize.TabIndex = 4;
            this.lblSize.Text = "大小：";
            // 
            // numSize
            // 
            this.numSize.DecimalPlaces = 1;
            //this.numSize.Increment = 0.5M;
            this.numSize.Location = new System.Drawing.Point(100, 116);
            this.numSize.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            this.numSize.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.numSize.Name = "numSize";
            this.numSize.Size = new System.Drawing.Size(160, 26);
            this.numSize.TabIndex = 5;
            this.numSize.Value = new decimal(new int[] { 8, 0, 0, 0 });
            this.numSize.ValueChanged += new System.EventHandler(this.numSize_ValueChanged);
            // 
            // lblOutline
            // 
            this.lblOutline.AutoSize = true;
            this.lblOutline.Location = new System.Drawing.Point(16, 186);
            this.lblOutline.Name = "lblOutline";
            this.lblOutline.Size = new System.Drawing.Size(80, 18);
            this.lblOutline.TabIndex = 6;
            this.lblOutline.Text = "轮廓色：";
            // 
            // btnOutlineColor
            // 
            this.btnOutlineColor.Location = new System.Drawing.Point(100, 180);
            this.btnOutlineColor.Name = "btnOutlineColor";
            this.btnOutlineColor.Size = new System.Drawing.Size(160, 30);
            this.btnOutlineColor.TabIndex = 7;
            this.btnOutlineColor.UseVisualStyleBackColor = false;
            this.btnOutlineColor.Click += new System.EventHandler(this.btnOutlineColor_Click);
            // 
            // lblOutlineWidth
            // 
            this.lblOutlineWidth.AutoSize = true;
            this.lblOutlineWidth.Location = new System.Drawing.Point(16, 226);
            this.lblOutlineWidth.Name = "lblOutlineWidth";
            this.lblOutlineWidth.Size = new System.Drawing.Size(80, 18);
            this.lblOutlineWidth.TabIndex = 8;
            this.lblOutlineWidth.Text = "轮廓宽：";
            // 
            // numOutlineWidth
            // 
            this.numOutlineWidth.DecimalPlaces = 1;
            this.numOutlineWidth.Location = new System.Drawing.Point(100, 222);
            this.numOutlineWidth.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            this.numOutlineWidth.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            this.numOutlineWidth.Name = "numOutlineWidth";
            this.numOutlineWidth.Size = new System.Drawing.Size(160, 26);
            this.numOutlineWidth.TabIndex = 9;
            this.numOutlineWidth.Value = new decimal(new int[] { 1, 0, 0, 0 });
            this.numOutlineWidth.ValueChanged += new System.EventHandler(this.numOutlineWidth_ValueChanged);
            // 
            // chkDrawOutline
            // 
            this.chkDrawOutline.AutoSize = true;
            this.chkDrawOutline.Checked = true;
            this.chkDrawOutline.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDrawOutline.Location = new System.Drawing.Point(100, 152);
            this.chkDrawOutline.Name = "chkDrawOutline";
            this.chkDrawOutline.Size = new System.Drawing.Size(160, 22);
            this.chkDrawOutline.TabIndex = 10;
            this.chkDrawOutline.Text = "显示外轮廓线";
            this.chkDrawOutline.UseVisualStyleBackColor = true;
            this.chkDrawOutline.CheckedChanged += new System.EventHandler(this.chkDrawOutline_CheckedChanged);
            // 
            // groupPreview
            // 
            this.groupPreview.Controls.Add(this.panelPreview);
            this.groupPreview.Location = new System.Drawing.Point(312, 80);
            this.groupPreview.Name = "groupPreview";
            this.groupPreview.Size = new System.Drawing.Size(168, 250);
            this.groupPreview.TabIndex = 4;
            this.groupPreview.TabStop = false;
            this.groupPreview.Text = "预览";
            // 
            // panelPreview
            // 
            this.panelPreview.BackColor = System.Drawing.Color.White;
            this.panelPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPreview.Location = new System.Drawing.Point(16, 28);
            this.panelPreview.Name = "panelPreview";
            this.panelPreview.Size = new System.Drawing.Size(136, 202);
            this.panelPreview.TabIndex = 0;
            this.panelPreview.Paint += new System.Windows.Forms.PaintEventHandler(this.panelPreview_Paint);
            // 
            // lblTip
            // 
            this.lblTip.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblTip.Location = new System.Drawing.Point(16, 342);
            this.lblTip.Name = "lblTip";
            this.lblTip.Size = new System.Drawing.Size(464, 36);
            this.lblTip.TabIndex = 5;
            this.lblTip.Text = "说明：在 TOC 中单击矢量图层图例符号可打开本窗体。\r\n仅支持点、线、面简单符号；栅格图层不可设置。";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(260, 390);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 34);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(380, 390);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 34);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // SymbolForm
            // 
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 442);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblTip);
            this.Controls.Add(this.groupPreview);
            this.Controls.Add(this.groupStyle);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.lblLayer);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SymbolForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "符号设置";
            this.groupStyle.ResumeLayout(false);
            this.groupStyle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOutlineWidth)).EndInit();
            this.groupPreview.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblLayer;
        private System.Windows.Forms.Label lblType;
        private System.Windows.Forms.GroupBox groupStyle;
        private System.Windows.Forms.Label lblStyle;
        private System.Windows.Forms.ComboBox cboStyle;
        private System.Windows.Forms.Label lblColor;
        private System.Windows.Forms.Button btnColor;
        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.NumericUpDown numSize;
        private System.Windows.Forms.Label lblOutline;
        private System.Windows.Forms.Button btnOutlineColor;
        private System.Windows.Forms.Label lblOutlineWidth;
        private System.Windows.Forms.NumericUpDown numOutlineWidth;
        private System.Windows.Forms.CheckBox chkDrawOutline;
        private System.Windows.Forms.GroupBox groupPreview;
        private System.Windows.Forms.Panel panelPreview;
        private System.Windows.Forms.Label lblTip;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}

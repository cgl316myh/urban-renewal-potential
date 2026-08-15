namespace UrbanRenewal.Host
{
    partial class RasterRenderForm
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
            this.lblLayerCaption = new System.Windows.Forms.Label();
            this.lblLayer = new System.Windows.Forms.Label();
            this.groupParam = new System.Windows.Forms.GroupBox();
            this.lblMode = new System.Windows.Forms.Label();
            this.cboMode = new System.Windows.Forms.ComboBox();
            this.lblClasses = new System.Windows.Forms.Label();
            this.numClasses = new System.Windows.Forms.NumericUpDown();
            this.lblMethod = new System.Windows.Forms.Label();
            this.cboMethod = new System.Windows.Forms.ComboBox();
            this.lblColorLow = new System.Windows.Forms.Label();
            this.btnColorLow = new System.Windows.Forms.Button();
            this.lblColorHigh = new System.Windows.Forms.Label();
            this.btnColorHigh = new System.Windows.Forms.Button();
            this.chkDrawOutline = new System.Windows.Forms.CheckBox();
            this.lblOutlineColor = new System.Windows.Forms.Label();
            this.btnOutlineColor = new System.Windows.Forms.Button();
            this.lblOutlineWidth = new System.Windows.Forms.Label();
            this.numOutlineWidth = new System.Windows.Forms.NumericUpDown();
            this.chkReverseRamp = new System.Windows.Forms.CheckBox();
            this.lblTip = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupParam.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numClasses)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOutlineWidth)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblTitle.Location = new System.Drawing.Point(16, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(448, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "栅格图层渲染";
            // 
            // lblLayerCaption
            // 
            this.lblLayerCaption.AutoSize = true;
            this.lblLayerCaption.Location = new System.Drawing.Point(16, 52);
            this.lblLayerCaption.Name = "lblLayerCaption";
            this.lblLayerCaption.Size = new System.Drawing.Size(80, 18);
            this.lblLayerCaption.TabIndex = 1;
            this.lblLayerCaption.Text = "当前图层：";
            // 
            // lblLayer
            // 
            this.lblLayer.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblLayer.Location = new System.Drawing.Point(100, 52);
            this.lblLayer.Name = "lblLayer";
            this.lblLayer.Size = new System.Drawing.Size(364, 18);
            this.lblLayer.TabIndex = 2;
            this.lblLayer.Text = "";
            // 
            // groupParam
            // 
            this.groupParam.Controls.Add(this.lblMode);
            this.groupParam.Controls.Add(this.cboMode);
            this.groupParam.Controls.Add(this.lblClasses);
            this.groupParam.Controls.Add(this.numClasses);
            this.groupParam.Controls.Add(this.lblMethod);
            this.groupParam.Controls.Add(this.cboMethod);
            this.groupParam.Controls.Add(this.lblColorLow);
            this.groupParam.Controls.Add(this.btnColorLow);
            this.groupParam.Controls.Add(this.lblColorHigh);
            this.groupParam.Controls.Add(this.btnColorHigh);
            this.groupParam.Controls.Add(this.chkDrawOutline);
            this.groupParam.Controls.Add(this.lblOutlineColor);
            this.groupParam.Controls.Add(this.btnOutlineColor);
            this.groupParam.Controls.Add(this.lblOutlineWidth);
            this.groupParam.Controls.Add(this.numOutlineWidth);
            this.groupParam.Controls.Add(this.chkReverseRamp);
            this.groupParam.Location = new System.Drawing.Point(16, 84);
            this.groupParam.Name = "groupParam";
            this.groupParam.Size = new System.Drawing.Size(448, 300);
            this.groupParam.TabIndex = 3;
            this.groupParam.TabStop = false;
            this.groupParam.Text = "渲染参数";
            // 
            // lblMode
            // 
            this.lblMode.AutoSize = true;
            this.lblMode.Location = new System.Drawing.Point(20, 40);
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new System.Drawing.Size(116, 18);
            this.lblMode.TabIndex = 0;
            this.lblMode.Text = "渲染方式：";
            // 
            // cboMode
            // 
            this.cboMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMode.FormattingEnabled = true;
            this.cboMode.Location = new System.Drawing.Point(160, 36);
            this.cboMode.Name = "cboMode";
            this.cboMode.Size = new System.Drawing.Size(260, 26);
            this.cboMode.TabIndex = 1;
            this.cboMode.SelectedIndexChanged += new System.EventHandler(this.cboMode_SelectedIndexChanged);
            // 
            // lblClasses
            // 
            this.lblClasses.AutoSize = true;
            this.lblClasses.Location = new System.Drawing.Point(20, 80);
            this.lblClasses.Name = "lblClasses";
            this.lblClasses.Size = new System.Drawing.Size(116, 18);
            this.lblClasses.TabIndex = 2;
            this.lblClasses.Text = "分类级数：";
            // 
            // numClasses
            // 
            this.numClasses.Location = new System.Drawing.Point(160, 76);
            this.numClasses.Maximum = new decimal(new int[] { 12, 0, 0, 0 });
            this.numClasses.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
            this.numClasses.Name = "numClasses";
            this.numClasses.Size = new System.Drawing.Size(120, 26);
            this.numClasses.TabIndex = 3;
            this.numClasses.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // lblMethod
            // 
            this.lblMethod.AutoSize = true;
            this.lblMethod.Location = new System.Drawing.Point(20, 120);
            this.lblMethod.Name = "lblMethod";
            this.lblMethod.Size = new System.Drawing.Size(116, 18);
            this.lblMethod.TabIndex = 4;
            this.lblMethod.Text = "分级方法：";
            // 
            // cboMethod
            // 
            this.cboMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMethod.FormattingEnabled = true;
            this.cboMethod.Location = new System.Drawing.Point(160, 116);
            this.cboMethod.Name = "cboMethod";
            this.cboMethod.Size = new System.Drawing.Size(260, 26);
            this.cboMethod.TabIndex = 5;
            // 
            // lblColorLow
            // 
            this.lblColorLow.AutoSize = true;
            this.lblColorLow.Location = new System.Drawing.Point(20, 160);
            this.lblColorLow.Name = "lblColorLow";
            this.lblColorLow.Size = new System.Drawing.Size(98, 18);
            this.lblColorLow.TabIndex = 6;
            this.lblColorLow.Text = "低值颜色：";
            // 
            // btnColorLow
            // 
            this.btnColorLow.Location = new System.Drawing.Point(160, 154);
            this.btnColorLow.Name = "btnColorLow";
            this.btnColorLow.Size = new System.Drawing.Size(100, 30);
            this.btnColorLow.TabIndex = 7;
            this.btnColorLow.UseVisualStyleBackColor = false;
            this.btnColorLow.Click += new System.EventHandler(this.btnColorLow_Click);
            // 
            // lblColorHigh
            // 
            this.lblColorHigh.AutoSize = true;
            this.lblColorHigh.Location = new System.Drawing.Point(280, 160);
            this.lblColorHigh.Name = "lblColorHigh";
            this.lblColorHigh.Size = new System.Drawing.Size(98, 18);
            this.lblColorHigh.TabIndex = 8;
            this.lblColorHigh.Text = "高值颜色：";
            // 
            // btnColorHigh
            // 
            this.btnColorHigh.Location = new System.Drawing.Point(380, 154);
            this.btnColorHigh.Name = "btnColorHigh";
            this.btnColorHigh.Size = new System.Drawing.Size(40, 30);
            this.btnColorHigh.TabIndex = 9;
            this.btnColorHigh.UseVisualStyleBackColor = false;
            this.btnColorHigh.Click += new System.EventHandler(this.btnColorHigh_Click);
            // 
            // chkDrawOutline
            // 
            this.chkDrawOutline.AutoSize = true;
            this.chkDrawOutline.Location = new System.Drawing.Point(160, 198);
            this.chkDrawOutline.Name = "chkDrawOutline";
            this.chkDrawOutline.Size = new System.Drawing.Size(196, 22);
            this.chkDrawOutline.TabIndex = 10;
            this.chkDrawOutline.Text = "显示边线";
            this.chkDrawOutline.UseVisualStyleBackColor = true;
            this.chkDrawOutline.CheckedChanged += new System.EventHandler(this.chkDrawOutline_CheckedChanged);
            // 
            // lblOutlineColor
            // 
            this.lblOutlineColor.AutoSize = true;
            this.lblOutlineColor.Location = new System.Drawing.Point(20, 232);
            this.lblOutlineColor.Name = "lblOutlineColor";
            this.lblOutlineColor.Size = new System.Drawing.Size(98, 18);
            this.lblOutlineColor.TabIndex = 11;
            this.lblOutlineColor.Text = "边线颜色：";
            // 
            // btnOutlineColor
            // 
            this.btnOutlineColor.Location = new System.Drawing.Point(160, 226);
            this.btnOutlineColor.Name = "btnOutlineColor";
            this.btnOutlineColor.Size = new System.Drawing.Size(100, 30);
            this.btnOutlineColor.TabIndex = 12;
            this.btnOutlineColor.UseVisualStyleBackColor = false;
            this.btnOutlineColor.Click += new System.EventHandler(this.btnOutlineColor_Click);
            // 
            // lblOutlineWidth
            // 
            this.lblOutlineWidth.AutoSize = true;
            this.lblOutlineWidth.Location = new System.Drawing.Point(280, 232);
            this.lblOutlineWidth.Name = "lblOutlineWidth";
            this.lblOutlineWidth.Size = new System.Drawing.Size(80, 18);
            this.lblOutlineWidth.TabIndex = 13;
            this.lblOutlineWidth.Text = "线宽：";
            // 
            // numOutlineWidth
            // 
            this.numOutlineWidth.DecimalPlaces = 1;
            this.numOutlineWidth.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            this.numOutlineWidth.Location = new System.Drawing.Point(360, 228);
            this.numOutlineWidth.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            this.numOutlineWidth.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            this.numOutlineWidth.Name = "numOutlineWidth";
            this.numOutlineWidth.Size = new System.Drawing.Size(60, 26);
            this.numOutlineWidth.TabIndex = 14;
            this.numOutlineWidth.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // chkReverseRamp
            // 
            this.chkReverseRamp.AutoSize = true;
            this.chkReverseRamp.Location = new System.Drawing.Point(160, 266);
            this.chkReverseRamp.Name = "chkReverseRamp";
            this.chkReverseRamp.Size = new System.Drawing.Size(196, 22);
            this.chkReverseRamp.TabIndex = 15;
            this.chkReverseRamp.Text = "反转色带";
            this.chkReverseRamp.UseVisualStyleBackColor = true;
            // 
            // lblTip
            // 
            this.lblTip.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblTip.Location = new System.Drawing.Point(16, 396);
            this.lblTip.Name = "lblTip";
            this.lblTip.Size = new System.Drawing.Size(448, 48);
            this.lblTip.TabIndex = 4;
            this.lblTip.Text = "说明：拉伸适合连续得分；分级适合专题图；唯一值适合等级栅格。\r\n边线：另存临时栅格后转面叠加（避开中文路径）；浮点会先 Int。等级栅格效果更好。";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(244, 454);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 34);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "确定";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(364, 454);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 34);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // RasterRenderForm
            // 
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 506);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblTip);
            this.Controls.Add(this.groupParam);
            this.Controls.Add(this.lblLayer);
            this.Controls.Add(this.lblLayerCaption);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RasterRenderForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "栅格渲染";
            this.groupParam.ResumeLayout(false);
            this.groupParam.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numClasses)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOutlineWidth)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblLayerCaption;
        private System.Windows.Forms.Label lblLayer;
        private System.Windows.Forms.GroupBox groupParam;
        private System.Windows.Forms.Label lblMode;
        private System.Windows.Forms.ComboBox cboMode;
        private System.Windows.Forms.Label lblClasses;
        private System.Windows.Forms.NumericUpDown numClasses;
        private System.Windows.Forms.Label lblMethod;
        private System.Windows.Forms.ComboBox cboMethod;
        private System.Windows.Forms.Label lblColorLow;
        private System.Windows.Forms.Button btnColorLow;
        private System.Windows.Forms.Label lblColorHigh;
        private System.Windows.Forms.Button btnColorHigh;
        private System.Windows.Forms.CheckBox chkDrawOutline;
        private System.Windows.Forms.Label lblOutlineColor;
        private System.Windows.Forms.Button btnOutlineColor;
        private System.Windows.Forms.Label lblOutlineWidth;
        private System.Windows.Forms.NumericUpDown numOutlineWidth;
        private System.Windows.Forms.CheckBox chkReverseRamp;
        private System.Windows.Forms.Label lblTip;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}

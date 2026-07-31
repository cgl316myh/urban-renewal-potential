namespace UrbanRenewal.Host
{
    partial class UniqueValueRenderForm
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
            this.lblGeom = new System.Windows.Forms.Label();
            this.groupParam = new System.Windows.Forms.GroupBox();
            this.lblField = new System.Windows.Forms.Label();
            this.cboField = new System.Windows.Forms.ComboBox();
            this.chkDrawOutline = new System.Windows.Forms.CheckBox();
            this.lblTip = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupParam.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblTitle.Location = new System.Drawing.Point(16, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(448, 28);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "唯一值渲染";
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
            this.lblLayer.Size = new System.Drawing.Size(240, 18);
            this.lblLayer.TabIndex = 2;
            this.lblLayer.Text = "";
            // 
            // lblGeom
            // 
            this.lblGeom.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblGeom.Location = new System.Drawing.Point(350, 52);
            this.lblGeom.Name = "lblGeom";
            this.lblGeom.Size = new System.Drawing.Size(114, 18);
            this.lblGeom.TabIndex = 3;
            this.lblGeom.Text = "";
            this.lblGeom.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupParam
            // 
            this.groupParam.Controls.Add(this.lblField);
            this.groupParam.Controls.Add(this.cboField);
            this.groupParam.Controls.Add(this.chkDrawOutline);
            this.groupParam.Location = new System.Drawing.Point(16, 84);
            this.groupParam.Name = "groupParam";
            this.groupParam.Size = new System.Drawing.Size(448, 120);
            this.groupParam.TabIndex = 4;
            this.groupParam.TabStop = false;
            this.groupParam.Text = "渲染参数";
            // 
            // lblField
            // 
            this.lblField.AutoSize = true;
            this.lblField.Location = new System.Drawing.Point(20, 40);
            this.lblField.Name = "lblField";
            this.lblField.Size = new System.Drawing.Size(98, 18);
            this.lblField.TabIndex = 0;
            this.lblField.Text = "分类字段：";
            // 
            // cboField
            // 
            this.cboField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboField.FormattingEnabled = true;
            this.cboField.Location = new System.Drawing.Point(140, 36);
            this.cboField.Name = "cboField";
            this.cboField.Size = new System.Drawing.Size(280, 26);
            this.cboField.TabIndex = 1;
            // 
            // chkDrawOutline
            // 
            this.chkDrawOutline.AutoSize = true;
            this.chkDrawOutline.Checked = true;
            this.chkDrawOutline.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDrawOutline.Location = new System.Drawing.Point(140, 76);
            this.chkDrawOutline.Name = "chkDrawOutline";
            this.chkDrawOutline.Size = new System.Drawing.Size(196, 22);
            this.chkDrawOutline.TabIndex = 2;
            this.chkDrawOutline.Text = "绘制面外边线";
            this.chkDrawOutline.UseVisualStyleBackColor = true;
            // 
            // lblTip
            // 
            this.lblTip.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblTip.Location = new System.Drawing.Point(16, 220);
            this.lblTip.Name = "lblTip";
            this.lblTip.Size = new System.Drawing.Size(448, 48);
            this.lblTip.TabIndex = 5;
            this.lblTip.Text = "说明：按所选字段的每个唯一值分配不同颜色。\r\n适用于点、线、面图层。可从 TOC 右键菜单打开。";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(244, 284);
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
            this.btnCancel.Location = new System.Drawing.Point(364, 284);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 34);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // UniqueValueRenderForm
            // 
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 336);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblTip);
            this.Controls.Add(this.groupParam);
            this.Controls.Add(this.lblGeom);
            this.Controls.Add(this.lblLayer);
            this.Controls.Add(this.lblLayerCaption);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UniqueValueRenderForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "唯一值渲染";
            this.groupParam.ResumeLayout(false);
            this.groupParam.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblLayerCaption;
        private System.Windows.Forms.Label lblLayer;
        private System.Windows.Forms.Label lblGeom;
        private System.Windows.Forms.GroupBox groupParam;
        private System.Windows.Forms.Label lblField;
        private System.Windows.Forms.ComboBox cboField;
        private System.Windows.Forms.CheckBox chkDrawOutline;
        private System.Windows.Forms.Label lblTip;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}

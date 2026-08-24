namespace UrbanRenewal.Plugins.Motivation
{
    partial class MotivationRunForm
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
            this.grpWeights = new System.Windows.Forms.GroupBox();
            this.lblTraffic = new System.Windows.Forms.Label();
            this.nudTraffic = new System.Windows.Forms.NumericUpDown();
            this.lblEnvironment = new System.Windows.Forms.Label();
            this.nudEnvironment = new System.Windows.Forms.NumericUpDown();
            this.lblFacility = new System.Windows.Forms.Label();
            this.nudFacility = new System.Windows.Forms.NumericUpDown();
            this.lblPolicy = new System.Windows.Forms.Label();
            this.nudPolicy = new System.Windows.Forms.NumericUpDown();
            this.grpExternalTraffic = new System.Windows.Forms.GroupBox();
            this.chkClipToStudyArea = new System.Windows.Forms.CheckBox();
            this.cboTrafficScoreMode = new System.Windows.Forms.ComboBox();
            this.lblTrafficScoreMode = new System.Windows.Forms.Label();
            this.btnBrowseTraffic = new System.Windows.Forms.Button();
            this.txtTrafficRaster = new System.Windows.Forms.TextBox();
            this.lblTrafficRaster = new System.Windows.Forms.Label();
            this.chkUseExternalTraffic = new System.Windows.Forms.CheckBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.grpWeights.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTraffic)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEnvironment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFacility)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPolicy)).BeginInit();
            this.grpExternalTraffic.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpWeights
            // 
            this.grpWeights.Controls.Add(this.lblTraffic);
            this.grpWeights.Controls.Add(this.nudTraffic);
            this.grpWeights.Controls.Add(this.lblEnvironment);
            this.grpWeights.Controls.Add(this.nudEnvironment);
            this.grpWeights.Controls.Add(this.lblFacility);
            this.grpWeights.Controls.Add(this.nudFacility);
            this.grpWeights.Controls.Add(this.lblPolicy);
            this.grpWeights.Controls.Add(this.nudPolicy);
            this.grpWeights.Location = new System.Drawing.Point(20, 16);
            this.grpWeights.Name = "grpWeights";
            this.grpWeights.Size = new System.Drawing.Size(582, 110);
            this.grpWeights.TabIndex = 0;
            this.grpWeights.TabStop = false;
            this.grpWeights.Text = "准则层权重（%，可被城市配置覆盖）";
            // 
            // lblTraffic
            // 
            this.lblTraffic.AutoSize = true;
            this.lblTraffic.Location = new System.Drawing.Point(20, 32);
            this.lblTraffic.Name = "lblTraffic";
            this.lblTraffic.Size = new System.Drawing.Size(77, 12);
            this.lblTraffic.TabIndex = 0;
            this.lblTraffic.Text = "交通便捷度";
            // 
            // nudTraffic
            // 
            this.nudTraffic.Location = new System.Drawing.Point(110, 28);
            this.nudTraffic.Name = "nudTraffic";
            this.nudTraffic.Size = new System.Drawing.Size(60, 21);
            this.nudTraffic.TabIndex = 1;
            this.nudTraffic.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // lblEnvironment
            // 
            this.lblEnvironment.AutoSize = true;
            this.lblEnvironment.Location = new System.Drawing.Point(210, 32);
            this.lblEnvironment.Name = "lblEnvironment";
            this.lblEnvironment.Size = new System.Drawing.Size(77, 12);
            this.lblEnvironment.TabIndex = 2;
            this.lblEnvironment.Text = "环境舒适度";
            // 
            // nudEnvironment
            // 
            this.nudEnvironment.Location = new System.Drawing.Point(300, 28);
            this.nudEnvironment.Name = "nudEnvironment";
            this.nudEnvironment.Size = new System.Drawing.Size(60, 21);
            this.nudEnvironment.TabIndex = 3;
            this.nudEnvironment.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // lblFacility
            // 
            this.lblFacility.AutoSize = true;
            this.lblFacility.Location = new System.Drawing.Point(20, 68);
            this.lblFacility.Name = "lblFacility";
            this.lblFacility.Size = new System.Drawing.Size(77, 12);
            this.lblFacility.TabIndex = 4;
            this.lblFacility.Text = "设施完善度";
            // 
            // nudFacility
            // 
            this.nudFacility.Location = new System.Drawing.Point(110, 64);
            this.nudFacility.Name = "nudFacility";
            this.nudFacility.Size = new System.Drawing.Size(60, 21);
            this.nudFacility.TabIndex = 5;
            this.nudFacility.Value = new decimal(new int[] { 25, 0, 0, 0 });
            // 
            // lblPolicy
            // 
            this.lblPolicy.AutoSize = true;
            this.lblPolicy.Location = new System.Drawing.Point(210, 68);
            this.lblPolicy.Name = "lblPolicy";
            this.lblPolicy.Size = new System.Drawing.Size(77, 12);
            this.lblPolicy.TabIndex = 6;
            this.lblPolicy.Text = "政策支持度";
            // 
            // nudPolicy
            // 
            this.nudPolicy.Location = new System.Drawing.Point(300, 64);
            this.nudPolicy.Name = "nudPolicy";
            this.nudPolicy.Size = new System.Drawing.Size(60, 21);
            this.nudPolicy.TabIndex = 7;
            this.nudPolicy.Value = new decimal(new int[] { 25, 0, 0, 0 });
            // 
            // grpExternalTraffic
            // 
            this.grpExternalTraffic.Controls.Add(this.chkClipToStudyArea);
            this.grpExternalTraffic.Controls.Add(this.cboTrafficScoreMode);
            this.grpExternalTraffic.Controls.Add(this.lblTrafficScoreMode);
            this.grpExternalTraffic.Controls.Add(this.btnBrowseTraffic);
            this.grpExternalTraffic.Controls.Add(this.txtTrafficRaster);
            this.grpExternalTraffic.Controls.Add(this.lblTrafficRaster);
            this.grpExternalTraffic.Controls.Add(this.chkUseExternalTraffic);
            this.grpExternalTraffic.Location = new System.Drawing.Point(20, 118);
            this.grpExternalTraffic.Name = "grpExternalTraffic";
            this.grpExternalTraffic.Size = new System.Drawing.Size(582, 112);
            this.grpExternalTraffic.TabIndex = 1;
            this.grpExternalTraffic.TabStop = false;
            this.grpExternalTraffic.Text = "外部交通栅格（可选，跳过内置交通计算）";
            // 
            // chkClipToStudyArea
            // 
            this.chkClipToStudyArea.AutoSize = true;
            this.chkClipToStudyArea.Checked = true;
            this.chkClipToStudyArea.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkClipToStudyArea.Location = new System.Drawing.Point(400, 78);
            this.chkClipToStudyArea.Name = "chkClipToStudyArea";
            this.chkClipToStudyArea.Size = new System.Drawing.Size(120, 16);
            this.chkClipToStudyArea.TabIndex = 6;
            this.chkClipToStudyArea.Text = "按中心城区裁切";
            this.chkClipToStudyArea.UseVisualStyleBackColor = true;
            // 
            // cboTrafficScoreMode
            // 
            this.cboTrafficScoreMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboTrafficScoreMode.FormattingEnabled = true;
            this.cboTrafficScoreMode.Items.AddRange(new object[] {
            "原始分 0–5",
            "已标准化 0–100"});
            this.cboTrafficScoreMode.Location = new System.Drawing.Point(110, 76);
            this.cboTrafficScoreMode.Name = "cboTrafficScoreMode";
            this.cboTrafficScoreMode.Size = new System.Drawing.Size(140, 20);
            this.cboTrafficScoreMode.TabIndex = 5;
            // 
            // lblTrafficScoreMode
            // 
            this.lblTrafficScoreMode.AutoSize = true;
            this.lblTrafficScoreMode.Location = new System.Drawing.Point(20, 80);
            this.lblTrafficScoreMode.Name = "lblTrafficScoreMode";
            this.lblTrafficScoreMode.Size = new System.Drawing.Size(65, 12);
            this.lblTrafficScoreMode.TabIndex = 4;
            this.lblTrafficScoreMode.Text = "分值语义";
            // 
            // btnBrowseTraffic
            // 
            this.btnBrowseTraffic.Location = new System.Drawing.Point(516, 44);
            this.btnBrowseTraffic.Name = "btnBrowseTraffic";
            this.btnBrowseTraffic.Size = new System.Drawing.Size(54, 23);
            this.btnBrowseTraffic.TabIndex = 3;
            this.btnBrowseTraffic.Text = "浏览…";
            this.btnBrowseTraffic.UseVisualStyleBackColor = true;
            this.btnBrowseTraffic.Click += new System.EventHandler(this.btnBrowseTraffic_Click);
            // 
            // txtTrafficRaster
            // 
            this.txtTrafficRaster.Location = new System.Drawing.Point(110, 46);
            this.txtTrafficRaster.Name = "txtTrafficRaster";
            this.txtTrafficRaster.Size = new System.Drawing.Size(400, 21);
            this.txtTrafficRaster.TabIndex = 2;
            // 
            // lblTrafficRaster
            // 
            this.lblTrafficRaster.AutoSize = true;
            this.lblTrafficRaster.Location = new System.Drawing.Point(20, 50);
            this.lblTrafficRaster.Name = "lblTrafficRaster";
            this.lblTrafficRaster.Size = new System.Drawing.Size(65, 12);
            this.lblTrafficRaster.TabIndex = 1;
            this.lblTrafficRaster.Text = "栅格路径";
            // 
            // chkUseExternalTraffic
            // 
            this.chkUseExternalTraffic.AutoSize = true;
            this.chkUseExternalTraffic.Location = new System.Drawing.Point(20, 24);
            this.chkUseExternalTraffic.Name = "chkUseExternalTraffic";
            this.chkUseExternalTraffic.Size = new System.Drawing.Size(276, 16);
            this.chkUseExternalTraffic.TabIndex = 0;
            this.chkUseExternalTraffic.Text = "使用外部交通栅格（不重投影/重采样，仅裁切）";
            this.chkUseExternalTraffic.UseVisualStyleBackColor = true;
            this.chkUseExternalTraffic.CheckedChanged += new System.EventHandler(this.chkUseExternalTraffic_CheckedChanged);
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(420, 246);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(90, 30);
            this.btnRun.TabIndex = 2;
            this.btnRun.Text = "开始分析";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(516, 246);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(86, 30);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // MotivationRunForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 292);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.grpExternalTraffic);
            this.Controls.Add(this.grpWeights);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MotivationRunForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "动力性分析";
            this.grpWeights.ResumeLayout(false);
            this.grpWeights.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTraffic)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEnvironment)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFacility)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPolicy)).EndInit();
            this.grpExternalTraffic.ResumeLayout(false);
            this.grpExternalTraffic.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox grpWeights;
        private System.Windows.Forms.Label lblTraffic;
        private System.Windows.Forms.NumericUpDown nudTraffic;
        private System.Windows.Forms.Label lblEnvironment;
        private System.Windows.Forms.NumericUpDown nudEnvironment;
        private System.Windows.Forms.Label lblFacility;
        private System.Windows.Forms.NumericUpDown nudFacility;
        private System.Windows.Forms.Label lblPolicy;
        private System.Windows.Forms.NumericUpDown nudPolicy;
        private System.Windows.Forms.GroupBox grpExternalTraffic;
        private System.Windows.Forms.CheckBox chkUseExternalTraffic;
        private System.Windows.Forms.Label lblTrafficRaster;
        private System.Windows.Forms.TextBox txtTrafficRaster;
        private System.Windows.Forms.Button btnBrowseTraffic;
        private System.Windows.Forms.Label lblTrafficScoreMode;
        private System.Windows.Forms.ComboBox cboTrafficScoreMode;
        private System.Windows.Forms.CheckBox chkClipToStudyArea;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnClose;
    }
}

namespace UrbanRenewal.Plugins.Config
{
    partial class SystemConfigForm
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
            this.grpOverlay = new System.Windows.Forms.GroupBox();
            this.lblMotivW = new System.Windows.Forms.Label();
            this.nudMotivW = new System.Windows.Forms.NumericUpDown();
            this.lblFeasibW = new System.Windows.Forms.Label();
            this.nudFeasibW = new System.Windows.Forms.NumericUpDown();
            this.grpCity = new System.Windows.Forms.GroupBox();
            this.lblCity = new System.Windows.Forms.Label();
            this.lblTraffic = new System.Windows.Forms.Label();
            this.nudTraffic = new System.Windows.Forms.NumericUpDown();
            this.lblEnv = new System.Windows.Forms.Label();
            this.nudEnv = new System.Windows.Forms.NumericUpDown();
            this.lblFac = new System.Windows.Forms.Label();
            this.nudFac = new System.Windows.Forms.NumericUpDown();
            this.lblPol = new System.Windows.Forms.Label();
            this.nudPol = new System.Windows.Forms.NumericUpDown();
            this.grpBuffer = new System.Windows.Forms.GroupBox();
            this.lblPreset = new System.Windows.Forms.Label();
            this.cboMetroPreset = new System.Windows.Forms.ComboBox();
            this.lblPresetTip = new System.Windows.Forms.Label();
            this.lblMultiDist = new System.Windows.Forms.Label();
            this.txtMultiDist = new System.Windows.Forms.TextBox();
            this.lblMultiScore = new System.Windows.Forms.Label();
            this.txtMultiScore = new System.Windows.Forms.TextBox();
            this.lblSingleDist = new System.Windows.Forms.Label();
            this.txtSingleDist = new System.Windows.Forms.TextBox();
            this.lblSingleScore = new System.Windows.Forms.Label();
            this.txtSingleScore = new System.Windows.Forms.TextBox();
            this.lblCbd = new System.Windows.Forms.Label();
            this.nudCbdDist = new System.Windows.Forms.NumericUpDown();
            this.nudCbdScore = new System.Windows.Forms.NumericUpDown();
            this.lblTrafFac = new System.Windows.Forms.Label();
            this.nudTrafDist = new System.Windows.Forms.NumericUpDown();
            this.nudTrafScore = new System.Windows.Forms.NumericUpDown();
            this.grpSkin = new System.Windows.Forms.GroupBox();
            this.cboSkin = new System.Windows.Forms.ComboBox();
            this.btnApplySkin = new System.Windows.Forms.Button();
            this.lblPlugins = new System.Windows.Forms.Label();
            this.txtPlugins = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.grpOverlay.SuspendLayout();
            this.grpCity.SuspendLayout();
            this.grpBuffer.SuspendLayout();
            this.grpSkin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMotivW)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFeasibW)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTraffic)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEnv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFac)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPol)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCbdDist)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCbdScore)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTrafDist)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTrafScore)).BeginInit();
            this.SuspendLayout();
            // 
            // grpOverlay
            // 
            this.grpOverlay.Controls.Add(this.lblMotivW);
            this.grpOverlay.Controls.Add(this.nudMotivW);
            this.grpOverlay.Controls.Add(this.lblFeasibW);
            this.grpOverlay.Controls.Add(this.nudFeasibW);
            this.grpOverlay.Location = new System.Drawing.Point(16, 12);
            this.grpOverlay.Name = "grpOverlay";
            this.grpOverlay.Size = new System.Drawing.Size(590, 60);
            this.grpOverlay.TabIndex = 0;
            this.grpOverlay.TabStop = false;
            this.grpOverlay.Text = "综合潜力叠置权重%";
            // 
            // lblMotivW
            // 
            this.lblMotivW.AutoSize = true;
            this.lblMotivW.Location = new System.Drawing.Point(16, 28);
            this.lblMotivW.Name = "lblMotivW";
            this.lblMotivW.Size = new System.Drawing.Size(65, 12);
            this.lblMotivW.TabIndex = 0;
            this.lblMotivW.Text = "动力性：";
            // 
            // nudMotivW
            // 
            this.nudMotivW.DecimalPlaces = 1;
            this.nudMotivW.Location = new System.Drawing.Point(80, 24);
            this.nudMotivW.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.nudMotivW.Name = "nudMotivW";
            this.nudMotivW.Size = new System.Drawing.Size(60, 21);
            this.nudMotivW.TabIndex = 1;
            this.nudMotivW.Value = new decimal(new int[] { 70, 0, 0, 0 });
            // 
            // lblFeasibW
            // 
            this.lblFeasibW.AutoSize = true;
            this.lblFeasibW.Location = new System.Drawing.Point(200, 28);
            this.lblFeasibW.Name = "lblFeasibW";
            this.lblFeasibW.Size = new System.Drawing.Size(65, 12);
            this.lblFeasibW.TabIndex = 2;
            this.lblFeasibW.Text = "可行度：";
            // 
            // nudFeasibW
            // 
            this.nudFeasibW.DecimalPlaces = 1;
            this.nudFeasibW.Location = new System.Drawing.Point(260, 24);
            this.nudFeasibW.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.nudFeasibW.Name = "nudFeasibW";
            this.nudFeasibW.Size = new System.Drawing.Size(60, 21);
            this.nudFeasibW.TabIndex = 3;
            this.nudFeasibW.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // grpCity
            // 
            this.grpCity.Controls.Add(this.lblCity);
            this.grpCity.Controls.Add(this.lblTraffic);
            this.grpCity.Controls.Add(this.nudTraffic);
            this.grpCity.Controls.Add(this.lblEnv);
            this.grpCity.Controls.Add(this.nudEnv);
            this.grpCity.Controls.Add(this.lblFac);
            this.grpCity.Controls.Add(this.nudFac);
            this.grpCity.Controls.Add(this.lblPol);
            this.grpCity.Controls.Add(this.nudPol);
            this.grpCity.Location = new System.Drawing.Point(16, 84);
            this.grpCity.Name = "grpCity";
            this.grpCity.Size = new System.Drawing.Size(590, 100);
            this.grpCity.TabIndex = 1;
            this.grpCity.TabStop = false;
            this.grpCity.Text = "动力性准则层权重%";
            // 
            // lblCity
            // 
            this.lblCity.Location = new System.Drawing.Point(16, 22);
            this.lblCity.Name = "lblCity";
            this.lblCity.Size = new System.Drawing.Size(550, 18);
            this.lblCity.TabIndex = 0;
            this.lblCity.Text = "城市配置：";
            // 
            // lblTraffic
            // 
            this.lblTraffic.AutoSize = true;
            this.lblTraffic.Location = new System.Drawing.Point(16, 52);
            this.lblTraffic.Name = "lblTraffic";
            this.lblTraffic.Size = new System.Drawing.Size(41, 12);
            this.lblTraffic.TabIndex = 1;
            this.lblTraffic.Text = "交通：";
            // 
            // nudTraffic
            // 
            this.nudTraffic.Location = new System.Drawing.Point(60, 48);
            this.nudTraffic.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.nudTraffic.Name = "nudTraffic";
            this.nudTraffic.Size = new System.Drawing.Size(50, 21);
            this.nudTraffic.TabIndex = 2;
            this.nudTraffic.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // lblEnv
            // 
            this.lblEnv.AutoSize = true;
            this.lblEnv.Location = new System.Drawing.Point(150, 52);
            this.lblEnv.Name = "lblEnv";
            this.lblEnv.Size = new System.Drawing.Size(41, 12);
            this.lblEnv.TabIndex = 3;
            this.lblEnv.Text = "环境：";
            // 
            // nudEnv
            // 
            this.nudEnv.Location = new System.Drawing.Point(194, 48);
            this.nudEnv.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.nudEnv.Name = "nudEnv";
            this.nudEnv.Size = new System.Drawing.Size(50, 21);
            this.nudEnv.TabIndex = 4;
            this.nudEnv.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // lblFac
            // 
            this.lblFac.AutoSize = true;
            this.lblFac.Location = new System.Drawing.Point(286, 52);
            this.lblFac.Name = "lblFac";
            this.lblFac.Size = new System.Drawing.Size(41, 12);
            this.lblFac.TabIndex = 5;
            this.lblFac.Text = "设施：";
            // 
            // nudFac
            // 
            this.nudFac.Location = new System.Drawing.Point(330, 48);
            this.nudFac.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.nudFac.Name = "nudFac";
            this.nudFac.Size = new System.Drawing.Size(50, 21);
            this.nudFac.TabIndex = 6;
            this.nudFac.Value = new decimal(new int[] { 25, 0, 0, 0 });
            // 
            // lblPol
            // 
            this.lblPol.AutoSize = true;
            this.lblPol.Location = new System.Drawing.Point(420, 52);
            this.lblPol.Name = "lblPol";
            this.lblPol.Size = new System.Drawing.Size(41, 12);
            this.lblPol.TabIndex = 7;
            this.lblPol.Text = "政策：";
            // 
            // nudPol
            // 
            this.nudPol.Location = new System.Drawing.Point(464, 48);
            this.nudPol.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.nudPol.Name = "nudPol";
            this.nudPol.Size = new System.Drawing.Size(50, 21);
            this.nudPol.TabIndex = 8;
            this.nudPol.Value = new decimal(new int[] { 25, 0, 0, 0 });
            // 
            // grpBuffer
            // 
            this.grpBuffer.Controls.Add(this.lblPreset);
            this.grpBuffer.Controls.Add(this.cboMetroPreset);
            this.grpBuffer.Controls.Add(this.lblPresetTip);
            this.grpBuffer.Controls.Add(this.lblMultiDist);
            this.grpBuffer.Controls.Add(this.txtMultiDist);
            this.grpBuffer.Controls.Add(this.lblMultiScore);
            this.grpBuffer.Controls.Add(this.txtMultiScore);
            this.grpBuffer.Controls.Add(this.lblSingleDist);
            this.grpBuffer.Controls.Add(this.txtSingleDist);
            this.grpBuffer.Controls.Add(this.lblSingleScore);
            this.grpBuffer.Controls.Add(this.txtSingleScore);
            this.grpBuffer.Controls.Add(this.lblCbd);
            this.grpBuffer.Controls.Add(this.nudCbdDist);
            this.grpBuffer.Controls.Add(this.nudCbdScore);
            this.grpBuffer.Controls.Add(this.lblTrafFac);
            this.grpBuffer.Controls.Add(this.nudTrafDist);
            this.grpBuffer.Controls.Add(this.nudTrafScore);
            this.grpBuffer.Location = new System.Drawing.Point(16, 196);
            this.grpBuffer.Name = "grpBuffer";
            this.grpBuffer.Size = new System.Drawing.Size(590, 168);
            this.grpBuffer.TabIndex = 2;
            this.grpBuffer.TabStop = false;
            this.grpBuffer.Text = "交通缓冲/得分规则（下次动力性分析生效）";
            // 
            // lblPreset
            // 
            this.lblPreset.AutoSize = true;
            this.lblPreset.Location = new System.Drawing.Point(16, 28);
            this.lblPreset.Name = "lblPreset";
            this.lblPreset.Size = new System.Drawing.Size(77, 12);
            this.lblPreset.TabIndex = 0;
            this.lblPreset.Text = "地铁赋分预设：";
            // 
            // cboMetroPreset
            // 
            this.cboMetroPreset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboMetroPreset.FormattingEnabled = true;
            this.cboMetroPreset.Location = new System.Drawing.Point(110, 24);
            this.cboMetroPreset.Name = "cboMetroPreset";
            this.cboMetroPreset.Size = new System.Drawing.Size(200, 20);
            this.cboMetroPreset.TabIndex = 1;
            this.cboMetroPreset.SelectedIndexChanged += new System.EventHandler(this.cboMetroPreset_SelectedIndexChanged);
            // 
            // lblPresetTip
            // 
            this.lblPresetTip.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblPresetTip.Location = new System.Drawing.Point(320, 22);
            this.lblPresetTip.Name = "lblPresetTip";
            this.lblPresetTip.Size = new System.Drawing.Size(250, 32);
            this.lblPresetTip.TabIndex = 2;
            this.lblPresetTip.Text = "B=推荐（地铁作加分项）。得分≤0 的环不参与分析。";
            // 
            // lblMultiDist
            // 
            this.lblMultiDist.AutoSize = true;
            this.lblMultiDist.Location = new System.Drawing.Point(16, 64);
            this.lblMultiDist.Name = "lblMultiDist";
            this.lblMultiDist.Size = new System.Drawing.Size(101, 12);
            this.lblMultiDist.TabIndex = 3;
            this.lblMultiDist.Text = "多线距离(m,逗号)：";
            // 
            // txtMultiDist
            // 
            this.txtMultiDist.Location = new System.Drawing.Point(130, 60);
            this.txtMultiDist.Name = "txtMultiDist";
            this.txtMultiDist.Size = new System.Drawing.Size(140, 21);
            this.txtMultiDist.TabIndex = 4;
            this.txtMultiDist.TextChanged += new System.EventHandler(this.BufferRule_TextChanged);
            // 
            // lblMultiScore
            // 
            this.lblMultiScore.AutoSize = true;
            this.lblMultiScore.Location = new System.Drawing.Point(290, 64);
            this.lblMultiScore.Name = "lblMultiScore";
            this.lblMultiScore.Size = new System.Drawing.Size(89, 12);
            this.lblMultiScore.TabIndex = 5;
            this.lblMultiScore.Text = "多线得分(逗号)：";
            // 
            // txtMultiScore
            // 
            this.txtMultiScore.Location = new System.Drawing.Point(390, 60);
            this.txtMultiScore.Name = "txtMultiScore";
            this.txtMultiScore.Size = new System.Drawing.Size(120, 21);
            this.txtMultiScore.TabIndex = 6;
            this.txtMultiScore.TextChanged += new System.EventHandler(this.BufferRule_TextChanged);
            // 
            // lblSingleDist
            // 
            this.lblSingleDist.AutoSize = true;
            this.lblSingleDist.Location = new System.Drawing.Point(16, 96);
            this.lblSingleDist.Name = "lblSingleDist";
            this.lblSingleDist.Size = new System.Drawing.Size(101, 12);
            this.lblSingleDist.TabIndex = 7;
            this.lblSingleDist.Text = "单线距离(m,逗号)：";
            // 
            // txtSingleDist
            // 
            this.txtSingleDist.Location = new System.Drawing.Point(130, 92);
            this.txtSingleDist.Name = "txtSingleDist";
            this.txtSingleDist.Size = new System.Drawing.Size(140, 21);
            this.txtSingleDist.TabIndex = 8;
            this.txtSingleDist.TextChanged += new System.EventHandler(this.BufferRule_TextChanged);
            // 
            // lblSingleScore
            // 
            this.lblSingleScore.AutoSize = true;
            this.lblSingleScore.Location = new System.Drawing.Point(290, 96);
            this.lblSingleScore.Name = "lblSingleScore";
            this.lblSingleScore.Size = new System.Drawing.Size(89, 12);
            this.lblSingleScore.TabIndex = 9;
            this.lblSingleScore.Text = "单线得分(逗号)：";
            // 
            // txtSingleScore
            // 
            this.txtSingleScore.Location = new System.Drawing.Point(390, 92);
            this.txtSingleScore.Name = "txtSingleScore";
            this.txtSingleScore.Size = new System.Drawing.Size(120, 21);
            this.txtSingleScore.TabIndex = 10;
            this.txtSingleScore.TextChanged += new System.EventHandler(this.BufferRule_TextChanged);
            // 
            // lblCbd
            // 
            this.lblCbd.AutoSize = true;
            this.lblCbd.Location = new System.Drawing.Point(16, 130);
            this.lblCbd.Name = "lblCbd";
            this.lblCbd.Size = new System.Drawing.Size(95, 12);
            this.lblCbd.TabIndex = 11;
            this.lblCbd.Text = "CBD 距离/得分：";
            // 
            // nudCbdDist
            // 
            this.nudCbdDist.Location = new System.Drawing.Point(130, 126);
            this.nudCbdDist.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            this.nudCbdDist.Name = "nudCbdDist";
            this.nudCbdDist.Size = new System.Drawing.Size(70, 21);
            this.nudCbdDist.TabIndex = 12;
            this.nudCbdDist.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            this.nudCbdDist.ValueChanged += new System.EventHandler(this.BufferRule_TextChanged);
            // 
            // nudCbdScore
            // 
            this.nudCbdScore.Location = new System.Drawing.Point(210, 126);
            this.nudCbdScore.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            this.nudCbdScore.Name = "nudCbdScore";
            this.nudCbdScore.Size = new System.Drawing.Size(50, 21);
            this.nudCbdScore.TabIndex = 13;
            this.nudCbdScore.Value = new decimal(new int[] { 3, 0, 0, 0 });
            this.nudCbdScore.ValueChanged += new System.EventHandler(this.BufferRule_TextChanged);
            // 
            // lblTrafFac
            // 
            this.lblTrafFac.AutoSize = true;
            this.lblTrafFac.Location = new System.Drawing.Point(290, 130);
            this.lblTrafFac.Name = "lblTrafFac";
            this.lblTrafFac.Size = new System.Drawing.Size(101, 12);
            this.lblTrafFac.TabIndex = 14;
            this.lblTrafFac.Text = "交通设施距/分：";
            // 
            // nudTrafDist
            // 
            this.nudTrafDist.Location = new System.Drawing.Point(400, 126);
            this.nudTrafDist.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            this.nudTrafDist.Name = "nudTrafDist";
            this.nudTrafDist.Size = new System.Drawing.Size(70, 21);
            this.nudTrafDist.TabIndex = 15;
            this.nudTrafDist.Value = new decimal(new int[] { 300, 0, 0, 0 });
            this.nudTrafDist.ValueChanged += new System.EventHandler(this.BufferRule_TextChanged);
            // 
            // nudTrafScore
            // 
            this.nudTrafScore.Location = new System.Drawing.Point(480, 126);
            this.nudTrafScore.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            this.nudTrafScore.Name = "nudTrafScore";
            this.nudTrafScore.Size = new System.Drawing.Size(50, 21);
            this.nudTrafScore.TabIndex = 16;
            this.nudTrafScore.Value = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudTrafScore.ValueChanged += new System.EventHandler(this.BufferRule_TextChanged);
            // 
            // grpSkin
            // 
            this.grpSkin.Controls.Add(this.cboSkin);
            this.grpSkin.Controls.Add(this.btnApplySkin);
            this.grpSkin.Location = new System.Drawing.Point(16, 376);
            this.grpSkin.Name = "grpSkin";
            this.grpSkin.Size = new System.Drawing.Size(590, 60);
            this.grpSkin.TabIndex = 3;
            this.grpSkin.TabStop = false;
            this.grpSkin.Text = "界面皮肤 (DevExpress 13.1)";
            // 
            // cboSkin
            // 
            this.cboSkin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSkin.FormattingEnabled = true;
            this.cboSkin.Location = new System.Drawing.Point(16, 24);
            this.cboSkin.Name = "cboSkin";
            this.cboSkin.Size = new System.Drawing.Size(220, 20);
            this.cboSkin.TabIndex = 0;
            // 
            // btnApplySkin
            // 
            this.btnApplySkin.Location = new System.Drawing.Point(250, 22);
            this.btnApplySkin.Name = "btnApplySkin";
            this.btnApplySkin.Size = new System.Drawing.Size(100, 25);
            this.btnApplySkin.TabIndex = 1;
            this.btnApplySkin.Text = "应用皮肤";
            this.btnApplySkin.UseVisualStyleBackColor = true;
            this.btnApplySkin.Click += new System.EventHandler(this.btnApplySkin_Click);
            // 
            // lblPlugins
            // 
            this.lblPlugins.AutoSize = true;
            this.lblPlugins.Location = new System.Drawing.Point(16, 450);
            this.lblPlugins.Name = "lblPlugins";
            this.lblPlugins.Size = new System.Drawing.Size(173, 12);
            this.lblPlugins.TabIndex = 4;
            this.lblPlugins.Text = "plugins.config（只读预览）：";
            // 
            // txtPlugins
            // 
            this.txtPlugins.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtPlugins.Location = new System.Drawing.Point(16, 470);
            this.txtPlugins.Multiline = true;
            this.txtPlugins.Name = "txtPlugins";
            this.txtPlugins.ReadOnly = true;
            this.txtPlugins.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPlugins.Size = new System.Drawing.Size(590, 70);
            this.txtPlugins.TabIndex = 5;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(420, 556);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 30);
            this.btnSave.TabIndex = 6;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(516, 556);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(86, 30);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // SystemConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 602);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtPlugins);
            this.Controls.Add(this.lblPlugins);
            this.Controls.Add(this.grpSkin);
            this.Controls.Add(this.grpBuffer);
            this.Controls.Add(this.grpCity);
            this.Controls.Add(this.grpOverlay);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SystemConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "权重与缓冲赋分配置";
            this.grpOverlay.ResumeLayout(false);
            this.grpOverlay.PerformLayout();
            this.grpCity.ResumeLayout(false);
            this.grpCity.PerformLayout();
            this.grpBuffer.ResumeLayout(false);
            this.grpBuffer.PerformLayout();
            this.grpSkin.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudMotivW)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFeasibW)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTraffic)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEnv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFac)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPol)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCbdDist)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCbdScore)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTrafDist)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTrafScore)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox grpOverlay;
        private System.Windows.Forms.Label lblMotivW;
        private System.Windows.Forms.NumericUpDown nudMotivW;
        private System.Windows.Forms.Label lblFeasibW;
        private System.Windows.Forms.NumericUpDown nudFeasibW;
        private System.Windows.Forms.GroupBox grpCity;
        private System.Windows.Forms.Label lblCity;
        private System.Windows.Forms.Label lblTraffic;
        private System.Windows.Forms.NumericUpDown nudTraffic;
        private System.Windows.Forms.Label lblEnv;
        private System.Windows.Forms.NumericUpDown nudEnv;
        private System.Windows.Forms.Label lblFac;
        private System.Windows.Forms.NumericUpDown nudFac;
        private System.Windows.Forms.Label lblPol;
        private System.Windows.Forms.NumericUpDown nudPol;
        private System.Windows.Forms.GroupBox grpBuffer;
        private System.Windows.Forms.Label lblPreset;
        private System.Windows.Forms.ComboBox cboMetroPreset;
        private System.Windows.Forms.Label lblPresetTip;
        private System.Windows.Forms.Label lblMultiDist;
        private System.Windows.Forms.TextBox txtMultiDist;
        private System.Windows.Forms.Label lblMultiScore;
        private System.Windows.Forms.TextBox txtMultiScore;
        private System.Windows.Forms.Label lblSingleDist;
        private System.Windows.Forms.TextBox txtSingleDist;
        private System.Windows.Forms.Label lblSingleScore;
        private System.Windows.Forms.TextBox txtSingleScore;
        private System.Windows.Forms.Label lblCbd;
        private System.Windows.Forms.NumericUpDown nudCbdDist;
        private System.Windows.Forms.NumericUpDown nudCbdScore;
        private System.Windows.Forms.Label lblTrafFac;
        private System.Windows.Forms.NumericUpDown nudTrafDist;
        private System.Windows.Forms.NumericUpDown nudTrafScore;
        private System.Windows.Forms.GroupBox grpSkin;
        private System.Windows.Forms.ComboBox cboSkin;
        private System.Windows.Forms.Button btnApplySkin;
        private System.Windows.Forms.Label lblPlugins;
        private System.Windows.Forms.TextBox txtPlugins;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClose;
    }
}

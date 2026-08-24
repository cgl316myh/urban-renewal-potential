namespace UrbanRenewal.Plugins.DataManage
{
    partial class GlobalSettingsForm
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
            this.lblOutGdb = new System.Windows.Forms.Label();
            this.txtOutGdb = new System.Windows.Forms.TextBox();
            this.btnBrowseOut = new System.Windows.Forms.Button();
            this.btnSuggestOut = new System.Windows.Forms.Button();
            this.lblCity = new System.Windows.Forms.Label();
            this.cboCity = new System.Windows.Forms.ComboBox();
            this.btnDetect = new System.Windows.Forms.Button();
            this.btnDraft = new System.Windows.Forms.Button();
            this.btnOpenConfig = new System.Windows.Forms.Button();
            this.lblInput = new System.Windows.Forms.Label();
            this.txtInputGdb = new System.Windows.Forms.TextBox();
            this.btnBrowseInput = new System.Windows.Forms.Button();
            this.lblSpatialRef = new System.Windows.Forms.Label();
            this.txtSpatialRefName = new System.Windows.Forms.TextBox();
            this.btnSpatialRefShp = new System.Windows.Forms.Button();
            this.btnSpatialRefGdb = new System.Windows.Forms.Button();
            this.btnSpatialRefClear = new System.Windows.Forms.Button();
            this.lblSpatialRefSrc = new System.Windows.Forms.Label();
            this.lblCell = new System.Windows.Forms.Label();
            this.nudCellSize = new System.Windows.Forms.NumericUpDown();
            this.chkMaskResultToStudyArea = new System.Windows.Forms.CheckBox();
            this.lblHint = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudCellSize)).BeginInit();
            this.SuspendLayout();
            // 
            // lblInput
            // 
            this.lblInput.AutoSize = true;
            this.lblInput.Location = new System.Drawing.Point(18, 20);
            this.lblInput.Name = "lblInput";
            this.lblInput.Size = new System.Drawing.Size(77, 12);
            this.lblInput.TabIndex = 0;
            this.lblInput.Text = "输入 GDB：";
            // 
            // txtInputGdb
            // 
            this.txtInputGdb.Location = new System.Drawing.Point(101, 16);
            this.txtInputGdb.Name = "txtInputGdb";
            this.txtInputGdb.Size = new System.Drawing.Size(420, 21);
            this.txtInputGdb.TabIndex = 1;
            this.txtInputGdb.ShortcutsEnabled = true;
            // 
            // btnBrowseInput
            // 
            this.btnBrowseInput.Location = new System.Drawing.Point(528, 14);
            this.btnBrowseInput.Name = "btnBrowseInput";
            this.btnBrowseInput.Size = new System.Drawing.Size(75, 25);
            this.btnBrowseInput.TabIndex = 2;
            this.btnBrowseInput.Text = "浏览...";
            this.btnBrowseInput.UseVisualStyleBackColor = true;
            this.btnBrowseInput.Click += new System.EventHandler(this.btnBrowseInput_Click);
            // 
            // lblOutGdb
            // 
            this.lblOutGdb.AutoSize = true;
            this.lblOutGdb.Location = new System.Drawing.Point(18, 55);
            this.lblOutGdb.Name = "lblOutGdb";
            this.lblOutGdb.Size = new System.Drawing.Size(77, 12);
            this.lblOutGdb.TabIndex = 3;
            this.lblOutGdb.Text = "输出 GDB：";
            // 
            // txtOutGdb
            // 
            this.txtOutGdb.Location = new System.Drawing.Point(101, 51);
            this.txtOutGdb.Name = "txtOutGdb";
            this.txtOutGdb.Size = new System.Drawing.Size(340, 21);
            this.txtOutGdb.TabIndex = 4;
            this.txtOutGdb.ShortcutsEnabled = true;
            // 
            // btnBrowseOut
            // 
            this.btnBrowseOut.Location = new System.Drawing.Point(447, 49);
            this.btnBrowseOut.Name = "btnBrowseOut";
            this.btnBrowseOut.Size = new System.Drawing.Size(75, 25);
            this.btnBrowseOut.TabIndex = 5;
            this.btnBrowseOut.Text = "浏览...";
            this.btnBrowseOut.UseVisualStyleBackColor = true;
            this.btnBrowseOut.Click += new System.EventHandler(this.btnBrowseOut_Click);
            // 
            // btnSuggestOut
            // 
            this.btnSuggestOut.Location = new System.Drawing.Point(528, 49);
            this.btnSuggestOut.Name = "btnSuggestOut";
            this.btnSuggestOut.Size = new System.Drawing.Size(75, 25);
            this.btnSuggestOut.TabIndex = 6;
            this.btnSuggestOut.Text = "默认";
            this.btnSuggestOut.UseVisualStyleBackColor = true;
            this.btnSuggestOut.Click += new System.EventHandler(this.btnSuggestOut_Click);
            // 
            // lblSpatialRef
            // 
            this.lblSpatialRef.AutoSize = true;
            this.lblSpatialRef.Location = new System.Drawing.Point(18, 90);
            this.lblSpatialRef.Name = "lblSpatialRef";
            this.lblSpatialRef.Size = new System.Drawing.Size(77, 12);
            this.lblSpatialRef.TabIndex = 7;
            this.lblSpatialRef.Text = "基准坐标系：";
            // 
            // txtSpatialRefName
            // 
            this.txtSpatialRefName.Location = new System.Drawing.Point(101, 86);
            this.txtSpatialRefName.Name = "txtSpatialRefName";
            this.txtSpatialRefName.ReadOnly = true;
            this.txtSpatialRefName.Size = new System.Drawing.Size(250, 21);
            this.txtSpatialRefName.TabIndex = 8;
            // 
            // btnSpatialRefShp
            // 
            this.btnSpatialRefShp.Location = new System.Drawing.Point(357, 84);
            this.btnSpatialRefShp.Name = "btnSpatialRefShp";
            this.btnSpatialRefShp.Size = new System.Drawing.Size(80, 25);
            this.btnSpatialRefShp.TabIndex = 9;
            this.btnSpatialRefShp.Text = "从Shp...";
            this.btnSpatialRefShp.UseVisualStyleBackColor = true;
            this.btnSpatialRefShp.Click += new System.EventHandler(this.btnSpatialRefShp_Click);
            // 
            // btnSpatialRefGdb
            // 
            this.btnSpatialRefGdb.Location = new System.Drawing.Point(443, 84);
            this.btnSpatialRefGdb.Name = "btnSpatialRefGdb";
            this.btnSpatialRefGdb.Size = new System.Drawing.Size(90, 25);
            this.btnSpatialRefGdb.TabIndex = 10;
            this.btnSpatialRefGdb.Text = "从GDB图层";
            this.btnSpatialRefGdb.UseVisualStyleBackColor = true;
            this.btnSpatialRefGdb.Click += new System.EventHandler(this.btnSpatialRefGdb_Click);
            // 
            // btnSpatialRefClear
            // 
            this.btnSpatialRefClear.Location = new System.Drawing.Point(539, 84);
            this.btnSpatialRefClear.Name = "btnSpatialRefClear";
            this.btnSpatialRefClear.Size = new System.Drawing.Size(64, 25);
            this.btnSpatialRefClear.TabIndex = 11;
            this.btnSpatialRefClear.Text = "清除";
            this.btnSpatialRefClear.UseVisualStyleBackColor = true;
            this.btnSpatialRefClear.Click += new System.EventHandler(this.btnSpatialRefClear_Click);
            // 
            // lblSpatialRefSrc
            // 
            this.lblSpatialRefSrc.Location = new System.Drawing.Point(101, 112);
            this.lblSpatialRefSrc.Name = "lblSpatialRefSrc";
            this.lblSpatialRefSrc.Size = new System.Drawing.Size(502, 20);
            this.lblSpatialRefSrc.TabIndex = 12;
            this.lblSpatialRefSrc.Text = "来源：（未配置，完整性检查将自动推断）";
            // 
            // lblCity
            // 
            this.lblCity.AutoSize = true;
            this.lblCity.Location = new System.Drawing.Point(18, 145);
            this.lblCity.Name = "lblCity";
            this.lblCity.Size = new System.Drawing.Size(77, 12);
            this.lblCity.TabIndex = 13;
            this.lblCity.Text = "城市配置：";
            // 
            // cboCity
            // 
            this.cboCity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCity.FormattingEnabled = true;
            this.cboCity.Location = new System.Drawing.Point(101, 141);
            this.cboCity.Name = "cboCity";
            this.cboCity.Size = new System.Drawing.Size(220, 20);
            this.cboCity.TabIndex = 14;
            // 
            // btnDetect
            // 
            this.btnDetect.Location = new System.Drawing.Point(330, 139);
            this.btnDetect.Name = "btnDetect";
            this.btnDetect.Size = new System.Drawing.Size(90, 25);
            this.btnDetect.TabIndex = 15;
            this.btnDetect.Text = "检测匹配";
            this.btnDetect.UseVisualStyleBackColor = true;
            this.btnDetect.Click += new System.EventHandler(this.btnDetect_Click);
            // 
            // btnDraft
            // 
            this.btnDraft.Location = new System.Drawing.Point(426, 139);
            this.btnDraft.Name = "btnDraft";
            this.btnDraft.Size = new System.Drawing.Size(100, 25);
            this.btnDraft.TabIndex = 16;
            this.btnDraft.Text = "从GDB生成";
            this.btnDraft.UseVisualStyleBackColor = true;
            this.btnDraft.Click += new System.EventHandler(this.btnDraft_Click);
            // 
            // btnOpenConfig
            // 
            this.btnOpenConfig.Location = new System.Drawing.Point(532, 139);
            this.btnOpenConfig.Name = "btnOpenConfig";
            this.btnOpenConfig.Size = new System.Drawing.Size(75, 25);
            this.btnOpenConfig.TabIndex = 17;
            this.btnOpenConfig.Text = "配置目录";
            this.btnOpenConfig.UseVisualStyleBackColor = true;
            this.btnOpenConfig.Click += new System.EventHandler(this.btnOpenConfig_Click);
            // 
            // lblCell
            // 
            this.lblCell.AutoSize = true;
            this.lblCell.Location = new System.Drawing.Point(18, 180);
            this.lblCell.Name = "lblCell";
            this.lblCell.Size = new System.Drawing.Size(101, 12);
            this.lblCell.TabIndex = 18;
            this.lblCell.Text = "像元大小(米)：";
            // 
            // nudCellSize
            // 
            this.nudCellSize.Location = new System.Drawing.Point(125, 176);
            this.nudCellSize.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            this.nudCellSize.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            this.nudCellSize.Name = "nudCellSize";
            this.nudCellSize.Size = new System.Drawing.Size(80, 21);
            this.nudCellSize.TabIndex = 19;
            this.nudCellSize.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // chkMaskResultToStudyArea
            // 
            this.chkMaskResultToStudyArea.AutoSize = true;
            this.chkMaskResultToStudyArea.Checked = true;
            this.chkMaskResultToStudyArea.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkMaskResultToStudyArea.Location = new System.Drawing.Point(125, 202);
            this.chkMaskResultToStudyArea.Name = "chkMaskResultToStudyArea";
            this.chkMaskResultToStudyArea.Size = new System.Drawing.Size(468, 16);
            this.chkMaskResultToStudyArea.TabIndex = 20;
            this.chkMaskResultToStudyArea.Text = "分析成果按中心城区掩膜（动力性/可行度/叠置最终栅格）";
            this.chkMaskResultToStudyArea.UseVisualStyleBackColor = true;
            // 
            // lblHint
            // 
            this.lblHint.Location = new System.Drawing.Point(18, 228);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(585, 56);
            this.lblHint.TabIndex = 21;
            this.lblHint.Text = "像元大小供动力性/可行度/叠置等潜力分析共用。勾选成果掩膜时，mot_score、fea_score、pot_score、pot_level 写出前按 StudyArea 裁切；需 GDB 中存在分析范围图层。基准坐标系可从 Shapefile 或输入 GDB 图层读取。";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(426, 296);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 30);
            this.btnSave.TabIndex = 22;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(522, 296);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(86, 30);
            this.btnCancel.TabIndex = 23;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // GlobalSettingsForm
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(624, 344);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.chkMaskResultToStudyArea);
            this.Controls.Add(this.nudCellSize);
            this.Controls.Add(this.lblCell);
            this.Controls.Add(this.btnOpenConfig);
            this.Controls.Add(this.btnDraft);
            this.Controls.Add(this.btnDetect);
            this.Controls.Add(this.cboCity);
            this.Controls.Add(this.lblCity);
            this.Controls.Add(this.lblSpatialRefSrc);
            this.Controls.Add(this.btnSpatialRefClear);
            this.Controls.Add(this.btnSpatialRefGdb);
            this.Controls.Add(this.btnSpatialRefShp);
            this.Controls.Add(this.txtSpatialRefName);
            this.Controls.Add(this.lblSpatialRef);
            this.Controls.Add(this.btnSuggestOut);
            this.Controls.Add(this.btnBrowseOut);
            this.Controls.Add(this.txtOutGdb);
            this.Controls.Add(this.lblOutGdb);
            this.Controls.Add(this.btnBrowseInput);
            this.Controls.Add(this.txtInputGdb);
            this.Controls.Add(this.lblInput);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GlobalSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "全局设置";
            ((System.ComponentModel.ISupportInitialize)(this.nudCellSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblOutGdb;
        private System.Windows.Forms.TextBox txtOutGdb;
        private System.Windows.Forms.Button btnBrowseOut;
        private System.Windows.Forms.Button btnSuggestOut;
        private System.Windows.Forms.Label lblCity;
        private System.Windows.Forms.ComboBox cboCity;
        private System.Windows.Forms.Button btnDetect;
        private System.Windows.Forms.Button btnDraft;
        private System.Windows.Forms.Button btnOpenConfig;
        private System.Windows.Forms.Label lblInput;
        private System.Windows.Forms.TextBox txtInputGdb;
        private System.Windows.Forms.Button btnBrowseInput;
        private System.Windows.Forms.Label lblSpatialRef;
        private System.Windows.Forms.TextBox txtSpatialRefName;
        private System.Windows.Forms.Button btnSpatialRefShp;
        private System.Windows.Forms.Button btnSpatialRefGdb;
        private System.Windows.Forms.Button btnSpatialRefClear;
        private System.Windows.Forms.Label lblSpatialRefSrc;
        private System.Windows.Forms.Label lblCell;
        private System.Windows.Forms.NumericUpDown nudCellSize;
        private System.Windows.Forms.CheckBox chkMaskResultToStudyArea;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}

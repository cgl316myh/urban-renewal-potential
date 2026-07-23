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
            this.lblHint = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
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
            this.txtInputGdb.ReadOnly = true;
            this.txtInputGdb.Size = new System.Drawing.Size(500, 21);
            this.txtInputGdb.TabIndex = 1;
            // 
            // lblOutGdb
            // 
            this.lblOutGdb.AutoSize = true;
            this.lblOutGdb.Location = new System.Drawing.Point(18, 55);
            this.lblOutGdb.Name = "lblOutGdb";
            this.lblOutGdb.Size = new System.Drawing.Size(77, 12);
            this.lblOutGdb.TabIndex = 2;
            this.lblOutGdb.Text = "输出 GDB：";
            // 
            // txtOutGdb
            // 
            this.txtOutGdb.Location = new System.Drawing.Point(101, 51);
            this.txtOutGdb.Name = "txtOutGdb";
            this.txtOutGdb.Size = new System.Drawing.Size(340, 21);
            this.txtOutGdb.TabIndex = 3;
            // 
            // btnBrowseOut
            // 
            this.btnBrowseOut.Location = new System.Drawing.Point(447, 49);
            this.btnBrowseOut.Name = "btnBrowseOut";
            this.btnBrowseOut.Size = new System.Drawing.Size(75, 25);
            this.btnBrowseOut.TabIndex = 4;
            this.btnBrowseOut.Text = "浏览...";
            this.btnBrowseOut.UseVisualStyleBackColor = true;
            this.btnBrowseOut.Click += new System.EventHandler(this.btnBrowseOut_Click);
            // 
            // btnSuggestOut
            // 
            this.btnSuggestOut.Location = new System.Drawing.Point(528, 49);
            this.btnSuggestOut.Name = "btnSuggestOut";
            this.btnSuggestOut.Size = new System.Drawing.Size(75, 25);
            this.btnSuggestOut.TabIndex = 5;
            this.btnSuggestOut.Text = "默认";
            this.btnSuggestOut.UseVisualStyleBackColor = true;
            this.btnSuggestOut.Click += new System.EventHandler(this.btnSuggestOut_Click);
            // 
            // lblCity
            // 
            this.lblCity.AutoSize = true;
            this.lblCity.Location = new System.Drawing.Point(18, 92);
            this.lblCity.Name = "lblCity";
            this.lblCity.Size = new System.Drawing.Size(77, 12);
            this.lblCity.TabIndex = 6;
            this.lblCity.Text = "城市配置：";
            // 
            // cboCity
            // 
            this.cboCity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCity.FormattingEnabled = true;
            this.cboCity.Location = new System.Drawing.Point(101, 88);
            this.cboCity.Name = "cboCity";
            this.cboCity.Size = new System.Drawing.Size(220, 20);
            this.cboCity.TabIndex = 7;
            // 
            // btnDetect
            // 
            this.btnDetect.Location = new System.Drawing.Point(330, 86);
            this.btnDetect.Name = "btnDetect";
            this.btnDetect.Size = new System.Drawing.Size(90, 25);
            this.btnDetect.TabIndex = 8;
            this.btnDetect.Text = "检测匹配";
            this.btnDetect.UseVisualStyleBackColor = true;
            this.btnDetect.Click += new System.EventHandler(this.btnDetect_Click);
            // 
            // btnDraft
            // 
            this.btnDraft.Location = new System.Drawing.Point(426, 86);
            this.btnDraft.Name = "btnDraft";
            this.btnDraft.Size = new System.Drawing.Size(100, 25);
            this.btnDraft.TabIndex = 9;
            this.btnDraft.Text = "从GDB生成";
            this.btnDraft.UseVisualStyleBackColor = true;
            this.btnDraft.Click += new System.EventHandler(this.btnDraft_Click);
            // 
            // btnOpenConfig
            // 
            this.btnOpenConfig.Location = new System.Drawing.Point(532, 86);
            this.btnOpenConfig.Name = "btnOpenConfig";
            this.btnOpenConfig.Size = new System.Drawing.Size(75, 25);
            this.btnOpenConfig.TabIndex = 10;
            this.btnOpenConfig.Text = "配置目录";
            this.btnOpenConfig.UseVisualStyleBackColor = true;
            this.btnOpenConfig.Click += new System.EventHandler(this.btnOpenConfig_Click);
            // 
            // lblHint
            // 
            this.lblHint.Location = new System.Drawing.Point(18, 130);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(585, 55);
            this.lblHint.TabIndex = 11;
            this.lblHint.Text = "此处设置为全局有效：动力性及后续各分析模块的中间数据与结果均写入「输出 GDB」。城市配置决定图层角色映射。保存后立即生效。";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(426, 200);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 30);
            this.btnSave.TabIndex = 12;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(522, 200);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(86, 30);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // GlobalSettingsForm
            // 
            this.AcceptButton = this.btnSave;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(624, 250);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.btnOpenConfig);
            this.Controls.Add(this.btnDraft);
            this.Controls.Add(this.btnDetect);
            this.Controls.Add(this.cboCity);
            this.Controls.Add(this.lblCity);
            this.Controls.Add(this.btnSuggestOut);
            this.Controls.Add(this.btnBrowseOut);
            this.Controls.Add(this.txtOutGdb);
            this.Controls.Add(this.lblOutGdb);
            this.Controls.Add(this.txtInputGdb);
            this.Controls.Add(this.lblInput);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GlobalSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "全局设置";
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
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}

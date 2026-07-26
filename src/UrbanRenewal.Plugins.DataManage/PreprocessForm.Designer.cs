namespace UrbanRenewal.Plugins.DataManage
{
    partial class PreprocessForm
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
            this.lblInput = new System.Windows.Forms.Label();
            this.txtInputGdb = new System.Windows.Forms.TextBox();
            this.lblOut = new System.Windows.Forms.Label();
            this.txtOutGdb = new System.Windows.Forms.TextBox();
            this.lblClip = new System.Windows.Forms.Label();
            this.cboClip = new System.Windows.Forms.ComboBox();
            this.chkProject = new System.Windows.Forms.CheckBox();
            this.chkClip = new System.Windows.Forms.CheckBox();
            this.lstLayers = new System.Windows.Forms.CheckedListBox();
            this.btnSelectMismatch = new System.Windows.Forms.Button();
            this.btnSelectAll = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.lblHint = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblInput
            // 
            this.lblInput.AutoSize = true;
            this.lblInput.Location = new System.Drawing.Point(16, 18);
            this.lblInput.Name = "lblInput";
            this.lblInput.Size = new System.Drawing.Size(65, 12);
            this.lblInput.TabIndex = 0;
            this.lblInput.Text = "输入 GDB：";
            // 
            // txtInputGdb
            // 
            this.txtInputGdb.Location = new System.Drawing.Point(90, 14);
            this.txtInputGdb.Name = "txtInputGdb";
            this.txtInputGdb.ReadOnly = true;
            this.txtInputGdb.Size = new System.Drawing.Size(520, 21);
            this.txtInputGdb.TabIndex = 1;
            // 
            // lblOut
            // 
            this.lblOut.AutoSize = true;
            this.lblOut.Location = new System.Drawing.Point(16, 48);
            this.lblOut.Name = "lblOut";
            this.lblOut.Size = new System.Drawing.Size(65, 12);
            this.lblOut.TabIndex = 2;
            this.lblOut.Text = "暂存 GDB：";
            // 
            // txtOutGdb
            // 
            this.txtOutGdb.Location = new System.Drawing.Point(90, 44);
            this.txtOutGdb.Name = "txtOutGdb";
            this.txtOutGdb.Size = new System.Drawing.Size(520, 21);
            this.txtOutGdb.TabIndex = 3;
            // 
            // lblClip
            // 
            this.lblClip.AutoSize = true;
            this.lblClip.Location = new System.Drawing.Point(16, 80);
            this.lblClip.Name = "lblClip";
            this.lblClip.Size = new System.Drawing.Size(65, 12);
            this.lblClip.TabIndex = 4;
            this.lblClip.Text = "裁剪范围：";
            // 
            // cboClip
            // 
            this.cboClip.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboClip.FormattingEnabled = true;
            this.cboClip.Location = new System.Drawing.Point(90, 76);
            this.cboClip.Name = "cboClip";
            this.cboClip.Size = new System.Drawing.Size(320, 20);
            this.cboClip.TabIndex = 5;
            // 
            // chkProject
            // 
            this.chkProject.AutoSize = true;
            this.chkProject.Checked = true;
            this.chkProject.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkProject.Location = new System.Drawing.Point(430, 78);
            this.chkProject.Name = "chkProject";
            this.chkProject.Size = new System.Drawing.Size(72, 16);
            this.chkProject.TabIndex = 6;
            this.chkProject.Text = "统一投影";
            this.chkProject.UseVisualStyleBackColor = true;
            // 
            // chkClip
            // 
            this.chkClip.AutoSize = true;
            this.chkClip.Checked = true;
            this.chkClip.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkClip.Location = new System.Drawing.Point(510, 78);
            this.chkClip.Name = "chkClip";
            this.chkClip.Size = new System.Drawing.Size(96, 16);
            this.chkClip.TabIndex = 7;
            this.chkClip.Text = "建成区裁剪";
            this.chkClip.UseVisualStyleBackColor = true;
            // 
            // lstLayers
            // 
            this.lstLayers.CheckOnClick = true;
            this.lstLayers.FormattingEnabled = true;
            this.lstLayers.Location = new System.Drawing.Point(18, 130);
            this.lstLayers.Name = "lstLayers";
            this.lstLayers.Size = new System.Drawing.Size(592, 292);
            this.lstLayers.TabIndex = 8;
            // 
            // btnSelectMismatch
            // 
            this.btnSelectMismatch.Location = new System.Drawing.Point(18, 100);
            this.btnSelectMismatch.Name = "btnSelectMismatch";
            this.btnSelectMismatch.Size = new System.Drawing.Size(100, 25);
            this.btnSelectMismatch.TabIndex = 9;
            this.btnSelectMismatch.Text = "仅不一致";
            this.btnSelectMismatch.UseVisualStyleBackColor = true;
            this.btnSelectMismatch.Click += new System.EventHandler(this.btnSelectMismatch_Click);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Location = new System.Drawing.Point(124, 100);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(75, 25);
            this.btnSelectAll.TabIndex = 10;
            this.btnSelectAll.Text = "全选";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(205, 100);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 25);
            this.btnClear.TabIndex = 11;
            this.btnClear.Text = "清空";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // lblHint
            // 
            this.lblHint.Location = new System.Drawing.Point(286, 100);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(324, 28);
            this.lblHint.TabIndex = 12;
            this.lblHint.Text = "勾选不正确图层 → 投影/裁剪后覆盖写回输入GDB";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(18, 432);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(29, 12);
            this.lblStatus.TabIndex = 13;
            this.lblStatus.Text = "就绪";
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(454, 426);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(75, 28);
            this.btnRun.TabIndex = 14;
            this.btnRun.Text = "开始";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(535, 426);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 28);
            this.btnClose.TabIndex = 15;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // PreprocessForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(628, 466);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnSelectAll);
            this.Controls.Add(this.btnSelectMismatch);
            this.Controls.Add(this.lstLayers);
            this.Controls.Add(this.chkClip);
            this.Controls.Add(this.chkProject);
            this.Controls.Add(this.cboClip);
            this.Controls.Add(this.lblClip);
            this.Controls.Add(this.txtOutGdb);
            this.Controls.Add(this.lblOut);
            this.Controls.Add(this.txtInputGdb);
            this.Controls.Add(this.lblInput);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PreprocessForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "投影/裁剪预处理";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblInput;
        private System.Windows.Forms.TextBox txtInputGdb;
        private System.Windows.Forms.Label lblOut;
        private System.Windows.Forms.TextBox txtOutGdb;
        private System.Windows.Forms.Label lblClip;
        private System.Windows.Forms.ComboBox cboClip;
        private System.Windows.Forms.CheckBox chkProject;
        private System.Windows.Forms.CheckBox chkClip;
        private System.Windows.Forms.CheckedListBox lstLayers;
        private System.Windows.Forms.Button btnSelectMismatch;
        private System.Windows.Forms.Button btnSelectAll;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnClose;
    }
}

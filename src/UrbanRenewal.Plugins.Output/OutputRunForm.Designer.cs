namespace UrbanRenewal.Plugins.Output
{
    partial class OutputRunForm
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
            this.lblFolder = new System.Windows.Forms.Label();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.chkTiff = new System.Windows.Forms.CheckBox();
            this.chkShp = new System.Windows.Forms.CheckBox();
            this.chkCsv = new System.Windows.Forms.CheckBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblFolder
            // 
            this.lblFolder.AutoSize = true;
            this.lblFolder.Location = new System.Drawing.Point(18, 20);
            this.lblFolder.Name = "lblFolder";
            this.lblFolder.Size = new System.Drawing.Size(65, 12);
            this.lblFolder.TabIndex = 0;
            this.lblFolder.Text = "导出目录：";
            // 
            // txtFolder
            // 
            this.txtFolder.Location = new System.Drawing.Point(90, 16);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(420, 21);
            this.txtFolder.TabIndex = 1;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(520, 14);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 25);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "浏览...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // chkTiff
            // 
            this.chkTiff.AutoSize = true;
            this.chkTiff.Checked = true;
            this.chkTiff.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTiff.Location = new System.Drawing.Point(20, 50);
            this.chkTiff.Name = "chkTiff";
            this.chkTiff.Size = new System.Drawing.Size(132, 16);
            this.chkTiff.TabIndex = 3;
            this.chkTiff.Text = "导出潜力/等级 TIFF";
            this.chkTiff.UseVisualStyleBackColor = true;
            // 
            // chkShp
            // 
            this.chkShp.AutoSize = true;
            this.chkShp.Checked = true;
            this.chkShp.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShp.Location = new System.Drawing.Point(200, 50);
            this.chkShp.Name = "chkShp";
            this.chkShp.Size = new System.Drawing.Size(96, 16);
            this.chkShp.TabIndex = 4;
            this.chkShp.Text = "导出宗地 SHP";
            this.chkShp.UseVisualStyleBackColor = true;
            // 
            // chkCsv
            // 
            this.chkCsv.AutoSize = true;
            this.chkCsv.Checked = true;
            this.chkCsv.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCsv.Location = new System.Drawing.Point(340, 50);
            this.chkCsv.Name = "chkCsv";
            this.chkCsv.Size = new System.Drawing.Size(156, 16);
            this.chkCsv.TabIndex = 5;
            this.chkCsv.Text = "导出 CSV 报表(Excel可开)";
            this.chkCsv.UseVisualStyleBackColor = true;
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(420, 90);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(90, 30);
            this.btnRun.TabIndex = 6;
            this.btnRun.Text = "开始导出";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(516, 90);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(86, 30);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // OutputRunForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 140);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.chkCsv);
            this.Controls.Add(this.chkShp);
            this.Controls.Add(this.chkTiff);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtFolder);
            this.Controls.Add(this.lblFolder);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OutputRunForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "导出评价成果";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.CheckBox chkTiff;
        private System.Windows.Forms.CheckBox chkShp;
        private System.Windows.Forms.CheckBox chkCsv;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnClose;
    }
}

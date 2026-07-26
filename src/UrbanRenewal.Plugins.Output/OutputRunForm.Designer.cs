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
            this.lblOutInfo = new System.Windows.Forms.Label();
            this.lblFolder = new System.Windows.Forms.Label();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.chkTiff = new System.Windows.Forms.CheckBox();
            this.chkShp = new System.Windows.Forms.CheckBox();
            this.chkCsv = new System.Windows.Forms.CheckBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblHint = new System.Windows.Forms.Label();
            this.SuspendLayout();
            this.lblOutInfo.Location = new System.Drawing.Point(18, 16);
            this.lblOutInfo.Size = new System.Drawing.Size(580, 28);
            this.lblOutInfo.Text = "输出 GDB：";
            this.lblFolder.AutoSize = true;
            this.lblFolder.Location = new System.Drawing.Point(18, 60);
            this.lblFolder.Text = "导出目录：";
            this.txtFolder.Location = new System.Drawing.Point(90, 56);
            this.txtFolder.Size = new System.Drawing.Size(420, 21);
            this.btnBrowse.Location = new System.Drawing.Point(520, 54);
            this.btnBrowse.Size = new System.Drawing.Size(75, 25);
            this.btnBrowse.Text = "浏览...";
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            this.chkTiff.AutoSize = true;
            this.chkTiff.Checked = true;
            this.chkTiff.Location = new System.Drawing.Point(20, 100);
            this.chkTiff.Text = "导出潜力/等级 TIFF";
            this.chkShp.AutoSize = true;
            this.chkShp.Checked = true;
            this.chkShp.Location = new System.Drawing.Point(200, 100);
            this.chkShp.Text = "导出宗地 SHP";
            this.chkCsv.AutoSize = true;
            this.chkCsv.Checked = true;
            this.chkCsv.Location = new System.Drawing.Point(340, 100);
            this.chkCsv.Text = "导出 CSV 报表(Excel可开)";
            this.btnRun.Location = new System.Drawing.Point(420, 150);
            this.btnRun.Size = new System.Drawing.Size(90, 30);
            this.btnRun.Text = "开始导出";
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            this.btnClose.Location = new System.Drawing.Point(516, 150);
            this.btnClose.Size = new System.Drawing.Size(86, 30);
            this.btnClose.Text = "关闭";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(20, 158);
            this.lblStatus.Text = "就绪";
            this.lblHint.Location = new System.Drawing.Point(18, 200);
            this.lblHint.Size = new System.Drawing.Size(584, 48);
            this.lblHint.Text = "专题图：先在地图加载 pot_score（全域）或 parcel_pot（宗地），再用 Ribbon「导出地图PDF/TIFF」。数据导出写入 Export 目录。";
            this.ClientSize = new System.Drawing.Size(624, 268);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.chkCsv);
            this.Controls.Add(this.chkShp);
            this.Controls.Add(this.chkTiff);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtFolder);
            this.Controls.Add(this.lblFolder);
            this.Controls.Add(this.lblOutInfo);
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

        private System.Windows.Forms.Label lblOutInfo;
        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.CheckBox chkTiff;
        private System.Windows.Forms.CheckBox chkShp;
        private System.Windows.Forms.CheckBox chkCsv;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblHint;
    }
}

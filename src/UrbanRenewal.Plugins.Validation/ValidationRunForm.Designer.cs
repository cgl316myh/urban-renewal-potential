namespace UrbanRenewal.Plugins.Validation
{
    partial class ValidationRunForm
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
            this.lblCityInfo = new System.Windows.Forms.Label();
            this.btnOpenGlobal = new System.Windows.Forms.Button();
            this.lblHigh = new System.Windows.Forms.Label();
            this.nudHighThr = new System.Windows.Forms.NumericUpDown();
            this.lblPass = new System.Windows.Forms.Label();
            this.nudPassRatio = new System.Windows.Forms.NumericUpDown();
            this.lblComment = new System.Windows.Forms.Label();
            this.txtComment = new System.Windows.Forms.TextBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblHint = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudHighThr)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPassRatio)).BeginInit();
            this.SuspendLayout();
            this.lblOutInfo.Location = new System.Drawing.Point(18, 16);
            this.lblOutInfo.Size = new System.Drawing.Size(480, 28);
            this.lblOutInfo.Text = "输出 GDB：";
            this.lblCityInfo.Location = new System.Drawing.Point(18, 48);
            this.lblCityInfo.Size = new System.Drawing.Size(480, 20);
            this.lblCityInfo.Text = "城市配置：";
            this.btnOpenGlobal.Location = new System.Drawing.Point(510, 16);
            this.btnOpenGlobal.Size = new System.Drawing.Size(95, 40);
            this.btnOpenGlobal.Text = "全局设置";
            this.btnOpenGlobal.Click += new System.EventHandler(this.btnOpenGlobal_Click);
            this.lblHigh.AutoSize = true;
            this.lblHigh.Location = new System.Drawing.Point(18, 90);
            this.lblHigh.Text = "高等级得分阈值：";
            this.nudHighThr.Location = new System.Drawing.Point(140, 86);
            this.nudHighThr.Maximum = 100;
            this.nudHighThr.Value = 60;
            this.lblPass.AutoSize = true;
            this.lblPass.Location = new System.Drawing.Point(250, 90);
            this.lblPass.Text = "通过占比%：";
            this.nudPassRatio.Location = new System.Drawing.Point(340, 86);
            this.nudPassRatio.Maximum = 100;
            this.nudPassRatio.Value = 60;
            this.lblComment.AutoSize = true;
            this.lblComment.Location = new System.Drawing.Point(18, 124);
            this.lblComment.Text = "审核意见：";
            this.txtComment.Location = new System.Drawing.Point(20, 144);
            this.txtComment.Multiline = true;
            this.txtComment.Size = new System.Drawing.Size(582, 60);
            this.btnRun.Location = new System.Drawing.Point(420, 220);
            this.btnRun.Size = new System.Drawing.Size(90, 30);
            this.btnRun.Text = "开始验证";
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            this.btnClose.Location = new System.Drawing.Point(516, 220);
            this.btnClose.Size = new System.Drawing.Size(86, 30);
            this.btnClose.Text = "关闭";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(20, 228);
            this.lblStatus.Text = "就绪";
            this.lblHint.Location = new System.Drawing.Point(18, 260);
            this.lblHint.Size = new System.Drawing.Size(584, 40);
            this.lblHint.Text = "对标「已更新宗地」与 parcel_pot。已更新地块应主要落在高/极高等级；偏低者写入 valid_diff 并生成 HTML 报告。";
            this.ClientSize = new System.Drawing.Size(624, 318);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.txtComment);
            this.Controls.Add(this.lblComment);
            this.Controls.Add(this.nudPassRatio);
            this.Controls.Add(this.lblPass);
            this.Controls.Add(this.nudHighThr);
            this.Controls.Add(this.lblHigh);
            this.Controls.Add(this.btnOpenGlobal);
            this.Controls.Add(this.lblCityInfo);
            this.Controls.Add(this.lblOutInfo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ValidationRunForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "验证校核";
            ((System.ComponentModel.ISupportInitialize)(this.nudHighThr)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPassRatio)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblOutInfo;
        private System.Windows.Forms.Label lblCityInfo;
        private System.Windows.Forms.Button btnOpenGlobal;
        private System.Windows.Forms.Label lblHigh;
        private System.Windows.Forms.NumericUpDown nudHighThr;
        private System.Windows.Forms.Label lblPass;
        private System.Windows.Forms.NumericUpDown nudPassRatio;
        private System.Windows.Forms.Label lblComment;
        private System.Windows.Forms.TextBox txtComment;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblHint;
    }
}

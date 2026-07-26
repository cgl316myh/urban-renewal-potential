namespace UrbanRenewal.Plugins.Overlay
{
    partial class ParcelLinkRunForm
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
            this.lblGlobal = new System.Windows.Forms.Label();
            this.lblOutInfo = new System.Windows.Forms.Label();
            this.lblCityInfo = new System.Windows.Forms.Label();
            this.btnOpenGlobal = new System.Windows.Forms.Button();
            this.lblStat = new System.Windows.Forms.Label();
            this.cboStat = new System.Windows.Forms.ComboBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblHint = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblGlobal
            // 
            this.lblGlobal.AutoSize = true;
            this.lblGlobal.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold);
            this.lblGlobal.Location = new System.Drawing.Point(18, 16);
            this.lblGlobal.Name = "lblGlobal";
            this.lblGlobal.Size = new System.Drawing.Size(57, 12);
            this.lblGlobal.TabIndex = 0;
            this.lblGlobal.Text = "全局设置";
            // 
            // lblOutInfo
            // 
            this.lblOutInfo.Location = new System.Drawing.Point(18, 38);
            this.lblOutInfo.Name = "lblOutInfo";
            this.lblOutInfo.Size = new System.Drawing.Size(480, 32);
            this.lblOutInfo.TabIndex = 1;
            this.lblOutInfo.Text = "输出 GDB：（未设置）";
            // 
            // lblCityInfo
            // 
            this.lblCityInfo.Location = new System.Drawing.Point(18, 72);
            this.lblCityInfo.Name = "lblCityInfo";
            this.lblCityInfo.Size = new System.Drawing.Size(480, 20);
            this.lblCityInfo.TabIndex = 2;
            this.lblCityInfo.Text = "城市配置：（未设置）";
            // 
            // btnOpenGlobal
            // 
            this.btnOpenGlobal.Location = new System.Drawing.Point(510, 38);
            this.btnOpenGlobal.Name = "btnOpenGlobal";
            this.btnOpenGlobal.Size = new System.Drawing.Size(95, 40);
            this.btnOpenGlobal.TabIndex = 3;
            this.btnOpenGlobal.Text = "修改全局设置";
            this.btnOpenGlobal.UseVisualStyleBackColor = true;
            this.btnOpenGlobal.Click += new System.EventHandler(this.btnOpenGlobal_Click);
            // 
            // lblStat
            // 
            this.lblStat.AutoSize = true;
            this.lblStat.Location = new System.Drawing.Point(18, 112);
            this.lblStat.Name = "lblStat";
            this.lblStat.Size = new System.Drawing.Size(113, 12);
            this.lblStat.TabIndex = 4;
            this.lblStat.Text = "区统计方式：";
            // 
            // cboStat
            // 
            this.cboStat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStat.FormattingEnabled = true;
            this.cboStat.Items.AddRange(new object[] {
            "MEAN（均值，推荐）",
            "MAXIMUM（最大值）"});
            this.cboStat.Location = new System.Drawing.Point(140, 108);
            this.cboStat.Name = "cboStat";
            this.cboStat.Size = new System.Drawing.Size(200, 20);
            this.cboStat.TabIndex = 5;
            this.cboStat.SelectedIndex = 0;
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(420, 160);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(90, 30);
            this.btnRun.TabIndex = 6;
            this.btnRun.Text = "开始关联";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(516, 160);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(86, 30);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(20, 168);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(29, 12);
            this.lblStatus.TabIndex = 8;
            this.lblStatus.Text = "就绪";
            // 
            // lblHint
            // 
            this.lblHint.Location = new System.Drawing.Point(18, 210);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(584, 48);
            this.lblHint.TabIndex = 9;
            this.lblHint.Text = "依赖 pot_score（及可选 mot_score / fea_score）。输出 parcel_pot，字段：POTENTIAL_SCORE、MOTIV_SCORE、FEASIB_SCORE、POTENTIAL_LEVEL。";
            // 
            // ParcelLinkRunForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 278);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.cboStat);
            this.Controls.Add(this.lblStat);
            this.Controls.Add(this.btnOpenGlobal);
            this.Controls.Add(this.lblCityInfo);
            this.Controls.Add(this.lblOutInfo);
            this.Controls.Add(this.lblGlobal);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ParcelLinkRunForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "宗地关联";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblGlobal;
        private System.Windows.Forms.Label lblOutInfo;
        private System.Windows.Forms.Label lblCityInfo;
        private System.Windows.Forms.Button btnOpenGlobal;
        private System.Windows.Forms.Label lblStat;
        private System.Windows.Forms.ComboBox cboStat;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblHint;
    }
}

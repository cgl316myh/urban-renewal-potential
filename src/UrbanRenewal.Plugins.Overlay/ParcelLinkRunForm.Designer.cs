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
            this.lblStat = new System.Windows.Forms.Label();
            this.cboStat = new System.Windows.Forms.ComboBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblStat
            // 
            this.lblStat.AutoSize = true;
            this.lblStat.Location = new System.Drawing.Point(18, 20);
            this.lblStat.Name = "lblStat";
            this.lblStat.Size = new System.Drawing.Size(113, 12);
            this.lblStat.TabIndex = 0;
            this.lblStat.Text = "区统计方式：";
            // 
            // cboStat
            // 
            this.cboStat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboStat.FormattingEnabled = true;
            this.cboStat.Items.AddRange(new object[] {
            "MEAN（均值，推荐）",
            "MAXIMUM（最大值）"});
            this.cboStat.Location = new System.Drawing.Point(140, 16);
            this.cboStat.Name = "cboStat";
            this.cboStat.Size = new System.Drawing.Size(200, 20);
            this.cboStat.TabIndex = 1;
            this.cboStat.SelectedIndex = 0;
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(420, 60);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(90, 30);
            this.btnRun.TabIndex = 2;
            this.btnRun.Text = "开始关联";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(516, 60);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(86, 30);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // ParcelLinkRunForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 110);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.cboStat);
            this.Controls.Add(this.lblStat);
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

        private System.Windows.Forms.Label lblStat;
        private System.Windows.Forms.ComboBox cboStat;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnClose;
    }
}

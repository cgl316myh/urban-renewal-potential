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
            this.lblHigh = new System.Windows.Forms.Label();
            this.nudHighThr = new System.Windows.Forms.NumericUpDown();
            this.lblPass = new System.Windows.Forms.Label();
            this.nudPassRatio = new System.Windows.Forms.NumericUpDown();
            this.lblComment = new System.Windows.Forms.Label();
            this.txtComment = new System.Windows.Forms.TextBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.nudHighThr)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPassRatio)).BeginInit();
            this.SuspendLayout();
            // 
            // lblHigh
            // 
            this.lblHigh.AutoSize = true;
            this.lblHigh.Location = new System.Drawing.Point(18, 20);
            this.lblHigh.Name = "lblHigh";
            this.lblHigh.Size = new System.Drawing.Size(113, 12);
            this.lblHigh.TabIndex = 0;
            this.lblHigh.Text = "高等级得分阈值：";
            // 
            // nudHighThr
            // 
            this.nudHighThr.Location = new System.Drawing.Point(140, 16);
            this.nudHighThr.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.nudHighThr.Name = "nudHighThr";
            this.nudHighThr.Size = new System.Drawing.Size(80, 21);
            this.nudHighThr.TabIndex = 1;
            this.nudHighThr.Value = new decimal(new int[] { 60, 0, 0, 0 });
            // 
            // lblPass
            // 
            this.lblPass.AutoSize = true;
            this.lblPass.Location = new System.Drawing.Point(250, 20);
            this.lblPass.Name = "lblPass";
            this.lblPass.Size = new System.Drawing.Size(77, 12);
            this.lblPass.TabIndex = 2;
            this.lblPass.Text = "通过占比%：";
            // 
            // nudPassRatio
            // 
            this.nudPassRatio.Location = new System.Drawing.Point(340, 16);
            this.nudPassRatio.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.nudPassRatio.Name = "nudPassRatio";
            this.nudPassRatio.Size = new System.Drawing.Size(80, 21);
            this.nudPassRatio.TabIndex = 3;
            this.nudPassRatio.Value = new decimal(new int[] { 60, 0, 0, 0 });
            // 
            // lblComment
            // 
            this.lblComment.AutoSize = true;
            this.lblComment.Location = new System.Drawing.Point(18, 54);
            this.lblComment.Name = "lblComment";
            this.lblComment.Size = new System.Drawing.Size(65, 12);
            this.lblComment.TabIndex = 4;
            this.lblComment.Text = "审核意见：";
            // 
            // txtComment
            // 
            this.txtComment.Location = new System.Drawing.Point(20, 74);
            this.txtComment.Multiline = true;
            this.txtComment.Name = "txtComment";
            this.txtComment.Size = new System.Drawing.Size(582, 60);
            this.txtComment.TabIndex = 5;
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(420, 150);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(90, 30);
            this.btnRun.TabIndex = 6;
            this.btnRun.Text = "开始验证";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(516, 150);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(86, 30);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // ValidationRunForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 230);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.txtComment);
            this.Controls.Add(this.lblComment);
            this.Controls.Add(this.nudPassRatio);
            this.Controls.Add(this.lblPass);
            this.Controls.Add(this.nudHighThr);
            this.Controls.Add(this.lblHigh);
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

        private System.Windows.Forms.Label lblHigh;
        private System.Windows.Forms.NumericUpDown nudHighThr;
        private System.Windows.Forms.Label lblPass;
        private System.Windows.Forms.NumericUpDown nudPassRatio;
        private System.Windows.Forms.Label lblComment;
        private System.Windows.Forms.TextBox txtComment;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnClose;
    }
}

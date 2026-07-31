namespace UrbanRenewal.Plugins.Feasibility
{
    partial class FeasibilityRunForm
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
            this.grpParams = new System.Windows.Forms.GroupBox();
            this.lblElev = new System.Windows.Forms.Label();
            this.nudElevThr = new System.Windows.Forms.NumericUpDown();
            this.lblSlope = new System.Windows.Forms.Label();
            this.nudSlopeThr = new System.Windows.Forms.NumericUpDown();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.grpParams.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudElevThr)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSlopeThr)).BeginInit();
            this.SuspendLayout();
            // 
            // grpParams
            // 
            this.grpParams.Controls.Add(this.lblElev);
            this.grpParams.Controls.Add(this.nudElevThr);
            this.grpParams.Controls.Add(this.lblSlope);
            this.grpParams.Controls.Add(this.nudSlopeThr);
            this.grpParams.Location = new System.Drawing.Point(20, 16);
            this.grpParams.Name = "grpParams";
            this.grpParams.Size = new System.Drawing.Size(582, 80);
            this.grpParams.TabIndex = 0;
            this.grpParams.TabStop = false;
            this.grpParams.Text = "地形阈值（方案默认：高程 50m、坡度 15°）";
            // 
            // lblElev
            // 
            this.lblElev.AutoSize = true;
            this.lblElev.Location = new System.Drawing.Point(20, 36);
            this.lblElev.Name = "lblElev";
            this.lblElev.Size = new System.Drawing.Size(113, 12);
            this.lblElev.TabIndex = 0;
            this.lblElev.Text = "高程阈值(米)：";
            // 
            // nudElevThr
            // 
            this.nudElevThr.DecimalPlaces = 1;
            this.nudElevThr.Location = new System.Drawing.Point(140, 32);
            this.nudElevThr.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
            this.nudElevThr.Name = "nudElevThr";
            this.nudElevThr.Size = new System.Drawing.Size(80, 21);
            this.nudElevThr.TabIndex = 1;
            this.nudElevThr.Value = new decimal(new int[] { 50, 0, 0, 0 });
            // 
            // lblSlope
            // 
            this.lblSlope.AutoSize = true;
            this.lblSlope.Location = new System.Drawing.Point(280, 36);
            this.lblSlope.Name = "lblSlope";
            this.lblSlope.Size = new System.Drawing.Size(113, 12);
            this.lblSlope.TabIndex = 2;
            this.lblSlope.Text = "坡度阈值(度)：";
            // 
            // nudSlopeThr
            // 
            this.nudSlopeThr.DecimalPlaces = 1;
            this.nudSlopeThr.Location = new System.Drawing.Point(400, 32);
            this.nudSlopeThr.Maximum = new decimal(new int[] { 90, 0, 0, 0 });
            this.nudSlopeThr.Name = "nudSlopeThr";
            this.nudSlopeThr.Size = new System.Drawing.Size(80, 21);
            this.nudSlopeThr.TabIndex = 3;
            this.nudSlopeThr.Value = new decimal(new int[] { 15, 0, 0, 0 });
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(420, 110);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(90, 30);
            this.btnRun.TabIndex = 1;
            this.btnRun.Text = "开始分析";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(516, 110);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(86, 30);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // FeasibilityRunForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 160);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.grpParams);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FeasibilityRunForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "可行度分析";
            this.grpParams.ResumeLayout(false);
            this.grpParams.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudElevThr)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSlopeThr)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox grpParams;
        private System.Windows.Forms.Label lblElev;
        private System.Windows.Forms.NumericUpDown nudElevThr;
        private System.Windows.Forms.Label lblSlope;
        private System.Windows.Forms.NumericUpDown nudSlopeThr;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnClose;
    }
}

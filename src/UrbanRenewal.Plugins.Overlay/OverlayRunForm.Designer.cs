namespace UrbanRenewal.Plugins.Overlay
{
    partial class OverlayRunForm
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
            this.grpWeights = new System.Windows.Forms.GroupBox();
            this.lblMotivW = new System.Windows.Forms.Label();
            this.nudMotivW = new System.Windows.Forms.NumericUpDown();
            this.lblFeasibW = new System.Windows.Forms.Label();
            this.nudFeasibW = new System.Windows.Forms.NumericUpDown();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.grpWeights.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMotivW)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFeasibW)).BeginInit();
            this.SuspendLayout();
            // 
            // grpWeights
            // 
            this.grpWeights.Controls.Add(this.lblMotivW);
            this.grpWeights.Controls.Add(this.nudMotivW);
            this.grpWeights.Controls.Add(this.lblFeasibW);
            this.grpWeights.Controls.Add(this.nudFeasibW);
            this.grpWeights.Location = new System.Drawing.Point(20, 16);
            this.grpWeights.Name = "grpWeights";
            this.grpWeights.Size = new System.Drawing.Size(582, 70);
            this.grpWeights.TabIndex = 0;
            this.grpWeights.TabStop = false;
            this.grpWeights.Text = "叠置权重（方案默认：动力 70% + 可行 30%）";
            // 
            // lblMotivW
            // 
            this.lblMotivW.AutoSize = true;
            this.lblMotivW.Location = new System.Drawing.Point(20, 32);
            this.lblMotivW.Name = "lblMotivW";
            this.lblMotivW.Size = new System.Drawing.Size(89, 12);
            this.lblMotivW.TabIndex = 0;
            this.lblMotivW.Text = "动力性权重%：";
            // 
            // nudMotivW
            // 
            this.nudMotivW.DecimalPlaces = 1;
            this.nudMotivW.Location = new System.Drawing.Point(120, 28);
            this.nudMotivW.Name = "nudMotivW";
            this.nudMotivW.Size = new System.Drawing.Size(70, 21);
            this.nudMotivW.TabIndex = 1;
            this.nudMotivW.Value = new decimal(new int[] { 70, 0, 0, 0 });
            // 
            // lblFeasibW
            // 
            this.lblFeasibW.AutoSize = true;
            this.lblFeasibW.Location = new System.Drawing.Point(220, 32);
            this.lblFeasibW.Name = "lblFeasibW";
            this.lblFeasibW.Size = new System.Drawing.Size(89, 12);
            this.lblFeasibW.TabIndex = 2;
            this.lblFeasibW.Text = "可行度权重%：";
            // 
            // nudFeasibW
            // 
            this.nudFeasibW.DecimalPlaces = 1;
            this.nudFeasibW.Location = new System.Drawing.Point(320, 28);
            this.nudFeasibW.Name = "nudFeasibW";
            this.nudFeasibW.Size = new System.Drawing.Size(70, 21);
            this.nudFeasibW.TabIndex = 3;
            this.nudFeasibW.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(420, 100);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(90, 30);
            this.btnRun.TabIndex = 1;
            this.btnRun.Text = "开始叠置";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(516, 100);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(86, 30);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // OverlayRunForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 150);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.grpWeights);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OverlayRunForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "综合潜力叠置";
            this.grpWeights.ResumeLayout(false);
            this.grpWeights.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMotivW)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFeasibW)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox grpWeights;
        private System.Windows.Forms.Label lblMotivW;
        private System.Windows.Forms.NumericUpDown nudMotivW;
        private System.Windows.Forms.Label lblFeasibW;
        private System.Windows.Forms.NumericUpDown nudFeasibW;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnClose;
    }
}

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
            this.lblGlobal = new System.Windows.Forms.Label();
            this.lblOutInfo = new System.Windows.Forms.Label();
            this.lblCityInfo = new System.Windows.Forms.Label();
            this.btnOpenGlobal = new System.Windows.Forms.Button();
            this.grpWeights = new System.Windows.Forms.GroupBox();
            this.lblMotivW = new System.Windows.Forms.Label();
            this.nudMotivW = new System.Windows.Forms.NumericUpDown();
            this.lblFeasibW = new System.Windows.Forms.Label();
            this.nudFeasibW = new System.Windows.Forms.NumericUpDown();
            this.lblCell = new System.Windows.Forms.Label();
            this.nudCellSize = new System.Windows.Forms.NumericUpDown();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblHint = new System.Windows.Forms.Label();
            this.grpWeights.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMotivW)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFeasibW)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCellSize)).BeginInit();
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
            // grpWeights
            // 
            this.grpWeights.Controls.Add(this.lblMotivW);
            this.grpWeights.Controls.Add(this.nudMotivW);
            this.grpWeights.Controls.Add(this.lblFeasibW);
            this.grpWeights.Controls.Add(this.nudFeasibW);
            this.grpWeights.Controls.Add(this.lblCell);
            this.grpWeights.Controls.Add(this.nudCellSize);
            this.grpWeights.Location = new System.Drawing.Point(20, 110);
            this.grpWeights.Name = "grpWeights";
            this.grpWeights.Size = new System.Drawing.Size(582, 90);
            this.grpWeights.TabIndex = 4;
            this.grpWeights.TabStop = false;
            this.grpWeights.Text = "叠置权重与像元（方案默认：动力 70% + 可行 30%）";
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
            // lblCell
            // 
            this.lblCell.AutoSize = true;
            this.lblCell.Location = new System.Drawing.Point(20, 62);
            this.lblCell.Name = "lblCell";
            this.lblCell.Size = new System.Drawing.Size(101, 12);
            this.lblCell.TabIndex = 4;
            this.lblCell.Text = "像元大小(米)：";
            // 
            // nudCellSize
            // 
            this.nudCellSize.Location = new System.Drawing.Point(120, 58);
            this.nudCellSize.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            this.nudCellSize.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            this.nudCellSize.Name = "nudCellSize";
            this.nudCellSize.Size = new System.Drawing.Size(70, 21);
            this.nudCellSize.TabIndex = 5;
            this.nudCellSize.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(420, 220);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(90, 30);
            this.btnRun.TabIndex = 5;
            this.btnRun.Text = "开始叠置";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(516, 220);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(86, 30);
            this.btnClose.TabIndex = 6;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(20, 228);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(29, 12);
            this.lblStatus.TabIndex = 7;
            this.lblStatus.Text = "就绪";
            // 
            // lblHint
            // 
            this.lblHint.Location = new System.Drawing.Point(18, 264);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(584, 40);
            this.lblHint.TabIndex = 8;
            this.lblHint.Text = "依赖输出 GDB 中的 mot_score（动力性）与 fea_score（可行度）。结果：pot_score（0–100）、pot_level（1偏低～5极高）。";
            // 
            // OverlayRunForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 318);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.grpWeights);
            this.Controls.Add(this.btnOpenGlobal);
            this.Controls.Add(this.lblCityInfo);
            this.Controls.Add(this.lblOutInfo);
            this.Controls.Add(this.lblGlobal);
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
            ((System.ComponentModel.ISupportInitialize)(this.nudCellSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblGlobal;
        private System.Windows.Forms.Label lblOutInfo;
        private System.Windows.Forms.Label lblCityInfo;
        private System.Windows.Forms.Button btnOpenGlobal;
        private System.Windows.Forms.GroupBox grpWeights;
        private System.Windows.Forms.Label lblMotivW;
        private System.Windows.Forms.NumericUpDown nudMotivW;
        private System.Windows.Forms.Label lblFeasibW;
        private System.Windows.Forms.NumericUpDown nudFeasibW;
        private System.Windows.Forms.Label lblCell;
        private System.Windows.Forms.NumericUpDown nudCellSize;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblHint;
    }
}

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
            this.lblGlobal = new System.Windows.Forms.Label();
            this.lblOutInfo = new System.Windows.Forms.Label();
            this.lblCityInfo = new System.Windows.Forms.Label();
            this.btnOpenGlobal = new System.Windows.Forms.Button();
            this.lblCell = new System.Windows.Forms.Label();
            this.nudCellSize = new System.Windows.Forms.NumericUpDown();
            this.grpParams = new System.Windows.Forms.GroupBox();
            this.lblElev = new System.Windows.Forms.Label();
            this.nudElevThr = new System.Windows.Forms.NumericUpDown();
            this.lblSlope = new System.Windows.Forms.Label();
            this.nudSlopeThr = new System.Windows.Forms.NumericUpDown();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblHint = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudCellSize)).BeginInit();
            this.grpParams.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudElevThr)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSlopeThr)).BeginInit();
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
            // lblCell
            // 
            this.lblCell.AutoSize = true;
            this.lblCell.Location = new System.Drawing.Point(18, 110);
            this.lblCell.Name = "lblCell";
            this.lblCell.Size = new System.Drawing.Size(101, 12);
            this.lblCell.TabIndex = 4;
            this.lblCell.Text = "像元大小(米)：";
            // 
            // nudCellSize
            // 
            this.nudCellSize.Location = new System.Drawing.Point(125, 106);
            this.nudCellSize.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            this.nudCellSize.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            this.nudCellSize.Name = "nudCellSize";
            this.nudCellSize.Size = new System.Drawing.Size(80, 21);
            this.nudCellSize.TabIndex = 5;
            this.nudCellSize.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // grpParams
            // 
            this.grpParams.Controls.Add(this.lblElev);
            this.grpParams.Controls.Add(this.nudElevThr);
            this.grpParams.Controls.Add(this.lblSlope);
            this.grpParams.Controls.Add(this.nudSlopeThr);
            this.grpParams.Location = new System.Drawing.Point(20, 140);
            this.grpParams.Name = "grpParams";
            this.grpParams.Size = new System.Drawing.Size(582, 80);
            this.grpParams.TabIndex = 6;
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
            this.btnRun.Location = new System.Drawing.Point(420, 240);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(90, 30);
            this.btnRun.TabIndex = 7;
            this.btnRun.Text = "开始分析";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(516, 240);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(86, 30);
            this.btnClose.TabIndex = 8;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(20, 248);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(29, 12);
            this.lblStatus.TabIndex = 9;
            this.lblStatus.Text = "就绪";
            // 
            // lblHint
            // 
            this.lblHint.Location = new System.Drawing.Point(18, 284);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(584, 48);
            this.lblHint.TabIndex = 10;
            this.lblHint.Text = "因子：宗地 SI/PD、DEM 高程/坡度、人口密度。合成后写入输出 GDB（fea_score）。图层角色在城市配置中映射：Parcel / DEM / Population。";
            // 
            // FeasibilityRunForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 348);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.grpParams);
            this.Controls.Add(this.nudCellSize);
            this.Controls.Add(this.lblCell);
            this.Controls.Add(this.btnOpenGlobal);
            this.Controls.Add(this.lblCityInfo);
            this.Controls.Add(this.lblOutInfo);
            this.Controls.Add(this.lblGlobal);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FeasibilityRunForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "可行度分析";
            ((System.ComponentModel.ISupportInitialize)(this.nudCellSize)).EndInit();
            this.grpParams.ResumeLayout(false);
            this.grpParams.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudElevThr)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudSlopeThr)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblGlobal;
        private System.Windows.Forms.Label lblOutInfo;
        private System.Windows.Forms.Label lblCityInfo;
        private System.Windows.Forms.Button btnOpenGlobal;
        private System.Windows.Forms.Label lblCell;
        private System.Windows.Forms.NumericUpDown nudCellSize;
        private System.Windows.Forms.GroupBox grpParams;
        private System.Windows.Forms.Label lblElev;
        private System.Windows.Forms.NumericUpDown nudElevThr;
        private System.Windows.Forms.Label lblSlope;
        private System.Windows.Forms.NumericUpDown nudSlopeThr;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblHint;
    }
}

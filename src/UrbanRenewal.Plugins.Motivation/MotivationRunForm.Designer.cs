namespace UrbanRenewal.Plugins.Motivation
{
    partial class MotivationRunForm
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
            this.lblGdb = new System.Windows.Forms.Label();
            this.txtGdbPath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblCell = new System.Windows.Forms.Label();
            this.nudCellSize = new System.Windows.Forms.NumericUpDown();
            this.grpWeights = new System.Windows.Forms.GroupBox();
            this.lblTraffic = new System.Windows.Forms.Label();
            this.nudTraffic = new System.Windows.Forms.NumericUpDown();
            this.lblEnvironment = new System.Windows.Forms.Label();
            this.nudEnvironment = new System.Windows.Forms.NumericUpDown();
            this.lblFacility = new System.Windows.Forms.Label();
            this.nudFacility = new System.Windows.Forms.NumericUpDown();
            this.lblPolicy = new System.Windows.Forms.Label();
            this.nudPolicy = new System.Windows.Forms.NumericUpDown();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblHint = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudCellSize)).BeginInit();
            this.grpWeights.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTraffic)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEnvironment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFacility)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPolicy)).BeginInit();
            this.SuspendLayout();
            // 
            // lblGdb
            // 
            this.lblGdb.AutoSize = true;
            this.lblGdb.Location = new System.Drawing.Point(18, 20);
            this.lblGdb.Name = "lblGdb";
            this.lblGdb.Size = new System.Drawing.Size(77, 12);
            this.lblGdb.TabIndex = 0;
            this.lblGdb.Text = "工作 GDB：";
            // 
            // txtGdbPath
            // 
            this.txtGdbPath.Location = new System.Drawing.Point(101, 16);
            this.txtGdbPath.Name = "txtGdbPath";
            this.txtGdbPath.Size = new System.Drawing.Size(420, 21);
            this.txtGdbPath.TabIndex = 1;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(527, 14);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 25);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "浏览...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // lblCell
            // 
            this.lblCell.AutoSize = true;
            this.lblCell.Location = new System.Drawing.Point(18, 55);
            this.lblCell.Name = "lblCell";
            this.lblCell.Size = new System.Drawing.Size(101, 12);
            this.lblCell.TabIndex = 3;
            this.lblCell.Text = "像元大小(米)：";
            // 
            // nudCellSize
            // 
            this.nudCellSize.Location = new System.Drawing.Point(125, 51);
            this.nudCellSize.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            this.nudCellSize.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            this.nudCellSize.Name = "nudCellSize";
            this.nudCellSize.Size = new System.Drawing.Size(80, 21);
            this.nudCellSize.TabIndex = 4;
            this.nudCellSize.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // grpWeights
            // 
            this.grpWeights.Controls.Add(this.lblTraffic);
            this.grpWeights.Controls.Add(this.nudTraffic);
            this.grpWeights.Controls.Add(this.lblEnvironment);
            this.grpWeights.Controls.Add(this.nudEnvironment);
            this.grpWeights.Controls.Add(this.lblFacility);
            this.grpWeights.Controls.Add(this.nudFacility);
            this.grpWeights.Controls.Add(this.lblPolicy);
            this.grpWeights.Controls.Add(this.nudPolicy);
            this.grpWeights.Location = new System.Drawing.Point(20, 90);
            this.grpWeights.Name = "grpWeights";
            this.grpWeights.Size = new System.Drawing.Size(582, 110);
            this.grpWeights.TabIndex = 5;
            this.grpWeights.TabStop = false;
            this.grpWeights.Text = "准则层权重（%）";
            // 
            // lblTraffic
            // 
            this.lblTraffic.AutoSize = true;
            this.lblTraffic.Location = new System.Drawing.Point(20, 32);
            this.lblTraffic.Name = "lblTraffic";
            this.lblTraffic.Size = new System.Drawing.Size(77, 12);
            this.lblTraffic.TabIndex = 0;
            this.lblTraffic.Text = "交通便捷度";
            // 
            // nudTraffic
            // 
            this.nudTraffic.Location = new System.Drawing.Point(110, 28);
            this.nudTraffic.Name = "nudTraffic";
            this.nudTraffic.Size = new System.Drawing.Size(60, 21);
            this.nudTraffic.TabIndex = 1;
            this.nudTraffic.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // lblEnvironment
            // 
            this.lblEnvironment.AutoSize = true;
            this.lblEnvironment.Location = new System.Drawing.Point(210, 32);
            this.lblEnvironment.Name = "lblEnvironment";
            this.lblEnvironment.Size = new System.Drawing.Size(77, 12);
            this.lblEnvironment.TabIndex = 2;
            this.lblEnvironment.Text = "环境舒适度";
            // 
            // nudEnvironment
            // 
            this.nudEnvironment.Location = new System.Drawing.Point(300, 28);
            this.nudEnvironment.Name = "nudEnvironment";
            this.nudEnvironment.Size = new System.Drawing.Size(60, 21);
            this.nudEnvironment.TabIndex = 3;
            this.nudEnvironment.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // lblFacility
            // 
            this.lblFacility.AutoSize = true;
            this.lblFacility.Location = new System.Drawing.Point(20, 68);
            this.lblFacility.Name = "lblFacility";
            this.lblFacility.Size = new System.Drawing.Size(77, 12);
            this.lblFacility.TabIndex = 4;
            this.lblFacility.Text = "设施完善度";
            // 
            // nudFacility
            // 
            this.nudFacility.Location = new System.Drawing.Point(110, 64);
            this.nudFacility.Name = "nudFacility";
            this.nudFacility.Size = new System.Drawing.Size(60, 21);
            this.nudFacility.TabIndex = 5;
            this.nudFacility.Value = new decimal(new int[] { 25, 0, 0, 0 });
            // 
            // lblPolicy
            // 
            this.lblPolicy.AutoSize = true;
            this.lblPolicy.Location = new System.Drawing.Point(210, 68);
            this.lblPolicy.Name = "lblPolicy";
            this.lblPolicy.Size = new System.Drawing.Size(77, 12);
            this.lblPolicy.TabIndex = 6;
            this.lblPolicy.Text = "政策支持度";
            // 
            // nudPolicy
            // 
            this.nudPolicy.Location = new System.Drawing.Point(300, 64);
            this.nudPolicy.Name = "nudPolicy";
            this.nudPolicy.Size = new System.Drawing.Size(60, 21);
            this.nudPolicy.TabIndex = 7;
            this.nudPolicy.Value = new decimal(new int[] { 25, 0, 0, 0 });
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(420, 220);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(90, 30);
            this.btnRun.TabIndex = 6;
            this.btnRun.Text = "开始分析";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(516, 220);
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
            this.lblStatus.Location = new System.Drawing.Point(20, 228);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(29, 12);
            this.lblStatus.TabIndex = 8;
            this.lblStatus.Text = "就绪";
            // 
            // lblHint
            // 
            this.lblHint.Location = new System.Drawing.Point(18, 265);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(584, 48);
            this.lblHint.TabIndex = 9;
            this.lblHint.Text = "说明：按图层名称关键词自动匹配地铁/CBD/绿地/公服/政策区等；缺项准则自动跳过并重分配权重。路网 OD 可达性暂用 CBD 缓冲近似，后续可接 Network Analyst。";
            // 
            // MotivationRunForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 331);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.grpWeights);
            this.Controls.Add(this.nudCellSize);
            this.Controls.Add(this.lblCell);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtGdbPath);
            this.Controls.Add(this.lblGdb);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MotivationRunForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "动力性分析";
            ((System.ComponentModel.ISupportInitialize)(this.nudCellSize)).EndInit();
            this.grpWeights.ResumeLayout(false);
            this.grpWeights.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudTraffic)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEnvironment)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFacility)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPolicy)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblGdb;
        private System.Windows.Forms.TextBox txtGdbPath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblCell;
        private System.Windows.Forms.NumericUpDown nudCellSize;
        private System.Windows.Forms.GroupBox grpWeights;
        private System.Windows.Forms.Label lblTraffic;
        private System.Windows.Forms.NumericUpDown nudTraffic;
        private System.Windows.Forms.Label lblEnvironment;
        private System.Windows.Forms.NumericUpDown nudEnvironment;
        private System.Windows.Forms.Label lblFacility;
        private System.Windows.Forms.NumericUpDown nudFacility;
        private System.Windows.Forms.Label lblPolicy;
        private System.Windows.Forms.NumericUpDown nudPolicy;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblHint;
    }
}

namespace UrbanRenewal.Plugins.DataManage
{
    partial class DataIntegrityForm
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
            this.lblProfile = new System.Windows.Forms.Label();
            this.lblSummary = new System.Windows.Forms.Label();
            this.lblLegend = new System.Windows.Forms.Label();
            this.grid = new System.Windows.Forms.DataGridView();
            this.lblHint = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.SuspendLayout();
            // 
            // lblGdb
            // 
            this.lblGdb.Location = new System.Drawing.Point(16, 12);
            this.lblGdb.Name = "lblGdb";
            this.lblGdb.Size = new System.Drawing.Size(760, 20);
            this.lblGdb.TabIndex = 0;
            this.lblGdb.Text = "当前 GDB：";
            // 
            // lblProfile
            // 
            this.lblProfile.Location = new System.Drawing.Point(16, 34);
            this.lblProfile.Name = "lblProfile";
            this.lblProfile.Size = new System.Drawing.Size(560, 20);
            this.lblProfile.TabIndex = 1;
            this.lblProfile.Text = "配置文件：";
            // 
            // lblSummary
            // 
            this.lblSummary.Location = new System.Drawing.Point(580, 34);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new System.Drawing.Size(200, 20);
            this.lblSummary.TabIndex = 2;
            this.lblSummary.Text = "通过 / 未通过";
            this.lblSummary.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblLegend
            // 
            this.lblLegend.Location = new System.Drawing.Point(16, 56);
            this.lblLegend.Name = "lblLegend";
            this.lblLegend.Size = new System.Drawing.Size(760, 20);
            this.lblLegend.TabIndex = 3;
            this.lblLegend.Text = "图例：绿色「通过」；红色「未通过」；灰色「选填未配置」。橙色底行为必填项。";
            // 
            // grid
            // 
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Location = new System.Drawing.Point(16, 80);
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.ReadOnly = true;
            this.grid.RowHeadersVisible = false;
            this.grid.RowTemplate.Height = 24;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid.Size = new System.Drawing.Size(760, 420);
            this.grid.TabIndex = 4;
            // 
            // lblHint
            // 
            this.lblHint.Location = new System.Drawing.Point(16, 512);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(560, 36);
            this.lblHint.TabIndex = 5;
            this.lblHint.Text = "按当前「数据配置 / 城市配置」检查输入 GDB 中各角色图层是否可解析；并检查输出库、坐标系与预建路网。";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(580, 516);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(90, 30);
            this.btnRefresh.TabIndex = 6;
            this.btnRefresh.Text = "重新检查";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(686, 516);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(90, 30);
            this.btnClose.TabIndex = 7;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // DataIntegrityForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 562);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.lblLegend);
            this.Controls.Add(this.lblSummary);
            this.Controls.Add(this.lblProfile);
            this.Controls.Add(this.lblGdb);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DataIntegrityForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "数据完整性检查";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblGdb;
        private System.Windows.Forms.Label lblProfile;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.Label lblLegend;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnClose;
    }
}

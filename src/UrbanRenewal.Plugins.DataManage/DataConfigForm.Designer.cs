namespace UrbanRenewal.Plugins.DataManage
{
    partial class DataConfigForm
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
            this.lblCount = new System.Windows.Forms.Label();
            this.grid = new System.Windows.Forms.DataGridView();
            this.grpNet = new System.Windows.Forms.GroupBox();
            this.lblNdFd = new System.Windows.Forms.Label();
            this.txtNdFd = new System.Windows.Forms.TextBox();
            this.lblNdName = new System.Windows.Forms.Label();
            this.txtNdName = new System.Windows.Forms.TextBox();
            this.lblNdImp = new System.Windows.Forms.Label();
            this.txtNdImp = new System.Windows.Forms.TextBox();
            this.btnAuto = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.lblLegend = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.grpNet.SuspendLayout();
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
            // lblCount
            // 
            this.lblCount.Location = new System.Drawing.Point(580, 34);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(200, 20);
            this.lblCount.TabIndex = 2;
            this.lblCount.Text = "要素/栅格统计";
            this.lblCount.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblLegend
            // 
            this.lblLegend.Location = new System.Drawing.Point(16, 56);
            this.lblLegend.Name = "lblLegend";
            this.lblLegend.Size = new System.Drawing.Size(760, 20);
            this.lblLegend.TabIndex = 3;
            this.lblLegend.Text = "图例：橙色底行为必填角色；绿色「选填」可留空（运算时跳过该因子）。";
            // 
            // grid
            // 
            this.grid.AllowUserToAddRows = false;
            this.grid.AllowUserToDeleteRows = false;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Location = new System.Drawing.Point(16, 80);
            this.grid.MultiSelect = false;
            this.grid.Name = "grid";
            this.grid.RowHeadersVisible = false;
            this.grid.RowTemplate.Height = 24;
            this.grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.grid.Size = new System.Drawing.Size(760, 360);
            this.grid.TabIndex = 4;
            this.grid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.grid_DataError);
            // 
            // grpNet
            // 
            this.grpNet.Controls.Add(this.lblNdFd);
            this.grpNet.Controls.Add(this.txtNdFd);
            this.grpNet.Controls.Add(this.lblNdName);
            this.grpNet.Controls.Add(this.txtNdName);
            this.grpNet.Controls.Add(this.lblNdImp);
            this.grpNet.Controls.Add(this.txtNdImp);
            this.grpNet.Location = new System.Drawing.Point(16, 450);
            this.grpNet.Name = "grpNet";
            this.grpNet.Size = new System.Drawing.Size(760, 58);
            this.grpNet.TabIndex = 5;
            this.grpNet.TabStop = false;
            this.grpNet.Text = "预建路网 Network Dataset（可选；须在 ArcGIS 中预先构建）";
            // 
            // lblNdFd
            // 
            this.lblNdFd.AutoSize = true;
            this.lblNdFd.Location = new System.Drawing.Point(12, 26);
            this.lblNdFd.Name = "lblNdFd";
            this.lblNdFd.Size = new System.Drawing.Size(89, 12);
            this.lblNdFd.TabIndex = 0;
            this.lblNdFd.Text = "要素数据集：";
            // 
            // txtNdFd
            // 
            this.txtNdFd.Location = new System.Drawing.Point(100, 22);
            this.txtNdFd.Name = "txtNdFd";
            this.txtNdFd.Size = new System.Drawing.Size(120, 21);
            this.txtNdFd.TabIndex = 1;
            // 
            // lblNdName
            // 
            this.lblNdName.AutoSize = true;
            this.lblNdName.Location = new System.Drawing.Point(240, 26);
            this.lblNdName.Name = "lblNdName";
            this.lblNdName.Size = new System.Drawing.Size(53, 12);
            this.lblNdName.TabIndex = 2;
            this.lblNdName.Text = "ND名称：";
            // 
            // txtNdName
            // 
            this.txtNdName.Location = new System.Drawing.Point(300, 22);
            this.txtNdName.Name = "txtNdName";
            this.txtNdName.Size = new System.Drawing.Size(140, 21);
            this.txtNdName.TabIndex = 3;
            // 
            // lblNdImp
            // 
            this.lblNdImp.AutoSize = true;
            this.lblNdImp.Location = new System.Drawing.Point(460, 26);
            this.lblNdImp.Name = "lblNdImp";
            this.lblNdImp.Size = new System.Drawing.Size(41, 12);
            this.lblNdImp.TabIndex = 4;
            this.lblNdImp.Text = "阻抗：";
            // 
            // txtNdImp
            // 
            this.txtNdImp.Location = new System.Drawing.Point(510, 22);
            this.txtNdImp.Name = "txtNdImp";
            this.txtNdImp.Size = new System.Drawing.Size(100, 21);
            this.txtNdImp.TabIndex = 5;
            // 
            // btnAuto
            // 
            this.btnAuto.Location = new System.Drawing.Point(430, 518);
            this.btnAuto.Name = "btnAuto";
            this.btnAuto.Size = new System.Drawing.Size(110, 30);
            this.btnAuto.TabIndex = 6;
            this.btnAuto.Text = "按关键词匹配";
            this.btnAuto.UseVisualStyleBackColor = true;
            this.btnAuto.Click += new System.EventHandler(this.btnAuto_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(560, 518);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(90, 30);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(666, 518);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(90, 30);
            this.btnClose.TabIndex = 8;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // DataConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 562);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnAuto);
            this.Controls.Add(this.grpNet);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.lblLegend);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.lblProfile);
            this.Controls.Add(this.lblGdb);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DataConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "数据配置";
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.grpNet.ResumeLayout(false);
            this.grpNet.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label lblGdb;
        private System.Windows.Forms.Label lblProfile;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.Label lblLegend;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.GroupBox grpNet;
        private System.Windows.Forms.Label lblNdFd;
        private System.Windows.Forms.TextBox txtNdFd;
        private System.Windows.Forms.Label lblNdName;
        private System.Windows.Forms.TextBox txtNdName;
        private System.Windows.Forms.Label lblNdImp;
        private System.Windows.Forms.TextBox txtNdImp;
        private System.Windows.Forms.Button btnAuto;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClose;
    }
}

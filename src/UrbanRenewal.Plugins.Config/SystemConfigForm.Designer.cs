namespace UrbanRenewal.Plugins.Config
{
    partial class SystemConfigForm
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
            this.grpOverlay = new System.Windows.Forms.GroupBox();
            this.lblMotivW = new System.Windows.Forms.Label();
            this.nudMotivW = new System.Windows.Forms.NumericUpDown();
            this.lblFeasibW = new System.Windows.Forms.Label();
            this.nudFeasibW = new System.Windows.Forms.NumericUpDown();
            this.grpCity = new System.Windows.Forms.GroupBox();
            this.lblCity = new System.Windows.Forms.Label();
            this.lblTraffic = new System.Windows.Forms.Label();
            this.nudTraffic = new System.Windows.Forms.NumericUpDown();
            this.lblEnv = new System.Windows.Forms.Label();
            this.nudEnv = new System.Windows.Forms.NumericUpDown();
            this.lblFac = new System.Windows.Forms.Label();
            this.nudFac = new System.Windows.Forms.NumericUpDown();
            this.lblPol = new System.Windows.Forms.Label();
            this.nudPol = new System.Windows.Forms.NumericUpDown();
            this.grpSkin = new System.Windows.Forms.GroupBox();
            this.cboSkin = new System.Windows.Forms.ComboBox();
            this.btnApplySkin = new System.Windows.Forms.Button();
            this.lblPlugins = new System.Windows.Forms.Label();
            this.txtPlugins = new System.Windows.Forms.TextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.grpOverlay.SuspendLayout();
            this.grpCity.SuspendLayout();
            this.grpSkin.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMotivW)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFeasibW)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTraffic)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEnv)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFac)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPol)).BeginInit();
            this.SuspendLayout();
            this.grpOverlay.Text = "综合潜力叠置权重%";
            this.grpOverlay.Location = new System.Drawing.Point(16, 12);
            this.grpOverlay.Size = new System.Drawing.Size(590, 60);
            this.grpOverlay.Controls.Add(this.lblMotivW);
            this.grpOverlay.Controls.Add(this.nudMotivW);
            this.grpOverlay.Controls.Add(this.lblFeasibW);
            this.grpOverlay.Controls.Add(this.nudFeasibW);
            this.lblMotivW.AutoSize = true;
            this.lblMotivW.Location = new System.Drawing.Point(16, 28);
            this.lblMotivW.Text = "动力性：";
            this.nudMotivW.Location = new System.Drawing.Point(80, 24);
            this.nudMotivW.DecimalPlaces = 1;
            this.nudMotivW.Maximum = 100;
            this.nudMotivW.Value = 70;
            this.lblFeasibW.AutoSize = true;
            this.lblFeasibW.Location = new System.Drawing.Point(200, 28);
            this.lblFeasibW.Text = "可行度：";
            this.nudFeasibW.Location = new System.Drawing.Point(260, 24);
            this.nudFeasibW.DecimalPlaces = 1;
            this.nudFeasibW.Maximum = 100;
            this.nudFeasibW.Value = 30;
            this.grpCity.Text = "动力性准则层权重%";
            this.grpCity.Location = new System.Drawing.Point(16, 84);
            this.grpCity.Size = new System.Drawing.Size(590, 100);
            this.grpCity.Controls.Add(this.lblCity);
            this.grpCity.Controls.Add(this.lblTraffic);
            this.grpCity.Controls.Add(this.nudTraffic);
            this.grpCity.Controls.Add(this.lblEnv);
            this.grpCity.Controls.Add(this.nudEnv);
            this.grpCity.Controls.Add(this.lblFac);
            this.grpCity.Controls.Add(this.nudFac);
            this.grpCity.Controls.Add(this.lblPol);
            this.grpCity.Controls.Add(this.nudPol);
            this.lblCity.Location = new System.Drawing.Point(16, 22);
            this.lblCity.Size = new System.Drawing.Size(550, 18);
            this.lblCity.Text = "城市配置：";
            this.lblTraffic.AutoSize = true;
            this.lblTraffic.Location = new System.Drawing.Point(16, 52);
            this.lblTraffic.Text = "交通：";
            this.nudTraffic.Location = new System.Drawing.Point(60, 48);
            this.nudTraffic.Maximum = 100;
            this.nudTraffic.Value = 30;
            this.lblEnv.AutoSize = true;
            this.lblEnv.Location = new System.Drawing.Point(150, 52);
            this.lblEnv.Text = "环境：";
            this.nudEnv.Location = new System.Drawing.Point(194, 48);
            this.nudEnv.Maximum = 100;
            this.nudEnv.Value = 20;
            this.lblFac.AutoSize = true;
            this.lblFac.Location = new System.Drawing.Point(286, 52);
            this.lblFac.Text = "设施：";
            this.nudFac.Location = new System.Drawing.Point(330, 48);
            this.nudFac.Maximum = 100;
            this.nudFac.Value = 25;
            this.lblPol.AutoSize = true;
            this.lblPol.Location = new System.Drawing.Point(420, 52);
            this.lblPol.Text = "政策：";
            this.nudPol.Location = new System.Drawing.Point(464, 48);
            this.nudPol.Maximum = 100;
            this.nudPol.Value = 25;
            this.grpSkin.Text = "界面皮肤 (DevExpress 13.1)";
            this.grpSkin.Location = new System.Drawing.Point(16, 196);
            this.grpSkin.Size = new System.Drawing.Size(590, 60);
            this.grpSkin.Controls.Add(this.cboSkin);
            this.grpSkin.Controls.Add(this.btnApplySkin);
            this.cboSkin.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSkin.Location = new System.Drawing.Point(16, 24);
            this.cboSkin.Size = new System.Drawing.Size(220, 20);
            this.btnApplySkin.Location = new System.Drawing.Point(250, 22);
            this.btnApplySkin.Size = new System.Drawing.Size(100, 25);
            this.btnApplySkin.Text = "应用皮肤";
            this.btnApplySkin.Click += new System.EventHandler(this.btnApplySkin_Click);
            this.lblPlugins.AutoSize = true;
            this.lblPlugins.Location = new System.Drawing.Point(16, 270);
            this.lblPlugins.Text = "plugins.config（只读预览）：";
            this.txtPlugins.Location = new System.Drawing.Point(16, 290);
            this.txtPlugins.Multiline = true;
            this.txtPlugins.ReadOnly = true;
            this.txtPlugins.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPlugins.Size = new System.Drawing.Size(590, 90);
            this.txtPlugins.Font = new System.Drawing.Font("Consolas", 9F);
            this.btnSave.Location = new System.Drawing.Point(420, 396);
            this.btnSave.Size = new System.Drawing.Size(90, 30);
            this.btnSave.Text = "保存";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            this.btnClose.Location = new System.Drawing.Point(516, 396);
            this.btnClose.Size = new System.Drawing.Size(86, 30);
            this.btnClose.Text = "关闭";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            this.ClientSize = new System.Drawing.Size(624, 444);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtPlugins);
            this.Controls.Add(this.lblPlugins);
            this.Controls.Add(this.grpSkin);
            this.Controls.Add(this.grpCity);
            this.Controls.Add(this.grpOverlay);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SystemConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "权重与皮肤配置";
            this.grpOverlay.ResumeLayout(false);
            this.grpOverlay.PerformLayout();
            this.grpCity.ResumeLayout(false);
            this.grpCity.PerformLayout();
            this.grpSkin.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nudMotivW)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFeasibW)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTraffic)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEnv)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFac)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPol)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.GroupBox grpOverlay;
        private System.Windows.Forms.Label lblMotivW;
        private System.Windows.Forms.NumericUpDown nudMotivW;
        private System.Windows.Forms.Label lblFeasibW;
        private System.Windows.Forms.NumericUpDown nudFeasibW;
        private System.Windows.Forms.GroupBox grpCity;
        private System.Windows.Forms.Label lblCity;
        private System.Windows.Forms.Label lblTraffic;
        private System.Windows.Forms.NumericUpDown nudTraffic;
        private System.Windows.Forms.Label lblEnv;
        private System.Windows.Forms.NumericUpDown nudEnv;
        private System.Windows.Forms.Label lblFac;
        private System.Windows.Forms.NumericUpDown nudFac;
        private System.Windows.Forms.Label lblPol;
        private System.Windows.Forms.NumericUpDown nudPol;
        private System.Windows.Forms.GroupBox grpSkin;
        private System.Windows.Forms.ComboBox cboSkin;
        private System.Windows.Forms.Button btnApplySkin;
        private System.Windows.Forms.Label lblPlugins;
        private System.Windows.Forms.TextBox txtPlugins;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClose;
    }
}

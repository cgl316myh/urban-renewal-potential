using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using UrbanRenewal.Contracts;
using UrbanRenewal.Model;

namespace UrbanRenewal.Plugins.Config
{
    public partial class SystemConfigForm : Form
    {
        private readonly IAppContext _context;
        private CityProfile _profile;

        public SystemConfigForm()
        {
            InitializeComponent();
        }

        public SystemConfigForm(IAppContext context)
            : this()
        {
            _context = context;
            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
            {
                LoadData();
            }
        }

        private void LoadData()
        {
            if (_context == null)
            {
                return;
            }
            _context.ReloadGlobalSettings();
            this.nudMotivW.Value = ToPercent(_context.MotivationWeight);
            this.nudFeasibW.Value = ToPercent(_context.FeasibilityWeight);

            this.cboSkin.Items.Clear();
            string[] skins = new string[]
            {
                "Office 2013", "Office 2010 Blue", "Office 2010 Silver", "Office 2010 Black",
                "DevExpress Style", "Caramel", "Money Twins", "Lilian", "The Asphalt World"
            };
            for (int i = 0; i < skins.Length; i++)
            {
                this.cboSkin.Items.Add(skins[i]);
            }
            string cur = _context.SkinName ?? "Office 2013";
            int idx = this.cboSkin.Items.IndexOf(cur);
            this.cboSkin.SelectedIndex = idx >= 0 ? idx : 0;

            _profile = CityProfileStore.ResolveActive(_context.ActiveCityProfileId);
            if (_profile != null)
            {
                this.lblCity.Text = "城市配置：" + _profile.DisplayName + " [" + _profile.Id + "]";
                this.nudTraffic.Value = ToPercent(_profile.TrafficWeight);
                this.nudEnv.Value = ToPercent(_profile.EnvironmentWeight);
                this.nudFac.Value = ToPercent(_profile.FacilityWeight);
                this.nudPol.Value = ToPercent(_profile.PolicyWeight);
            }
            else
            {
                this.lblCity.Text = "城市配置：（未设置）";
            }

            string pluginsCfg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins.config");
            this.txtPlugins.Text = File.Exists(pluginsCfg)
                ? File.ReadAllText(pluginsCfg, Encoding.UTF8)
                : "（未找到 plugins.config）";
        }

        private static decimal ToPercent(double w)
        {
            double p = w <= 1.0001 ? w * 100.0 : w;
            if (p < 0)
            {
                p = 0;
            }
            if (p > 100)
            {
                p = 100;
            }
            return (decimal)Math.Round(p, 1);
        }

        private void btnApplySkin_Click(object sender, EventArgs e)
        {
            if (_context == null || this.cboSkin.SelectedItem == null)
            {
                return;
            }
            string skin = this.cboSkin.SelectedItem.ToString();
            _context.ApplySkin(skin);
            _context.SaveGlobalSettings();
            MessageBox.Show(this, "皮肤已应用并保存: " + skin, "系统配置", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_context == null)
            {
                return;
            }
            _context.MotivationWeight = (double)this.nudMotivW.Value / 100.0;
            _context.FeasibilityWeight = (double)this.nudFeasibW.Value / 100.0;
            if (this.cboSkin.SelectedItem != null)
            {
                _context.SkinName = this.cboSkin.SelectedItem.ToString();
                _context.ApplySkin(_context.SkinName);
            }
            _context.SaveGlobalSettings();

            if (_profile != null && !string.IsNullOrEmpty(_profile.SourcePath))
            {
                _profile.TrafficWeight = (double)this.nudTraffic.Value / 100.0;
                _profile.EnvironmentWeight = (double)this.nudEnv.Value / 100.0;
                _profile.FacilityWeight = (double)this.nudFac.Value / 100.0;
                _profile.PolicyWeight = (double)this.nudPol.Value / 100.0;
                CityProfileStore.NormalizeWeights(_profile);
                CityProfileStore.Save(_profile, _profile.SourcePath);
                _context.LogInfo("城市准则层权重已保存: " + _profile.SourcePath);
            }

            MessageBox.Show(this, "配置已保存。叠置权重立即生效；准则层权重在下次动力性分析时生效。",
                "系统配置", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

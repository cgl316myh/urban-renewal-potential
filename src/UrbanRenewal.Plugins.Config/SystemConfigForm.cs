using System;
using System.ComponentModel;
using System.Windows.Forms;
using UrbanRenewal.Contracts;
using UrbanRenewal.Model;

namespace UrbanRenewal.Plugins.Config
{
    public partial class SystemConfigForm : Form
    {
        private readonly IAppContext _context;
        private CityProfile _profile;
        private bool _loadingRules;

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
                if (_profile.BufferScoreRules == null)
                {
                    _profile.BufferScoreRules = BufferScoreRules.CreateOriginal();
                }
                LoadBufferRulesToUi(_profile.BufferScoreRules);
            }
            else
            {
                this.lblCity.Text = "城市配置：（未设置）";
                this.grpBuffer.Enabled = false;
            }
        }

        private void LoadBufferRulesToUi(BufferScoreRules rules)
        {
            _loadingRules = true;
            try
            {
                this.cboMetroPreset.Items.Clear();
                this.cboMetroPreset.Items.Add("Original - 现状(地铁偏强)");
                this.cboMetroPreset.Items.Add("A - 温和");
                this.cboMetroPreset.Items.Add("B - 推荐(加分项)");
                this.cboMetroPreset.Items.Add("C - 激进");
                this.cboMetroPreset.Items.Add("Custom - 自定义");

                string preset = rules.MetroPreset ?? "Original";
                int sel = 0;
                if (string.Equals(preset, "A", StringComparison.OrdinalIgnoreCase)) sel = 1;
                else if (string.Equals(preset, "B", StringComparison.OrdinalIgnoreCase)) sel = 2;
                else if (string.Equals(preset, "C", StringComparison.OrdinalIgnoreCase)) sel = 3;
                else if (string.Equals(preset, "Custom", StringComparison.OrdinalIgnoreCase)) sel = 4;
                this.cboMetroPreset.SelectedIndex = sel;

                this.txtMultiDist.Text = rules.MetroMulti != null ? (rules.MetroMulti.Distances ?? "") : "";
                this.txtMultiScore.Text = rules.MetroMulti != null ? (rules.MetroMulti.Scores ?? "") : "";
                this.txtSingleDist.Text = rules.MetroSingle != null ? (rules.MetroSingle.Distances ?? "") : "";
                this.txtSingleScore.Text = rules.MetroSingle != null ? (rules.MetroSingle.Scores ?? "") : "";

                ApplySingleRing(rules.Cbd, this.nudCbdDist, this.nudCbdScore, 1000, 3);
                ApplySingleRing(rules.TrafficFacility, this.nudTrafDist, this.nudTrafScore, 300, 1);
                ApplySingleRing(rules.EcoCorridor, this.nudEcoDist, this.nudEcoScore, 500, 2);
                ApplySingleRing(rules.OpenSpace, this.nudOpenDist, this.nudOpenScore, 500, 2);
                ApplySingleRing(rules.Green, this.nudGreenDist, this.nudGreenScore, 300, 1);
                ApplySingleRing(rules.PublicService, this.nudPubDist, this.nudPubScore, 1000, 2);
                ApplySingleRing(rules.Convenience, this.nudConvDist, this.nudConvScore, 300, 1);
                ApplySingleRing(rules.Commercial, this.nudShopDist, this.nudShopScore, 1000, 1);

                this.nudPolBeltScore.Value = ClampDec(
                    rules.PolicyBelt != null ? rules.PolicyBelt.Score : 1,
                    this.nudPolBeltScore.Minimum, this.nudPolBeltScore.Maximum);
                this.nudPolStrategyScore.Value = ClampDec(
                    rules.PolicyStrategy != null ? rules.PolicyStrategy.Score : 1,
                    this.nudPolStrategyScore.Minimum, this.nudPolStrategyScore.Maximum);
                this.nudPolKeyScore.Value = ClampDec(
                    rules.PolicyKey != null ? rules.PolicyKey.Score : 2,
                    this.nudPolKeyScore.Minimum, this.nudPolKeyScore.Maximum);
            }
            finally
            {
                _loadingRules = false;
            }
        }

        private void ApplySingleRing(
            SingleRingRule rule,
            NumericUpDown nudDist,
            NumericUpDown nudScore,
            double defaultDist,
            int defaultScore)
        {
            double dist = rule != null ? rule.Distance : defaultDist;
            int score = rule != null ? rule.Score : defaultScore;
            nudDist.Value = ClampDec((decimal)dist, nudDist.Minimum, nudDist.Maximum);
            nudScore.Value = ClampDec(score, nudScore.Minimum, nudScore.Maximum);
        }

        private static decimal ClampDec(decimal v, decimal min, decimal max)
        {
            if (v < min) return min;
            if (v > max) return max;
            return v;
        }

        private static decimal ToPercent(double w)
        {
            double p = w <= 1.0001 ? w * 100.0 : w;
            if (p < 0) p = 0;
            if (p > 100) p = 100;
            return (decimal)Math.Round(p, 1);
        }

        private void cboMetroPreset_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingRules || this.cboMetroPreset.SelectedIndex < 0)
            {
                return;
            }
            int idx = this.cboMetroPreset.SelectedIndex;
            if (idx == 4)
            {
                return;
            }
            BufferScoreRules preset;
            if (idx == 1) preset = BufferScoreRules.CreatePresetA();
            else if (idx == 2) preset = BufferScoreRules.CreatePresetB();
            else if (idx == 3) preset = BufferScoreRules.CreatePresetC();
            else preset = BufferScoreRules.CreateOriginal();

            // 预设只改地铁，保留其余因子当前界面/配置值
            BufferScoreRules keep = CollectNonMetroFromUiOrProfile();
            preset.Cbd = keep.Cbd;
            preset.TrafficFacility = keep.TrafficFacility;
            preset.EcoCorridor = keep.EcoCorridor;
            preset.OpenSpace = keep.OpenSpace;
            preset.Green = keep.Green;
            preset.PublicService = keep.PublicService;
            preset.Convenience = keep.Convenience;
            preset.Commercial = keep.Commercial;
            preset.PolicyBelt = keep.PolicyBelt;
            preset.PolicyStrategy = keep.PolicyStrategy;
            preset.PolicyKey = keep.PolicyKey;

            LoadBufferRulesToUi(preset);
        }

        private BufferScoreRules CollectNonMetroFromUiOrProfile()
        {
            BufferScoreRules r = new BufferScoreRules();
            r.Cbd = SingleRingRule.Create((double)this.nudCbdDist.Value, (int)this.nudCbdScore.Value);
            r.TrafficFacility = SingleRingRule.Create((double)this.nudTrafDist.Value, (int)this.nudTrafScore.Value);
            r.EcoCorridor = SingleRingRule.Create((double)this.nudEcoDist.Value, (int)this.nudEcoScore.Value);
            r.OpenSpace = SingleRingRule.Create((double)this.nudOpenDist.Value, (int)this.nudOpenScore.Value);
            r.Green = SingleRingRule.Create((double)this.nudGreenDist.Value, (int)this.nudGreenScore.Value);
            r.PublicService = SingleRingRule.Create((double)this.nudPubDist.Value, (int)this.nudPubScore.Value);
            r.Convenience = SingleRingRule.Create((double)this.nudConvDist.Value, (int)this.nudConvScore.Value);
            r.Commercial = SingleRingRule.Create((double)this.nudShopDist.Value, (int)this.nudShopScore.Value);
            r.PolicyBelt = PolygonScoreRule.Create((int)this.nudPolBeltScore.Value);
            r.PolicyStrategy = PolygonScoreRule.Create((int)this.nudPolStrategyScore.Value);
            r.PolicyKey = PolygonScoreRule.Create((int)this.nudPolKeyScore.Value);
            return r;
        }

        private void BufferRule_TextChanged(object sender, EventArgs e)
        {
            if (_loadingRules || this.cboMetroPreset.Items.Count == 0)
            {
                return;
            }
            if (this.cboMetroPreset.SelectedIndex != 4)
            {
                _loadingRules = true;
                try
                {
                    this.cboMetroPreset.SelectedIndex = 4;
                }
                finally
                {
                    _loadingRules = false;
                }
            }
        }

        private BufferScoreRules CollectBufferRulesFromUi()
        {
            BufferScoreRules baseRules = BufferScoreRules.CreateOriginal();

            int idx = this.cboMetroPreset.SelectedIndex;
            if (idx == 0) baseRules.MetroPreset = "Original";
            else if (idx == 1) baseRules.MetroPreset = "A";
            else if (idx == 2) baseRules.MetroPreset = "B";
            else if (idx == 3) baseRules.MetroPreset = "C";
            else baseRules.MetroPreset = "Custom";

            baseRules.MetroMulti = new MultiRingRule();
            baseRules.MetroMulti.Distances = (this.txtMultiDist.Text ?? "").Trim();
            baseRules.MetroMulti.Scores = (this.txtMultiScore.Text ?? "").Trim();
            baseRules.MetroSingle = new MultiRingRule();
            baseRules.MetroSingle.Distances = (this.txtSingleDist.Text ?? "").Trim();
            baseRules.MetroSingle.Scores = (this.txtSingleScore.Text ?? "").Trim();

            baseRules.Cbd = SingleRingRule.Create((double)this.nudCbdDist.Value, (int)this.nudCbdScore.Value);
            baseRules.TrafficFacility = SingleRingRule.Create((double)this.nudTrafDist.Value, (int)this.nudTrafScore.Value);
            baseRules.EcoCorridor = SingleRingRule.Create((double)this.nudEcoDist.Value, (int)this.nudEcoScore.Value);
            baseRules.OpenSpace = SingleRingRule.Create((double)this.nudOpenDist.Value, (int)this.nudOpenScore.Value);
            baseRules.Green = SingleRingRule.Create((double)this.nudGreenDist.Value, (int)this.nudGreenScore.Value);
            baseRules.PublicService = SingleRingRule.Create((double)this.nudPubDist.Value, (int)this.nudPubScore.Value);
            baseRules.Convenience = SingleRingRule.Create((double)this.nudConvDist.Value, (int)this.nudConvScore.Value);
            baseRules.Commercial = SingleRingRule.Create((double)this.nudShopDist.Value, (int)this.nudShopScore.Value);
            baseRules.PolicyBelt = PolygonScoreRule.Create((int)this.nudPolBeltScore.Value);
            baseRules.PolicyStrategy = PolygonScoreRule.Create((int)this.nudPolStrategyScore.Value);
            baseRules.PolicyKey = PolygonScoreRule.Create((int)this.nudPolKeyScore.Value);
            return baseRules;
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
                _profile.BufferScoreRules = CollectBufferRulesFromUi();
                CityProfileStore.NormalizeWeights(_profile);
                CityProfileStore.Save(_profile, _profile.SourcePath);
                _context.LogInfo("城市准则层权重与缓冲规则已保存: " + _profile.SourcePath
                    + " | " + _profile.BufferScoreRules.DescribeMetro());
            }

            MessageBox.Show(this,
                "配置已保存。\r\n叠置权重立即生效；准则层权重与缓冲/得分规则在下次「运行动力性分析」时生效。",
                "系统配置", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

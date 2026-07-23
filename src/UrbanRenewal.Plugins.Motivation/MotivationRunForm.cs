using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using UrbanRenewal.Analysis;
using UrbanRenewal.Contracts;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Plugins.Motivation
{
    /// <summary>
    /// 动力性分析运行窗体：使用全局输出 GDB / 城市配置。
    /// </summary>
    public partial class MotivationRunForm : Form
    {
        private readonly IAppContext _context;

        public MotivationRunForm()
        {
            InitializeComponent();
        }

        public MotivationRunForm(IAppContext context)
            : this()
        {
            _context = context;
            if (!IsDesignModeSafe())
            {
                RefreshGlobalInfo();
            }
        }

        private static bool IsDesignModeSafe()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }

        private void RefreshGlobalInfo()
        {
            if (_context == null)
            {
                return;
            }
            _context.ReloadGlobalSettings();

            string outGdb = _context.OutputGdbPath;
            this.lblOutInfo.Text = "输出 GDB："
                + (string.IsNullOrEmpty(outGdb) ? "（未设置 — 请先在全局设置中指定）" : outGdb);

            CityProfile profile = CityProfileStore.ResolveActive(_context.ActiveCityProfileId);
            if (profile != null)
            {
                CityProfileStore.NormalizeWeights(profile);
                this.lblCityInfo.Text = "城市配置：" + profile.DisplayName + " [" + profile.Id + "]";
                ApplyProfileToUi(profile);
            }
            else
            {
                this.lblCityInfo.Text = "城市配置：（未设置 — 可在全局设置中选择）";
            }
        }

        private void ApplyProfileToUi(CityProfile profile)
        {
            if (profile == null)
            {
                return;
            }
            if (profile.CellSize >= (double)this.nudCellSize.Minimum
                && profile.CellSize <= (double)this.nudCellSize.Maximum)
            {
                this.nudCellSize.Value = (decimal)profile.CellSize;
            }
            SetWeightPercent(this.nudTraffic, profile.TrafficWeight);
            SetWeightPercent(this.nudEnvironment, profile.EnvironmentWeight);
            SetWeightPercent(this.nudFacility, profile.FacilityWeight);
            SetWeightPercent(this.nudPolicy, profile.PolicyWeight);
        }

        private static void SetWeightPercent(NumericUpDown nud, double weight01)
        {
            double p = weight01;
            if (p <= 1.0)
            {
                p = p * 100.0;
            }
            if (p < (double)nud.Minimum)
            {
                p = (double)nud.Minimum;
            }
            if (p > (double)nud.Maximum)
            {
                p = (double)nud.Maximum;
            }
            nud.Value = (decimal)p;
        }

        private void btnOpenGlobal_Click(object sender, EventArgs e)
        {
            if (_context == null)
            {
                return;
            }
            _context.ShowGlobalSettings();
            RefreshGlobalInfo();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (_context == null)
            {
                MessageBox.Show(this, "运行上下文无效。", "动力性分析", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _context.ReloadGlobalSettings();
            string gdb = _context.GdbPath;
            string outGdb = _context.OutputGdbPath;

            if (string.IsNullOrEmpty(gdb) || !System.IO.Directory.Exists(gdb))
            {
                MessageBox.Show(this, "请先在「数据管理 → 打开 GDB」中指定输入工作空间。",
                    "动力性分析", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(outGdb) || !outGdb.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "请先在「数据管理 → 全局设置」中指定输出 File GDB。\r\n中间数据与结果均写入该库。",
                    "动力性分析", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MotivationJob job = new MotivationJob();
            job.GdbPath = gdb;
            job.OutputGdbPath = outGdb;
            job.CellSize = (double)this.nudCellSize.Value;
            job.TrafficWeight = (double)this.nudTraffic.Value / 100.0;
            job.EnvironmentWeight = (double)this.nudEnvironment.Value / 100.0;
            job.FacilityWeight = (double)this.nudFacility.Value / 100.0;
            job.PolicyWeight = (double)this.nudPolicy.Value / 100.0;

            List<string> applyMsgs = new List<string>();
            CityProfile profile = CityProfileStore.ResolveActive(_context.ActiveCityProfileId);
            List<string> names = WorkspaceCatalog.ListFeatureClassNames(gdb);
            if (profile != null)
            {
                string reqMsg;
                if (!profile.ValidateRequired(names, out reqMsg))
                {
                    MessageBox.Show(this,
                        "城市配置「" + profile.DisplayName + "」必选图层未齐备，已取消分析。\r\n\r\n" + reqMsg,
                        "动力性分析", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                profile.ApplyToJob(job, names, applyMsgs);
            }

            List<string> usedLayers = SpatialReferenceAudit.CollectMotivationLayerNames(job.LayerHints, names);
            SpatialReferenceAuditResult srAudit = usedLayers.Count > 0
                ? SpatialReferenceAudit.Audit(gdb, usedLayers)
                : SpatialReferenceAudit.Audit(gdb);
            if (!srAudit.Success || !srAudit.IsUnified)
            {
                string block = srAudit.ToBlockMessage();
                this.lblStatus.Text = "空间参考不统一";
                _context.LogError(block);
                MessageBox.Show(this, block, "动力性分析", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.btnRun.Enabled = false;
            this.lblStatus.Text = "正在分析...";
            Application.DoEvents();

            try
            {
                MotivationAnalysisEngine engine = new MotivationAnalysisEngine();
                MotivationResult result = engine.Run(job, OnProgress);

                // 引擎可能规范化了输出路径，回写全局
                if (result.Success && !string.IsNullOrEmpty(result.OutputGdbPath))
                {
                    _context.OutputGdbPath = result.OutputGdbPath;
                    _context.SaveGlobalSettings();
                }

                StringBuilder sb = new StringBuilder();
                if (profile != null)
                {
                    sb.AppendLine("城市配置: " + profile.DisplayName);
                }
                sb.AppendLine("输出 GDB: " + (result.OutputGdbPath ?? outGdb));
                for (int i = 0; i < applyMsgs.Count; i++)
                {
                    sb.AppendLine(applyMsgs[i]);
                }
                for (int i = 0; i < result.Messages.Count; i++)
                {
                    sb.AppendLine(result.Messages[i]);
                    _context.LogInfo(result.Messages[i]);
                }

                if (result.Success && !string.IsNullOrEmpty(result.MotivationRasterPath))
                {
                    string msg = string.Empty;
                    if (_context.AddRasterLayer(result.MotivationRasterPath, "动力性得分", out msg))
                    {
                        sb.AppendLine(msg);
                        _context.ZoomToFullExtent();
                    }
                    else
                    {
                        sb.AppendLine(msg);
                    }

                    foreach (KeyValuePair<string, string> kv in result.CriterionRasters)
                    {
                        string m2 = string.Empty;
                        _context.AddRasterLayer(kv.Value, kv.Key, out m2);
                    }
                }

                this.lblStatus.Text = result.Success ? "完成" : "失败";
                MessageBox.Show(this, sb.ToString(), "动力性分析", MessageBoxButtons.OK,
                    result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                this.lblStatus.Text = "异常";
                _context.LogError(ex.Message);
                MessageBox.Show(this, ex.Message, "动力性分析失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.btnRun.Enabled = true;
                _context.HideProgress();
                RefreshGlobalInfo();
            }
        }

        private void OnProgress(string text, int percent)
        {
            this.lblStatus.Text = text + "  " + percent + "%";
            if (_context != null)
            {
                _context.ShowProgress(text, percent);
            }
            Application.DoEvents();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

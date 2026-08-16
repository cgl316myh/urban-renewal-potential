using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using UrbanRenewal.Analysis;
using UrbanRenewal.Contracts;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Plugins.Motivation
{
    /// <summary>
    /// 动力性分析运行窗体：使用全局输出 GDB / 城市配置。
    /// 分析在 STA 后台线程执行，避免界面假死。
    /// </summary>
    public partial class MotivationRunForm : Form
    {
        private readonly IAppContext _context;
        private bool _busy;
        private StaBackgroundRunner.ProgressUiGate _progressGate;

        public MotivationRunForm()
        {
            InitializeComponent();
        }

        public MotivationRunForm(IAppContext context)
            : this()
        {
            _context = context;
            _progressGate = new StaBackgroundRunner.ProgressUiGate(this, ApplyProgressUi);
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

            CityProfile profile = CityProfileStore.ResolveActive(_context.ActiveCityProfileId);
            if (profile != null)
            {
                CityProfileStore.NormalizeWeights(profile);
                ApplyProfileToUi(profile);
            }
        }

        private void ApplyProfileToUi(CityProfile profile)
        {
            if (profile == null)
            {
                return;
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

        private void SetBusy(bool busy)
        {
            _busy = busy;
            this.btnRun.Enabled = !busy;
            this.btnClose.Enabled = !busy;
            this.nudTraffic.Enabled = !busy;
            this.nudEnvironment.Enabled = !busy;
            this.nudFacility.Enabled = !busy;
            this.nudPolicy.Enabled = !busy;
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (_busy)
            {
                return;
            }
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
                MessageBox.Show(this, "请先在「数据管理 → 全局设置」中指定输入 GDB。",
                    "动力性分析", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(outGdb) || !outGdb.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "请先在「数据管理 → 全局设置」中指定输出 File GDB。\r\n中间数据与结果均写入该库。",
                    "动力性分析", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 输入与输出同库时改用旁路工作库（正常：输入=clip.gdb，输出=分析库）
            string sameNote;
            outGdb = OutputGdbHelper.EnsureSeparateAnalysisOutput(gdb, outGdb, out sameNote);
            if (!string.IsNullOrEmpty(sameNote))
            {
                _context.LogInfo(sameNote);
                _context.OutputGdbPath = outGdb;
                _context.SaveGlobalSettings();
            }

            MotivationJob job = new MotivationJob();
            job.GdbPath = gdb;
            job.OutputGdbPath = outGdb;
            job.CellSize = _context.CellSize;
            job.TrafficWeight = (double)this.nudTraffic.Value / 100.0;
            job.EnvironmentWeight = (double)this.nudEnvironment.Value / 100.0;
            job.FacilityWeight = (double)this.nudFacility.Value / 100.0;
            job.PolicyWeight = (double)this.nudPolicy.Value / 100.0;

            string cityId = _context.ActiveCityProfileId;
            string srSource = _context.SpatialRefSourcePath;
            string srLayer = _context.SpatialRefLayerName;

            SetBusy(true);
            _context.LogInfo("======== 开始动力性分析 ========");
            _context.LogInfo("输入 GDB: " + gdb);
            _context.LogInfo("输出 GDB: " + outGdb);
            _context.LogInfo("像元大小: " + job.CellSize + " 米");

            // 上次结果栅格若仍在地图上，会锁住 traf10mx 等 VAT，导致 CellStatistics 000871
            int removed = _context.RemoveMapLayersFromGdb(outGdb);
            if (removed > 0)
            {
                _context.LogInfo("已从地图移除输出库相关图层 " + removed + " 个（释放 schema lock）");
            }

            StaBackgroundRunner.Run(
                this,
                delegate
                {
                    List<string> applyMsgs = new List<string>();
                    CityProfile profile = CityProfileStore.ResolveActive(cityId);
                    string profileDisplay = profile != null ? profile.DisplayName : null;
                    List<string> names = WorkspaceCatalog.ListFeatureClassNames(gdb);
                    if (profile != null)
                    {
                        string reqMsg;
                        if (!profile.ValidateRequired(names, out reqMsg))
                        {
                            throw new InvalidOperationException(
                                "城市配置「" + profile.DisplayName + "」必选图层未齐备。\r\n\r\n" + reqMsg);
                        }
                        profile.ApplyToJob(job, names, applyMsgs);
                    }

                    List<string> usedLayers = SpatialReferenceAudit.CollectMotivationLayerNames(job.LayerHints, names);
                    SpatialReferenceAuditResult srAudit = usedLayers.Count > 0
                        ? SpatialReferenceAudit.Audit(gdb, usedLayers, srSource, srLayer)
                        : SpatialReferenceAudit.Audit(gdb, null, srSource, srLayer);
                    if (!srAudit.Success || !srAudit.IsUnified)
                    {
                        throw new InvalidOperationException(srAudit.ToBlockMessage());
                    }

                    MotivationAnalysisEngine engine = new MotivationAnalysisEngine();
                    MotivationResult result = engine.Run(job, OnProgress);
                    return new MotivationWorkPack
                    {
                        Result = result,
                        ApplyMsgs = applyMsgs,
                        ProfileDisplay = profileDisplay,
                        OutGdb = outGdb
                    };
                },
                FinishOk,
                FinishError);
        }

        private void FinishOk(MotivationWorkPack pack)
        {
            MotivationResult result = pack != null ? pack.Result : null;
            if (result == null)
            {
                EndBusy();
                return;
            }
            List<string> applyMsgs = pack.ApplyMsgs ?? new List<string>();
            try
            {
                if (result.Success && !string.IsNullOrEmpty(result.OutputGdbPath))
                {
                    _context.OutputGdbPath = result.OutputGdbPath;
                    _context.SaveGlobalSettings();
                }

                if (!string.IsNullOrEmpty(pack.ProfileDisplay))
                {
                    _context.LogInfo("城市配置: " + pack.ProfileDisplay);
                }
                _context.LogInfo("输出 GDB: " + (result.OutputGdbPath ?? pack.OutGdb));
                for (int i = 0; i < applyMsgs.Count; i++)
                {
                    _context.LogInfo(applyMsgs[i]);
                }
                for (int i = 0; i < result.Messages.Count; i++)
                {
                    _context.LogInfo(result.Messages[i]);
                }
                _context.LogInfo(result.Success
                    ? "======== 动力性分析完成 ========"
                    : "======== 动力性分析失败 ========");

                if (result.Success && !string.IsNullOrEmpty(result.MotivationRasterPath))
                {
                    string msg = string.Empty;
                    if (_context.AddRasterLayer(result.MotivationRasterPath, "动力性得分", out msg))
                    {
                        _context.LogInfo(msg);
                        _context.ZoomToFullExtent();
                    }
                    else
                    {
                        _context.LogInfo(msg);
                    }

                    foreach (KeyValuePair<string, string> kv in result.CriterionRasters)
                    {
                        string m2 = string.Empty;
                        _context.AddRasterLayer(kv.Value, kv.Key, out m2);
                    }
                }

                if (!result.Success)
                {
                    MessageBox.Show(this, string.Join("\r\n", result.Messages.ToArray()),
                        "动力性分析", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                EndBusy();
            }
        }

        private void FinishError(Exception ex)
        {
            try
            {
                string msg = ex != null ? ex.Message : "未知错误";
                _context.LogError(msg);
                MessageBox.Show(this, msg, "动力性分析失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                EndBusy();
            }
        }

        private void EndBusy()
        {
            SetBusy(false);
            if (_context != null)
            {
                _context.HideProgress();
            }
            RefreshGlobalInfo();
        }

        private void OnProgress(string text, int percent)
        {
            if (_context != null)
            {
                _context.LogInfo("[" + percent + "%] " + (text ?? string.Empty));
            }
            if (_progressGate != null)
            {
                _progressGate.Report(text, percent);
            }
        }

        private void ApplyProgressUi(string text, int percent)
        {
            if (IsDisposed)
            {
                return;
            }
            if (_context != null)
            {
                _context.ShowProgress(text, percent);
            }
        }

        private sealed class MotivationWorkPack
        {
            public MotivationResult Result;
            public List<string> ApplyMsgs;
            public string ProfileDisplay;
            public string OutGdb;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_busy)
            {
                MessageBox.Show(this, "动力性分析正在后台执行，请等待完成后再关闭。",
                    "动力性分析", MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
                return;
            }
            base.OnFormClosing(e);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using UrbanRenewal.Analysis;
using UrbanRenewal.Contracts;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Plugins.Feasibility
{
    /// <summary>
    /// 可行度分析运行窗体：使用全局输出 GDB / 城市配置。
    /// 分析在 STA 后台线程执行，避免界面假死。
    /// </summary>
    public partial class FeasibilityRunForm : Form
    {
        private readonly IAppContext _context;
        private bool _busy;
        private StaBackgroundRunner.ProgressUiGate _progressGate;

        public FeasibilityRunForm()
        {
            InitializeComponent();
        }

        public FeasibilityRunForm(IAppContext context)
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

            string outGdb = _context.OutputGdbPath;
            this.lblOutInfo.Text = "输出 GDB："
                + (string.IsNullOrEmpty(outGdb) ? "（未设置 — 请先在全局设置中指定）" : outGdb);

            CityProfile profile = CityProfileStore.ResolveActive(_context.ActiveCityProfileId);
            if (profile != null)
            {
                this.lblCityInfo.Text = "城市配置：" + profile.DisplayName + " [" + profile.Id + "]";
                if (profile.CellSize >= (double)this.nudCellSize.Minimum
                    && profile.CellSize <= (double)this.nudCellSize.Maximum)
                {
                    this.nudCellSize.Value = (decimal)profile.CellSize;
                }
            }
            else
            {
                this.lblCityInfo.Text = "城市配置：（未设置 — 可在全局设置中选择）";
            }
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            this.btnRun.Enabled = !busy;
            this.btnClose.Enabled = !busy;
            this.btnOpenGlobal.Enabled = !busy;
            this.nudCellSize.Enabled = !busy;
            this.nudElevThr.Enabled = !busy;
            this.nudSlopeThr.Enabled = !busy;
        }

        private void btnOpenGlobal_Click(object sender, EventArgs e)
        {
            if (_context == null || _busy)
            {
                return;
            }
            _context.ShowGlobalSettings();
            RefreshGlobalInfo();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (_busy)
            {
                return;
            }
            if (_context == null)
            {
                MessageBox.Show(this, "运行上下文无效。", "可行度分析", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _context.ReloadGlobalSettings();
            string gdb = _context.GdbPath;
            string outGdb = _context.OutputGdbPath;

            if (string.IsNullOrEmpty(gdb) || !System.IO.Directory.Exists(gdb))
            {
                MessageBox.Show(this, "请先在「数据管理 → 全局设置」中指定输入 GDB。",
                    "可行度分析", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(outGdb) || !outGdb.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "请先在「数据管理 → 全局设置」中指定输出 File GDB。\r\n中间数据与结果均写入该库。",
                    "可行度分析", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FeasibilityJob job = new FeasibilityJob();
            job.GdbPath = gdb;
            job.OutputGdbPath = outGdb;
            job.CellSize = (double)this.nudCellSize.Value;
            job.ElevationThreshold = (double)this.nudElevThr.Value;
            job.SlopeThresholdDegrees = (double)this.nudSlopeThr.Value;

            string cityId = _context.ActiveCityProfileId;
            string srSource = _context.SpatialRefSourcePath;
            string srLayer = _context.SpatialRefLayerName;

            SetBusy(true);
            this.lblStatus.Text = "准备中（后台）...";
            _context.LogInfo("======== 开始可行度分析 ========");

            StaBackgroundRunner.Run(
                this,
                delegate
                {
                    List<string> applyMsgs = new List<string>();
                    CityProfile profile = CityProfileStore.ResolveActive(cityId);
                    string profileDisplay = profile != null ? profile.DisplayName : null;
                    List<string> names = WorkspaceCatalog.ListFeatureClassNames(gdb);
                    List<string> rasters = WorkspaceCatalog.ListRasterDatasetNames(gdb);
                    if (profile != null)
                    {
                        string reqMsg;
                        if (!profile.ValidateRequired(names, out reqMsg))
                        {
                            throw new InvalidOperationException(
                                "城市配置「" + profile.DisplayName + "」必选图层未齐备。\r\n\r\n" + reqMsg);
                        }
                        profile.ApplyToFeasibilityJob(job, names, rasters, applyMsgs);
                    }

                    List<string> usedLayers = SpatialReferenceAudit.CollectFeasibilityLayerNames(job.LayerHints, names);
                    SpatialReferenceAuditResult srAudit = usedLayers.Count > 0
                        ? SpatialReferenceAudit.Audit(gdb, usedLayers, srSource, srLayer)
                        : SpatialReferenceAudit.Audit(gdb, null, srSource, srLayer);
                    if (!srAudit.Success || !srAudit.IsUnified)
                    {
                        throw new InvalidOperationException(srAudit.ToBlockMessage());
                    }

                    FeasibilityAnalysisEngine engine = new FeasibilityAnalysisEngine();
                    FeasibilityResult result = engine.Run(job, OnProgress);
                    return new FeasibilityWorkPack
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

        private void FinishOk(FeasibilityWorkPack pack)
        {
            FeasibilityResult result = pack != null ? pack.Result : null;
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

                StringBuilder sb = new StringBuilder();
                if (!string.IsNullOrEmpty(pack.ProfileDisplay))
                {
                    sb.AppendLine("城市配置: " + pack.ProfileDisplay);
                }
                sb.AppendLine("输出 GDB: " + (result.OutputGdbPath ?? pack.OutGdb));
                for (int i = 0; i < applyMsgs.Count; i++)
                {
                    sb.AppendLine(applyMsgs[i]);
                    _context.LogInfo(applyMsgs[i]);
                }
                for (int i = 0; i < result.Messages.Count; i++)
                {
                    sb.AppendLine(result.Messages[i]);
                }
                _context.LogInfo(result.Success
                    ? "======== 可行度分析完成 ========"
                    : "======== 可行度分析失败 ========");

                if (result.Success && !string.IsNullOrEmpty(result.FeasibilityRasterPath))
                {
                    string msg = string.Empty;
                    if (_context.AddRasterLayer(result.FeasibilityRasterPath, "可行度得分", out msg))
                    {
                        sb.AppendLine(msg);
                        _context.ZoomToFullExtent();
                    }
                    else
                    {
                        sb.AppendLine(msg);
                    }

                    foreach (KeyValuePair<string, string> kv in result.FactorRasters)
                    {
                        string m2 = string.Empty;
                        _context.AddRasterLayer(kv.Value, kv.Key, out m2);
                    }
                }

                this.lblStatus.Text = result.Success ? "完成" : "失败";
                MessageBox.Show(this, sb.ToString(), "可行度分析", MessageBoxButtons.OK,
                    result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
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
                this.lblStatus.Text = "异常";
                string msg = ex != null ? ex.Message : "未知错误";
                _context.LogError(msg);
                MessageBox.Show(this, msg, "可行度分析失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            this.lblStatus.Text = text + "  " + percent + "%";
            if (_context != null)
            {
                _context.ShowProgress(text, percent);
            }
        }

        private sealed class FeasibilityWorkPack
        {
            public FeasibilityResult Result;
            public List<string> ApplyMsgs;
            public string ProfileDisplay;
            public string OutGdb;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_busy)
            {
                MessageBox.Show(this, "可行度分析正在后台执行，请等待完成后再关闭。",
                    "可行度分析", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

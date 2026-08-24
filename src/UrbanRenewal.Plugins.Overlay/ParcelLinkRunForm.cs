using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using UrbanRenewal.Analysis;
using UrbanRenewal.Contracts;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Plugins.Overlay
{
    /// <summary>宗地关联；STA 后台。</summary>
    public partial class ParcelLinkRunForm : Form
    {
        private readonly IAppContext _context;
        private bool _busy;
        private StaBackgroundRunner.ProgressUiGate _progressGate;

        public ParcelLinkRunForm()
        {
            InitializeComponent();
        }

        public ParcelLinkRunForm(IAppContext context)
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
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            this.btnRun.Enabled = !busy;
            this.btnClose.Enabled = !busy;
            this.cboStat.Enabled = !busy;
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (_busy)
            {
                return;
            }
            if (_context == null)
            {
                MessageBox.Show(this, "运行上下文无效。", "宗地关联", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _context.ReloadGlobalSettings();
            string gdb = _context.GdbPath;
            string outGdb = _context.OutputGdbPath;
            if (string.IsNullOrEmpty(gdb) || !System.IO.Directory.Exists(gdb))
            {
                MessageBox.Show(this, "请先打开输入 GDB。", "宗地关联", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(outGdb) || !outGdb.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "请先指定输出 File GDB。", "宗地关联", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sameNote;
            outGdb = OutputGdbHelper.EnsureSeparateAnalysisOutput(gdb, outGdb, out sameNote);
            if (!string.IsNullOrEmpty(sameNote))
            {
                _context.LogInfo(sameNote);
                _context.OutputGdbPath = outGdb;
                _context.SaveGlobalSettings();
            }

            ParcelLinkJob job = new ParcelLinkJob();
            job.GdbPath = gdb;
            job.OutputGdbPath = outGdb;
            job.StatisticType = this.cboStat.SelectedIndex == 1 ? "MAXIMUM" : "MEAN";

            CityProfile profile = CityProfileStore.ResolveActive(_context.ActiveCityProfileId);
            List<string> applyMsgs = new List<string>();
            if (profile != null)
            {
                List<string> names = WorkspaceCatalog.ListFeatureClassNames(gdb);
                List<string> rasters = WorkspaceCatalog.ListRasterDatasetNames(gdb);
                FeasibilityJob tmp = new FeasibilityJob();
                profile.ApplyToFeasibilityJob(tmp, names, rasters, applyMsgs);
                if (tmp.LayerHints != null)
                {
                    if (tmp.LayerHints.ContainsKey("Parcel"))
                    {
                        job.LayerHints["Parcel"] = tmp.LayerHints["Parcel"];
                    }
                    if (tmp.LayerHints.ContainsKey("StudyArea"))
                    {
                        job.LayerHints["StudyArea"] = tmp.LayerHints["StudyArea"];
                    }
                }
            }

            SetBusy(true);
            _context.LogInfo("======== 开始宗地关联 ========");
            _context.LogInfo("输入 GDB（分析数据）: " + gdb);
            _context.LogInfo("输出 GDB: " + outGdb);

            StaBackgroundRunner.Run(
                this,
                delegate
                {
                    ParcelLinkEngine engine = new ParcelLinkEngine();
                    return engine.Run(job, OnProgress);
                },
                delegate(ParcelLinkResult result)
                {
                    FinishOk(result, applyMsgs);
                },
                FinishError);
        }

        private void FinishOk(ParcelLinkResult result, List<string> applyMsgs)
        {
            try
            {
                if (result.Success && !string.IsNullOrEmpty(result.OutputGdbPath))
                {
                    _context.OutputGdbPath = result.OutputGdbPath;
                    _context.SaveGlobalSettings();
                }

                for (int i = 0; i < applyMsgs.Count; i++)
                {
                    _context.LogInfo(applyMsgs[i]);
                }
                for (int i = 0; i < result.Messages.Count; i++)
                {
                    _context.LogInfo(result.Messages[i]);
                }
                _context.LogInfo(result.Success
                    ? "======== 宗地关联完成 ========"
                    : "======== 宗地关联失败 ========");

                if (result.Success && !string.IsNullOrEmpty(result.ParcelFeatureClassPath))
                {
                    string msg;
                    if (_context.AddFeatureLayer(result.ParcelFeatureClassPath, "宗地潜力", out msg))
                    {
                        _context.LogInfo(msg);
                        _context.ZoomToFullExtent();
                    }
                    else
                    {
                        _context.LogInfo(msg);
                    }
                }

                if (!result.Success)
                {
                    MessageBox.Show(this, string.Join("\r\n", result.Messages.ToArray()),
                        "宗地关联", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show(this, msg, "宗地关联失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_busy)
            {
                MessageBox.Show(this, "宗地关联正在后台执行，请等待完成后再关闭。",
                    "宗地关联", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

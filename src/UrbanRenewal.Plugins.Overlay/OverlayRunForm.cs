using System;
using System.ComponentModel;
using System.Windows.Forms;
using UrbanRenewal.Analysis;
using UrbanRenewal.Contracts;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Plugins.Overlay
{
    /// <summary>叠置评价；分析在 STA 后台线程执行。</summary>
    public partial class OverlayRunForm : Form
    {
        private readonly IAppContext _context;
        private bool _busy;
        private StaBackgroundRunner.ProgressUiGate _progressGate;

        public OverlayRunForm()
        {
            InitializeComponent();
        }

        public OverlayRunForm(IAppContext context)
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

            double mw = _context.MotivationWeight;
            double fw = _context.FeasibilityWeight;
            if (mw <= 0 && fw <= 0)
            {
                mw = 0.7;
                fw = 0.3;
            }
            this.nudMotivW.Value = ClampWeightPercent(mw);
            this.nudFeasibW.Value = ClampWeightPercent(fw);
        }

        private static decimal ClampWeightPercent(double weight01)
        {
            double p = weight01 <= 1.0 ? weight01 * 100.0 : weight01;
            if (p < 0)
            {
                p = 0;
            }
            if (p > 100)
            {
                p = 100;
            }
            return (decimal)p;
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            this.btnRun.Enabled = !busy;
            this.btnClose.Enabled = !busy;
            this.nudMotivW.Enabled = !busy;
            this.nudFeasibW.Enabled = !busy;
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (_busy)
            {
                return;
            }
            if (_context == null)
            {
                MessageBox.Show(this, "运行上下文无效。", "叠置评价", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _context.ReloadGlobalSettings();
            string outGdb = _context.OutputGdbPath;
            string gdb = _context.GdbPath;
            if (string.IsNullOrEmpty(outGdb) || !outGdb.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "请先在「数据管理 → 全局设置」中指定输出 File GDB。",
                    "叠置评价", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            OverlayJob job = new OverlayJob();
            job.GdbPath = gdb;
            job.OutputGdbPath = outGdb;
            job.CellSize = _context.CellSize;
            job.MotivationWeight = (double)this.nudMotivW.Value / 100.0;
            job.FeasibilityWeight = (double)this.nudFeasibW.Value / 100.0;
            double motivW = job.MotivationWeight;
            double feasibW = job.FeasibilityWeight;

            SetBusy(true);
            _context.LogInfo("======== 开始叠置评价 ========");
            _context.LogInfo("输入 GDB（分析数据）: " + (gdb ?? "(空)"));
            _context.LogInfo("输出 GDB: " + outGdb);
            _context.LogInfo("像元大小: " + job.CellSize + " 米");

            StaBackgroundRunner.Run(
                this,
                delegate
                {
                    OverlayAnalysisEngine engine = new OverlayAnalysisEngine();
                    return engine.Run(job, OnProgress);
                },
                delegate(OverlayResult result)
                {
                    FinishOk(result, motivW, feasibW);
                },
                FinishError);
        }

        private void FinishOk(OverlayResult result, double motivW, double feasibW)
        {
            try
            {
                if (result.Success)
                {
                    _context.MotivationWeight = motivW;
                    _context.FeasibilityWeight = feasibW;
                    _context.OutputGdbPath = result.OutputGdbPath;
                    _context.SaveGlobalSettings();
                }

                for (int i = 0; i < result.Messages.Count; i++)
                {
                    _context.LogInfo(result.Messages[i]);
                }
                _context.LogInfo(result.Success
                    ? "======== 叠置评价完成 ========"
                    : "======== 叠置评价失败 ========");

                if (result.Success)
                {
                    string msg;
                    if (!string.IsNullOrEmpty(result.PotentialRasterPath)
                        && _context.AddRasterLayer(result.PotentialRasterPath, "综合潜力得分", out msg))
                    {
                        _context.LogInfo(msg);
                    }
                    if (!string.IsNullOrEmpty(result.LevelRasterPath)
                        && _context.AddRasterLayer(result.LevelRasterPath, "潜力等级", out msg))
                    {
                        _context.LogInfo(msg);
                    }
                    _context.ZoomToFullExtent();
                }

                if (!result.Success)
                {
                    MessageBox.Show(this, string.Join("\r\n", result.Messages.ToArray()),
                        "叠置评价", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show(this, msg, "叠置评价失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show(this, "叠置评价正在后台执行，请等待完成后再关闭。",
                    "叠置评价", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

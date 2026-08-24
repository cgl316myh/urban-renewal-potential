using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using UrbanRenewal.Analysis;
using UrbanRenewal.Contracts;
using UrbanRenewal.Model;

namespace UrbanRenewal.Plugins.Output
{
    /// <summary>成果导出；STA 后台。</summary>
    public partial class OutputRunForm : Form
    {
        private readonly IAppContext _context;
        private bool _busy;
        private StaBackgroundRunner.ProgressUiGate _progressGate;

        public OutputRunForm()
        {
            InitializeComponent();
        }

        public OutputRunForm(IAppContext context)
            : this()
        {
            _context = context;
            _progressGate = new StaBackgroundRunner.ProgressUiGate(this, ApplyProgressUi);
            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime && _context != null)
            {
                _context.ReloadGlobalSettings();
                if (!string.IsNullOrEmpty(_context.OutputGdbPath))
                {
                    string parent = Path.GetDirectoryName(_context.OutputGdbPath);
                    this.txtFolder.Text = Path.Combine(string.IsNullOrEmpty(parent) ? _context.OutputGdbPath : parent, "Export");
                }
            }
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            this.btnRun.Enabled = !busy;
            this.btnClose.Enabled = !busy;
            this.btnBrowse.Enabled = !busy;
            this.txtFolder.Enabled = !busy;
            this.chkTiff.Enabled = !busy;
            this.chkShp.Enabled = !busy;
            this.chkCsv.Enabled = !busy;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (_busy)
            {
                return;
            }
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.SelectedPath = this.txtFolder.Text;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    this.txtFolder.Text = dlg.SelectedPath;
                }
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (_busy)
            {
                return;
            }
            if (_context == null)
            {
                return;
            }
            _context.ReloadGlobalSettings();
            string outGdb = _context.OutputGdbPath;
            if (string.IsNullOrEmpty(outGdb) || !outGdb.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "请先指定输出 File GDB。", "成果输出", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            OutputJob job = new OutputJob();
            job.OutputGdbPath = outGdb;
            job.ExportFolder = this.txtFolder.Text;
            job.ExportTiff = this.chkTiff.Checked;
            job.ExportShp = this.chkShp.Checked;
            job.ExportCsv = this.chkCsv.Checked;

            SetBusy(true);
            _context.LogInfo("======== 开始成果输出 ========");

            StaBackgroundRunner.Run(
                this,
                delegate
                {
                    OutputExportEngine engine = new OutputExportEngine();
                    return engine.Run(job, OnProgress);
                },
                FinishOk,
                FinishError);
        }

        private void FinishOk(OutputResult result)
        {
            try
            {
                for (int i = 0; i < result.Messages.Count; i++)
                {
                    _context.LogInfo(result.Messages[i]);
                }
                _context.LogInfo(result.Success
                    ? "======== 成果输出完成 ========"
                    : "======== 成果输出失败 ========");
                if (!string.IsNullOrEmpty(result.ExportFolder) && Directory.Exists(result.ExportFolder))
                {
                    try
                    {
                        Process.Start(result.ExportFolder);
                    }
                    catch
                    {
                    }
                }
                if (!result.Success)
                {
                    MessageBox.Show(this, string.Join("\r\n", result.Messages.ToArray()),
                        "成果输出", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                MessageBox.Show(this, msg, "导出失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show(this, "成果导出正在后台执行，请等待完成后再关闭。",
                    "成果输出", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

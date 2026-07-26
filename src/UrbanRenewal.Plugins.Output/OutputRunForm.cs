using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using UrbanRenewal.Analysis;
using UrbanRenewal.Contracts;
using UrbanRenewal.Model;

namespace UrbanRenewal.Plugins.Output
{
    public partial class OutputRunForm : Form
    {
        private readonly IAppContext _context;

        public OutputRunForm()
        {
            InitializeComponent();
        }

        public OutputRunForm(IAppContext context)
            : this()
        {
            _context = context;
            if (LicenseManager.UsageMode != LicenseUsageMode.Designtime && _context != null)
            {
                _context.ReloadGlobalSettings();
                this.lblOutInfo.Text = "输出 GDB：" + (_context.OutputGdbPath ?? "（未设置）");
                if (!string.IsNullOrEmpty(_context.OutputGdbPath))
                {
                    string parent = Path.GetDirectoryName(_context.OutputGdbPath);
                    this.txtFolder.Text = Path.Combine(string.IsNullOrEmpty(parent) ? _context.OutputGdbPath : parent, "Export");
                }
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
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

            this.btnRun.Enabled = false;
            try
            {
                OutputExportEngine engine = new OutputExportEngine();
                OutputResult result = engine.Run(job, delegate(string t, int p)
                {
                    this.lblStatus.Text = t + " " + p + "%";
                    _context.ShowProgress(t, p);
                    Application.DoEvents();
                });

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < result.Messages.Count; i++)
                {
                    sb.AppendLine(result.Messages[i]);
                    _context.LogInfo(result.Messages[i]);
                }
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
                this.lblStatus.Text = result.Success ? "完成" : "失败";
                MessageBox.Show(this, sb.ToString(), "成果输出", MessageBoxButtons.OK,
                    result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                _context.LogError(ex.Message);
                MessageBox.Show(this, ex.Message, "导出失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.btnRun.Enabled = true;
                _context.HideProgress();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

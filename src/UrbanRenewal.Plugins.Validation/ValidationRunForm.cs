using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using UrbanRenewal.Analysis;
using UrbanRenewal.Contracts;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Plugins.Validation
{
    public partial class ValidationRunForm : Form
    {
        private readonly IAppContext _context;

        public ValidationRunForm()
        {
            InitializeComponent();
        }

        public ValidationRunForm(IAppContext context)
            : this()
        {
            _context = context;
            if (!IsDesignModeSafe())
            {
                RefreshInfo();
            }
        }

        private static bool IsDesignModeSafe()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }

        private void RefreshInfo()
        {
            if (_context == null)
            {
                return;
            }
            _context.ReloadGlobalSettings();
            this.lblOutInfo.Text = "输出 GDB：" + (_context.OutputGdbPath ?? "（未设置）");
            CityProfile p = CityProfileStore.ResolveActive(_context.ActiveCityProfileId);
            this.lblCityInfo.Text = p != null ? ("城市配置：" + p.DisplayName) : "城市配置：（未设置）";
        }

        private void btnOpenGlobal_Click(object sender, EventArgs e)
        {
            if (_context != null)
            {
                _context.ShowGlobalSettings();
                RefreshInfo();
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (_context == null)
            {
                return;
            }
            _context.ReloadGlobalSettings();
            string gdb = _context.GdbPath;
            string outGdb = _context.OutputGdbPath;
            if (string.IsNullOrEmpty(gdb) || !System.IO.Directory.Exists(gdb))
            {
                MessageBox.Show(this, "请先打开输入 GDB。", "验证校核", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(outGdb) || !outGdb.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "请先指定输出 File GDB。", "验证校核", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ValidationJob job = new ValidationJob();
            job.GdbPath = gdb;
            job.OutputGdbPath = outGdb;
            job.HighLevelThreshold = (double)this.nudHighThr.Value;
            job.PassHighRatio = (double)this.nudPassRatio.Value / 100.0;
            job.ReviewComment = this.txtComment.Text;

            CityProfile profile = CityProfileStore.ResolveActive(_context.ActiveCityProfileId);
            if (profile != null && profile.Layers != null)
            {
                for (int i = 0; i < profile.Layers.Count; i++)
                {
                    CityLayerMapping m = profile.Layers[i];
                    if (m != null && string.Equals(m.Role, "UpdatedParcel", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrEmpty(m.Name))
                    {
                        job.LayerHints["UpdatedParcel"] = m.Name;
                    }
                }
            }

            this.btnRun.Enabled = false;
            this.lblStatus.Text = "正在验证...";
            if (_context != null)
            {
                _context.LogInfo("======== 开始验证校核 ========");
            }
            Application.DoEvents();
            try
            {
                ValidationAnalysisEngine engine = new ValidationAnalysisEngine();
                ValidationResult result = engine.Run(job, OnProgress);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < result.Messages.Count; i++)
                {
                    sb.AppendLine(result.Messages[i]);
                }
                _context.LogInfo(result.Success
                    ? "======== 验证校核完成 ========"
                    : "======== 验证校核失败 ========");
                if (result.Success && !string.IsNullOrEmpty(result.DiffFeatureClassPath))
                {
                    string msg;
                    _context.AddFeatureLayer(result.DiffFeatureClassPath, "验证差异(偏低已更新)", out msg);
                    sb.AppendLine(msg);
                }
                if (result.Success && !string.IsNullOrEmpty(result.ReportPath))
                {
                    try
                    {
                        Process.Start(result.ReportPath);
                    }
                    catch
                    {
                    }
                }
                this.lblStatus.Text = result.Success ? (result.Passed ? "通过" : "未通过") : "失败";
                MessageBox.Show(this, sb.ToString(), "验证校核", MessageBoxButtons.OK,
                    result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                this.lblStatus.Text = "异常";
                _context.LogError(ex.Message);
                MessageBox.Show(this, ex.Message, "验证失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.btnRun.Enabled = true;
                _context.HideProgress();
            }
        }

        private void OnProgress(string text, int percent)
        {
            this.lblStatus.Text = text + " " + percent + "%";
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

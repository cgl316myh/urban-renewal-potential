using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;
using UrbanRenewal.Analysis;
using UrbanRenewal.Contracts;
using UrbanRenewal.Model;

namespace UrbanRenewal.Plugins.Motivation
{
    /// <summary>
    /// 动力性分析运行窗体（可在 VS 设计器中打开）。
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
            if (!IsDesignModeSafe() && _context != null)
            {
                this.txtGdbPath.Text = _context.GdbPath ?? string.Empty;
            }
        }

        private static bool IsDesignModeSafe()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "选择 File GDB（*.gdb 文件夹）";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    this.txtGdbPath.Text = dlg.SelectedPath;
                }
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            string gdb = this.txtGdbPath.Text != null ? this.txtGdbPath.Text.Trim() : string.Empty;
            if (string.IsNullOrEmpty(gdb) || !Directory.Exists(gdb))
            {
                MessageBox.Show(this, "请先指定有效的 File GDB 路径。", "动力性分析", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_context != null)
            {
                _context.GdbPath = gdb;
            }

            MotivationJob job = new MotivationJob();
            job.GdbPath = gdb;
            job.CellSize = (double)this.nudCellSize.Value;
            job.TrafficWeight = (double)this.nudTraffic.Value / 100.0;
            job.EnvironmentWeight = (double)this.nudEnvironment.Value / 100.0;
            job.FacilityWeight = (double)this.nudFacility.Value / 100.0;
            job.PolicyWeight = (double)this.nudPolicy.Value / 100.0;
            // 避免中文工程路径导致 GP 工具失败，工作目录放到系统 Temp
            job.WorkDirectory = Path.Combine(
                Path.GetTempPath(),
                "UrbanRenewal",
                "Motivation_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));

            // 苏州示例数据默认图层提示（可被自动匹配覆盖）
            ApplySuzhouDefaultHints(job);

            this.btnRun.Enabled = false;
            this.lblStatus.Text = "正在分析...";
            Application.DoEvents();

            try
            {
                MotivationAnalysisEngine engine = new MotivationAnalysisEngine();
                MotivationResult result = engine.Run(job, OnProgress);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < result.Messages.Count; i++)
                {
                    sb.AppendLine(result.Messages[i]);
                    if (_context != null)
                    {
                        _context.LogInfo(result.Messages[i]);
                    }
                }

                if (result.Success && !string.IsNullOrEmpty(result.MotivationRasterPath))
                {
                    string msg = string.Empty;
                    if (_context != null && _context.AddRasterLayer(result.MotivationRasterPath, "动力性得分", out msg))
                    {
                        sb.AppendLine(msg);
                        _context.ZoomToFullExtent();
                    }
                    else if (_context != null)
                    {
                        sb.AppendLine(msg);
                    }

                    foreach (System.Collections.Generic.KeyValuePair<string, string> kv in result.CriterionRasters)
                    {
                        string m2 = string.Empty;
                        if (_context != null)
                        {
                            _context.AddRasterLayer(kv.Value, kv.Key, out m2);
                        }
                    }
                }

                this.lblStatus.Text = result.Success ? "完成" : "失败";
                MessageBox.Show(this, sb.ToString(), "动力性分析", MessageBoxButtons.OK,
                    result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                this.lblStatus.Text = "异常";
                if (_context != null)
                {
                    _context.LogError(ex.Message);
                }
                MessageBox.Show(this, ex.Message, "动力性分析失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.btnRun.Enabled = true;
                if (_context != null)
                {
                    _context.HideProgress();
                }
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

        /// <summary>
        /// 按苏州示例 GDB 常见图层名预填提示（仅当对应要素类确实存在时生效）。
        /// </summary>
        private static void ApplySuzhouDefaultHints(MotivationJob job)
        {
            if (job == null || job.LayerHints == null)
            {
                return;
            }

            System.Collections.Generic.List<string> names = UrbanRenewal.GIS.WorkspaceCatalog.ListFeatureClassNames(job.GdbPath);
            TryHint(job, names, "MetroMulti", "两线地铁站");
            TryHint(job, names, "Metro", "一线地铁站");
            TryHint(job, names, "CBD", "开发强度高区域（CBD）");
            TryHint(job, names, "EcoCorridor", "重要生态廊道");
            TryHint(job, names, "OpenSpace", "大型开敞空间");
            TryHint(job, names, "Green", "城市公园绿地");
            TryHint(job, names, "PublicService", "市级医院");
            TryHint(job, names, "Convenience", "苏州市建成区文体设施");
            TryHint(job, names, "PolicyBelt", "城市战略圈层和片区");
            TryHint(job, names, "PolicyKey", "近期重点发展区域");
        }

        private static void TryHint(MotivationJob job, System.Collections.Generic.List<string> names, string key, string exactName)
        {
            if (job.LayerHints.ContainsKey(key) && !string.IsNullOrEmpty(job.LayerHints[key]))
            {
                return;
            }
            for (int i = 0; i < names.Count; i++)
            {
                if (string.Equals(names[i], exactName, StringComparison.OrdinalIgnoreCase)
                    || names[i].IndexOf(exactName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    job.LayerHints[key] = names[i];
                    return;
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

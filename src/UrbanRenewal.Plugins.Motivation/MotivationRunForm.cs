using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using UrbanRenewal.Analysis;
using UrbanRenewal.Contracts;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Plugins.Motivation
{
    /// <summary>
    /// 动力性分析运行窗体（可在 VS 设计器中打开）。
    /// </summary>
    public partial class MotivationRunForm : Form
    {
        private readonly IAppContext _context;
        private bool _loadingCities;

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
                if (string.IsNullOrEmpty(_context.ActiveCityProfileId))
                {
                    _context.ActiveCityProfileId = CityProfileStore.LoadRememberedId();
                }
            }
            if (!IsDesignModeSafe())
            {
                InitOutputGdbPath();
                LoadCityProfiles();
            }
        }

        private void InitOutputGdbPath()
        {
            string remembered = OutputGdbHelper.LoadRemembered();
            if (!string.IsNullOrEmpty(remembered))
            {
                this.txtOutGdb.Text = remembered;
                return;
            }
            string input = this.txtGdbPath.Text != null ? this.txtGdbPath.Text.Trim() : string.Empty;
            this.txtOutGdb.Text = OutputGdbHelper.SuggestDefaultBesideInput(input);
        }

        private static bool IsDesignModeSafe()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }

        private void LoadCityProfiles()
        {
            _loadingCities = true;
            try
            {
                this.cboCity.Items.Clear();
                this.cboCity.Items.Add("(自动匹配 / 不使用配置)");
                List<CityProfile> profiles = CityProfileStore.LoadAll();
                string preferId = null;
                if (_context != null)
                {
                    preferId = _context.ActiveCityProfileId;
                }
                if (string.IsNullOrEmpty(preferId))
                {
                    preferId = CityProfileStore.LoadRememberedId();
                }

                int select = 0;
                for (int i = 0; i < profiles.Count; i++)
                {
                    CityProfileStore.NormalizeWeights(profiles[i]);
                    this.cboCity.Items.Add(profiles[i]);
                    if (!string.IsNullOrEmpty(preferId)
                        && string.Equals(profiles[i].Id, preferId, StringComparison.OrdinalIgnoreCase))
                    {
                        select = i + 1;
                    }
                    else if (select == 0 && profiles[i].IsDefault)
                    {
                        select = i + 1;
                    }
                }
                if (this.cboCity.Items.Count > 0)
                {
                    this.cboCity.SelectedIndex = select;
                }
                if (profiles.Count == 0)
                {
                    this.lblHint.Text = "未找到城市配置。请点「从GDB生成」或将 XML 放到："
                        + CityProfileStore.GetCitiesDirectory();
                }
            }
            finally
            {
                _loadingCities = false;
            }
            ApplySelectedProfileToUi(false);
            SyncActiveCityToContext();
        }

        private CityProfile GetSelectedProfile()
        {
            return this.cboCity.SelectedItem as CityProfile;
        }

        private void SyncActiveCityToContext()
        {
            CityProfile profile = GetSelectedProfile();
            string id = profile != null ? profile.Id : null;
            if (_context != null)
            {
                _context.ActiveCityProfileId = id;
            }
            if (!string.IsNullOrEmpty(id))
            {
                CityProfileStore.RememberId(id);
            }
        }

        private void cboCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingCities || IsDesignModeSafe())
            {
                return;
            }
            ApplySelectedProfileToUi(true);
            SyncActiveCityToContext();
        }

        private void ApplySelectedProfileToUi(bool showTip)
        {
            CityProfile profile = GetSelectedProfile();
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

            if (showTip)
            {
                this.lblStatus.Text = "已载入配置: " + profile.DisplayName;
            }
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

        private void btnDetect_Click(object sender, EventArgs e)
        {
            string gdb = this.txtGdbPath.Text != null ? this.txtGdbPath.Text.Trim() : string.Empty;
            if (string.IsNullOrEmpty(gdb) || !Directory.Exists(gdb))
            {
                MessageBox.Show(this, "请先指定有效的 File GDB 路径。", "图层匹配", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CityProfile profile = GetSelectedProfile();
            if (profile == null)
            {
                MessageBox.Show(this,
                    "当前为自动匹配模式。请选择城市配置，或点「从GDB生成」新建配置。",
                    "图层匹配", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<string> names = WorkspaceCatalog.ListFeatureClassNames(gdb);
            string report = profile.BuildLayerPresenceReport(names);
            string reqMsg;
            if (!profile.ValidateRequired(names, out reqMsg) && !string.IsNullOrEmpty(reqMsg))
            {
                report += Environment.NewLine + "[警告] 必选角色未齐备：" + Environment.NewLine + reqMsg;
            }
            SpatialReferenceAuditResult audit = SpatialReferenceAudit.Audit(gdb);
            report += Environment.NewLine + audit.ToCheckReport();
            MessageBox.Show(this, report, "城市配置检测 - " + profile.DisplayName, MessageBoxButtons.OK,
                audit.IsUnified ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private void btnOpenConfig_Click(object sender, EventArgs e)
        {
            string dir = CityProfileStore.GetCitiesDirectory();
            try
            {
                Directory.CreateDirectory(dir);
                Process.Start("explorer.exe", dir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "无法打开配置目录: " + dir + "\r\n" + ex.Message,
                    "配置目录", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnDraft_Click(object sender, EventArgs e)
        {
            string gdb = this.txtGdbPath.Text != null ? this.txtGdbPath.Text.Trim() : string.Empty;
            if (string.IsNullOrEmpty(gdb) || !Directory.Exists(gdb))
            {
                MessageBox.Show(this, "请先指定有效的 File GDB，再生成城市配置草稿。",
                    "从GDB生成", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string displayName = Path.GetFileNameWithoutExtension(gdb);
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = "新城市";
            }
            // 去掉常见后缀
            if (displayName.EndsWith("更新潜力评价数据", StringComparison.Ordinal))
            {
                displayName = displayName.Substring(0, displayName.Length - "更新潜力评价数据".Length);
            }
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = "新城市";
            }

            string id = PromptText("城市配置 Id（英文/拼音，用作文件名）", "NewCity");
            if (string.IsNullOrEmpty(id))
            {
                return;
            }
            id = SanitizeId(id);
            displayName = PromptText("显示名称", displayName);
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = id;
            }

            List<string> names = WorkspaceCatalog.ListFeatureClassNames(gdb);
            CityProfile draft = CityProfile.CreateDraft(id, displayName, names);

            SpatialReferenceAuditResult audit = SpatialReferenceAudit.Audit(gdb);
            if (audit.Success && !string.IsNullOrEmpty(audit.ReferenceSpatialReferenceName))
            {
                draft.PreferredCrsName = audit.ReferenceSpatialReferenceName;
            }

            string dir = CityProfileStore.GetCitiesDirectory();
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, id + ".xml");
            if (File.Exists(path))
            {
                DialogResult overwrite = MessageBox.Show(this,
                    "配置已存在：\r\n" + path + "\r\n是否覆盖？",
                    "从GDB生成", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (overwrite != DialogResult.Yes)
                {
                    return;
                }
            }

            CityProfileStore.Save(draft, path);
            draft.SourcePath = path;

            StringBuilder tip = new StringBuilder();
            tip.AppendLine("已生成城市配置草稿：");
            tip.AppendLine(path);
            tip.AppendLine();
            tip.AppendLine("请核对并微调 name/keywords（尤其必选 StudyArea），然后在下拉框中选择该城市。");
            tip.AppendLine();
            tip.Append(draft.BuildLayerPresenceReport(names));
            tip.AppendLine();
            tip.Append(audit.ToCheckReport());

            MessageBox.Show(this, tip.ToString(), "从GDB生成", MessageBoxButtons.OK,
                audit.IsUnified ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            if (_context != null)
            {
                _context.ActiveCityProfileId = id;
            }
            CityProfileStore.RememberId(id);
            LoadCityProfiles();
        }

        private static string SanitizeId(string id)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < id.Length; i++)
            {
                char c = id[i];
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_' || c == '-')
                {
                    sb.Append(c);
                }
            }
            if (sb.Length == 0)
            {
                return "NewCity";
            }
            return sb.ToString();
        }

        private string PromptText(string caption, string defaultValue)
        {
            using (Form dlg = new Form())
            {
                dlg.Text = caption;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.ClientSize = new System.Drawing.Size(420, 100);
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.ShowInTaskbar = false;

                TextBox txt = new TextBox();
                txt.Location = new System.Drawing.Point(12, 16);
                txt.Size = new System.Drawing.Size(396, 21);
                txt.Text = defaultValue ?? string.Empty;

                Button ok = new Button();
                ok.Text = "确定";
                ok.DialogResult = DialogResult.OK;
                ok.Location = new System.Drawing.Point(252, 55);
                ok.Size = new System.Drawing.Size(75, 28);

                Button cancel = new Button();
                cancel.Text = "取消";
                cancel.DialogResult = DialogResult.Cancel;
                cancel.Location = new System.Drawing.Point(333, 55);
                cancel.Size = new System.Drawing.Size(75, 28);

                dlg.Controls.Add(txt);
                dlg.Controls.Add(ok);
                dlg.Controls.Add(cancel);
                dlg.AcceptButton = ok;
                dlg.CancelButton = cancel;

                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return null;
                }
                return txt.Text != null ? txt.Text.Trim() : string.Empty;
            }
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "选择输入 File GDB（*.gdb 文件夹）";
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    this.txtGdbPath.Text = dlg.SelectedPath;
                    if (string.IsNullOrEmpty(this.txtOutGdb.Text)
                        || string.Equals(this.txtOutGdb.Text.Trim(),
                            OutputGdbHelper.SuggestDefaultBesideInput(null), StringComparison.OrdinalIgnoreCase))
                    {
                        this.txtOutGdb.Text = OutputGdbHelper.SuggestDefaultBesideInput(dlg.SelectedPath);
                    }
                }
            }
        }

        private void btnBrowseOut_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "选择输出 File GDB（*.gdb 文件夹；不存在时可先点「默认」再运行自动创建）";
                string cur = this.txtOutGdb.Text != null ? this.txtOutGdb.Text.Trim() : string.Empty;
                if (!string.IsNullOrEmpty(cur) && Directory.Exists(Path.GetDirectoryName(cur)))
                {
                    dlg.SelectedPath = Directory.Exists(cur) ? cur : Path.GetDirectoryName(cur);
                }
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    string path = dlg.SelectedPath;
                    if (!path.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
                    {
                        // 选的是父目录：追加建议名
                        path = Path.Combine(path, "Motivation_Output.gdb");
                    }
                    this.txtOutGdb.Text = path;
                }
            }
        }

        private void btnSuggestOut_Click(object sender, EventArgs e)
        {
            string input = this.txtGdbPath.Text != null ? this.txtGdbPath.Text.Trim() : string.Empty;
            this.txtOutGdb.Text = OutputGdbHelper.SuggestDefaultBesideInput(input);
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

            string outGdb = this.txtOutGdb.Text != null ? this.txtOutGdb.Text.Trim() : string.Empty;
            if (string.IsNullOrEmpty(outGdb))
            {
                MessageBox.Show(this, "请指定输出 File GDB 路径（中间数据与结果均写入该库）。",
                    "动力性分析", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!outGdb.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "输出路径必须以 .gdb 结尾（File Geodatabase 文件夹）。",
                    "动力性分析", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.Equals(Path.GetFullPath(gdb), Path.GetFullPath(outGdb), StringComparison.OrdinalIgnoreCase))
            {
                DialogResult same = MessageBox.Show(this,
                    "输出 GDB 与输入 GDB 相同，分析结果将写入源库。是否继续？",
                    "动力性分析", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (same != DialogResult.Yes)
                {
                    return;
                }
            }

            SpatialReferenceAuditResult srAudit = SpatialReferenceAudit.Audit(gdb);
            if (!srAudit.Success || !srAudit.IsUnified)
            {
                string block = srAudit.ToBlockMessage();
                this.lblStatus.Text = "空间参考不统一";
                if (_context != null)
                {
                    _context.LogError(block);
                }
                MessageBox.Show(this, block, "动力性分析", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            CityProfile profile = GetSelectedProfile();
            List<string> names = WorkspaceCatalog.ListFeatureClassNames(gdb);
            if (profile != null)
            {
                string reqMsg;
                if (!profile.ValidateRequired(names, out reqMsg))
                {
                    MessageBox.Show(this,
                        "城市配置「" + profile.DisplayName + "」必选图层未齐备，已取消分析。\r\n\r\n" + reqMsg
                        + "\r\n请用「检测匹配」核对，或编辑 Config\\Cities\\" + profile.Id + ".xml。",
                        "动力性分析", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                profile.ApplyToJob(job, names, applyMsgs);
                SyncActiveCityToContext();
            }

            this.btnRun.Enabled = false;
            this.lblStatus.Text = "正在分析...";
            Application.DoEvents();

            try
            {
                MotivationAnalysisEngine engine = new MotivationAnalysisEngine();
                MotivationResult result = engine.Run(job, OnProgress);

                StringBuilder sb = new StringBuilder();
                if (profile != null)
                {
                    sb.AppendLine("城市配置: " + profile.DisplayName);
                }
                if (!string.IsNullOrEmpty(result.OutputGdbPath))
                {
                    sb.AppendLine("输出 GDB: " + result.OutputGdbPath);
                }
                for (int i = 0; i < applyMsgs.Count; i++)
                {
                    sb.AppendLine(applyMsgs[i]);
                }
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

                    foreach (KeyValuePair<string, string> kv in result.CriterionRasters)
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

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

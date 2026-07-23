using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using UrbanRenewal.Contracts;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Plugins.DataManage
{
    /// <summary>
    /// 全局设置：输出 GDB + 城市配置（全模块共用，持久化）。
    /// </summary>
    public partial class GlobalSettingsForm : Form
    {
        private readonly IAppContext _context;
        private bool _loadingCities;

        public GlobalSettingsForm()
        {
            InitializeComponent();
        }

        public GlobalSettingsForm(IAppContext context)
            : this()
        {
            _context = context;
            if (!IsDesignModeSafe() && _context != null)
            {
                _context.ReloadGlobalSettings();
                this.txtInputGdb.Text = _context.GdbPath ?? string.Empty;
                this.txtOutGdb.Text = _context.OutputGdbPath ?? string.Empty;
                if (string.IsNullOrEmpty(this.txtOutGdb.Text) && !string.IsNullOrEmpty(_context.GdbPath))
                {
                    this.txtOutGdb.Text = OutputGdbHelper.SuggestDefaultBesideInput(_context.GdbPath);
                }
                LoadCityProfiles();
            }
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
                this.cboCity.Items.Add("(未选择)");
                List<CityProfile> profiles = CityProfileStore.LoadAll();
                string preferId = _context != null ? _context.ActiveCityProfileId : null;
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
            }
            finally
            {
                _loadingCities = false;
            }
        }

        private CityProfile GetSelectedProfile()
        {
            return this.cboCity.SelectedItem as CityProfile;
        }

        private void btnBrowseOut_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "选择输出 File GDB（*.gdb；可为尚不存在的路径，分析时自动创建）";
                string cur = this.txtOutGdb.Text != null ? this.txtOutGdb.Text.Trim() : string.Empty;
                if (!string.IsNullOrEmpty(cur))
                {
                    string parent = Directory.Exists(cur) ? cur : Path.GetDirectoryName(cur);
                    if (!string.IsNullOrEmpty(parent) && Directory.Exists(parent))
                    {
                        dlg.SelectedPath = parent;
                    }
                }
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    string path = dlg.SelectedPath;
                    if (!path.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
                    {
                        path = Path.Combine(path, "Motivation_Output.gdb");
                    }
                    this.txtOutGdb.Text = path;
                }
            }
        }

        private void btnSuggestOut_Click(object sender, EventArgs e)
        {
            string input = this.txtInputGdb.Text != null ? this.txtInputGdb.Text.Trim() : string.Empty;
            this.txtOutGdb.Text = OutputGdbHelper.SuggestDefaultBesideInput(input);
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

        private void btnDetect_Click(object sender, EventArgs e)
        {
            string gdb = this.txtInputGdb.Text != null ? this.txtInputGdb.Text.Trim() : string.Empty;
            if (string.IsNullOrEmpty(gdb) || !Directory.Exists(gdb))
            {
                MessageBox.Show(this, "请先在「打开 GDB」中指定输入工作空间。", "图层匹配",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            CityProfile profile = GetSelectedProfile();
            if (profile == null)
            {
                MessageBox.Show(this, "请先选择城市配置。", "图层匹配", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void btnDraft_Click(object sender, EventArgs e)
        {
            string gdb = this.txtInputGdb.Text != null ? this.txtInputGdb.Text.Trim() : string.Empty;
            if (string.IsNullOrEmpty(gdb) || !Directory.Exists(gdb))
            {
                MessageBox.Show(this, "请先打开输入 GDB，再生成城市配置草稿。", "从GDB生成",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string displayName = Path.GetFileNameWithoutExtension(gdb);
            if (displayName != null && displayName.EndsWith("更新潜力评价数据", StringComparison.Ordinal))
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
                if (MessageBox.Show(this, "配置已存在，是否覆盖？\r\n" + path, "从GDB生成",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }
            }
            CityProfileStore.Save(draft, path);
            draft.SourcePath = path;

            StringBuilder tip = new StringBuilder();
            tip.AppendLine("已生成并可选为当前城市配置：");
            tip.AppendLine(path);
            tip.AppendLine();
            tip.Append(draft.BuildLayerPresenceReport(names));
            MessageBox.Show(this, tip.ToString(), "从GDB生成", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (_context != null)
            {
                _context.ActiveCityProfileId = id;
            }
            LoadCityProfiles();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_context == null)
            {
                this.DialogResult = DialogResult.Cancel;
                return;
            }

            string outGdb = this.txtOutGdb.Text != null ? this.txtOutGdb.Text.Trim() : string.Empty;
            if (string.IsNullOrEmpty(outGdb))
            {
                MessageBox.Show(this, "请指定输出 File GDB（后续所有分析结果均写入该库）。",
                    "全局设置", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!outGdb.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "输出路径必须以 .gdb 结尾。", "全局设置",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CityProfile profile = GetSelectedProfile();
            string input = this.txtInputGdb.Text != null ? this.txtInputGdb.Text.Trim() : null;
            if (!string.IsNullOrEmpty(input))
            {
                _context.GdbPath = input;
            }
            _context.OutputGdbPath = outGdb;
            _context.ActiveCityProfileId = profile != null ? profile.Id : null;
            _context.SaveGlobalSettings();

            MessageBox.Show(this,
                "全局设置已保存。\r\n输出 GDB: " + outGdb
                + "\r\n城市: " + (profile != null ? profile.DisplayName : "(未选择)")
                + "\r\n\r\n后续动力性及所有分析模块将写入该输出库。",
                "全局设置", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK;
            this.Close();
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
            return sb.Length == 0 ? "NewCity" : sb.ToString();
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
    }
}

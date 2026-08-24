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
        private string _spatialRefSourcePath;
        private string _spatialRefLayerName;
        private string _spatialRefName;
        private int _spatialRefFactoryCode;

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
                _spatialRefSourcePath = _context.SpatialRefSourcePath;
                _spatialRefLayerName = _context.SpatialRefLayerName;
                _spatialRefName = _context.SpatialRefName;
                _spatialRefFactoryCode = _context.SpatialRefFactoryCode;
                RefreshSpatialRefUi();
                LoadCityProfiles();
                double cell = _context.CellSize;
                if (cell < (double)this.nudCellSize.Minimum)
                {
                    cell = (double)this.nudCellSize.Minimum;
                }
                if (cell > (double)this.nudCellSize.Maximum)
                {
                    cell = (double)this.nudCellSize.Maximum;
                }
                this.nudCellSize.Value = (decimal)cell;
                this.chkMaskResultToStudyArea.Checked = _context.MaskResultToStudyArea;
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

        private void btnBrowseInput_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "选择输入 File GDB（*.gdb 文件夹）";
                string cur = NormalizeGdbPath(this.txtInputGdb.Text);
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
                        MessageBox.Show(this, "请选择以 .gdb 结尾的 File GDB 文件夹。",
                            "输入 GDB", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    this.txtInputGdb.Text = path;
                    if (string.IsNullOrEmpty(NormalizeGdbPath(this.txtOutGdb.Text)))
                    {
                        this.txtOutGdb.Text = OutputGdbHelper.SuggestDefaultBesideInput(path);
                    }
                }
            }
        }

        private void btnBrowseOut_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dlg = new FolderBrowserDialog())
            {
                dlg.Description = "选择输出 File GDB（*.gdb；可为尚不存在的路径，分析时自动创建）";
                string cur = NormalizeGdbPath(this.txtOutGdb.Text);
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
            string input = NormalizeGdbPath(this.txtInputGdb.Text);
            this.txtOutGdb.Text = OutputGdbHelper.SuggestDefaultBesideInput(input);
        }

        private void RefreshSpatialRefUi()
        {
            if (string.IsNullOrEmpty(_spatialRefName) && string.IsNullOrEmpty(_spatialRefSourcePath))
            {
                this.txtSpatialRefName.Text = string.Empty;
                this.lblSpatialRefSrc.Text = "来源：（未配置，完整性检查将自动推断）";
                return;
            }

            string display = _spatialRefName ?? string.Empty;
            if (_spatialRefFactoryCode > 0)
            {
                display = display + " [WKID=" + _spatialRefFactoryCode + "]";
            }
            this.txtSpatialRefName.Text = display;

            if (!string.IsNullOrEmpty(_spatialRefSourcePath)
                && _spatialRefSourcePath.EndsWith(".shp", StringComparison.OrdinalIgnoreCase))
            {
                this.lblSpatialRefSrc.Text = "来源：Shapefile → " + _spatialRefSourcePath;
            }
            else if (!string.IsNullOrEmpty(_spatialRefSourcePath))
            {
                this.lblSpatialRefSrc.Text = "来源：GDB图层 → "
                    + (_spatialRefLayerName ?? "")
                    + " @ " + _spatialRefSourcePath;
            }
            else
            {
                this.lblSpatialRefSrc.Text = "来源：（仅缓存名称）";
            }
        }

        private void ApplySpatialRefReading(string sourcePath, string layerName)
        {
            string name;
            int code;
            string msg;
            if (!FeatureProjectionHelper.TryReadSpatialReferenceInfo(
                sourcePath, layerName, out name, out code, out msg))
            {
                MessageBox.Show(this, msg ?? "读取失败", "基准坐标系",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _spatialRefSourcePath = sourcePath;
            _spatialRefLayerName = layerName;
            _spatialRefName = name;
            _spatialRefFactoryCode = code;
            RefreshSpatialRefUi();
            MessageBox.Show(this, "已读取基准坐标系：\r\n" + msg, "基准坐标系",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnSpatialRefShp_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "选择含目标坐标系的 Shapefile";
                dlg.Filter = "Shapefile (*.shp)|*.shp|所有文件 (*.*)|*.*";
                dlg.CheckFileExists = true;
                if (dlg.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                ApplySpatialRefReading(dlg.FileName, null);
            }
        }

        private void btnSpatialRefGdb_Click(object sender, EventArgs e)
        {
            string gdb = NormalizeGdbPath(this.txtInputGdb.Text);
            if (string.IsNullOrEmpty(gdb) || !Directory.Exists(gdb))
            {
                MessageBox.Show(this, "请先指定有效的输入 GDB，再从其中选择图层。",
                    "基准坐标系", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<string> names = WorkspaceCatalog.ListFeatureClassNames(gdb);
            if (names == null || names.Count == 0)
            {
                MessageBox.Show(this, "输入 GDB 中没有要素类。", "基准坐标系",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (Form dlg = new Form())
            {
                dlg.Text = "选择基准坐标系图层";
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.ClientSize = new System.Drawing.Size(420, 120);
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.ShowInTaskbar = false;

                Label tip = new Label();
                tip.Location = new System.Drawing.Point(12, 12);
                tip.Size = new System.Drawing.Size(396, 20);
                tip.Text = "从输入 GDB 选择图层，读取其空间参考作为全局基准：";

                ComboBox cbo = new ComboBox();
                cbo.DropDownStyle = ComboBoxStyle.DropDownList;
                cbo.Location = new System.Drawing.Point(12, 40);
                cbo.Size = new System.Drawing.Size(396, 20);
                for (int i = 0; i < names.Count; i++)
                {
                    cbo.Items.Add(names[i]);
                }
                if (!string.IsNullOrEmpty(_spatialRefLayerName))
                {
                    int idx = cbo.Items.IndexOf(_spatialRefLayerName);
                    cbo.SelectedIndex = idx >= 0 ? idx : 0;
                }
                else if (cbo.Items.Count > 0)
                {
                    string preferred = WorkspaceCatalog.FindByKeywords(names, "中心城区", "分析范围", "建成区");
                    int idx = string.IsNullOrEmpty(preferred) ? 0 : cbo.Items.IndexOf(preferred);
                    cbo.SelectedIndex = idx >= 0 ? idx : 0;
                }

                Button ok = new Button();
                ok.Text = "确定";
                ok.DialogResult = DialogResult.OK;
                ok.Location = new System.Drawing.Point(252, 78);
                ok.Size = new System.Drawing.Size(75, 28);

                Button cancel = new Button();
                cancel.Text = "取消";
                cancel.DialogResult = DialogResult.Cancel;
                cancel.Location = new System.Drawing.Point(333, 78);
                cancel.Size = new System.Drawing.Size(75, 28);

                dlg.Controls.Add(tip);
                dlg.Controls.Add(cbo);
                dlg.Controls.Add(ok);
                dlg.Controls.Add(cancel);
                dlg.AcceptButton = ok;
                dlg.CancelButton = cancel;

                if (dlg.ShowDialog(this) != DialogResult.OK || cbo.SelectedItem == null)
                {
                    return;
                }
                ApplySpatialRefReading(gdb, cbo.SelectedItem.ToString());
            }
        }

        private void btnSpatialRefClear_Click(object sender, EventArgs e)
        {
            _spatialRefSourcePath = null;
            _spatialRefLayerName = null;
            _spatialRefName = null;
            _spatialRefFactoryCode = 0;
            RefreshSpatialRefUi();
        }

        /// <summary>去除首尾空白与资源管理器复制时常见的引号。</summary>
        private static string NormalizeGdbPath(string raw)
        {
            if (raw == null)
            {
                return string.Empty;
            }
            string path = raw.Trim();
            if (path.Length >= 2)
            {
                if ((path[0] == '"' && path[path.Length - 1] == '"')
                    || (path[0] == '\'' && path[path.Length - 1] == '\''))
                {
                    path = path.Substring(1, path.Length - 2).Trim();
                }
            }
            return path;
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
            string gdb = NormalizeGdbPath(this.txtInputGdb.Text);
            if (string.IsNullOrEmpty(gdb) || !Directory.Exists(gdb))
            {
                MessageBox.Show(this, "请先指定有效的输入 GDB（可浏览或粘贴路径）。", "图层匹配",
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
            SpatialReferenceAuditResult audit = SpatialReferenceAudit.Audit(
                gdb, null, _spatialRefSourcePath, _spatialRefLayerName);
            report += Environment.NewLine + audit.ToCheckReport();
            MessageBox.Show(this, report, "城市配置检测 - " + profile.DisplayName, MessageBoxButtons.OK,
                audit.IsUnified ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private void btnDraft_Click(object sender, EventArgs e)
        {
            string gdb = NormalizeGdbPath(this.txtInputGdb.Text);
            if (string.IsNullOrEmpty(gdb) || !Directory.Exists(gdb))
            {
                MessageBox.Show(this, "请先指定有效的输入 GDB，再生成城市配置草稿。", "从GDB生成",
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
            SpatialReferenceAuditResult audit = SpatialReferenceAudit.Audit(
                gdb, null, _spatialRefSourcePath, _spatialRefLayerName);
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

            string input = NormalizeGdbPath(this.txtInputGdb.Text);
            this.txtInputGdb.Text = input;
            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show(this, "请指定输入 File GDB（可浏览或粘贴 *.gdb 路径）。",
                    "全局设置", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.txtInputGdb.Focus();
                return;
            }
            if (!input.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "输入路径必须以 .gdb 结尾。", "全局设置",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.txtInputGdb.Focus();
                return;
            }
            if (!Directory.Exists(input))
            {
                MessageBox.Show(this, "输入 GDB 不存在：\r\n" + input, "全局设置",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.txtInputGdb.Focus();
                return;
            }

            string outGdb = NormalizeGdbPath(this.txtOutGdb.Text);
            this.txtOutGdb.Text = outGdb;
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
            _context.OutputGdbPath = outGdb;
            _context.ActiveCityProfileId = profile != null ? profile.Id : null;
            _context.CellSize = (double)this.nudCellSize.Value;
            _context.MaskResultToStudyArea = this.chkMaskResultToStudyArea.Checked;
            _context.SpatialRefSourcePath = _spatialRefSourcePath;
            _context.SpatialRefLayerName = _spatialRefLayerName;
            _context.SpatialRefName = _spatialRefName;
            _context.SpatialRefFactoryCode = _spatialRefFactoryCode;

            string openMsg;
            bool opened = _context.OpenFileGdb(input, out openMsg);
            if (!opened)
            {
                // 仍保存路径，便于下次打开；地图加载失败单独提示
                _context.GdbPath = input;
                _context.SaveGlobalSettings();
                MessageBox.Show(this,
                    "路径已保存，但加载到地图失败：\r\n" + (openMsg ?? "未知错误"),
                    "全局设置", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // OpenFileGdb 可能已自动建议输出路径；以界面值为准再写回
            _context.OutputGdbPath = outGdb;
            _context.ActiveCityProfileId = profile != null ? profile.Id : null;
            _context.CellSize = (double)this.nudCellSize.Value;
            _context.MaskResultToStudyArea = this.chkMaskResultToStudyArea.Checked;
            _context.SpatialRefSourcePath = _spatialRefSourcePath;
            _context.SpatialRefLayerName = _spatialRefLayerName;
            _context.SpatialRefName = _spatialRefName;
            _context.SpatialRefFactoryCode = _spatialRefFactoryCode;
            _context.SaveGlobalSettings();
            _context.ZoomToFullExtent();
            _context.LogInfo(openMsg);

            MessageBox.Show(this,
                "全局设置已保存到本地：\r\n"
                + GlobalAppSettingsStore.GetSettingsFilePath()
                + "\r\n\r\n"
                + openMsg
                + "\r\n\r\n输入: " + input
                + "\r\n输出: " + outGdb
                + "\r\n城市: " + (profile != null ? profile.DisplayName : "(未选择)")
                + "\r\n像元: " + this.nudCellSize.Value.ToString() + " 米"
                + "\r\n成果掩膜: " + (this.chkMaskResultToStudyArea.Checked ? "是" : "否")
                + "\r\nSpatialRef: " + (string.IsNullOrEmpty(_spatialRefName) ? "(自动推断)" : _spatialRefName)
                + "\r\n\r\n提示：配置已写入用户目录，重启/重新编译后仍会保留。"
                + "\r\n若需下次启动自动恢复当前地图，请再单击「数据管理 → 保存工程」。",
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

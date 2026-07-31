using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using UrbanRenewal.Contracts;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Plugins.DataManage
{
    /// <summary>
    /// 投影/裁剪预处理：全局输入 → clip.gdb；不改原库、不改分析输出、不清空地图。
    /// </summary>
    public partial class PreprocessForm : Form
    {
        private readonly IAppContext _context;
        private bool _busy;
        private StaBackgroundRunner.ProgressUiGate _progressGate;

        public PreprocessForm()
        {
            InitializeComponent();
        }

        public PreprocessForm(IAppContext context)
            : this()
        {
            _context = context;
            _progressGate = new StaBackgroundRunner.ProgressUiGate(this, ApplyProgressUi);
            if (!IsDesignModeSafe() && _context != null)
            {
                _context.ReloadGlobalSettings();
                this.txtInputGdb.Text = _context.GdbPath ?? string.Empty;
                // 裁切结果默认写入 clip.gdb，与全局「分析输出 GDB」分离
                string clipDefault = OutputGdbHelper.SuggestClipGdbBeside(_context.GdbPath);
                this.txtOutGdb.Text = clipDefault;
                LoadLayers();
            }
        }

        private static bool IsDesignModeSafe()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }

        private void LoadLayers()
        {
            this.lstLayers.Items.Clear();
            this.cboClip.Items.Clear();
            string gdb = this.txtInputGdb.Text != null ? this.txtInputGdb.Text.Trim() : string.Empty;
            if (string.IsNullOrEmpty(gdb) || !System.IO.Directory.Exists(gdb))
            {
                return;
            }

            List<string> names = WorkspaceCatalog.ListFeatureClassNames(gdb);
            SpatialReferenceAuditResult audit = _context != null
                ? SpatialReferenceAudit.Audit(gdb, null, _context.SpatialRefSourcePath, _context.SpatialRefLayerName)
                : SpatialReferenceAudit.Audit(gdb);

            string study = WorkspaceCatalog.FindByKeywords(names, "中心城区", "分析范围", "建成区");
            CityProfile profile = CityProfileStore.ResolveActive(
                _context != null ? _context.ActiveCityProfileId : null);
            if (profile != null && profile.Layers != null)
            {
                for (int i = 0; i < profile.Layers.Count; i++)
                {
                    CityLayerMapping map = profile.Layers[i];
                    if (map == null
                        || !string.Equals(map.Role, "StudyArea", StringComparison.OrdinalIgnoreCase)
                        || string.IsNullOrEmpty(map.Name))
                    {
                        continue;
                    }
                    for (int j = 0; j < names.Count; j++)
                    {
                        if (string.Equals(names[j], map.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            study = names[j];
                            break;
                        }
                    }
                }
            }

            int clipIndex = 0;
            for (int i = 0; i < names.Count; i++)
            {
                string name = names[i];
                this.cboClip.Items.Add(name);
                if (!string.IsNullOrEmpty(study)
                    && string.Equals(name, study, StringComparison.OrdinalIgnoreCase))
                {
                    clipIndex = this.cboClip.Items.Count - 1;
                }
            }

            for (int i = 0; i < names.Count; i++)
            {
                string name = names[i];
                if (FeaturePreprocessBuilder.IsNetworkArtifact(name))
                {
                    continue;
                }

                string srLabel = "";
                bool mismatch = false;
                for (int k = 0; k < audit.Layers.Count; k++)
                {
                    if (string.Equals(audit.Layers[k].LayerName, name, StringComparison.OrdinalIgnoreCase))
                    {
                        srLabel = audit.Layers[k].SpatialReferenceName ?? "(无SR)";
                        mismatch = !audit.Layers[k].MatchesReference;
                        break;
                    }
                }

                string display = mismatch
                    ? name + "  [需投影: " + srLabel + "]"
                    : name + "  [" + srLabel + "]";
                LayerItem item = new LayerItem(name, display);
                int idx = this.lstLayers.Items.Add(item);
                this.lstLayers.SetItemChecked(idx, mismatch);
            }

            if (this.cboClip.Items.Count > 0)
            {
                this.cboClip.SelectedIndex = clipIndex;
            }
        }

        private void btnSelectMismatch_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.lstLayers.Items.Count; i++)
            {
                LayerItem item = this.lstLayers.Items[i] as LayerItem;
                bool mismatch = item != null && item.Display.IndexOf("需投影", StringComparison.Ordinal) >= 0;
                this.lstLayers.SetItemChecked(i, mismatch);
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.lstLayers.Items.Count; i++)
            {
                this.lstLayers.SetItemChecked(i, true);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.lstLayers.Items.Count; i++)
            {
                this.lstLayers.SetItemChecked(i, false);
            }
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (_busy || _context == null)
            {
                return;
            }

            string inGdb = this.txtInputGdb.Text != null ? this.txtInputGdb.Text.Trim() : string.Empty;
            string outGdb = this.txtOutGdb.Text != null ? this.txtOutGdb.Text.Trim() : string.Empty;
            if (string.IsNullOrEmpty(inGdb) || !System.IO.Directory.Exists(inGdb))
            {
                MessageBox.Show(this, "请先在「全局设置」中指定输入 GDB。", "预处理", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(outGdb) || !outGdb.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "请指定裁切结果 File GDB（建议 clip.gdb，与全局分析输出库分开）。", "预处理",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.Equals(
                System.IO.Path.GetFullPath(inGdb).TrimEnd('\\', '/'),
                System.IO.Path.GetFullPath(outGdb).TrimEnd('\\', '/'),
                StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this,
                    "输出 GDB 不能与输入 GDB 相同。\r\n请另指定一个库保存处理结果，以保护原始数据。",
                    "预处理", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!this.chkProject.Checked && !this.chkClip.Checked)
            {
                MessageBox.Show(this, "请至少勾选「投影」或「裁剪」。", "预处理",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (this.chkClip.Checked && this.cboClip.SelectedItem == null)
            {
                MessageBox.Show(this, "请选择建成区/分析范围作为裁剪图层。", "预处理",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FeaturePreprocessJob job = new FeaturePreprocessJob();
            job.InputGdbPath = inGdb;
            job.OutputGdbPath = outGdb;
            job.DoProject = this.chkProject.Checked;
            job.DoClip = this.chkClip.Checked;
            job.ReplaceInInputGdb = false; // 非破坏：绝不覆盖原库
            job.ClipLayerName = this.cboClip.SelectedItem != null ? this.cboClip.SelectedItem.ToString() : null;

            for (int i = 0; i < this.lstLayers.Items.Count; i++)
            {
                if (!this.lstLayers.GetItemChecked(i))
                {
                    continue;
                }
                LayerItem item = this.lstLayers.Items[i] as LayerItem;
                if (item != null)
                {
                    job.LayerNames.Add(item.Name);
                }
            }
            if (job.LayerNames.Count == 0)
            {
                MessageBox.Show(this, "请至少勾选一个图层。", "预处理", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show(this,
                "将对选中的 " + job.LayerNames.Count + " 个图层做投影/裁剪。\r\n\r\n"
                + "流程：全局输入 GDB → 裁切结果库（不改原库）。\r\n"
                + "裁切结果写入：\r\n" + outGdb + "\r\n\r\n"
                + "完成后可将「全局输入」切换为该裁切库；\r\n"
                + "全局「分析输出 GDB」保持不变，后续动力性分析不再与输入同库抢锁。\r\n\r\n"
                + "是否继续？",
                "预处理 — 非破坏模式",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            // 记住当前分析输出库，完成后不得被 clip.gdb 覆盖
            string analysisOutGdb = _context.OutputGdbPath;

            this.btnRun.Enabled = false;
            if (_context != null)
            {
                _context.LogInfo("======== 开始投影/裁剪预处理（全局输入 → clip，分析输出不变） ========");
                _context.LogInfo("输入(源): " + inGdb);
                _context.LogInfo("裁切结果: " + outGdb);
                _context.LogInfo("分析输出(保持): " + (analysisOutGdb ?? "(未设置)"));
            }

            _busy = true;
            this.btnClose.Enabled = false;

            StaBackgroundRunner.Run(
                this,
                delegate
                {
                    return FeaturePreprocessBuilder.Run(job, OnProgress);
                },
                delegate(FeaturePreprocessResult result)
                {
                    FinishPreprocess(result, inGdb, outGdb, analysisOutGdb);
                },
                FinishPreprocessError);
        }

        private void FinishPreprocess(FeaturePreprocessResult result, string inGdb, string clipGdb, string analysisOutGdb)
        {
            try
            {
                for (int i = 0; i < result.Messages.Count; i++)
                {
                    _context.LogInfo(result.Messages[i]);
                }
                _context.LogInfo(result.Success
                    ? "======== 预处理完成 ========"
                    : "======== 预处理失败 ========");

                if (result.Success)
                {
                    // 分析输出库始终保持用户原来的设置，绝不用 clip.gdb 覆盖
                    if (!string.IsNullOrEmpty(analysisOutGdb)
                        && analysisOutGdb.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase)
                        && !OutputGdbHelper.IsSameGdb(analysisOutGdb, clipGdb))
                    {
                        _context.OutputGdbPath = analysisOutGdb;
                    }

                    // 裁切完成后：全局输入固定切到 clip.gdb，后续动力性/可行度/叠置/宗地/验证均从此库读数
                    _context.GdbPath = clipGdb;
                    _context.SaveGlobalSettings();
                    this.txtInputGdb.Text = clipGdb;

                    _context.LogInfo("已将全局输入切换为裁切库: " + clipGdb);
                    _context.LogInfo("后续分析（动力性/可行度/叠置/宗地关联/验证）均从此库读取。");
                    _context.LogInfo("分析输出仍为: " + (_context.OutputGdbPath ?? "(未设置)"));

                    MessageBox.Show(this,
                        "预处理完成。原输入 GDB 未改动。\r\n\r\n"
                        + "裁切结果：\r\n" + clipGdb + "\r\n\r\n"
                        + "已将全局「输入 GDB」切换为该裁切库。\r\n"
                        + "动力性、可行度、叠置、宗地关联、验证等分析\r\n"
                        + "均从此库读取数据；分析输出库保持不变。\r\n\r\n"
                        + "地图图层未清空。",
                        "预处理完成",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    // 不调用 OpenFileGdb，避免清空当前地图图层
                    LoadLayers();
                }
                else
                {
                    MessageBox.Show(this, string.Join("\r\n", result.Messages.ToArray()),
                        "预处理", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                EndPreprocessBusy();
            }
        }

        private void FinishPreprocessError(Exception ex)
        {
            try
            {
                _context.LogError(ex != null ? ex.ToString() : "未知错误");
                MessageBox.Show(this, ex != null ? ex.Message : "未知错误", "预处理失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                EndPreprocessBusy();
            }
        }

        private void EndPreprocessBusy()
        {
            _busy = false;
            this.btnRun.Enabled = true;
            this.btnClose.Enabled = true;
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
                MessageBox.Show(this, "预处理正在后台执行，请等待完成后再关闭。",
                    "预处理", MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
                return;
            }
            base.OnFormClosing(e);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private sealed class LayerItem
        {
            public LayerItem(string name, string display)
            {
                Name = name;
                Display = display;
            }

            public string Name { get; private set; }
            public string Display { get; private set; }

            public override string ToString()
            {
                return Display;
            }
        }
    }
}

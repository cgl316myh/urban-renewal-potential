using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using UrbanRenewal.Contracts;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Plugins.DataManage
{
    /// <summary>
    /// 投影/裁剪预处理：纠正坐标系/范围后，用正确图层替换输入 GDB 中的原图层。
    /// </summary>
    public partial class PreprocessForm : Form
    {
        private readonly IAppContext _context;

        public PreprocessForm()
        {
            InitializeComponent();
        }

        public PreprocessForm(IAppContext context)
            : this()
        {
            _context = context;
            if (!IsDesignModeSafe() && _context != null)
            {
                _context.ReloadGlobalSettings();
                this.txtInputGdb.Text = _context.GdbPath ?? string.Empty;
                this.txtOutGdb.Text = _context.OutputGdbPath ?? string.Empty;
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

            this.lblHint.Text = "处理后替换输入GDB原图层；暂存库仅作中间结果。"
                + (audit.Success && !string.IsNullOrEmpty(audit.ReferenceSpatialReferenceName)
                    ? " 基准: " + audit.ReferenceSpatialReferenceName
                    : string.Empty);
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
            if (_context == null)
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
                MessageBox.Show(this, "请指定暂存 File GDB（用于中间结果；可在全局设置中配置输出库）。", "预处理",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            job.ReplaceInInputGdb = true;
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
                "将对选中的 " + job.LayerNames.Count + " 个图层做投影/裁剪，\r\n"
                + "并用正确结果覆盖输入 GDB 中的同名原图层。\r\n\r\n"
                + "输入 GDB:\r\n" + inGdb + "\r\n\r\n"
                + "此操作会删除并替换原图层，建议事先备份。是否继续？",
                "预处理 — 替换输入库图层",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            this.btnRun.Enabled = false;
            this.lblStatus.Text = "正在处理...";
            Application.DoEvents();

            // 释放地图对输入库图层的占用，否则删除/替换可能失败
            try
            {
                MapWorkspaceService.ClearLayersFromObject(_context.MapControl);
            }
            catch
            {
            }

            try
            {
                FeaturePreprocessResult result = FeaturePreprocessBuilder.Run(job, OnProgress);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < result.Messages.Count; i++)
                {
                    sb.AppendLine(result.Messages[i]);
                    _context.LogInfo(result.Messages[i]);
                }

                if (result.Success)
                {
                    _context.OutputGdbPath = job.OutputGdbPath;
                    _context.SaveGlobalSettings();

                    string openMsg;
                    if (_context.OpenFileGdb(inGdb, out openMsg))
                    {
                        _context.ZoomToFullExtent();
                        sb.AppendLine();
                        sb.AppendLine(openMsg);
                    }

                    this.lblStatus.Text = "完成（已替换 " + result.ReplacedLayers.Count + " 个）";
                    LoadLayers();
                    MessageBox.Show(this, sb.ToString(), "预处理完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    this.lblStatus.Text = "未成功";
                    MessageBox.Show(this, sb.ToString(), "预处理", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                this.lblStatus.Text = "失败";
                _context.LogError(ex.ToString());
                MessageBox.Show(this, ex.Message, "预处理失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.btnRun.Enabled = true;
            }
        }

        private void OnProgress(string text, int percent)
        {
            this.lblStatus.Text = percent + "% " + text;
            Application.DoEvents();
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

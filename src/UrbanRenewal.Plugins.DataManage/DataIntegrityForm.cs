using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using UrbanRenewal.Contracts;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Plugins.DataManage
{
    /// <summary>
    /// 数据完整性检查：表格展示各角色匹配结果及是否通过。
    /// </summary>
    public partial class DataIntegrityForm : Form
    {
        private const string PassText = "通过";
        private const string FailText = "未通过";
        private const string OptionalMissText = "选填未配置";

        private readonly IAppContext _context;

        public DataIntegrityForm()
        {
            InitializeComponent();
        }

        public DataIntegrityForm(IAppContext context)
            : this()
        {
            _context = context;
            if (!IsDesignModeSafe())
            {
                RunCheck();
            }
        }

        private static bool IsDesignModeSafe()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RunCheck();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void RunCheck()
        {
            if (_context == null)
            {
                return;
            }

            _context.ReloadGlobalSettings();
            string gdb = _context.GdbPath;
            this.lblGdb.Text = "当前 GDB：" + (string.IsNullOrEmpty(gdb) ? "（未设置）" : gdb);

            CityProfile profile = CityProfileStore.ResolveActive(_context.ActiveCityProfileId);
            if (profile != null)
            {
                LayerRoleCatalog.EnsureRoles(profile);
                this.lblProfile.Text = "配置文件："
                    + (string.IsNullOrEmpty(profile.DisplayName) ? profile.Id : profile.DisplayName)
                    + (string.IsNullOrEmpty(profile.SourcePath)
                        ? string.Empty
                        : " [" + Path.GetFileName(profile.SourcePath) + "]");
            }
            else
            {
                this.lblProfile.Text = "配置文件：（未选择城市配置，将按标准角色关键词匹配）";
            }

            EnsureColumns();
            this.grid.Rows.Clear();

            int passCount = 0;
            int failCount = 0;
            int optionalMiss = 0;

            // —— 环境项 ——
            bool gdbOk = !string.IsNullOrEmpty(gdb) && Directory.Exists(gdb);
            AddEnvRow("InputGdb", "输入 GDB", "—", true,
                gdbOk ? gdb : "（无效）",
                gdbOk, ref passCount, ref failCount, ref optionalMiss);

            string outGdb = _context.OutputGdbPath;
            bool outOk = !string.IsNullOrEmpty(outGdb) && outGdb.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase);
            AddEnvRow("OutputGdb", "输出 GDB", "—", true,
                outOk ? outGdb : "（未设置）",
                outOk, ref passCount, ref failCount, ref optionalMiss);

            bool cityOk = profile != null && !string.IsNullOrEmpty(profile.Id);
            AddEnvRow("CityProfile", "城市配置", "—", false,
                cityOk
                    ? (string.IsNullOrEmpty(profile.DisplayName) ? profile.Id : profile.DisplayName)
                    : "（未选择）",
                cityOk, ref passCount, ref failCount, ref optionalMiss);

            List<string> featureNames = gdbOk
                ? WorkspaceCatalog.ListFeatureClassNames(gdb)
                : new List<string>();
            List<string> rasterNames = gdbOk
                ? WorkspaceCatalog.ListRasterDatasetNames(gdb)
                : new List<string>();

            if (gdbOk)
            {
                SpatialReferenceAuditResult audit = SpatialReferenceAudit.Audit(
                    gdb, null, _context.SpatialRefSourcePath, _context.SpatialRefLayerName);
                string crsDetail;
                bool crsPass;
                if (!audit.Success)
                {
                    crsPass = false;
                    crsDetail = audit.ErrorMessage ?? "检查失败";
                }
                else if (audit.Layers.Count == 0)
                {
                    crsPass = false;
                    crsDetail = "无要素类";
                }
                else
                {
                    crsPass = audit.IsUnified;
                    string cfgTag = string.IsNullOrEmpty(_context.SpatialRefName)
                        ? string.Empty
                        : "（全局配置）";
                    crsDetail = (audit.ReferenceSpatialReferenceName ?? "")
                        + cfgTag
                        + (audit.IsUnified
                            ? "（全部一致）"
                            : "（不一致 " + audit.MismatchedLayers.Count + " 个）");
                }
                AddEnvRow("SpatialRef", "空间参考一致性", "—", false,
                    crsDetail, crsPass, ref passCount, ref failCount, ref optionalMiss);

                bool projPass = audit.Success && audit.ReferenceIsProjected;
                AddEnvRow("ProjectedCrs", "投影坐标系", "—", false,
                    audit.Success
                        ? (audit.ReferenceIsProjected ? "已为投影坐标系" : "非投影（缓冲前需投影）")
                        : "—",
                    projPass, ref passCount, ref failCount, ref optionalMiss);

                List<string> nets = NetworkDatasetHelper.ListNetworkDatasets(gdb);
                string ndExpected = "roadNet\\roadNet_ND";
                if (profile != null && profile.NetworkDataset != null)
                {
                    string fd = profile.NetworkDataset.FeatureDataset ?? string.Empty;
                    string nd = profile.NetworkDataset.Name ?? string.Empty;
                    if (!string.IsNullOrEmpty(fd) || !string.IsNullOrEmpty(nd))
                    {
                        ndExpected = fd + "\\" + nd;
                    }
                }
                bool ndPass = nets.Count > 0;
                string ndDetail = ndPass
                    ? string.Join(", ", nets.ToArray())
                    : "未检测到（期望: " + ndExpected + "）";
                AddEnvRow("NetworkDataset", "预建路网 Network Dataset", "Network", false,
                    ndDetail, ndPass, ref passCount, ref failCount, ref optionalMiss);
            }

            // —— 图层角色 ——
            IList<LayerRoleDefinition> defs = LayerRoleCatalog.GetAll();
            for (int i = 0; i < defs.Count; i++)
            {
                LayerRoleDefinition def = defs[i];
                CityLayerMapping map = FindMapping(profile, def.Role);
                if (map == null)
                {
                    map = new CityLayerMapping();
                    map.Role = def.Role;
                    map.Required = def.Required;
                    map.Keywords = def.DefaultKeywords;
                    map.Name = string.Empty;
                }

                IList<string> pool = def.IsRaster ? (IList<string>)rasterNames : featureNames;
                string resolved = gdbOk ? CityProfile.ResolveLayerName(map, pool) : null;

                string matched = string.IsNullOrEmpty(resolved) ? "（未匹配）" : resolved;
                string status;
                bool isFail;
                bool isOptionalMiss;
                if (!string.IsNullOrEmpty(resolved))
                {
                    status = PassText;
                    isFail = false;
                    isOptionalMiss = false;
                    passCount++;
                }
                else if (def.Required)
                {
                    status = FailText;
                    isFail = true;
                    isOptionalMiss = false;
                    failCount++;
                }
                else
                {
                    status = OptionalMissText;
                    isFail = false;
                    isOptionalMiss = true;
                    optionalMiss++;
                }

                int rowIndex = this.grid.Rows.Add(
                    def.Role,
                    def.DisplayNameZh,
                    def.GeometryType,
                    def.Required ? "必填" : "选填",
                    matched,
                    status);
                StyleRow(this.grid.Rows[rowIndex], def.Required, isFail, isOptionalMiss, !isFail && !isOptionalMiss);
            }

            this.lblSummary.Text = "通过 " + passCount + " / 未通过 " + failCount
                + (optionalMiss > 0 ? " / 选填未配 " + optionalMiss : string.Empty);

            StringBuilder log = new StringBuilder();
            log.Append("完整性检查完成: 通过=").Append(passCount)
                .Append(", 未通过=").Append(failCount)
                .Append(", 选填未配置=").Append(optionalMiss);
            _context.LogInfo(log.ToString());
        }

        private void EnsureColumns()
        {
            if (this.grid.Columns.Count > 0)
            {
                return;
            }

            DataGridViewTextBoxColumn colRole = new DataGridViewTextBoxColumn();
            colRole.Name = "colRole";
            colRole.HeaderText = "图层名 (role)";
            colRole.Width = 120;

            DataGridViewTextBoxColumn colZh = new DataGridViewTextBoxColumn();
            colZh.Name = "colZh";
            colZh.HeaderText = "图层中文名";
            colZh.Width = 150;

            DataGridViewTextBoxColumn colGeom = new DataGridViewTextBoxColumn();
            colGeom.Name = "colGeom";
            colGeom.HeaderText = "几何类型";
            colGeom.Width = 70;

            DataGridViewTextBoxColumn colReq = new DataGridViewTextBoxColumn();
            colReq.Name = "colReq";
            colReq.HeaderText = "必填/选填";
            colReq.Width = 70;

            DataGridViewTextBoxColumn colLayer = new DataGridViewTextBoxColumn();
            colLayer.Name = "colLayer";
            colLayer.HeaderText = "对应图层（匹配结果）";
            colLayer.Width = 220;

            DataGridViewTextBoxColumn colPass = new DataGridViewTextBoxColumn();
            colPass.Name = "colPass";
            colPass.HeaderText = "是否检查通过";
            colPass.Width = 110;

            this.grid.Columns.Add(colRole);
            this.grid.Columns.Add(colZh);
            this.grid.Columns.Add(colGeom);
            this.grid.Columns.Add(colReq);
            this.grid.Columns.Add(colLayer);
            this.grid.Columns.Add(colPass);
        }

        private void AddEnvRow(
            string role,
            string zh,
            string geom,
            bool required,
            string detail,
            bool pass,
            ref int passCount,
            ref int failCount,
            ref int optionalMiss)
        {
            string status;
            bool isFail = false;
            bool isOptionalMiss = false;
            if (pass)
            {
                status = PassText;
                passCount++;
            }
            else
            {
                // 环境检查项：未通过即标红（必填/选填仅作提示）
                status = FailText;
                isFail = true;
                failCount++;
            }

            int rowIndex = this.grid.Rows.Add(
                role,
                zh,
                geom,
                required ? "必填" : "选填",
                detail,
                status);
            StyleRow(this.grid.Rows[rowIndex], required, isFail, isOptionalMiss, pass);
        }

        private static void StyleRow(DataGridViewRow row, bool required, bool isFail, bool isOptionalMiss, bool isPass)
        {
            if (row == null)
            {
                return;
            }

            if (required)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 238);
            }
            else
            {
                row.DefaultCellStyle.BackColor = Color.White;
            }

            DataGridViewCell reqCell = row.Cells["colReq"];
            if (reqCell != null)
            {
                reqCell.Style.ForeColor = required
                    ? Color.FromArgb(192, 57, 43)
                    : Color.FromArgb(39, 174, 96);
                reqCell.Style.Font = new Font(row.DataGridView.Font, required ? FontStyle.Bold : FontStyle.Regular);
            }

            DataGridViewCell passCell = row.Cells["colPass"];
            if (passCell != null)
            {
                if (isFail)
                {
                    passCell.Style.ForeColor = Color.FromArgb(192, 57, 43);
                    passCell.Style.Font = new Font(row.DataGridView.Font, FontStyle.Bold);
                }
                else if (isPass)
                {
                    passCell.Style.ForeColor = Color.FromArgb(39, 174, 96);
                    passCell.Style.Font = new Font(row.DataGridView.Font, FontStyle.Bold);
                }
                else if (isOptionalMiss)
                {
                    passCell.Style.ForeColor = Color.FromArgb(127, 140, 141);
                }
            }
        }

        private static CityLayerMapping FindMapping(CityProfile profile, string role)
        {
            if (profile == null || profile.Layers == null || string.IsNullOrEmpty(role))
            {
                return null;
            }
            for (int i = 0; i < profile.Layers.Count; i++)
            {
                CityLayerMapping map = profile.Layers[i];
                if (map != null && string.Equals(map.Role, role, StringComparison.OrdinalIgnoreCase))
                {
                    return map;
                }
            }
            return null;
        }
    }
}

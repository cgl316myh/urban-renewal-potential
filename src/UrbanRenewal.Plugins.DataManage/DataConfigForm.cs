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
    /// <summary>图层角色映射，保存到城市配置 XML。</summary>
    public partial class DataConfigForm : Form
    {
        private const string Unselected = "（未选择）";
        private readonly IAppContext _context;
        private CityProfile _profile;
        private List<string> _featureNames = new List<string>();
        private List<string> _rasterNames = new List<string>();
        private readonly List<LayerRoleDefinition> _roleOrder = new List<LayerRoleDefinition>();

        public DataConfigForm()
        {
            InitializeComponent();
        }

        public DataConfigForm(IAppContext context)
            : this()
        {
            _context = context;
            if (!IsDesignModeSafe())
            {
                LoadData();
            }
        }

        private static bool IsDesignModeSafe()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }

        private void LoadData()
        {
            if (_context == null)
            {
                return;
            }
            _context.ReloadGlobalSettings();
            string gdb = _context.GdbPath;
            this.lblGdb.Text = "当前 GDB：" + (string.IsNullOrEmpty(gdb) ? "（未设置）" : gdb);

            _featureNames = WorkspaceCatalog.ListFeatureClassNames(gdb);
            _rasterNames = WorkspaceCatalog.ListRasterDatasetNames(gdb);
            this.lblCount.Text = "要素类 " + _featureNames.Count + " 个，栅格 " + _rasterNames.Count + " 个";

            _profile = CityProfileStore.ResolveActive(_context.ActiveCityProfileId);
            if (_profile == null)
            {
                _profile = new CityProfile();
                _profile.Id = "Local";
                _profile.DisplayName = "本地数据配置";
                _profile.IsDefault = true;
                _profile.CellSize = 30;
                _profile.TrafficWeight = 0.30;
                _profile.EnvironmentWeight = 0.20;
                _profile.FacilityWeight = 0.25;
                _profile.PolicyWeight = 0.25;
            }
            LayerRoleCatalog.EnsureRoles(_profile);
            this.lblProfile.Text = "配置文件："
                + (string.IsNullOrEmpty(_profile.SourcePath)
                    ? "（新建，保存后写入 Config/Cities/" + _profile.Id + ".xml）"
                    : Path.GetFileName(_profile.SourcePath));

            if (_profile.NetworkDataset == null)
            {
                _profile.NetworkDataset = new CityNetworkDataset();
                _profile.NetworkDataset.FeatureDataset = "roadNet";
                _profile.NetworkDataset.Name = "roadNet_ND";
                _profile.NetworkDataset.ImpedanceAttribute = "Length";
            }
            this.txtNdFd.Text = _profile.NetworkDataset.FeatureDataset ?? string.Empty;
            this.txtNdName.Text = _profile.NetworkDataset.Name ?? string.Empty;
            this.txtNdImp.Text = _profile.NetworkDataset.ImpedanceAttribute ?? "Length";

            BuildGrid();
        }

        private void BuildGrid()
        {
            this.grid.Rows.Clear();
            this.grid.Columns.Clear();
            _roleOrder.Clear();

            DataGridViewTextBoxColumn colRole = new DataGridViewTextBoxColumn();
            colRole.Name = "colRole";
            colRole.HeaderText = "图层名 (role)";
            colRole.ReadOnly = true;
            colRole.Width = 120;

            DataGridViewTextBoxColumn colZh = new DataGridViewTextBoxColumn();
            colZh.Name = "colZh";
            colZh.HeaderText = "图层中文名";
            colZh.ReadOnly = true;
            colZh.Width = 160;

            DataGridViewTextBoxColumn colGeom = new DataGridViewTextBoxColumn();
            colGeom.Name = "colGeom";
            colGeom.HeaderText = "几何类型";
            colGeom.ReadOnly = true;
            colGeom.Width = 80;

            DataGridViewTextBoxColumn colReq = new DataGridViewTextBoxColumn();
            colReq.Name = "colReq";
            colReq.HeaderText = "必填/选填";
            colReq.ReadOnly = true;
            colReq.Width = 70;

            DataGridViewComboBoxColumn colLayer = new DataGridViewComboBoxColumn();
            colLayer.Name = "colLayer";
            colLayer.HeaderText = "选择对应的图层（当前 GDB）";
            colLayer.Width = 260;
            colLayer.FlatStyle = FlatStyle.Flat;

            this.grid.Columns.Add(colRole);
            this.grid.Columns.Add(colZh);
            this.grid.Columns.Add(colGeom);
            this.grid.Columns.Add(colReq);
            this.grid.Columns.Add(colLayer);

            IList<LayerRoleDefinition> defs = LayerRoleCatalog.GetAll();
            for (int i = 0; i < defs.Count; i++)
            {
                LayerRoleDefinition def = defs[i];
                _roleOrder.Add(def);

                string selected = ResolveSelectedName(def);
                List<string> choices = BuildChoices(def, selected);

                int rowIndex = this.grid.Rows.Add();
                DataGridViewRow row = this.grid.Rows[rowIndex];
                row.Cells["colRole"].Value = def.Role;
                row.Cells["colZh"].Value = def.DisplayNameZh;
                row.Cells["colGeom"].Value = def.GeometryType;
                row.Cells["colReq"].Value = def.Required ? "必填" : "选填";

                DataGridViewComboBoxCell combo = row.Cells["colLayer"] as DataGridViewComboBoxCell;
                if (combo != null)
                {
                    combo.Items.Clear();
                    for (int c = 0; c < choices.Count; c++)
                    {
                        combo.Items.Add(choices[c]);
                    }
                    combo.Value = string.IsNullOrEmpty(selected) ? Unselected : selected;
                }

                if (def.Required)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 238);
                    row.Cells["colReq"].Style.ForeColor = Color.FromArgb(192, 57, 43);
                    row.Cells["colReq"].Style.Font = new Font(this.grid.Font, FontStyle.Bold);
                }
                else
                {
                    row.DefaultCellStyle.BackColor = Color.White;
                    row.Cells["colReq"].Style.ForeColor = Color.FromArgb(39, 174, 96);
                }
            }
        }

        private string ResolveSelectedName(LayerRoleDefinition def)
        {
            if (_profile == null || _profile.Layers == null || def == null)
            {
                return null;
            }
            for (int i = 0; i < _profile.Layers.Count; i++)
            {
                CityLayerMapping map = _profile.Layers[i];
                if (map == null || !string.Equals(map.Role, def.Role, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(map.Name))
                {
                    IList<string> pool = def.IsRaster ? (IList<string>)_rasterNames : _featureNames;
                    for (int p = 0; p < pool.Count; p++)
                    {
                        if (string.Equals(pool[p], map.Name, StringComparison.OrdinalIgnoreCase)
                            || pool[p].IndexOf(map.Name, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            return pool[p];
                        }
                    }
                    // 配置名当前 GDB 中不存在，仍显示配置值便于用户改选
                    return map.Name;
                }
                if (!string.IsNullOrEmpty(map.Keywords))
                {
                    string[] parts = map.Keywords.Split(new char[] { ',', '，', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    IList<string> pool = def.IsRaster ? (IList<string>)_rasterNames : _featureNames;
                    return WorkspaceCatalog.FindByKeywords(pool, parts);
                }
            }
            // 无配置时按默认关键词猜一次
            if (!string.IsNullOrEmpty(def.DefaultKeywords))
            {
                string[] parts = def.DefaultKeywords.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                IList<string> pool = def.IsRaster ? (IList<string>)_rasterNames : _featureNames;
                return WorkspaceCatalog.FindByKeywords(pool, parts);
            }
            return null;
        }

        private List<string> BuildChoices(LayerRoleDefinition def, string selected)
        {
            List<string> list = new List<string>();
            list.Add(Unselected);
            IList<string> pool = def.IsRaster ? (IList<string>)_rasterNames : _featureNames;
            for (int i = 0; i < pool.Count; i++)
            {
                list.Add(pool[i]);
            }
            if (!string.IsNullOrEmpty(selected) && list.IndexOf(selected) < 0)
            {
                list.Add(selected);
            }
            return list;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_context == null || _profile == null)
            {
                return;
            }

            // 结束编辑
            this.grid.EndEdit();

            StringBuilder missing = new StringBuilder();
            for (int i = 0; i < _roleOrder.Count && i < this.grid.Rows.Count; i++)
            {
                LayerRoleDefinition def = _roleOrder[i];
                object val = this.grid.Rows[i].Cells["colLayer"].Value;
                string chosen = val == null ? null : Convert.ToString(val);
                if (string.Equals(chosen, Unselected, StringComparison.Ordinal) || string.IsNullOrEmpty(chosen))
                {
                    chosen = string.Empty;
                }

                CityLayerMapping map = FindOrCreateMapping(def.Role);
                map.Name = chosen;
                map.Required = def.Required;
                if (string.IsNullOrEmpty(map.Keywords))
                {
                    map.Keywords = def.DefaultKeywords;
                }

                if (def.Required && string.IsNullOrEmpty(chosen))
                {
                    missing.AppendLine("- " + def.Role + "（" + def.DisplayNameZh + "）");
                }
            }

            if (missing.Length > 0)
            {
                DialogResult dr = MessageBox.Show(this,
                    "以下必填图层尚未选择对应数据，仍要保存吗？\r\n\r\n" + missing.ToString(),
                    "数据配置", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dr != DialogResult.Yes)
                {
                    return;
                }
            }

            if (_profile.NetworkDataset == null)
            {
                _profile.NetworkDataset = new CityNetworkDataset();
            }
            _profile.NetworkDataset.FeatureDataset = this.txtNdFd.Text != null ? this.txtNdFd.Text.Trim() : string.Empty;
            _profile.NetworkDataset.Name = this.txtNdName.Text != null ? this.txtNdName.Text.Trim() : string.Empty;
            _profile.NetworkDataset.ImpedanceAttribute = this.txtNdImp.Text != null ? this.txtNdImp.Text.Trim() : "Length";

            string path = _profile.SourcePath;
            if (string.IsNullOrEmpty(path))
            {
                string dir = CityProfileStore.GetCitiesDirectory();
                Directory.CreateDirectory(dir);
                if (string.IsNullOrEmpty(_profile.Id))
                {
                    _profile.Id = "Local";
                }
                path = Path.Combine(dir, _profile.Id + ".xml");
            }

            CityProfileStore.Save(_profile, path);
            _profile.SourcePath = path;
            _context.ActiveCityProfileId = _profile.Id;
            CityProfileStore.RememberId(_profile.Id);
            _context.SaveGlobalSettings();

            _context.LogInfo("数据配置已保存: " + path);
            MessageBox.Show(this,
                "已保存到本地：\r\n" + path
                + "\r\n\r\n启动后将按此配置匹配图层参与动力性/可行度/叠置/验证等运算。",
                "数据配置", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private CityLayerMapping FindOrCreateMapping(string role)
        {
            if (_profile.Layers == null)
            {
                _profile.Layers = new List<CityLayerMapping>();
            }
            for (int i = 0; i < _profile.Layers.Count; i++)
            {
                if (_profile.Layers[i] != null
                    && string.Equals(_profile.Layers[i].Role, role, StringComparison.OrdinalIgnoreCase))
                {
                    return _profile.Layers[i];
                }
            }
            CityLayerMapping map = new CityLayerMapping();
            map.Role = role;
            _profile.Layers.Add(map);
            return map;
        }

        private void btnAuto_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < _roleOrder.Count && i < this.grid.Rows.Count; i++)
            {
                LayerRoleDefinition def = _roleOrder[i];
                string guessed = null;
                if (!string.IsNullOrEmpty(def.DefaultKeywords))
                {
                    string[] parts = def.DefaultKeywords.Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                    IList<string> pool = def.IsRaster ? (IList<string>)_rasterNames : _featureNames;
                    guessed = WorkspaceCatalog.FindByKeywords(pool, parts);
                }
                DataGridViewComboBoxCell combo = this.grid.Rows[i].Cells["colLayer"] as DataGridViewComboBoxCell;
                if (combo != null)
                {
                    combo.Value = string.IsNullOrEmpty(guessed) ? Unselected : guessed;
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void grid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            // ComboBox 值不在列表时避免弹未处理异常
            e.ThrowException = false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using UrbanRenewal.Analysis;
using UrbanRenewal.Contracts;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Plugins.Overlay
{
    public partial class ParcelLinkRunForm : Form
    {
        private readonly IAppContext _context;

        public ParcelLinkRunForm()
        {
            InitializeComponent();
        }

        public ParcelLinkRunForm(IAppContext context)
            : this()
        {
            _context = context;
            if (!IsDesignModeSafe())
            {
                RefreshGlobalInfo();
            }
        }

        private static bool IsDesignModeSafe()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        }

        private void RefreshGlobalInfo()
        {
            if (_context == null)
            {
                return;
            }
            _context.ReloadGlobalSettings();
            string outGdb = _context.OutputGdbPath;
            this.lblOutInfo.Text = "输出 GDB："
                + (string.IsNullOrEmpty(outGdb) ? "（未设置）" : outGdb);
            CityProfile profile = CityProfileStore.ResolveActive(_context.ActiveCityProfileId);
            this.lblCityInfo.Text = profile != null
                ? ("城市配置：" + profile.DisplayName + " [" + profile.Id + "]")
                : "城市配置：（未设置）";
        }

        private void btnOpenGlobal_Click(object sender, EventArgs e)
        {
            if (_context == null)
            {
                return;
            }
            _context.ShowGlobalSettings();
            RefreshGlobalInfo();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if (_context == null)
            {
                MessageBox.Show(this, "运行上下文无效。", "宗地关联", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _context.ReloadGlobalSettings();
            string gdb = _context.GdbPath;
            string outGdb = _context.OutputGdbPath;
            if (string.IsNullOrEmpty(gdb) || !System.IO.Directory.Exists(gdb))
            {
                MessageBox.Show(this, "请先打开输入 GDB。", "宗地关联", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(outGdb) || !outGdb.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(this, "请先指定输出 File GDB。", "宗地关联", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ParcelLinkJob job = new ParcelLinkJob();
            job.GdbPath = gdb;
            job.OutputGdbPath = outGdb;
            job.StatisticType = this.cboStat.SelectedIndex == 1 ? "MAXIMUM" : "MEAN";

            CityProfile profile = CityProfileStore.ResolveActive(_context.ActiveCityProfileId);
            List<string> applyMsgs = new List<string>();
            if (profile != null)
            {
                List<string> names = WorkspaceCatalog.ListFeatureClassNames(gdb);
                List<string> rasters = WorkspaceCatalog.ListRasterDatasetNames(gdb);
                // 仅取 Parcel / StudyArea 提示
                FeasibilityJob tmp = new FeasibilityJob();
                profile.ApplyToFeasibilityJob(tmp, names, rasters, applyMsgs);
                if (tmp.LayerHints != null)
                {
                    if (tmp.LayerHints.ContainsKey("Parcel"))
                    {
                        job.LayerHints["Parcel"] = tmp.LayerHints["Parcel"];
                    }
                    if (tmp.LayerHints.ContainsKey("StudyArea"))
                    {
                        job.LayerHints["StudyArea"] = tmp.LayerHints["StudyArea"];
                    }
                }
            }

            this.btnRun.Enabled = false;
            this.lblStatus.Text = "正在关联...";
            Application.DoEvents();

            try
            {
                ParcelLinkEngine engine = new ParcelLinkEngine();
                ParcelLinkResult result = engine.Run(job, OnProgress);

                if (result.Success && !string.IsNullOrEmpty(result.OutputGdbPath))
                {
                    _context.OutputGdbPath = result.OutputGdbPath;
                    _context.SaveGlobalSettings();
                }

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < applyMsgs.Count; i++)
                {
                    sb.AppendLine(applyMsgs[i]);
                }
                for (int i = 0; i < result.Messages.Count; i++)
                {
                    sb.AppendLine(result.Messages[i]);
                    _context.LogInfo(result.Messages[i]);
                }

                if (result.Success && !string.IsNullOrEmpty(result.ParcelFeatureClassPath))
                {
                    string msg;
                    if (_context.AddFeatureLayer(result.ParcelFeatureClassPath, "宗地潜力", out msg))
                    {
                        sb.AppendLine(msg);
                        _context.ZoomToFullExtent();
                    }
                    else
                    {
                        sb.AppendLine(msg);
                    }
                }

                this.lblStatus.Text = result.Success ? "完成" : "失败";
                MessageBox.Show(this, sb.ToString(), "宗地关联", MessageBoxButtons.OK,
                    result.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                this.lblStatus.Text = "异常";
                _context.LogError(ex.Message);
                MessageBox.Show(this, ex.Message, "宗地关联失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.btnRun.Enabled = true;
                _context.HideProgress();
                RefreshGlobalInfo();
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

using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace UrbanRenewal.Host
{
    public partial class AttributeTableForm : Form
    {
        private const int MaxRows = 50000;

        public AttributeTableForm()
        {
            InitializeComponent();
        }

        public void LoadFeatureLayer(IFeatureLayer featureLayer)
        {
            if (featureLayer == null || featureLayer.FeatureClass == null)
            {
                MessageBox.Show("无效的矢量图层。", "提示");
                return;
            }

            string layerName = featureLayer.Name;
            ITable table = featureLayer.FeatureClass as ITable;
            DataTable dt = BuildDataTableFromITable(table);
            BindTable(dt, layerName);
        }

        public void LoadFromFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                MessageBox.Show("文件不存在。", "提示");
                return;
            }

            string ext = Path.GetExtension(filePath).ToLower();
            DataTable dt = null;

            try
            {
                if (ext == ".csv" || ext == ".txt")
                {
                    dt = ReadCsv(filePath);
                }
                else if (ext == ".dbf")
                {
                    dt = ReadDbfViaShapeWorkspace(filePath);
                }
                else
                {
                    MessageBox.Show("暂支持 .csv / .txt / .dbf 表格文件。", "提示");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("打开表格失败：\n" + ex.Message, "错误");
                return;
            }

            if (dt != null)
            {
                BindTable(dt, Path.GetFileName(filePath));
            }
        }

        private void BindTable(DataTable dt, string title)
        {
            dataGridView1.DataSource = dt;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;

            int count = dt == null ? 0 : dt.Rows.Count;
            string tip = count >= MaxRows ? "（已达上限，仅显示前 " + MaxRows + " 行）" : "";
            lblInfo.Text = title + "    记录数：" + count + tip;
            this.Text = "属性表 - " + title;
        }

        private DataTable BuildDataTableFromITable(ITable table)
        {
            DataTable dt = new DataTable();
            if (table == null)
            {
                return dt;
            }

            IFields fields = table.Fields;
            for (int i = 0; i < fields.FieldCount; i++)
            {
                IField field = fields.get_Field(i);
                if (field.Type == esriFieldType.esriFieldTypeGeometry)
                {
                    continue;
                }

                string colName = field.Name;
                if (dt.Columns.Contains(colName))
                {
                    colName = colName + "_" + i;
                }

                dt.Columns.Add(colName, typeof(string));
            }

            ICursor cursor = table.Search(null, false);
            try
            {
                IRow row = cursor.NextRow();
                int rowCount = 0;
                while (row != null && rowCount < MaxRows)
                {
                    DataRow dr = dt.NewRow();
                    int colIndex = 0;
                    for (int i = 0; i < fields.FieldCount; i++)
                    {
                        IField field = fields.get_Field(i);
                        if (field.Type == esriFieldType.esriFieldTypeGeometry)
                        {
                            continue;
                        }

                        object val = row.get_Value(i);
                        dr[colIndex] = val == null || val is DBNull ? "" : val.ToString();
                        colIndex++;
                    }

                    dt.Rows.Add(dr);
                    rowCount++;
                    row = cursor.NextRow();
                }
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(cursor);
            }

            return dt;
        }

        private DataTable ReadCsv(string filePath)
        {
            DataTable dt = new DataTable();
            string[] lines = File.ReadAllLines(filePath, Encoding.Default);
            if (lines.Length == 0)
            {
                return dt;
            }

            string[] headers = SplitCsvLine(lines[0]);
            for (int i = 0; i < headers.Length; i++)
            {
                string name = string.IsNullOrEmpty(headers[i]) ? "列" + (i + 1) : headers[i].Trim();
                if (dt.Columns.Contains(name))
                {
                    name = name + "_" + i;
                }
                dt.Columns.Add(name);
            }

            int max = Math.Min(lines.Length - 1, MaxRows);
            for (int r = 1; r <= max; r++)
            {
                if (string.IsNullOrWhiteSpace(lines[r]))
                {
                    continue;
                }

                string[] cells = SplitCsvLine(lines[r]);
                DataRow dr = dt.NewRow();
                for (int c = 0; c < dt.Columns.Count; c++)
                {
                    dr[c] = c < cells.Length ? cells[c] : "";
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }

        private string[] SplitCsvLine(string line)
        {
            return line.Split(',');
        }

        private DataTable ReadDbfViaShapeWorkspace(string dbfPath)
        {
            string folder = Path.GetDirectoryName(dbfPath);
            string name = Path.GetFileNameWithoutExtension(dbfPath);

            ESRI.ArcGIS.Geodatabase.IWorkspaceFactory factory =
                new ESRI.ArcGIS.DataSourcesFile.ShapefileWorkspaceFactoryClass();
            IWorkspace workspace = factory.OpenFromFile(folder, 0);
            IFeatureWorkspace featureWorkspace = workspace as IFeatureWorkspace;

            ITable table = null;
            try
            {
                table = featureWorkspace.OpenTable(name);
            }
            catch
            {
                throw new Exception("无法打开 DBF 表，请确认文件有效且未被占用。");
            }

            return BuildDataTableFromITable(table);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

using System;
using System.IO;
using ESRI.ArcGIS.DataManagementTools;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// 输出 File GDB：创建/确保存在，并生成库内数据集路径。
    /// </summary>
    public static class OutputGdbHelper
    {
        public static bool IsFileGdbPath(string path)
        {
            return !string.IsNullOrEmpty(path)
                && path.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 若不存在则创建 File GDB；返回规范化路径。
        /// </summary>
        public static string EnsureExists(GeoprocessorHelper gp, string gdbPath)
        {
            if (string.IsNullOrEmpty(gdbPath))
            {
                throw new ArgumentException("输出 GDB 路径为空。");
            }
            if (!IsFileGdbPath(gdbPath))
            {
                throw new ArgumentException("输出路径必须是 File GDB（*.gdb 文件夹）: " + gdbPath);
            }

            string full = Path.GetFullPath(gdbPath);
            if (Directory.Exists(full))
            {
                return full;
            }

            string parent = Path.GetDirectoryName(full);
            string name = Path.GetFileName(full);
            if (string.IsNullOrEmpty(parent))
            {
                throw new ArgumentException("无法解析输出 GDB 父目录: " + gdbPath);
            }
            Directory.CreateDirectory(parent);

            if (gp == null)
            {
                throw new ArgumentNullException("gp");
            }

            CreateFileGDB create = new CreateFileGDB();
            create.out_folder_path = parent;
            create.out_name = name;
            gp.Execute(create, "CreateFileGDB");
            return full;
        }

        public static string DatasetPath(string gdbPath, string datasetName)
        {
            if (string.IsNullOrEmpty(gdbPath) || string.IsNullOrEmpty(datasetName))
            {
                return null;
            }
            return Path.Combine(gdbPath, datasetName);
        }

        /// <summary>
        /// 删除 GDB 内已有同名要素类/栅格（覆盖失败时的兜底）。
        /// </summary>
        public static void TryDeleteDataset(GeoprocessorHelper gp, string datasetPath)
        {
            if (string.IsNullOrEmpty(datasetPath) || gp == null)
            {
                return;
            }
            try
            {
                Delete delete = new Delete();
                delete.in_data = datasetPath;
                gp.Execute(delete, "Delete-" + Path.GetFileName(datasetPath));
            }
            catch
            {
            }
        }

        public static string SuggestDefaultBesideInput(string inputGdbPath)
        {
            string parent;
            if (!string.IsNullOrEmpty(inputGdbPath) && Directory.Exists(inputGdbPath))
            {
                parent = Path.GetDirectoryName(inputGdbPath);
            }
            else
            {
                parent = Path.Combine(Path.GetTempPath(), "UrbanRenewal");
            }
            if (string.IsNullOrEmpty(parent))
            {
                parent = Path.GetTempPath();
            }
            return Path.Combine(parent, "Motivation_Output.gdb");
        }

        public static string GetRememberFilePath()
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
            return Path.Combine(dir, "_last_output_gdb.txt");
        }

        public static string LoadRemembered()
        {
            try
            {
                string path = GetRememberFilePath();
                if (File.Exists(path))
                {
                    return File.ReadAllText(path).Trim();
                }
            }
            catch
            {
            }
            return null;
        }

        public static void Remember(string gdbPath)
        {
            if (string.IsNullOrEmpty(gdbPath))
            {
                return;
            }
            try
            {
                string file = GetRememberFilePath();
                string dir = Path.GetDirectoryName(file);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                File.WriteAllText(file, gdbPath.Trim());
            }
            catch
            {
            }
        }
    }
}

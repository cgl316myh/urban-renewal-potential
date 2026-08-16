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
                string gdb;
                string name;
                if (TrySplitGdbDataset(datasetPath, out gdb, out name))
                {
                    string msg;
                    if (FileGdbLockHelper.TryDeleteFeatureClassExclusive(gdb, name, out msg))
                    {
                        return;
                    }
                    // 栅格（含 VAT）常被地图占用；要素类删除失败后再试栅格删除
                    if (FileGdbLockHelper.TryDeleteRasterExclusive(gdb, name, out msg))
                    {
                        return;
                    }
                }
            }
            catch
            {
            }

            FileGdbLockHelper.ForceComRelease();

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

        private static bool TrySplitGdbDataset(string datasetPath, out string gdb, out string name)
        {
            gdb = null;
            name = null;
            if (string.IsNullOrEmpty(datasetPath)
                || datasetPath.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string full = Path.GetFullPath(datasetPath);
            string parent = Path.GetDirectoryName(full);
            name = Path.GetFileName(full);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name))
            {
                return false;
            }

            if (parent.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
            {
                gdb = parent;
                return true;
            }

            string up = Path.GetDirectoryName(parent);
            if (!string.IsNullOrEmpty(up) && up.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
            {
                gdb = up;
                name = Path.GetFileName(parent) + "\\" + name;
                return true;
            }

            return false;
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

        /// <summary>
        /// 预处理/裁切结果库（与分析输出库分离）：全局输入 → clip.gdb。
        /// </summary>
        public static string SuggestClipGdbBeside(string inputGdbPath)
        {
            string parent;
            if (!string.IsNullOrEmpty(inputGdbPath) && Directory.Exists(inputGdbPath))
            {
                parent = Path.GetDirectoryName(inputGdbPath);
            }
            else if (!string.IsNullOrEmpty(inputGdbPath))
            {
                try { parent = Path.GetDirectoryName(Path.GetFullPath(inputGdbPath)); }
                catch { parent = null; }
            }
            else
            {
                parent = null;
            }
            if (string.IsNullOrEmpty(parent))
            {
                parent = Path.Combine(Path.GetTempPath(), "UrbanRenewal");
            }
            return Path.Combine(parent, "clip.gdb");
        }

        /// <summary>
        /// 分析工作库：与输入/预处理库分开，避免缓冲中间层与源数据同库抢锁。
        /// 例如 Motivation_Output.gdb → Motivation_Output_Work.gdb
        /// </summary>
        public static string SuggestWorkGdbBeside(string sourceGdbPath)
        {
            if (string.IsNullOrEmpty(sourceGdbPath))
            {
                return Path.Combine(Path.GetTempPath(), "UrbanRenewal", "Analysis_Work.gdb");
            }
            string full = Path.GetFullPath(sourceGdbPath);
            string parent = Path.GetDirectoryName(full);
            string leaf = Path.GetFileName(full);
            if (string.IsNullOrEmpty(parent))
            {
                parent = Path.GetTempPath();
            }
            string stem = leaf;
            if (stem.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
            {
                stem = stem.Substring(0, stem.Length - 4);
            }
            if (stem.EndsWith("_Work", StringComparison.OrdinalIgnoreCase))
            {
                return Path.Combine(parent, stem + ".gdb");
            }
            return Path.Combine(parent, stem + "_Work.gdb");
        }

        /// <summary>判断两个 GDB 路径是否指向同一库。</summary>
        public static bool IsSameGdb(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            {
                return false;
            }
            try
            {
                string fa = Path.GetFullPath(a).TrimEnd('\\', '/');
                string fb = Path.GetFullPath(b).TrimEnd('\\', '/');
                return string.Equals(fa, fb, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 分析输出须与输入（通常为 clip.gdb）分库；同库时改用旁路 _Work.gdb。
        /// </summary>
        public static string EnsureSeparateAnalysisOutput(string inputGdb, string outputGdb, out string note)
        {
            note = null;
            if (string.IsNullOrEmpty(outputGdb))
            {
                return outputGdb;
            }
            if (!IsSameGdb(inputGdb, outputGdb))
            {
                return outputGdb;
            }
            string work = SuggestWorkGdbBeside(inputGdb);
            note = "输入与分析输出同库，已改用工作库: " + work
                + "（分析数据应从 clip.gdb 读取，结果写入独立输出库）";
            return work;
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

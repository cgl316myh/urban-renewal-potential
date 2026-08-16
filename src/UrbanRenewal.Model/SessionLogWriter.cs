using System;
using System.IO;
using System.Text;

namespace UrbanRenewal.Model
{
    /// <summary>
    /// 会话运行日志：仅在已配置「输入 GDB」时落盘，目录为该 GDB 同级文件夹，
    /// 文件名 log_yyyyMMddHHmmss.txt；边输出边 Flush。未配置输入 GDB 时不写本地文件。
    /// </summary>
    public static class SessionLogWriter
    {
        private static readonly object Sync = new object();
        private static StreamWriter _writer;
        private static string _filePath;
        private static string _directory;

        public static bool IsFileLoggingEnabled
        {
            get
            {
                lock (Sync)
                {
                    return _writer != null;
                }
            }
        }

        public static string CurrentFilePath
        {
            get
            {
                lock (Sync)
                {
                    return _filePath;
                }
            }
        }

        public static string CurrentDirectory
        {
            get
            {
                lock (Sync)
                {
                    return _directory;
                }
            }
        }

        /// <summary>
        /// 仅当设置了输入 GDB 时返回其父目录；否则返回 null（表示不落盘）。
        /// </summary>
        public static string ResolveLogDirectory()
        {
            return ResolveLogDirectory(GlobalAppSettingsStore.Load());
        }

        public static string ResolveLogDirectory(GlobalAppSettings settings)
        {
            if (settings == null)
            {
                settings = GlobalAppSettingsStore.Load();
            }
            if (settings == null || string.IsNullOrEmpty(settings.InputGdbPath))
            {
                return null;
            }
            return GetGdbParentFolder(settings.InputGdbPath);
        }

        public static string GetGdbParentFolder(string gdbPath)
        {
            if (string.IsNullOrEmpty(gdbPath))
            {
                return null;
            }
            try
            {
                string full = Path.GetFullPath(gdbPath.Trim().TrimEnd('\\', '/'));
                if (full.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
                {
                    string parent = Path.GetDirectoryName(full);
                    return string.IsNullOrEmpty(parent) ? null : parent;
                }
                if (Directory.Exists(full))
                {
                    return full;
                }
                string asParent = Path.GetDirectoryName(full);
                return string.IsNullOrEmpty(asParent) ? null : asParent;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 按当前输入 GDB 启动/关闭落盘。无输入 GDB 时关闭文件并返回 null。
        /// </summary>
        public static string StartNewSession()
        {
            return StartNewSession(null);
        }

        public static string StartNewSession(GlobalAppSettings settings)
        {
            lock (Sync)
            {
                return StartNewSessionNoLock(settings);
            }
        }

        /// <summary>
        /// 输入 GDB 变化时调用：有 GDB 则立即在同目录开日志；无 GDB 则停止落盘。
        /// </summary>
        public static string EnsureDirectoryMatchesSettings(GlobalAppSettings settings)
        {
            lock (Sync)
            {
                string target = ResolveLogDirectory(settings);
                if (string.IsNullOrEmpty(target))
                {
                    CloseNoLock(false);
                    _filePath = null;
                    _directory = null;
                    return null;
                }
                if (_writer != null
                    && !string.IsNullOrEmpty(_directory)
                    && string.Equals(_directory, target, StringComparison.OrdinalIgnoreCase))
                {
                    return _filePath;
                }
                return StartNewSessionNoLock(settings);
            }
        }

        public static void Append(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return;
            }
            lock (Sync)
            {
                if (_writer == null)
                {
                    return;
                }
                try
                {
                    _writer.WriteLine(line);
                    _writer.Flush();
                }
                catch
                {
                }
            }
        }

        /// <summary>写入异常详情（若当前未落盘则忽略；崩溃兜底见 WriteCrashReport）。</summary>
        public static void AppendException(string title, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("======== " + (title ?? "异常") + " ========");
            sb.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            if (ex != null)
            {
                sb.AppendLine(ex.ToString());
            }
            else
            {
                sb.AppendLine("(无异常对象)");
            }
            sb.AppendLine("========");
            Append(sb.ToString());
        }

        /// <summary>
        /// 未处理异常兜底：优先写会话日志；否则写 %LocalAppData%\UrbanRenewal\crash_*.txt。
        /// </summary>
        public static string WriteCrashReport(string source, Exception ex)
        {
            string text = BuildCrashText(source, ex);
            lock (Sync)
            {
                if (_writer != null)
                {
                    try
                    {
                        _writer.WriteLine(text);
                        _writer.Flush();
                        return _filePath;
                    }
                    catch
                    {
                    }
                }
            }

            try
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "UrbanRenewal");
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, "crash_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
                File.WriteAllText(path, text, new UTF8Encoding(true));
                return path;
            }
            catch
            {
                return null;
            }
        }

        public static void Close()
        {
            lock (Sync)
            {
                CloseNoLock(true);
                _filePath = null;
                _directory = null;
            }
        }

        private static string StartNewSessionNoLock(GlobalAppSettings settings)
        {
            CloseNoLock(true);
            if (settings == null)
            {
                settings = GlobalAppSettingsStore.Load();
            }
            string dir = ResolveLogDirectory(settings);
            if (string.IsNullOrEmpty(dir))
            {
                _filePath = null;
                _directory = null;
                return null;
            }

            Directory.CreateDirectory(dir);
            string name = "log_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
            _directory = dir;
            _filePath = Path.Combine(dir, name);
            FileStream fs = new FileStream(
                _filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.Read);
            _writer = new StreamWriter(fs, new UTF8Encoding(true));
            _writer.AutoFlush = true;
            _writer.WriteLine("# 城市更新潜力评价与验证系统 — 运行日志");
            _writer.WriteLine("# 开始: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            _writer.WriteLine("# 输入GDB: " + (settings.InputGdbPath ?? ""));
            _writer.WriteLine("# 日志目录: " + dir);
            _writer.WriteLine("# 写入方式: 边输出边保存");
            _writer.WriteLine("# --------------------------------");
            _writer.Flush();
            return _filePath;
        }

        private static void CloseNoLock(bool writeFooter)
        {
            if (_writer != null)
            {
                try
                {
                    if (writeFooter)
                    {
                        _writer.WriteLine("# 结束: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    _writer.Flush();
                    _writer.Dispose();
                }
                catch
                {
                }
                _writer = null;
            }
        }

        private static string BuildCrashText(string source, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("======== 未处理异常 / 崩溃报告 ========");
            sb.AppendLine("时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("来源: " + (source ?? "(未知)"));
            sb.AppendLine("--------------------------------");
            if (ex != null)
            {
                sb.AppendLine(ex.ToString());
            }
            else
            {
                sb.AppendLine("(无异常对象，可能为非托管/强制终止)");
            }
            sb.AppendLine("========");
            return sb.ToString();
        }
    }
}

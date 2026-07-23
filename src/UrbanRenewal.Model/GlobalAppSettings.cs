using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace UrbanRenewal.Model
{
    /// <summary>
    /// 全局工作区设置：输出 GDB、城市配置等，设置一次全模块共用。
    /// </summary>
    [XmlRoot("GlobalAppSettings")]
    public class GlobalAppSettings
    {
        public GlobalAppSettings()
        {
            SkinName = "Office 2013";
            PluginsDirectoryName = "Plugins";
            MotivationWeight = 0.7;
            FeasibilityWeight = 0.3;
        }

        public string SkinName { get; set; }

        public string PluginsDirectoryName { get; set; }

        /// <summary>输入工作空间 GDB。</summary>
        public string InputGdbPath { get; set; }

        /// <summary>分析结果输出 GDB（中间与结果数据均写入）。</summary>
        public string OutputGdbPath { get; set; }

        /// <summary>当前城市配置 Id。</summary>
        public string ActiveCityProfileId { get; set; }

        public double MotivationWeight { get; set; }

        public double FeasibilityWeight { get; set; }
    }

    /// <summary>
    /// 读写 Config/app_settings.xml。
    /// </summary>
    public static class GlobalAppSettingsStore
    {
        public static string GetSettingsFilePath()
        {
            string dir = GetConfigDirectory();
            return Path.Combine(dir, "app_settings.xml");
        }

        public static string GetConfigDirectory()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string dir = Path.Combine(baseDir, "Config");
            if (Directory.Exists(dir))
            {
                return dir;
            }
            string alt = Path.Combine(baseDir, "..", "..", "..", "Config");
            alt = Path.GetFullPath(alt);
            if (Directory.Exists(alt))
            {
                return alt;
            }
            return dir;
        }

        public static GlobalAppSettings Load()
        {
            string path = GetSettingsFilePath();
            if (!File.Exists(path))
            {
                // 兼容旧分散记忆文件
                GlobalAppSettings migrated = new GlobalAppSettings();
                TryMigrateLegacy(migrated);
                return migrated;
            }
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(GlobalAppSettings));
                using (FileStream fs = File.OpenRead(path))
                {
                    GlobalAppSettings s = xs.Deserialize(fs) as GlobalAppSettings;
                    return s ?? new GlobalAppSettings();
                }
            }
            catch
            {
                return new GlobalAppSettings();
            }
        }

        public static void Save(GlobalAppSettings settings)
        {
            if (settings == null)
            {
                return;
            }
            string dir = GetConfigDirectory();
            Directory.CreateDirectory(dir);
            string path = GetSettingsFilePath();
            XmlSerializer xs = new XmlSerializer(typeof(GlobalAppSettings));
            using (FileStream fs = File.Create(path))
            {
                xs.Serialize(fs, settings);
            }
        }

        private static void TryMigrateLegacy(GlobalAppSettings settings)
        {
            try
            {
                string outFile = Path.Combine(GetConfigDirectory(), "_last_output_gdb.txt");
                if (File.Exists(outFile))
                {
                    settings.OutputGdbPath = File.ReadAllText(outFile, Encoding.UTF8).Trim();
                }
            }
            catch
            {
            }
            try
            {
                string cities = Path.Combine(GetConfigDirectory(), "Cities", "_active_city.txt");
                if (File.Exists(cities))
                {
                    settings.ActiveCityProfileId = File.ReadAllText(cities, Encoding.UTF8).Trim();
                }
            }
            catch
            {
            }
        }
    }
}

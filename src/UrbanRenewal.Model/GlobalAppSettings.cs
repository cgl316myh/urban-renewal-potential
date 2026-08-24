using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace UrbanRenewal.Model
{
    /// <summary>全局工作区设置：输出 GDB、城市配置等，设置一次全模块共用。</summary>
    [XmlRoot("GlobalAppSettings")]
    public class GlobalAppSettings
    {
        public GlobalAppSettings()
        {
            SkinName = "Office 2013";
            PluginsDirectoryName = "Plugins";
            MotivationWeight = 0.7;
            FeasibilityWeight = 0.3;
            CellSize = 30;
            MaskResultToStudyArea = true;
        }

        public string SkinName { get; set; }

        public string PluginsDirectoryName { get; set; }

        public string InputGdbPath { get; set; }

        public string OutputGdbPath { get; set; }

        public string ActiveCityProfileId { get; set; }

        /// <summary>基准坐标系来源：*.shp 或 *.gdb；空则完整性检查按 GDB 内自动推断。</summary>
        public string SpatialRefSourcePath { get; set; }

        /// <summary>来源为 GDB 时的要素类名；Shapefile 可空。</summary>
        public string SpatialRefLayerName { get; set; }

        public string SpatialRefName { get; set; }

        /// <summary>FactoryCode；0 表示未知。</summary>
        public int SpatialRefFactoryCode { get; set; }

        /// <summary>工程 MXD；启动时自动加载。</summary>
        public string ProjectMxdPath { get; set; }

        public double MotivationWeight { get; set; }

        public double FeasibilityWeight { get; set; }

        /// <summary>统一像元大小（米），动力性/可行度/叠置共用。</summary>
        public double CellSize { get; set; }

        /// <summary>分析最终成果是否按 StudyArea 掩膜；动力性/可行度/叠置共用。</summary>
        public bool MaskResultToStudyArea { get; set; }

        /// <summary>清空工作区设置（新建工程）；保留皮肤等界面偏好。</summary>
        public void ClearWorkspaceSettings()
        {
            InputGdbPath = null;
            OutputGdbPath = null;
            ActiveCityProfileId = null;
            SpatialRefSourcePath = null;
            SpatialRefLayerName = null;
            SpatialRefName = null;
            SpatialRefFactoryCode = 0;
            ProjectMxdPath = null;
        }
    }

    /// <summary>读写全局设置；优先用户目录，避免 VS 编译覆盖 bin\Config。</summary>
    public static class GlobalAppSettingsStore
    {
        private const string AppFolderName = "UrbanRenewal";

        public static string GetSettingsFilePath()
        {
            return Path.Combine(GetConfigDirectory(), "app_settings.xml");
        }

        /// <summary>可写配置目录：%LocalAppData%\UrbanRenewal\Config（不随 bin 清理丢失）。</summary>
        public static string GetConfigDirectory()
        {
            string userRoot = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dir = Path.Combine(userRoot, AppFolderName, "Config");
            Directory.CreateDirectory(dir);
            EnsureSeeded(dir);
            return dir;
        }

        /// <summary>安装/开发目录下的模板 Config（只读种子）。</summary>
        public static string GetInstallConfigDirectory()
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
                    if (s == null)
                    {
                        return new GlobalAppSettings();
                    }
                    if (s.CellSize <= 0)
                    {
                        s.CellSize = 30;
                    }
                    return s;
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

            string temp = path + ".tmp";
            XmlSerializer xs = new XmlSerializer(typeof(GlobalAppSettings));
            using (FileStream fs = File.Create(temp))
            {
                xs.Serialize(fs, settings);
            }
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.Move(temp, path);
        }

        /// <summary>首次使用：从安装目录 / 旧 bin\Config 迁移模板与已有配置。</summary>
        private static void EnsureSeeded(string userConfigDir)
        {
            try
            {
                string userSettings = Path.Combine(userConfigDir, "app_settings.xml");
                string userCities = Path.Combine(userConfigDir, "Cities");
                Directory.CreateDirectory(userCities);

                if (!File.Exists(userSettings))
                {
                    string legacyBin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "app_settings.xml");
                    if (File.Exists(legacyBin) && HasUsefulWorkspaceSettings(legacyBin))
                    {
                        File.Copy(legacyBin, userSettings, false);
                    }
                    else
                    {
                        string installDir = GetInstallConfigDirectory();
                        string installSettings = Path.Combine(installDir, "app_settings.xml");
                        string installTemplate = Path.Combine(installDir, "app_settings.template.xml");
                        if (File.Exists(installSettings))
                        {
                            File.Copy(installSettings, userSettings, false);
                        }
                        else if (File.Exists(installTemplate))
                        {
                            File.Copy(installTemplate, userSettings, false);
                        }
                    }
                }

                if (Directory.GetFiles(userCities, "*.xml").Length == 0)
                {
                    CopyCityTemplates(GetInstallConfigDirectory(), userCities);
                    string legacyCities = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "Cities");
                    if (Directory.Exists(legacyCities) && Directory.GetFiles(userCities, "*.xml").Length == 0)
                    {
                        CopyCityTemplates(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config"), userCities);
                    }
                }

                string legacyMxd = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "CurrentProject.mxd");
                string userMxd = Path.Combine(userConfigDir, "CurrentProject.mxd");
                if (File.Exists(legacyMxd) && !File.Exists(userMxd))
                {
                    try { File.Copy(legacyMxd, userMxd, false); }
                    catch { }
                }
            }
            catch
            {
            }
        }

        private static bool HasUsefulWorkspaceSettings(string settingsPath)
        {
            try
            {
                string text = File.ReadAllText(settingsPath, Encoding.UTF8);
                if (string.IsNullOrEmpty(text))
                {
                    return false;
                }
                if (text.IndexOf("<InputGdbPath>", StringComparison.OrdinalIgnoreCase) >= 0
                    && text.IndexOf("<InputGdbPath />", StringComparison.OrdinalIgnoreCase) < 0
                    && text.IndexOf("<InputGdbPath/>", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    int a = text.IndexOf("<InputGdbPath>", StringComparison.OrdinalIgnoreCase);
                    int b = text.IndexOf("</InputGdbPath>", StringComparison.OrdinalIgnoreCase);
                    if (a >= 0 && b > a + "<InputGdbPath>".Length)
                    {
                        string inner = text.Substring(a + "<InputGdbPath>".Length, b - a - "<InputGdbPath>".Length).Trim();
                        if (inner.Length > 0)
                        {
                            return true;
                        }
                    }
                }
                if (text.IndexOf("ProjectMxdPath>", StringComparison.OrdinalIgnoreCase) >= 0
                    && text.IndexOf(".mxd", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
                if (text.IndexOf("OutputGdbPath>", StringComparison.OrdinalIgnoreCase) >= 0
                    && text.IndexOf(".gdb", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }

        private static void CopyCityTemplates(string installConfigDir, string userCities)
        {
            string srcCities = Path.Combine(installConfigDir, "Cities");
            if (!Directory.Exists(srcCities))
            {
                return;
            }
            string[] files = Directory.GetFiles(srcCities, "*.xml");
            for (int i = 0; i < files.Length; i++)
            {
                string name = Path.GetFileName(files[i]);
                string dest = Path.Combine(userCities, name);
                if (!File.Exists(dest))
                {
                    try { File.Copy(files[i], dest, false); }
                    catch { }
                }
            }
            string readme = Path.Combine(srcCities, "README.txt");
            string readmeDest = Path.Combine(userCities, "README.txt");
            if (File.Exists(readme) && !File.Exists(readmeDest))
            {
                try { File.Copy(readme, readmeDest, false); }
                catch { }
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

using System;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace UrbanRenewal.Host
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            GlobalExceptionHandler.Register();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                BonusSkins.Register();
                SkinManager.EnableFormSkins();
                UserLookAndFeel.Default.SetSkinStyle("Office 2013");

                string licenseMessage;
                if (!ArcEngineBootstrap.TryInitialize(out licenseMessage))
                {
                    MessageBox.Show(licenseMessage + "\r\n\r\n程序仍可启动，但地图功能不可用。",
                        "ArcGIS 许可",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                Application.Run(new MainRibbonForm());
            }
            catch (Exception ex)
            {
                string path = SessionLogWriter.WriteCrashReport("Main", ex);
                MessageBox.Show(
                    "程序启动或主循环发生致命错误：\r\n" + ex.Message
                    + (string.IsNullOrEmpty(path) ? "" : ("\r\n\r\n详情已写入:\r\n" + path)),
                    "致命错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                try { SessionLogWriter.Close(); }
                catch { }
            }
        }
    }
}

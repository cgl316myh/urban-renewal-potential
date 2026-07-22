using System;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;
using UrbanRenewal.GIS;

namespace UrbanRenewal.Host
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // DevExpress 13.1 皮肤注册
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
    }
}

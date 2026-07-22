using System;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;

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

            // TODO P1-5：在此初始化 ArcGIS Engine License（AoInitialize）

            Application.Run(new MainRibbonForm());
        }
    }
}

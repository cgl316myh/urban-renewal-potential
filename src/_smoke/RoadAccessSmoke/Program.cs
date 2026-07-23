using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UrbanRenewal.GIS;

namespace RoadAccessSmoke
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            string gdb = args.Length > 0 ? args[0]
                : @"D:\外挂\淘宝2\城市更新动力分析与片区指引系统设计\资料\苏州更新潜力评价数据\苏州更新潜力评价数据.gdb";

            string msg;
            if (!ArcEngineBootstrap.TryInitialize(out msg))
            {
                Console.WriteLine("INIT_FAIL " + msg);
                return 2;
            }
            Console.WriteLine("INIT " + msg);
            Console.WriteLine(NetworkDatasetHelper.BuildIntegrityReport(gdb));

            try
            {
                string outGdb = Path.Combine(Path.GetTempPath(), "UrbanRenewal", "RoadAccessSmoke_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".gdb");
                GeoprocessorHelper gp = new GeoprocessorHelper();
                outGdb = OutputGdbHelper.EnsureExists(gp, outGdb);
                Console.WriteLine("OUT " + outGdb);

                List<string> names = WorkspaceCatalog.ListFeatureClassNames(gdb);
                string cbd = WorkspaceCatalog.FindByKeywords(names, "CBD", "开发强度");
                if (string.IsNullOrEmpty(cbd))
                {
                    Console.WriteLine("NO_CBD");
                    return 4;
                }
                string fac = WorkspaceCatalog.ToFeatureClassPath(gdb, cbd);
                Console.WriteLine("FAC " + fac);

                List<string> messages = new List<string>();
                string raster = RoadNetworkAccessibilityBuilder.Build(
                    gp, gdb, outGdb, fac,
                    "roadNet", "roadNet_ND", "Length",
                    30, messages);
                foreach (string m in messages)
                {
                    Console.WriteLine("MSG " + m);
                }

                if (string.IsNullOrEmpty(raster))
                {
                    Console.WriteLine("SMOKE_FAIL no raster");
                    return 1;
                }
                Console.WriteLine("RASTER " + raster);
                Console.WriteLine("SMOKE_OK");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("EX " + ex);
                return 3;
            }
            finally
            {
                ArcEngineBootstrap.Shutdown();
            }
        }
    }
}

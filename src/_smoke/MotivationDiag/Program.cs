using System;
using System.IO;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using UrbanRenewal.GIS;

namespace MotivationDiag
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

            IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
            IWorkspace ws = factory.OpenFromFile(gdb, 0);
            IEnumDataset enumFc = ws.get_Datasets(esriDatasetType.esriDTFeatureClass);
            IDataset ds;
            while ((ds = enumFc.Next()) != null)
            {
                Dump(ds);
            }
            IEnumDataset enumFd = ws.get_Datasets(esriDatasetType.esriDTFeatureDataset);
            IDataset fd;
            while ((fd = enumFd.Next()) != null)
            {
                Console.WriteLine("FD|" + fd.Name);
                IEnumDataset children = fd.Subsets;
                IDataset child;
                while ((child = children.Next()) != null)
                {
                    if (child.Type == esriDatasetType.esriDTFeatureClass)
                    {
                        Dump(child);
                    }
                }
            }

            // 检查最近冒烟工作目录下的 shapefile
            string root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "UrbanRenewal");
            if (Directory.Exists(root))
            {
                string[] dirs = Directory.GetDirectories(root, "MotivationSmoke_*");
                Array.Sort(dirs);
                if (dirs.Length > 0)
                {
                    string last = dirs[dirs.Length - 1];
                    Console.WriteLine("LAST_WORK=" + last);
                    foreach (string f in Directory.GetFiles(last, "*.shp"))
                    {
                        Console.WriteLine("SHP|" + System.IO.Path.GetFileName(f) + " size=" + new FileInfo(f).Length);
                    }
                    foreach (string d in Directory.GetDirectories(last))
                    {
                        Console.WriteLine("GRIDDIR|" + System.IO.Path.GetFileName(d));
                    }
                }
            }

            ArcEngineBootstrap.Shutdown();
            return 0;
        }

        private static void Dump(IDataset ds)
        {
            IFeatureClass fc = (IFeatureClass)ds;
            IGeoDataset gds = (IGeoDataset)fc;
            ISpatialReference sr = gds.SpatialReference;
            string srName = sr != null ? sr.Name : "(null)";
            IEnvelope env = gds.Extent;
            Console.WriteLine(string.Format(
                "FC|{0}|type={1}|count={2}|SR={3}|ext=[{4:F4},{5:F4},{6:F4},{7:F4}]|W={8:F4}|H={9:F4}",
                ds.Name,
                fc.ShapeType,
                fc.FeatureCount(null),
                srName,
                env != null ? env.XMin : 0,
                env != null ? env.YMin : 0,
                env != null ? env.XMax : 0,
                env != null ? env.YMax : 0,
                env != null ? env.Width : 0,
                env != null ? env.Height : 0));
        }
    }
}

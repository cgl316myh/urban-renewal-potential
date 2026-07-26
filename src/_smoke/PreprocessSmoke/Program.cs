using System;
using System.IO;
using System.Text;
using UrbanRenewal.GIS;

namespace PreprocessSmoke
{
    /// <summary>
    /// 投影/裁剪预处理冒烟：投影坐标系不一致图层并裁剪到分析范围。
    /// </summary>
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            string gdb = args != null && args.Length > 0
                ? args[0]
                : @"D:\外挂\淘宝2\城市更新动力分析与片区指引系统设计\资料\苏州更新潜力评价数据\苏州更新潜力评价数据.gdb";

            string msg;
            if (!ArcEngineBootstrap.TryInitialize(out msg))
            {
                Console.WriteLine("INIT_FAIL " + msg);
                return 2;
            }
            Console.WriteLine(msg);

            try
            {
                System.Collections.Generic.List<string> names = WorkspaceCatalog.ListFeatureClassNames(gdb);
                SpatialReferenceAuditResult audit = SpatialReferenceAudit.Audit(gdb);

                string study = WorkspaceCatalog.FindByKeywords(names, "中心城区", "分析范围");
                string mismatch = null;
                for (int i = 0; i < audit.MismatchedLayers.Count; i++)
                {
                    string n = audit.MismatchedLayers[i].LayerName;
                    if (FeaturePreprocessBuilder.IsNetworkArtifact(n))
                    {
                        continue;
                    }
                    mismatch = n;
                    break;
                }

                Console.WriteLine("STUDY=" + study);
                Console.WriteLine("MISMATCH=" + mismatch);
                if (string.IsNullOrEmpty(study) || string.IsNullOrEmpty(mismatch))
                {
                    Console.WriteLine("SMOKE_FAIL_LAYER");
                    return 1;
                }

                FeaturePreprocessJob job = new FeaturePreprocessJob();
                job.InputGdbPath = gdb;
                job.OutputGdbPath = Path.Combine(Path.GetTempPath(), "UrbanRenewal",
                    "PreprocessSmoke_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".gdb");
                job.DoProject = true;
                job.DoClip = true;
                job.ClipLayerName = study;
                job.LayerNames.Add(mismatch);
                job.LayerNames.Add(study);

                Console.WriteLine("OUT=" + job.OutputGdbPath);
                FeaturePreprocessResult result = FeaturePreprocessBuilder.Run(job, delegate(string t, int p)
                {
                    Console.WriteLine(p + "% " + t);
                });

                for (int i = 0; i < result.Messages.Count; i++)
                {
                    Console.WriteLine("MSG " + result.Messages[i]);
                }

                if (!result.Success)
                {
                    Console.WriteLine("SMOKE_FAIL");
                    return 1;
                }

                bool projected = false;
                bool clipped = false;
                for (int i = 0; i < result.Messages.Count; i++)
                {
                    string line = result.Messages[i];
                    if (line.IndexOf("[投影]", StringComparison.Ordinal) >= 0
                        && line.IndexOf("⇒", StringComparison.Ordinal) >= 0)
                    {
                        projected = true;
                    }
                    if (line.IndexOf("[裁剪]", StringComparison.Ordinal) >= 0
                        && line.IndexOf("分析范围本身", StringComparison.Ordinal) < 0)
                    {
                        clipped = true;
                    }
                }

                if (projected && clipped)
                {
                    Console.WriteLine("SMOKE_OK_PROJECT_CLIP");
                    return 0;
                }
                if (projected)
                {
                    Console.WriteLine("SMOKE_OK_PROJECT");
                    return 0;
                }
                Console.WriteLine("SMOKE_OK_PARTIAL");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION " + ex);
                return 3;
            }
            finally
            {
                ArcEngineBootstrap.Shutdown();
            }
        }
    }
}

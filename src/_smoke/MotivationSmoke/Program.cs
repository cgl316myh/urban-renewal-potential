using System;
using System.Collections.Generic;
using System.IO;
using UrbanRenewal.Analysis;
using UrbanRenewal.GIS;
using UrbanRenewal.Model;

namespace MotivationSmoke
{
    /// <summary>
    /// 动力性分析冒烟：直接调用引擎，不启 UI。
    /// </summary>
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            string gdb = args != null && args.Length > 0
                ? args[0]
                : @"D:\外挂\淘宝2\城市更新动力分析与片区指引系统设计\资料\苏州更新潜力评价数据\苏州更新潜力评价数据.gdb";

            string logPath = Path.Combine(Path.GetTempPath(), "UrbanRenewal", "motivation_smoke_log.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));

            try
            {
                Log(logPath, "GDB=" + gdb);
                Log(logPath, "Exists=" + Directory.Exists(gdb));

                string msg;
                if (!ArcEngineBootstrap.TryInitialize(out msg))
                {
                    Log(logPath, "INIT_FAIL: " + msg);
                    Console.WriteLine("INIT_FAIL");
                    return 2;
                }
                Log(logPath, "INIT_OK: " + msg);

                MotivationJob job = new MotivationJob();
                job.GdbPath = gdb;
                job.CellSize = 100; // 冒烟加速
                job.WorkDirectory = Path.Combine(Path.GetTempPath(), "UrbanRenewal", "MotivationSmoke_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                // 冒烟：输出到临时 File GDB
                job.OutputGdbPath = Path.Combine(Path.GetTempPath(), "UrbanRenewal", "MotivationSmoke_Out_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".gdb");
                ApplySuzhouDefaultHints(job);

                Log(logPath, "WorkDir=" + job.WorkDirectory);
                Log(logPath, "OutputGdb=" + job.OutputGdbPath);
                foreach (KeyValuePair<string, string> kv in job.LayerHints)
                {
                    Log(logPath, "Hint " + kv.Key + "=" + kv.Value);
                }

                MotivationAnalysisEngine engine = new MotivationAnalysisEngine();
                MotivationResult result = engine.Run(job, delegate(string text, int percent)
                {
                    string line = percent + "% " + text;
                    Console.WriteLine(line);
                    Log(logPath, line);
                });

                for (int i = 0; i < result.Messages.Count; i++)
                {
                    Log(logPath, result.Messages[i]);
                }

                // 坐标系：动力性仅校验分析所用图层；全库仍可能有宗地等未统一图层
                List<string> used = UrbanRenewal.GIS.SpatialReferenceAudit.CollectMotivationLayerNames(
                    job.LayerHints, WorkspaceCatalog.ListFeatureClassNames(gdb));
                UrbanRenewal.GIS.SpatialReferenceAuditResult auditUsed =
                    UrbanRenewal.GIS.SpatialReferenceAudit.Audit(gdb, used);
                UrbanRenewal.GIS.SpatialReferenceAuditResult auditAll =
                    UrbanRenewal.GIS.SpatialReferenceAudit.Audit(gdb);
                Log(logPath, "AuditUsedUnified=" + auditUsed.IsUnified + " UsedCount=" + used.Count);
                Log(logPath, "AuditAllUnified=" + auditAll.IsUnified + " MismatchAll=" + auditAll.MismatchedLayers.Count);
                if (!auditUsed.IsUnified)
                {
                    bool blocked = !result.Success
                        && string.Join("\n", result.Messages.ToArray()).IndexOf("空间参考", StringComparison.Ordinal) >= 0;
                    Console.WriteLine(blocked ? "SMOKE_OK_SR_BLOCKED" : "SMOKE_FAIL_EXPECTED_BLOCK");
                    Log(logPath, blocked ? "SMOKE_OK_SR_BLOCKED" : "SMOKE_FAIL_EXPECTED_BLOCK");
                    return blocked ? 0 : 1;
                }

                bool usedSa = false;
                bool usedEuc = false;
                for (int i = 0; i < result.Messages.Count; i++)
                {
                    string m = result.Messages[i];
                    if (m.IndexOf("路网可达性（服务区）", StringComparison.Ordinal) >= 0)
                    {
                        usedSa = true;
                    }
                    if (m.IndexOf("路网可达性（欧氏近似）", StringComparison.Ordinal) >= 0)
                    {
                        usedEuc = true;
                    }
                }
                Log(logPath, "RoadAccess_SA=" + usedSa + " RoadAccess_Euc=" + usedEuc);

                Log(logPath, "Success=" + result.Success);
                Log(logPath, "Raster=" + result.MotivationRasterPath);

                if (result.Success && usedSa)
                {
                    bool scaled = false;
                    for (int i = 0; i < result.Messages.Count; i++)
                    {
                        if (result.Messages[i].IndexOf("0–100", StringComparison.Ordinal) >= 0
                            || result.Messages[i].IndexOf("0-100", StringComparison.Ordinal) >= 0)
                        {
                            scaled = true;
                            break;
                        }
                    }
                    Console.WriteLine(scaled ? "SMOKE_OK_ROAD_SA_100" : "SMOKE_OK_ROAD_SA");
                    Log(logPath, scaled ? "SMOKE_OK_ROAD_SA_100" : "SMOKE_OK_ROAD_SA");
                    return 0;
                }
                if (result.Success)
                {
                    Console.WriteLine(usedEuc ? "SMOKE_OK_ROAD_EUC" : "SMOKE_OK");
                    Log(logPath, usedEuc ? "SMOKE_OK_ROAD_EUC" : "SMOKE_OK");
                    return 0;
                }

                Console.WriteLine("SMOKE_FAIL");
                Log(logPath, "SMOKE_FAIL");
                Log(logPath, "LogFile=" + logPath);
                return 1;
            }
            catch (Exception ex)
            {
                Log(logPath, "EXCEPTION: " + ex);
                Console.WriteLine("EXCEPTION: " + ex.Message);
                return 3;
            }
            finally
            {
                try { ArcEngineBootstrap.Shutdown(); }
                catch { }
            }
        }

        private static void ApplySuzhouDefaultHints(MotivationJob job)
        {
            List<string> names = WorkspaceCatalog.ListFeatureClassNames(job.GdbPath);
            Log(Path.Combine(Path.GetTempPath(), "UrbanRenewal", "motivation_smoke_log.txt"),
                "FeatureClassCount=" + names.Count);
            for (int i = 0; i < names.Count; i++)
            {
                Log(Path.Combine(Path.GetTempPath(), "UrbanRenewal", "motivation_smoke_log.txt"), "FC: " + names[i]);
            }

            TryHint(job, names, "MetroMulti", "两线地铁站");
            TryHint(job, names, "Metro", "一线地铁站");
            TryHint(job, names, "CBD", "开发强度高区域（CBD）");
            TryHint(job, names, "StudyArea", "中心城区");
            TryHint(job, names, "EcoCorridor", "重要生态廊道");
            TryHint(job, names, "OpenSpace", "大型开敞空间");
            TryHint(job, names, "Green", "城市公园绿地");
            TryHint(job, names, "PublicService", "市级医院");
            TryHint(job, names, "Convenience", "苏州市建成区文体设施");
            TryHint(job, names, "PolicyBelt", "城市战略圈层和片区");
            TryHint(job, names, "PolicyKey", "近期重点发展区域");

            // 预建路网（与 Suzhou.xml NetworkDataset 一致）
            job.LayerHints["RoadFeatureDataset"] = "roadNet";
            job.LayerHints["RoadNetwork"] = "roadNet_ND";
            job.LayerHints["RoadImpedance"] = "Length";
        }

        private static void TryHint(MotivationJob job, List<string> names, string key, string exactName)
        {
            for (int i = 0; i < names.Count; i++)
            {
                if (string.Equals(names[i], exactName, StringComparison.OrdinalIgnoreCase)
                    || names[i].IndexOf(exactName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    job.LayerHints[key] = names[i];
                    return;
                }
            }
        }

        private static void Log(string path, string line)
        {
            File.AppendAllText(path, DateTime.Now.ToString("HH:mm:ss") + " " + line + Environment.NewLine);
        }
    }
}

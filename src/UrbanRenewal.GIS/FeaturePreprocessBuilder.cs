using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ESRI.ArcGIS.AnalysisTools;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geometry;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// 投影/裁剪预处理作业参数。
    /// </summary>
    public sealed class FeaturePreprocessJob
    {
        public FeaturePreprocessJob()
        {
            LayerNames = new List<string>();
            DoProject = true;
            DoClip = true;
            ReplaceInInputGdb = true;
        }

        public string InputGdbPath { get; set; }

        /// <summary>中间暂存 File GDB（投影/裁剪中间结果）；最终可替换回输入库。</summary>
        public string OutputGdbPath { get; set; }

        public string ClipLayerName { get; set; }
        public List<string> LayerNames { get; private set; }
        public bool DoProject { get; set; }
        public bool DoClip { get; set; }

        /// <summary>
        /// true：处理成功后用正确图层替换输入 GDB 中的同名原图层（后期运算仍读输入库）。
        /// </summary>
        public bool ReplaceInInputGdb { get; set; }
    }

    /// <summary>
    /// 预处理结果。
    /// </summary>
    public sealed class FeaturePreprocessResult
    {
        public FeaturePreprocessResult()
        {
            Messages = new List<string>();
            OutputLayers = new List<string>();
            ReplacedLayers = new List<string>();
        }

        public bool Success { get; set; }
        public List<string> Messages { get; private set; }
        public List<string> OutputLayers { get; private set; }

        /// <summary>已成功替换进输入 GDB 的图层名。</summary>
        public List<string> ReplacedLayers { get; private set; }
    }

    /// <summary>
    /// 批量投影到目标坐标系，并按建成区/分析范围裁剪。
    /// 默认：中间结果写入输出 GDB，再替换输入 GDB 中的错误图层（同名），供后期运算继续使用输入库。
    /// 不处理 Network Dataset（须在 ArcGIS 中预建）；跳过路网拓扑附属要素。
    /// </summary>
    public static class FeaturePreprocessBuilder
    {
        public static FeaturePreprocessResult Run(FeaturePreprocessJob job, Action<string, int> progress)
        {
            FeaturePreprocessResult result = new FeaturePreprocessResult();
            if (job == null || string.IsNullOrEmpty(job.InputGdbPath) || !Directory.Exists(job.InputGdbPath))
            {
                result.Messages.Add("输入 GDB 无效。");
                return result;
            }
            if (string.IsNullOrEmpty(job.OutputGdbPath) || !OutputGdbHelper.IsFileGdbPath(job.OutputGdbPath))
            {
                result.Messages.Add("请指定中间暂存 File GDB（*.gdb）。");
                return result;
            }
            if (string.Equals(
                System.IO.Path.GetFullPath(job.InputGdbPath).TrimEnd('\\', '/'),
                System.IO.Path.GetFullPath(job.OutputGdbPath).TrimEnd('\\', '/'),
                StringComparison.OrdinalIgnoreCase))
            {
                result.Messages.Add("中间暂存 GDB 不能与输入 GDB 相同，请另指定输出库作为暂存。");
                return result;
            }
            if (!job.DoProject && !job.DoClip)
            {
                result.Messages.Add("请至少勾选「投影」或「裁剪」。");
                return result;
            }
            if (job.LayerNames == null || job.LayerNames.Count == 0)
            {
                result.Messages.Add("未选择任何图层。");
                return result;
            }

            GeoprocessorHelper gp = new GeoprocessorHelper();
            string scratchGdb = OutputGdbHelper.EnsureExists(gp, job.OutputGdbPath);
            job.OutputGdbPath = scratchGdb;
            result.Messages.Add("中间暂存 GDB: " + scratchGdb);
            if (job.ReplaceInInputGdb)
            {
                result.Messages.Add("模式: 处理成功后替换输入 GDB 中的原图层（后期运算仍读输入库）。");
            }
            else
            {
                result.Messages.Add("模式: 仅写入暂存 GDB，不替换输入库。");
            }

            Report(progress, result, "准备裁剪范围...", 5);
            string clipSrc = null;
            ISpatialReference targetSr = null;
            string clipPrepared = null;

            if (job.DoClip)
            {
                if (string.IsNullOrEmpty(job.ClipLayerName))
                {
                    result.Messages.Add("裁剪需要指定建成区/分析范围图层。");
                    return result;
                }
                clipSrc = WorkspaceCatalog.ToFeatureClassPath(job.InputGdbPath, job.ClipLayerName);
                targetSr = FeatureProjectionHelper.GetSpatialReference(clipSrc);
                if (targetSr == null)
                {
                    result.Messages.Add("无法读取裁剪图层空间参考: " + job.ClipLayerName);
                    return result;
                }
                if (!(targetSr is IProjectedCoordinateSystem))
                {
                    result.Messages.Add("裁剪图层应为投影坐标系（当前: "
                        + (targetSr.Name ?? "未知") + "）。请先将分析范围投影到 CGCS2000 等平面坐标系。");
                    return result;
                }

                clipPrepared = PrepareClipLayer(gp, clipSrc, job.ClipLayerName, targetSr, scratchGdb, result);
                if (string.IsNullOrEmpty(clipPrepared))
                {
                    return result;
                }
                result.Messages.Add("裁剪范围: " + job.ClipLayerName + " [" + targetSr.Name + "]");
            }
            else
            {
                if (!string.IsNullOrEmpty(job.ClipLayerName))
                {
                    clipSrc = WorkspaceCatalog.ToFeatureClassPath(job.InputGdbPath, job.ClipLayerName);
                    targetSr = FeatureProjectionHelper.GetSpatialReference(clipSrc);
                }
                if (targetSr == null || !(targetSr is IProjectedCoordinateSystem))
                {
                    targetSr = FindPreferredProjectedSr(job.InputGdbPath);
                }
                if (targetSr == null)
                {
                    result.Messages.Add("无法确定目标投影坐标系。请指定分析范围图层，或确保 GDB 中已有投影坐标系图层。");
                    return result;
                }
                result.Messages.Add("目标坐标系: " + targetSr.Name);
            }

            gp.ConfigureAnalysis(scratchGdb, null, 0, targetSr);

            int total = job.LayerNames.Count;
            int done = 0;
            int okCount = 0;
            for (int i = 0; i < job.LayerNames.Count; i++)
            {
                string layerName = job.LayerNames[i];
                done++;
                int pct = 10 + (int)(80.0 * done / Math.Max(1, total));
                Report(progress, result, "处理 " + layerName + "...", pct);

                if (IsNetworkArtifact(layerName))
                {
                    result.Messages.Add("[跳过] " + layerName + "（路网拓扑/网络附属，请在 ArcGIS 中维护 Network Dataset）");
                    continue;
                }

                try
                {
                    string outPath = ProcessOneLayer(
                        gp, job.InputGdbPath, layerName, job.ClipLayerName, targetSr, clipPrepared,
                        job.DoProject, job.DoClip, scratchGdb, result);
                    if (string.IsNullOrEmpty(outPath))
                    {
                        continue;
                    }

                    result.OutputLayers.Add(outPath);
                    okCount++;

                    if (job.ReplaceInInputGdb)
                    {
                        if (TryReplaceInInputGdb(gp, job.InputGdbPath, layerName, outPath, result))
                        {
                            result.ReplacedLayers.Add(layerName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Messages.Add("[失败] " + layerName + ": " + ex.Message);
                }
            }

            result.Success = okCount > 0;
            result.Messages.Add("完成: 成功处理 " + okCount + " / 选择 " + total + " 个图层"
                + (job.ReplaceInInputGdb
                    ? "；已替换输入库 " + result.ReplacedLayers.Count + " 个。"
                    : "。"));
            Report(progress, result, "完成", 100);
            return result;
        }

        /// <summary>
        /// 用暂存库中的正确图层替换输入 GDB 中的错误图层。
        /// 优先写回原路径（含要素数据集内）；若因坐标系变更无法写回要素数据集，则落到 GDB 根目录同名要素类。
        /// </summary>
        private static bool TryReplaceInInputGdb(
            GeoprocessorHelper gp,
            string inputGdb,
            string layerName,
            string correctedPath,
            FeaturePreprocessResult result)
        {
            if (gp == null || string.IsNullOrEmpty(inputGdb) || string.IsNullOrEmpty(layerName)
                || string.IsNullOrEmpty(correctedPath))
            {
                return false;
            }

            string leafName = layerName;
            int slash = Math.Max(layerName.LastIndexOf('\\'), layerName.LastIndexOf('/'));
            if (slash >= 0 && slash < layerName.Length - 1)
            {
                leafName = layerName.Substring(slash + 1);
            }

            string originalPath = WorkspaceCatalog.ToFeatureClassPath(inputGdb, layerName);
            string rootPath = WorkspaceCatalog.ToFeatureClassPath(inputGdb, leafName);

            // 1) 优先覆盖原路径（后期数据配置中的图层名可保持不变）
            try
            {
                OutputGdbHelper.TryDeleteDataset(gp, originalPath);
                CopyTo(gp, correctedPath, originalPath, "Replace-" + leafName);
                result.Messages.Add("[替换] 输入GDB ← " + layerName
                    + "（错误图层已覆盖；后期运算仍读输入库）");
                return true;
            }
            catch (Exception exInPlace)
            {
                // 2) 要素数据集内无法混入新坐标系时，改写到 GDB 根目录
                if (slash >= 0
                    && !string.Equals(originalPath, rootPath, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        OutputGdbHelper.TryDeleteDataset(gp, originalPath);
                        OutputGdbHelper.TryDeleteDataset(gp, rootPath);
                        CopyTo(gp, correctedPath, rootPath, "ReplaceRoot-" + leafName);
                        result.Messages.Add("[替换] 输入GDB ← " + leafName
                            + "（原路径 " + layerName + " 因坐标系变更无法写回要素数据集，已放到 GDB 根目录；"
                            + "请到「数据配置」确认该角色仍指向此图层）");
                        return true;
                    }
                    catch (Exception exRoot)
                    {
                        result.Messages.Add("[替换失败] " + layerName + ": " + exRoot.Message
                            + "（先尝试原路径失败: " + exInPlace.Message + "）。"
                            + "正确结果仍在暂存库: " + correctedPath
                            + "。若图层被占用，请关闭地图图层后重试。");
                        return false;
                    }
                }

                result.Messages.Add("[替换失败] " + layerName + ": " + exInPlace.Message
                    + "。正确结果仍保留在暂存库: " + correctedPath
                    + "（若图层正被地图占用，请关闭后重试）");
                return false;
            }
        }

        /// <summary>
        /// 是否为路网 Network Dataset 附属要素（不可按普通图层裁剪）。
        /// </summary>
        public static bool IsNetworkArtifact(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                return false;
            }
            if (layerName.IndexOf("_ND", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
            if (layerName.IndexOf("Junctions", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
            return false;
        }

        private static string ProcessOneLayer(
            GeoprocessorHelper gp,
            string inputGdb,
            string layerName,
            string clipLayerName,
            ISpatialReference targetSr,
            string clipFeatures,
            bool doProject,
            bool doClip,
            string outGdb,
            FeaturePreprocessResult result)
        {
            string inPath = WorkspaceCatalog.ToFeatureClassPath(inputGdb, layerName);
            ISpatialReference srcSr = FeatureProjectionHelper.GetSpatialReference(inPath);
            bool sameSr = FeatureProjectionHelper.IsSameSpatialReference(srcSr, targetSr);

            string workName = SanitizeFcName(layerName);
            string prjWorkName = "p" + StableHash(layerName).ToString("0000");
            string finalPath = OutputGdbHelper.DatasetPath(outGdb, workName);
            OutputGdbHelper.TryDeleteDataset(gp, finalPath);

            string current = inPath;
            bool projected = false;

            if (doProject && !sameSr)
            {
                string prjPath = OutputGdbHelper.DatasetPath(outGdb, prjWorkName);
                OutputGdbHelper.TryDeleteDataset(gp, prjPath);
                FeatureProjectionHelper.ProjectFeatureClassToGdb(inPath, outGdb, prjWorkName, targetSr);
                current = prjPath;
                projected = true;
                result.Messages.Add("[投影] " + layerName + " → " + (srcSr != null ? srcSr.Name : "?")
                    + " ⇒ " + targetSr.Name);
            }
            else if (doProject && sameSr)
            {
                result.Messages.Add("[投影] " + layerName + " 已与目标坐标系一致，跳过投影。");
            }

            if (doClip && !string.IsNullOrEmpty(clipFeatures))
            {
                if (!string.IsNullOrEmpty(clipLayerName)
                    && string.Equals(layerName, clipLayerName, StringComparison.OrdinalIgnoreCase))
                {
                    CopyTo(gp, clipFeatures, finalPath, "Copy-clip-self-" + workName);
                    result.Messages.Add("[裁剪] " + layerName + " 为分析范围本身，已输出。");
                }
                else
                {
                    string clipTmpName = "c" + StableHash(layerName).ToString("0000");
                    string clipTmpPath = OutputGdbHelper.DatasetPath(outGdb, clipTmpName);
                    OutputGdbHelper.TryDeleteDataset(gp, clipTmpPath);
                    try
                    {
                        try
                        {
                            RepairGeometry repair = new RepairGeometry();
                            repair.in_features = current;
                            gp.Execute(repair, "RepairGeometry-" + prjWorkName);
                        }
                        catch
                        {
                        }

                        ESRI.ArcGIS.AnalysisTools.Clip clip = new ESRI.ArcGIS.AnalysisTools.Clip();
                        clip.in_features = current;
                        clip.clip_features = clipFeatures;
                        clip.out_feature_class = clipTmpPath;
                        gp.Execute(clip, "Clip-" + clipTmpName);
                        CopyTo(gp, clipTmpPath, finalPath, "Copy-clip-" + workName);
                        try { OutputGdbHelper.TryDeleteDataset(gp, clipTmpPath); }
                        catch { }
                        result.Messages.Add("[裁剪] " + layerName + " → " + workName);
                    }
                    catch (Exception exClip)
                    {
                        result.Messages.Add("[裁剪失败] " + layerName + ": " + exClip.Message
                            + "；已输出投影结果（未裁剪）。");
                        if (!string.Equals(current, finalPath, StringComparison.OrdinalIgnoreCase))
                        {
                            CopyTo(gp, current, finalPath, "Copy-noclip-" + workName);
                        }
                        try { OutputGdbHelper.TryDeleteDataset(gp, clipTmpPath); }
                        catch { }
                    }
                }
            }
            else
            {
                if (projected)
                {
                    CopyTo(gp, current, finalPath, "Copy-prj-" + workName);
                    try { OutputGdbHelper.TryDeleteDataset(gp, current); }
                    catch { }
                }
                else
                {
                    CopyTo(gp, inPath, finalPath, "Copy-" + workName);
                    result.Messages.Add("[复制] " + layerName + "（坐标系已一致）");
                }
            }

            if (projected && doClip)
            {
                string prjMid = OutputGdbHelper.DatasetPath(outGdb, prjWorkName);
                if (!string.Equals(prjMid, finalPath, StringComparison.OrdinalIgnoreCase))
                {
                    try { OutputGdbHelper.TryDeleteDataset(gp, prjMid); }
                    catch { }
                }
            }

            return finalPath;
        }

        private static int StableHash(string text)
        {
            int h = 23;
            if (!string.IsNullOrEmpty(text))
            {
                for (int i = 0; i < text.Length; i++)
                {
                    h = unchecked(h * 31 + text[i]);
                }
            }
            if (h < 0)
            {
                h = -h;
            }
            return h % 10000;
        }

        private static string PrepareClipLayer(
            GeoprocessorHelper gp,
            string clipSrc,
            string clipLayerName,
            ISpatialReference targetSr,
            string outGdb,
            FeaturePreprocessResult result)
        {
            ISpatialReference src = FeatureProjectionHelper.GetSpatialReference(clipSrc);
            string outName = SanitizeFcName(clipLayerName);
            if (string.IsNullOrEmpty(outName) || outName == "lyr")
            {
                outName = "study_clip";
            }
            string clipOutName = "prep_clip";
            string clipOut = OutputGdbHelper.DatasetPath(outGdb, clipOutName);
            OutputGdbHelper.TryDeleteDataset(gp, clipOut);

            if (!FeatureProjectionHelper.IsSameSpatialReference(src, targetSr))
            {
                FeatureProjectionHelper.ProjectFeatureClassToGdb(clipSrc, outGdb, clipOutName, targetSr);
                result.Messages.Add("[投影] 裁剪范围 " + clipLayerName);
            }
            else
            {
                CopyTo(gp, clipSrc, clipOut, "Copy-prep-clip");
            }
            return clipOut;
        }

        private static void CopyTo(GeoprocessorHelper gp, string inFeatures, string outPath, string step)
        {
            OutputGdbHelper.TryDeleteDataset(gp, outPath);
            CopyFeatures copy = new CopyFeatures();
            copy.in_features = inFeatures;
            copy.out_feature_class = outPath;
            gp.Execute(copy, step);
        }

        private static ISpatialReference FindPreferredProjectedSr(string gdbPath)
        {
            List<string> names = WorkspaceCatalog.ListFeatureClassNames(gdbPath);
            string preferred = WorkspaceCatalog.FindByKeywords(names, "中心城区", "分析范围", "建成区");
            if (!string.IsNullOrEmpty(preferred))
            {
                ISpatialReference sr = FeatureProjectionHelper.GetSpatialReference(
                    WorkspaceCatalog.ToFeatureClassPath(gdbPath, preferred));
                if (sr is IProjectedCoordinateSystem)
                {
                    return sr;
                }
            }
            for (int i = 0; i < names.Count; i++)
            {
                ISpatialReference sr = FeatureProjectionHelper.GetSpatialReference(
                    WorkspaceCatalog.ToFeatureClassPath(gdbPath, names[i]));
                if (sr is IProjectedCoordinateSystem)
                {
                    return sr;
                }
            }
            return null;
        }

        /// <summary>
        /// File GDB 要素类名：保留中文/字母数字，替换非法字符；过长截断。
        /// </summary>
        public static string SanitizeFcName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "lyr";
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < name.Length && sb.Length < 50; i++)
            {
                char c = name[i];
                if (c == '\\' || c == '/' || c == ':' || c == '*' || c == '?' || c == '"'
                    || c == '<' || c == '>' || c == '|' || c == ' '
                    || c == '(' || c == ')' || c == '（' || c == '）'
                    || c == '[' || c == ']' || c == '{' || c == '}'
                    || c == '.' || c == ',' || c == ';' || c == '\'')
                {
                    if (sb.Length > 0 && sb[sb.Length - 1] != '_')
                    {
                        sb.Append('_');
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            string s = sb.ToString().Trim('_');
            if (s.Length == 0)
            {
                return "lyr";
            }
            if (s[0] >= '0' && s[0] <= '9')
            {
                s = "f_" + s;
            }
            return s;
        }

        private static void Report(Action<string, int> progress, FeaturePreprocessResult result, string text, int percent)
        {
            if (progress != null)
            {
                progress(text, percent);
            }
        }
    }
}

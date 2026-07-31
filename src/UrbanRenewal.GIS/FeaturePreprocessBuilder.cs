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
            // 默认不破坏原库：结果只写入输出 GDB
            ReplaceInInputGdb = false;
        }

        public string InputGdbPath { get; set; }

        /// <summary>处理后输出 File GDB（投影/裁剪结果）；原输入库保持只读不变。</summary>
        public string OutputGdbPath { get; set; }

        public string ClipLayerName { get; set; }
        public List<string> LayerNames { get; private set; }
        public bool DoProject { get; set; }
        public bool DoClip { get; set; }

        /// <summary>
        /// true：额外用结果覆盖输入 GDB 同名图层（破坏性，默认 false，界面不再启用）。
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

        /// <summary>若启用破坏性写回，已替换进输入 GDB 的图层名。</summary>
        public List<string> ReplacedLayers { get; private set; }
    }

    /// <summary>
    /// 批量投影到目标坐标系，并按建成区/分析范围裁剪。
    /// 非破坏：从输入 GDB 读取，结果写入输出 GDB；原图层不删不改。
    /// 后续分析可将「输入 GDB」切换为该输出库。
    /// 不处理 Network Dataset（须在 ArcGIS 中预建）；跳过路网拓扑附属要素。
    /// </summary>
    public static class FeaturePreprocessBuilder
    {
        private static Action<string, int> _progress;
        private static int _progressPercent;

        public static FeaturePreprocessResult Run(FeaturePreprocessJob job, Action<string, int> progress)
        {
            _progress = progress;
            _progressPercent = 0;
            FeaturePreprocessResult result = new FeaturePreprocessResult();
            if (job == null || string.IsNullOrEmpty(job.InputGdbPath) || !Directory.Exists(job.InputGdbPath))
            {
                Note(result, "输入 GDB 无效。");
                return result;
            }
            if (string.IsNullOrEmpty(job.OutputGdbPath) || !OutputGdbHelper.IsFileGdbPath(job.OutputGdbPath))
            {
                Note(result, "请指定中间暂存 File GDB（*.gdb）。");
                return result;
            }
            if (string.Equals(
                System.IO.Path.GetFullPath(job.InputGdbPath).TrimEnd('\\', '/'),
                System.IO.Path.GetFullPath(job.OutputGdbPath).TrimEnd('\\', '/'),
                StringComparison.OrdinalIgnoreCase))
            {
                Note(result, "中间暂存 GDB 不能与输入 GDB 相同，请另指定输出库作为暂存。");
                return result;
            }
            if (!job.DoProject && !job.DoClip)
            {
                Note(result, "请至少勾选「投影」或「裁剪」。");
                return result;
            }
            if (job.LayerNames == null || job.LayerNames.Count == 0)
            {
                Note(result, "未选择任何图层。");
                return result;
            }

            GeoprocessorHelper gp = new GeoprocessorHelper();
            gp.BindToProgress(_progress, delegate { return _progressPercent; });
            string scratchGdb = OutputGdbHelper.EnsureExists(gp, job.OutputGdbPath);
            job.OutputGdbPath = scratchGdb;
            Note(result, "中间暂存 GDB: " + scratchGdb);
            if (job.ReplaceInInputGdb)
            {
                Note(result, "模式: 破坏性写回（将覆盖输入 GDB 原图层）— 不推荐。");
            }
            else
            {
                Note(result, "模式: 非破坏 — 原输入 GDB 只读，结果写入输出 GDB。");
            }

            Report(progress, result, "准备裁剪范围...", 5);
            string clipSrc = null;
            ISpatialReference targetSr = null;
            string clipPrepared = null;

            if (job.DoClip)
            {
                if (string.IsNullOrEmpty(job.ClipLayerName))
                {
                    Note(result, "裁剪需要指定建成区/分析范围图层。");
                    return result;
                }
                clipSrc = WorkspaceCatalog.ToFeatureClassPath(job.InputGdbPath, job.ClipLayerName);
                targetSr = FeatureProjectionHelper.GetSpatialReference(clipSrc);
                if (targetSr == null)
                {
                    Note(result, "无法读取裁剪图层空间参考: " + job.ClipLayerName);
                    return result;
                }
                if (!(targetSr is IProjectedCoordinateSystem))
                {
                    Note(result, "裁剪图层应为投影坐标系（当前: "
                        + (targetSr.Name ?? "未知") + "）。请先将分析范围投影到 CGCS2000 等平面坐标系。");
                    return result;
                }

                clipPrepared = PrepareClipLayer(gp, clipSrc, job.ClipLayerName, targetSr, scratchGdb, result);
                if (string.IsNullOrEmpty(clipPrepared))
                {
                    return result;
                }
                Note(result, "裁剪范围: " + job.ClipLayerName + " [" + targetSr.Name + "]");
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
                    Note(result, "无法确定目标投影坐标系。请指定分析范围图层，或确保 GDB 中已有投影坐标系图层。");
                    return result;
                }
                Note(result, "目标坐标系: " + targetSr.Name);
            }

            gp.ConfigureAnalysis(scratchGdb, null, 0, targetSr);

            int total = job.LayerNames.Count;
            int done = 0;
            int okCount = 0;
            // 先全部写入暂存，再统一写回输入库，避免 Clip 读输入库时的 schema 锁导致 CopyFeatures 失败
            List<string> pendingReplaceNames = new List<string>();
            List<string> pendingReplacePaths = new List<string>();

            for (int i = 0; i < job.LayerNames.Count; i++)
            {
                string layerName = job.LayerNames[i];
                done++;
                int pct = 10 + (int)(70.0 * done / Math.Max(1, total));
                Report(progress, result, "处理 " + layerName + "...", pct);

                if (IsNetworkArtifact(layerName))
                {
                    Note(result, "[跳过] " + layerName + "（路网拓扑/网络附属，请在 ArcGIS 中维护 Network Dataset）");
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
                        pendingReplaceNames.Add(layerName);
                        pendingReplacePaths.Add(outPath);
                    }
                }
                catch (Exception ex)
                {
                    Note(result, "[失败] " + layerName + ": " + ex.Message);
                }
            }

            // 释放裁剪阶段对输入库的占用后再替换
            clipPrepared = null;
            FileGdbLockHelper.ForceComRelease();

            if (job.ReplaceInInputGdb && pendingReplaceNames.Count > 0)
            {
                Report(progress, result, "写回输入 GDB（释放占用后）...", 85);
                Note(result, "开始写回输入库（" + pendingReplaceNames.Count + " 个），已尝试释放 File GDB 锁…");
                for (int i = 0; i < pendingReplaceNames.Count; i++)
                {
                    int pct = 85 + (int)(12.0 * (i + 1) / pendingReplaceNames.Count);
                    Report(progress, result, "替换 " + pendingReplaceNames[i] + "...", pct);
                    if (TryReplaceInInputGdb(gp, job.InputGdbPath, pendingReplaceNames[i], pendingReplacePaths[i], result))
                    {
                        result.ReplacedLayers.Add(pendingReplaceNames[i]);
                    }
                }
            }

            result.Success = okCount > 0;
            Note(result, "完成: 成功处理 " + okCount + " / 选择 " + total + " 个图层"
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
            string errInPlace;
            if (TryCopyReplace(gp, inputGdb, layerName, leafName, correctedPath, originalPath, out errInPlace))
            {
                Note(result, "[替换] 输入GDB ← " + layerName
                    + "（错误图层已覆盖；后期运算仍读输入库）");
                return true;
            }

            // 2) 要素数据集内无法混入新坐标系时，改写到 GDB 根目录
            if (slash >= 0
                && !string.Equals(originalPath, rootPath, StringComparison.OrdinalIgnoreCase))
            {
                string errRoot;
                // 尽量先删掉原路径再写根目录
                TryDeleteTarget(gp, inputGdb, layerName, originalPath);
                if (TryCopyReplace(gp, inputGdb, leafName, leafName, correctedPath, rootPath, out errRoot))
                {
                    Note(result, "[替换] 输入GDB ← " + leafName
                        + "（原路径 " + layerName + " 因坐标系变更无法写回要素数据集，已放到 GDB 根目录；"
                        + "请到「数据配置」确认该角色仍指向此图层）");
                    return true;
                }

                Note(result, "[替换失败] " + layerName + ": " + errRoot
                    + "（先尝试原路径失败: " + errInPlace + "）。"
                    + "正确结果仍在暂存库: " + correctedPath
                    + "。请确认地图已清空、无其他程序打开该 GDB 后重试。");
                return false;
            }

            Note(result, "[替换失败] " + layerName + ": " + errInPlace
                + "。正确结果仍保留在暂存库: " + correctedPath
                + "（请确认地图已清空、无其他程序打开该 GDB 后重试）");
            return false;
        }

        /// <summary>删除目标 → CopyFeatures；失败则 FeatureClassToFeatureClass；再失败则 staging 名中转。</summary>
        private static bool TryCopyReplace(
            GeoprocessorHelper gp,
            string inputGdb,
            string openName,
            string leafName,
            string correctedPath,
            string destPath,
            out string error)
        {
            error = null;
            FileGdbLockHelper.ForceComRelease();

            TryDeleteTarget(gp, inputGdb, openName, destPath);

            try
            {
                CopyFeatures copy = new CopyFeatures();
                copy.in_features = correctedPath;
                copy.out_feature_class = destPath;
                gp.Execute(copy, "Replace-" + leafName);
                return true;
            }
            catch (Exception exCopy)
            {
                error = exCopy.Message;
            }

            // 回退：先写到临时名，再删目标、再拷到正式名（避开「删不掉却覆盖失败」）
            string stagingName = "r_" + StableHash(leafName).ToString("0000");
            string stagingPath = OutputGdbHelper.DatasetPath(inputGdb, stagingName);
            try
            {
                OutputGdbHelper.TryDeleteDataset(gp, stagingPath);
                CopyFeatures copyStg = new CopyFeatures();
                copyStg.in_features = correctedPath;
                copyStg.out_feature_class = stagingPath;
                gp.Execute(copyStg, "ReplaceStg-" + leafName);

                TryDeleteTarget(gp, inputGdb, openName, destPath);
                FileGdbLockHelper.ForceComRelease();

                CopyFeatures copyFinal = new CopyFeatures();
                copyFinal.in_features = stagingPath;
                copyFinal.out_feature_class = destPath;
                gp.Execute(copyFinal, "ReplaceFinal-" + leafName);
                try { OutputGdbHelper.TryDeleteDataset(gp, stagingPath); }
                catch { }
                return true;
            }
            catch (Exception exStg)
            {
                error = (error ?? string.Empty) + "；中转写回失败: " + exStg.Message;
                try { OutputGdbHelper.TryDeleteDataset(gp, stagingPath); }
                catch { }
                return false;
            }
        }

        private static void TryDeleteTarget(
            GeoprocessorHelper gp,
            string inputGdb,
            string openName,
            string datasetPath)
        {
            string lockMsg;
            if (!FileGdbLockHelper.TryDeleteFeatureClassExclusive(inputGdb, openName, out lockMsg))
            {
                OutputGdbHelper.TryDeleteDataset(gp, datasetPath);
            }
            else if (FileGdbLockHelper.FeatureClassExists(inputGdb, openName))
            {
                OutputGdbHelper.TryDeleteDataset(gp, datasetPath);
            }
            FileGdbLockHelper.ForceComRelease();
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
                Note(result, "[投影] " + layerName + " → " + (srcSr != null ? srcSr.Name : "?")
                    + " ⇒ " + targetSr.Name);
            }
            else if (doProject && sameSr)
            {
                Note(result, "[投影] " + layerName + " 已与目标坐标系一致，跳过投影。");
            }

            if (doClip && !string.IsNullOrEmpty(clipFeatures))
            {
                if (!string.IsNullOrEmpty(clipLayerName)
                    && string.Equals(layerName, clipLayerName, StringComparison.OrdinalIgnoreCase))
                {
                    CopyTo(gp, clipFeatures, finalPath, "Copy-clip-self-" + workName);
                    Note(result, "[裁剪] " + layerName + " 为分析范围本身，已输出。");
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
                        Note(result, "[裁剪] " + layerName + " → " + workName);
                    }
                    catch (Exception exClip)
                    {
                        Note(result, "[裁剪失败] " + layerName + ": " + exClip.Message
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
                    Note(result, "[复制] " + layerName + "（坐标系已一致）");
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
                Note(result, "[投影] 裁剪范围 " + clipLayerName);
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

        private static void Note(FeaturePreprocessResult result, string text)
        {
            if (result != null)
            {
                result.Messages.Add(text);
            }
            if (_progress != null)
            {
                _progress(text, _progressPercent);
            }
        }

        private static void Report(Action<string, int> progress, FeaturePreprocessResult result, string text, int percent)
        {
            _progressPercent = percent;
            Note(result, text);
        }
    }
}

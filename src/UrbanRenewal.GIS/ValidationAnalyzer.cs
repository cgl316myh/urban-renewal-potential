using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ESRI.ArcGIS.AnalysisTools;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using UrbanRenewal.Model;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// 已更新宗地与评价结果对标：空间连接、等级分布、差异标注、验证报告。
    /// </summary>
    public static class ValidationAnalyzer
    {
        public static ValidationResult Run(
            GeoprocessorHelper gp,
            string updatedParcelFc,
            string scoredParcelFc,
            string outputGdb,
            string diffName,
            double highThreshold,
            double passRatio,
            string reviewComment,
            IList<string> messages)
        {
            ValidationResult result = new ValidationResult();
            result.OutputGdbPath = outputGdb;
            if (gp == null || string.IsNullOrEmpty(updatedParcelFc) || string.IsNullOrEmpty(scoredParcelFc))
            {
                if (messages != null)
                {
                    messages.Add("验证输入无效：需要已更新宗地与评价宗地。");
                }
                return result;
            }

            string joined = OutputGdbHelper.DatasetPath(outputGdb, "valid_join");
            OutputGdbHelper.TryDeleteDataset(gp, joined);

            try
            {
                if (messages != null)
                {
                    messages.Add("正在空间连接：已更新宗地 ∩ 评价宗地（可能较久）...");
                }
                SpatialJoin sj = new SpatialJoin();
                sj.target_features = updatedParcelFc;
                sj.join_features = scoredParcelFc;
                sj.out_feature_class = joined;
                sj.join_operation = "JOIN_ONE_TO_ONE";
                sj.join_type = "KEEP_ALL";
                sj.match_option = "INTERSECT";
                gp.Execute(sj, "SpatialJoin-Validation");
                if (messages != null)
                {
                    messages.Add("空间连接完成: 已更新宗地 ∩ 评价宗地");
                }
            }
            catch (Exception ex)
            {
                if (messages != null)
                {
                    messages.Add("空间连接失败: " + ex.Message);
                }
                return result;
            }

            if (messages != null)
            {
                messages.Add("统计已更新宗地得分分布...");
            }

            Dictionary<string, int> levelCounts = new Dictionary<string, int>();
            int updated = 0;
            int high = 0;
            int withScore = 0;
            List<int> lowOids = new List<int>();

            IFeatureClass fc = null;
            IFeatureCursor cursor = null;
            try
            {
                fc = OpenFeatureClass(joined);
                if (fc == null)
                {
                    if (messages != null)
                    {
                        messages.Add("无法打开连接结果。");
                    }
                    return result;
                }

                int scoreIdx = FindFieldIgnoreCase(fc, ParcelZonalLinker.FieldPotentialScore);
                int levelIdx = FindFieldIgnoreCase(fc, ParcelZonalLinker.FieldPotentialLevel);
                cursor = fc.Search(null, false);
                IFeature feature = cursor.NextFeature();
                while (feature != null)
                {
                    updated++;
                    double score = 0;
                    bool hasScore = false;
                    if (scoreIdx >= 0)
                    {
                        object v = feature.get_Value(scoreIdx);
                        if (v != null && v != DBNull.Value)
                        {
                            score = Convert.ToDouble(v, CultureInfo.InvariantCulture);
                            hasScore = true;
                            withScore++;
                        }
                    }

                    string level = null;
                    if (levelIdx >= 0)
                    {
                        object lv = feature.get_Value(levelIdx);
                        if (lv != null && lv != DBNull.Value)
                        {
                            level = Convert.ToString(lv);
                        }
                    }
                    if (string.IsNullOrEmpty(level) && hasScore)
                    {
                        level = PotentialLevel.ToName(score);
                    }
                    if (string.IsNullOrEmpty(level))
                    {
                        level = "未赋值";
                    }

                    if (!levelCounts.ContainsKey(level))
                    {
                        levelCounts[level] = 0;
                    }
                    levelCounts[level] = levelCounts[level] + 1;

                    if (hasScore && score >= highThreshold)
                    {
                        high++;
                    }
                    else if (hasScore && score < highThreshold)
                    {
                        lowOids.Add(feature.OID);
                    }

                    feature = cursor.NextFeature();
                }
            }
            finally
            {
                if (cursor != null)
                {
                    Marshal.FinalReleaseComObject(cursor);
                }
                if (fc != null)
                {
                    Marshal.FinalReleaseComObject(fc);
                }
            }

            result.UpdatedCount = updated;
            result.HighCount = high;
            result.LevelCounts = levelCounts;
            double denom = withScore > 0 ? withScore : updated;
            result.HighRatio = denom > 0 ? (double)high / denom : 0;
            result.Passed = denom > 0 && result.HighRatio >= passRatio;

            if (messages != null)
            {
                messages.Add("已更新宗地数: " + updated + "（有得分: " + withScore + "）");
                messages.Add("高等级(≥" + highThreshold + ")数量: " + high
                    + "，占比: " + (result.HighRatio * 100).ToString("0.##", CultureInfo.InvariantCulture) + "%");
                messages.Add(result.Passed
                    ? "验证结论: 通过（已更新地块主要集中在高/极高等级）"
                    : "验证结论: 未通过（建议调整权重后重新评价）");
            }

            // 差异：已更新但得分偏低
            string diff = OutputGdbHelper.DatasetPath(outputGdb, string.IsNullOrEmpty(diffName) ? "valid_diff" : diffName);
            try
            {
                OutputGdbHelper.TryDeleteDataset(gp, diff);
                if (lowOids.Count > 0)
                {
                    if (messages != null)
                    {
                        messages.Add("生成偏低已更新宗地差异图层...");
                    }
                    string where = BuildOidWhere(fcOidField(joined), lowOids);
                    Select select = new Select();
                    select.in_features = joined;
                    select.out_feature_class = diff;
                    select.where_clause = where;
                    gp.Execute(select, "Select-ValidDiff");
                    result.DiffFeatureClassPath = diff;
                    if (messages != null)
                    {
                        messages.Add("差异标注图层: " + Path.GetFileName(diff) + "（" + lowOids.Count + " 条偏低已更新宗地）");
                    }
                }
                else if (messages != null)
                {
                    messages.Add("无偏低已更新宗地，未生成差异图层。");
                }
            }
            catch (Exception ex)
            {
                if (messages != null)
                {
                    messages.Add("差异图层生成失败: " + ex.Message);
                }
            }

            string reportDir = Path.Combine(Path.GetDirectoryName(outputGdb) ?? outputGdb, "Reports");
            Directory.CreateDirectory(reportDir);
            string reportPath = Path.Combine(reportDir, "validation_report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".html");
            if (messages != null)
            {
                messages.Add("写入验证报告 HTML...");
            }
            File.WriteAllText(reportPath, BuildHtmlReport(result, reviewComment, highThreshold, passRatio), Encoding.UTF8);
            result.ReportPath = reportPath;
            if (messages != null)
            {
                messages.Add("验证报告: " + reportPath);
            }

            result.Success = true;
            return result;
        }

        private static string BuildHtmlReport(
            ValidationResult result,
            string comment,
            double highThreshold,
            double passRatio)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"/><title>验证校核报告</title>");
            sb.AppendLine("<style>body{font-family:Microsoft YaHei,sans-serif;margin:24px;} table{border-collapse:collapse;} td,th{border:1px solid #ccc;padding:6px 10px;} .ok{color:#27ae60;} .bad{color:#c0392b;}</style></head><body>");
            sb.AppendLine("<h1>城市更新潜力评价 — 验证校核报告</h1>");
            sb.AppendLine("<p>生成时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "</p>");
            sb.AppendLine("<p>判定规则: 已更新宗地中得分≥" + highThreshold
                + " 的占比 ≥ " + (passRatio * 100).ToString("0.#") + "% 则通过</p>");
            sb.AppendLine("<h2>结论</h2><p class=\"" + (result.Passed ? "ok" : "bad") + "\"><strong>"
                + (result.Passed ? "通过" : "未通过") + "</strong></p>");
            sb.AppendLine("<p>已更新宗地: " + result.UpdatedCount
                + "；高等级: " + result.HighCount
                + "；占比: " + (result.HighRatio * 100).ToString("0.##") + "%</p>");
            sb.AppendLine("<h2>等级分布</h2><table><tr><th>等级</th><th>数量</th></tr>");
            foreach (KeyValuePair<string, int> kv in result.LevelCounts)
            {
                sb.AppendLine("<tr><td>" + Escape(kv.Key) + "</td><td>" + kv.Value + "</td></tr>");
            }
            sb.AppendLine("</table>");
            if (!string.IsNullOrEmpty(comment))
            {
                sb.AppendLine("<h2>审核意见</h2><p>" + Escape(comment) + "</p>");
            }
            if (!string.IsNullOrEmpty(result.DiffFeatureClassPath))
            {
                sb.AppendLine("<h2>差异图层</h2><p>" + Escape(result.DiffFeatureClassPath) + "</p>");
            }
            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private static string fcOidField(string path)
        {
            IFeatureClass fc = null;
            try
            {
                fc = OpenFeatureClass(path);
                return fc != null ? fc.OIDFieldName : "OBJECTID";
            }
            catch
            {
                return "OBJECTID";
            }
            finally
            {
                if (fc != null)
                {
                    Marshal.FinalReleaseComObject(fc);
                }
            }
        }

        private static string BuildOidWhere(string oidField, List<int> oids)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(oidField);
            sb.Append(" IN (");
            for (int i = 0; i < oids.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }
                sb.Append(oids[i].ToString(CultureInfo.InvariantCulture));
                if (i >= 999)
                {
                    break;
                }
            }
            sb.Append(")");
            return sb.ToString();
        }

        private static int FindFieldIgnoreCase(IFeatureClass fc, string name)
        {
            if (fc == null || string.IsNullOrEmpty(name))
            {
                return -1;
            }
            int idx = fc.FindField(name);
            if (idx >= 0)
            {
                return idx;
            }
            for (int i = 0; i < fc.Fields.FieldCount; i++)
            {
                if (string.Equals(fc.Fields.get_Field(i).Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            // SpatialJoin 可能加前缀
            for (int i = 0; i < fc.Fields.FieldCount; i++)
            {
                string n = fc.Fields.get_Field(i).Name;
                if (n != null && n.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return i;
                }
            }
            return -1;
        }

        private static IFeatureClass OpenFeatureClass(string path)
        {
            string gdb = Path.GetDirectoryName(path);
            string name = Path.GetFileName(path);
            if (string.IsNullOrEmpty(gdb) || string.IsNullOrEmpty(name))
            {
                return null;
            }
            IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
            IFeatureWorkspace fw = factory.OpenFromFile(gdb, 0) as IFeatureWorkspace;
            if (fw == null)
            {
                return null;
            }
            try
            {
                return fw.OpenFeatureClass(name);
            }
            catch
            {
                return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.NetworkAnalyst;
using ESRI.ArcGIS.SpatialAnalystTools;
using ESRI.ArcGIS.esriSystem;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// 路网可达性：预建 Network Dataset 服务区赋 1–5 分；无路网时欧氏距离回退。
    /// </summary>
    public static class RoadNetworkAccessibilityBuilder
    {
        public static readonly double[] DefaultBreaksMeters = new double[] { 1000, 2000, 3000, 5000, 8000 };
        public static readonly int[] DefaultScores = new int[] { 5, 4, 3, 2, 1 };

        private static Action<string> _onMessage;

        public static string Build(
            GeoprocessorHelper gp,
            string sourceGdbPath,
            string outputGdb,
            string facilityFeatureClass,
            string featureDatasetName,
            string networkName,
            string impedanceAttribute,
            double cellSize,
            IList<string> messages,
            Action<string> onMessage)
        {
            _onMessage = onMessage;
            try
            {
                return BuildCore(gp, sourceGdbPath, outputGdb, facilityFeatureClass,
                    featureDatasetName, networkName, impedanceAttribute, cellSize, messages);
            }
            finally
            {
                _onMessage = null;
            }
        }

        public static string Build(
            GeoprocessorHelper gp,
            string sourceGdbPath,
            string outputGdb,
            string facilityFeatureClass,
            string featureDatasetName,
            string networkName,
            string impedanceAttribute,
            double cellSize,
            IList<string> messages)
        {
            return Build(gp, sourceGdbPath, outputGdb, facilityFeatureClass,
                featureDatasetName, networkName, impedanceAttribute, cellSize, messages, null);
        }

        // CSE：否则 AccessViolation 无法被捕获，进程直接退出
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private static string BuildCore(
            GeoprocessorHelper gp,
            string sourceGdbPath,
            string outputGdb,
            string facilityFeatureClass,
            string featureDatasetName,
            string networkName,
            string impedanceAttribute,
            double cellSize,
            IList<string> messages)
        {
            if (gp == null || string.IsNullOrEmpty(sourceGdbPath) || string.IsNullOrEmpty(outputGdb))
            {
                AddMsg(messages, "路网可达性：参数无效。");
                return null;
            }
            if (string.IsNullOrEmpty(facilityFeatureClass))
            {
                AddMsg(messages, "路网可达性：缺少中心点/CBD 设施图层，已跳过。");
                return null;
            }

            string fdName = string.IsNullOrEmpty(featureDatasetName)
                ? NetworkDatasetHelper.DefaultFeatureDataset : featureDatasetName;
            string ndName = string.IsNullOrEmpty(networkName)
                ? NetworkDatasetHelper.DefaultNetworkName : networkName;
            string impedance = string.IsNullOrEmpty(impedanceAttribute)
                ? NetworkDatasetHelper.DefaultImpedance : impedanceAttribute;

            string facilities = EnsurePointFacilities(gp, facilityFeatureClass, outputGdb, messages);
            if (string.IsNullOrEmpty(facilities))
            {
                return null;
            }

            INetworkDataset nd;
            string openMsg;
            if (!NetworkDatasetHelper.TryOpen(sourceGdbPath, fdName, ndName, out nd, out openMsg))
            {
                AddMsg(messages, openMsg);
                AddMsg(messages, "路网可达性：改用欧氏距离近似（无预建路网）。");
                return BuildEuclideanFallback(gp, facilities, outputGdb, cellSize, messages);
            }
            AddMsg(messages, openMsg);

            impedance = NetworkDatasetHelper.FindCostAttributeName(nd, impedance);
            AddMsg(messages, "路网阻抗属性: " + impedance);

            string saRaster = null;
            try
            {
                saRaster = BuildServiceAreaRaster(
                    gp, nd, facilities, outputGdb, impedance, cellSize, messages);
            }
            catch (AccessViolationException exAv)
            {
                AddMsg(messages, "服务区分析发生内存访问异常（已捕获）: " + exAv.Message);
            }
            catch (SEHException exSeh)
            {
                AddMsg(messages, "服务区分析发生原生异常（已捕获）: " + exSeh.Message);
            }
            catch (Exception ex)
            {
                AddMsg(messages, "服务区分析失败: " + ex.Message);
            }

            if (!string.IsNullOrEmpty(saRaster))
            {
                return saRaster;
            }

            // 已有 ND 时不回退欧氏，避免结果口径变化
            AddMsg(messages, "路网可达性：服务区失败（已有预建路网，不回退欧氏距离）。");
            return null;
        }

        // 首次 Solve/导出在部分 ArcGIS 上不稳定：预热 + 最多 3 次重试
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private static string BuildServiceAreaRaster(
            GeoprocessorHelper gp,
            INetworkDataset networkDataset,
            string facilitiesPath,
            string outputGdb,
            string impedance,
            double cellSize,
            IList<string> messages)
        {
            const int maxAttempts = 3;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                if (attempt == 1)
                {
                    WarmUpServiceAreaSolve(networkDataset, facilitiesPath, impedance, messages);
                }
                else
                {
                    AddMsg(messages, "路网服务区第 " + attempt + " 次尝试...");
                }
                TryComCleanup();

                string raster = null;
                try
                {
                    raster = TryBuildServiceAreaOnce(
                        gp, networkDataset, facilitiesPath, outputGdb, impedance, cellSize, messages);
                }
                catch (AccessViolationException exAv)
                {
                    AddMsg(messages, "第 " + attempt + " 次服务区尝试发生内存访问异常: " + exAv.Message);
                }
                catch (SEHException exSeh)
                {
                    AddMsg(messages, "第 " + attempt + " 次服务区尝试发生原生异常: " + exSeh.Message);
                }

                if (!string.IsNullOrEmpty(raster))
                {
                    return raster;
                }
            }
            return null;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private static void WarmUpServiceAreaSolve(
            INetworkDataset networkDataset,
            string facilitiesPath,
            string impedance,
            IList<string> messages)
        {
            AddMsg(messages, "路网服务区冷启动预热...");
            INAContext ctx = null;
            try
            {
                ctx = CreateServiceAreaContext(networkDataset, impedance);
                if (ctx == null)
                {
                    return;
                }
                if (LoadFacilities(ctx, facilitiesPath) <= 0)
                {
                    return;
                }
                try
                {
                    ctx.Solver.Solve(ctx, new GPMessagesClass(), null);
                }
                catch
                {
                }
            }
            catch (Exception ex)
            {
                if (ex is AccessViolationException || ex is SEHException)
                {
                    AddMsg(messages, "冷启动预热异常，继续正式求解。");
                }
                else
                {
                    AddMsg(messages, "冷启动预热跳过: " + ex.Message);
                }
            }
            finally
            {
                ReleaseCom(ctx);
            }
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private static string TryBuildServiceAreaOnce(
            GeoprocessorHelper gp,
            INetworkDataset networkDataset,
            string facilitiesPath,
            string outputGdb,
            string impedance,
            double cellSize,
            IList<string> messages)
        {
            INAContext naContext = CreateServiceAreaContext(networkDataset, impedance);
            if (naContext == null)
            {
                AddMsg(messages, "无法创建服务区分析上下文（请确认 Network Analyst 许可已签出）。");
                return null;
            }

            try
            {
                int loaded = LoadFacilities(naContext, facilitiesPath);
                AddMsg(messages, "服务区设施加载: " + loaded + " 个");
                if (loaded <= 0)
                {
                    AddMsg(messages, "设施未能定位到路网，请检查设施与路网是否同坐标系且空间邻近。");
                    return null;
                }
                LogFacilityLocateStatus(naContext, messages);

                AddMsg(messages, "正在求解路网服务区（打断 1/2/3/5/8 km，耗时可能较长）...");
                IGPMessages gpMessages = new GPMessagesClass();
                bool ok = false;
                try
                {
                    ok = naContext.Solver.Solve(naContext, gpMessages, null);
                }
                catch (Exception exSolve)
                {
                    AddMsg(messages, "NA Solve 异常: " + exSolve.Message);
                }
                LogGpMessages(gpMessages, messages);

                INAClass saClass = naContext.NAClasses.get_ItemByName("SAPolygons") as INAClass;
                IFeatureClass saFc = saClass as IFeatureClass;
                int polyCount;
                try
                {
                    polyCount = saFc == null ? 0 : saFc.FeatureCount(null);
                }
                catch (Exception exCnt)
                {
                    AddMsg(messages, "读取服务区多边形失败: " + exCnt.Message);
                    return null;
                }

                // Solve==false 时常仍有完整 SAPolygons（警告/部分未落网）；无多边形才失败
                if (polyCount <= 0)
                {
                    AddMsg(messages, ok
                        ? "服务区未生成多边形。"
                        : "NA Solve 返回 false，且未生成服务区多边形。");
                    return null;
                }
                if (!ok)
                {
                    int expected = loaded * DefaultBreaksMeters.Length;
                    AddMsg(messages, "NA Solve 返回 false，已有多边形 " + polyCount + " 个"
                        + (polyCount == expected ? "（与设施×打断数一致）" : "")
                        + "，继续导出。");
                }
                else
                {
                    AddMsg(messages, "服务区多边形: " + polyCount + " 个");
                }

                string polyOut = OutputGdbHelper.DatasetPath(outputGdb, "road_sa_poly");
                OutputGdbHelper.TryDeleteDataset(gp, polyOut);
                AddMsg(messages, "导出服务区多边形到输出库...");
                if (!ExportFeatureClass(gp, saFc, polyOut, messages))
                {
                    return null;
                }
                AddMsg(messages, "服务区多边形已导出: " + polyOut);

                AssignScoresByToBreak(gp, polyOut, DefaultBreaksMeters, DefaultScores);
                string raster = OutputGdbHelper.DatasetPath(outputGdb, "road_access");
                OutputGdbHelper.TryDeleteDataset(gp, raster);
                FeatureToRasterScore(gp, polyOut, raster, cellSize);
                AddMsg(messages, "路网可达性（服务区）栅格: " + raster);
                return raster;
            }
            finally
            {
                ReleaseCom(naContext);
            }
        }

        private static void LogFacilityLocateStatus(INAContext naContext, IList<string> messages)
        {
            try
            {
                INAClass facClass = naContext.NAClasses.get_ItemByName("Facilities") as INAClass;
                IFeatureClass fc = facClass as IFeatureClass;
                if (fc == null)
                {
                    return;
                }
                int statusIdx = fc.FindField("Status");
                if (statusIdx < 0)
                {
                    return;
                }

                int ok = 0, bad = 0;
                IFeatureCursor cur = fc.Search(null, true);
                try
                {
                    IFeature f;
                    while ((f = cur.NextFeature()) != null)
                    {
                        object v = f.get_Value(statusIdx);
                        int code = (v == null || v == DBNull.Value) ? -1 : Convert.ToInt32(v);
                        if (code == 0)
                        {
                            ok++;
                        }
                        else
                        {
                            bad++;
                        }
                    }
                }
                finally
                {
                    Marshal.FinalReleaseComObject(cur);
                }
                AddMsg(messages, "设施落网状态: 成功 " + ok + " / 异常 " + bad);
            }
            catch (Exception ex)
            {
                AddMsg(messages, "读取设施落网状态失败: " + ex.Message);
            }
        }

        private static void TryComCleanup()
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            catch
            {
            }
        }

        private static void ReleaseCom(object com)
        {
            if (com == null)
            {
                return;
            }
            try
            {
                Marshal.FinalReleaseComObject(com);
            }
            catch
            {
            }
        }

        private static INAContext CreateServiceAreaContext(INetworkDataset networkDataset, string impedance)
        {
            IDatasetComponent component = networkDataset as IDatasetComponent;
            if (component == null)
            {
                return null;
            }
            IDENetworkDataset deNd = component.DataElement as IDENetworkDataset;
            if (deNd == null)
            {
                return null;
            }

            INASolver solver = new NAServiceAreaSolverClass();
            INAContext context = solver.CreateContext(deNd, "RoadAccessSA");
            INAContextEdit contextEdit = (INAContextEdit)context;
            contextEdit.Bind(networkDataset, null);

            INASolverSettings settings = (INASolverSettings)solver;
            settings.ImpedanceAttributeName = impedance;

            INAServiceAreaSolver2 sa = (INAServiceAreaSolver2)solver;
            IDoubleArray breaks = new DoubleArrayClass();
            for (int i = 0; i < DefaultBreaksMeters.Length; i++)
            {
                breaks.Add(DefaultBreaksMeters[i]);
            }
            sa.DefaultBreaks = breaks;
            sa.TravelDirection = esriNATravelDirection.esriNATravelDirectionFromFacility;
            sa.OutputPolygons = esriNAOutputPolygonType.esriNAOutputPolygonSimplified;
            sa.OverlapPolygons = false; // 按最近设施，避免多中心重叠盖顶
            sa.SplitPolygonsAtBreaks = true;
            sa.MergeSimilarPolygonRanges = false; // 保留各级 ToBreak 环
            solver.UpdateContext(context, deNd, null); // 否则打断距离可能不生效

            return context;
        }

        private static int LoadFacilities(INAContext naContext, string facilitiesPath)
        {
            IFeatureClass fc = OpenFeatureClass(facilitiesPath);
            if (fc == null)
            {
                return 0;
            }

            INAClass naClass = naContext.NAClasses.get_ItemByName("Facilities") as INAClass;
            if (naClass == null)
            {
                return 0;
            }
            naClass.DeleteAllRows();

            INAClassLoader loader = new NAClassLoaderClass();
            INAClassFieldMap fieldMap = new NAClassFieldMapClass();
            fieldMap.CreateMapping(naClass.ClassDefinition, fc.Fields);
            loader.FieldMap = fieldMap;
            loader.NAClass = naClass;
            loader.Locator = naContext.Locator;
            if (loader.Locator != null)
            {
                loader.Locator.SnapTolerance = 2000;
            }

            IFeatureCursor cursor = fc.Search(null, false);
            int rowsIn = 0;
            int rowsLocated = 0;
            try
            {
                loader.Load((ICursor)cursor, null, ref rowsIn, ref rowsLocated);
            }
            finally
            {
                if (cursor != null)
                {
                    Marshal.FinalReleaseComObject(cursor);
                }
            }
            return rowsLocated;
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private static bool ExportFeatureClass(
            GeoprocessorHelper gp,
            IFeatureClass source,
            string outPath,
            IList<string> messages)
        {
            if (TryExportWithCopyFeatures(gp, source, outPath, messages))
            {
                return true;
            }
            if (TryExportWithFeatureClassToFeatureClass(gp, source, outPath, messages))
            {
                return true;
            }
            return TryExportByCursor(source, outPath, messages);
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private static bool TryExportWithCopyFeatures(
            GeoprocessorHelper gp, IFeatureClass source, string outPath, IList<string> messages)
        {
            try
            {
                AddMsg(messages, "服务区导出: CopyFeatures...");
                IFeatureLayer layer = new FeatureLayerClass();
                layer.FeatureClass = source;
                layer.Name = "SAPolygons";
                CopyFeatures copy = new CopyFeatures();
                copy.in_features = layer;
                copy.out_feature_class = outPath;
                gp.Execute(copy, "Copy-SAPolygons");
                AddMsg(messages, "服务区导出成功（CopyFeatures）。");
                return true;
            }
            catch (Exception ex)
            {
                AddMsg(messages, "CopyFeatures 失败: " + ex.Message);
                return false;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private static bool TryExportWithFeatureClassToFeatureClass(
            GeoprocessorHelper gp, IFeatureClass source, string outPath, IList<string> messages)
        {
            try
            {
                AddMsg(messages, "服务区导出: FeatureClassToFeatureClass...");
                IFeatureLayer layer = new FeatureLayerClass();
                layer.FeatureClass = source;
                layer.Name = "SAPolygons";
                ESRI.ArcGIS.ConversionTools.FeatureClassToFeatureClass convert =
                    new ESRI.ArcGIS.ConversionTools.FeatureClassToFeatureClass();
                convert.in_features = layer;
                convert.out_path = System.IO.Path.GetDirectoryName(outPath);
                convert.out_name = System.IO.Path.GetFileName(outPath);
                gp.Execute(convert, "Export-SAPolygons");
                AddMsg(messages, "服务区导出成功（FeatureClassToFeatureClass）。");
                return true;
            }
            catch (Exception ex)
            {
                AddMsg(messages, "FeatureClassToFeatureClass 失败: " + ex.Message);
                return false;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private static bool TryExportByCursor(IFeatureClass source, string outPath, IList<string> messages)
        {
            try
            {
                AddMsg(messages, "服务区导出: 游标复制...");
                if (source == null || string.IsNullOrEmpty(outPath))
                {
                    return false;
                }

                string gdb = System.IO.Path.GetDirectoryName(outPath);
                string name = System.IO.Path.GetFileName(outPath);
                IWorkspaceFactory factory = new ESRI.ArcGIS.DataSourcesGDB.FileGDBWorkspaceFactoryClass();
                IFeatureWorkspace fws = (IFeatureWorkspace)factory.OpenFromFile(gdb, 0);

                try
                {
                    IFeatureClass existing = fws.OpenFeatureClass(name);
                    ((IDataset)existing).Delete();
                }
                catch
                {
                }

                IFields fields = source.Fields;
                IFeatureClass outFc = fws.CreateFeatureClass(
                    name, fields, null, null, esriFeatureType.esriFTSimple, source.ShapeFieldName, "");

                IFeatureCursor inCur = source.Search(null, false);
                IFeatureCursor outCur = outFc.Insert(true);
                try
                {
                    IFeatureBuffer buf = outFc.CreateFeatureBuffer();
                    IFeature f;
                    int n = 0;
                    while ((f = inCur.NextFeature()) != null)
                    {
                        buf.Shape = f.ShapeCopy;
                        for (int i = 0; i < fields.FieldCount; i++)
                        {
                            IField field = fields.get_Field(i);
                            if (!field.Editable
                                || field.Type == esriFieldType.esriFieldTypeGeometry
                                || field.Type == esriFieldType.esriFieldTypeOID)
                            {
                                continue;
                            }
                            int outIdx = outFc.FindField(field.Name);
                            if (outIdx >= 0)
                            {
                                buf.set_Value(outIdx, f.get_Value(i));
                            }
                        }
                        outCur.InsertFeature(buf);
                        n++;
                    }
                    outCur.Flush();
                    AddMsg(messages, "服务区导出成功（游标，" + n + " 个）。");
                    return n > 0;
                }
                finally
                {
                    if (inCur != null)
                    {
                        Marshal.FinalReleaseComObject(inCur);
                    }
                    if (outCur != null)
                    {
                        Marshal.FinalReleaseComObject(outCur);
                    }
                }
            }
            catch (Exception ex)
            {
                AddMsg(messages, "游标导出失败: " + ex.Message);
                return false;
            }
        }

        private static void AssignScoresByToBreak(
            GeoprocessorHelper gp,
            string featureClass,
            double[] breaks,
            int[] scores)
        {
            try
            {
                AddField add = new AddField();
                add.in_table = featureClass;
                add.field_name = "SCORE";
                add.field_type = "SHORT";
                gp.Execute(add, "AddField-SCORE-road");
            }
            catch
            {
                // 字段可能已存在
            }

            // FileGDB CalculateField 嵌套 IIf 不稳定，改用游标
            IFeatureClass fc = OpenFeatureClass(featureClass);
            if (fc == null)
            {
                return;
            }
            int toBreakIdx = fc.FindField("ToBreak");
            int scoreIdx = fc.FindField("SCORE");
            if (scoreIdx < 0)
            {
                return;
            }

            IFeatureCursor cur = fc.Update(null, false);
            try
            {
                IFeature f;
                while ((f = cur.NextFeature()) != null)
                {
                    double tb = 0;
                    if (toBreakIdx >= 0 && f.get_Value(toBreakIdx) != null
                        && f.get_Value(toBreakIdx) != DBNull.Value)
                    {
                        tb = Convert.ToDouble(f.get_Value(toBreakIdx), CultureInfo.InvariantCulture);
                    }
                    f.set_Value(scoreIdx, ScoreFromToBreak(tb, breaks, scores));
                    cur.UpdateFeature(f);
                }
            }
            finally
            {
                if (cur != null)
                {
                    Marshal.FinalReleaseComObject(cur);
                }
            }
        }

        private static int ScoreFromToBreak(double toBreak, double[] breaks, int[] scores)
        {
            for (int i = 0; i < breaks.Length; i++)
            {
                if (toBreak <= breaks[i] + 0.01)
                {
                    return scores[i];
                }
            }
            return scores[scores.Length - 1];
        }

        private static string EnsurePointFacilities(
            GeoprocessorHelper gp,
            string inFeatures,
            string outputGdb,
            IList<string> messages)
        {
            try
            {
                IFeatureClass fc = OpenFeatureClass(inFeatures);
                if (fc == null)
                {
                    AddMsg(messages, "无法打开设施图层: " + inFeatures);
                    return null;
                }

                esriGeometryType gt = fc.ShapeType;
                if (gt == esriGeometryType.esriGeometryPoint
                    || gt == esriGeometryType.esriGeometryMultipoint)
                {
                    return inFeatures;
                }

            // FeatureToPoint 需 Advanced；Engine 用质心写点
            string points = OutputGdbHelper.DatasetPath(outputGdb, "road_fac_pt");
                OutputGdbHelper.TryDeleteDataset(gp, points);
                if (!CreateCentroidPoints(fc, points, messages))
                {
                    return null;
                }
                AddMsg(messages, "设施已转点(质心): " + points);
                return points;
            }
            catch (Exception ex)
            {
                AddMsg(messages, "设施转点失败: " + ex.Message);
                return null;
            }
        }

        private static bool CreateCentroidPoints(IFeatureClass source, string outPath, IList<string> messages)
        {
            try
            {
                string gdb = System.IO.Path.GetDirectoryName(outPath);
                string name = System.IO.Path.GetFileName(outPath);
                IWorkspaceFactory factory = new ESRI.ArcGIS.DataSourcesGDB.FileGDBWorkspaceFactoryClass();
                IFeatureWorkspace fws = (IFeatureWorkspace)factory.OpenFromFile(gdb, 0);

                try
                {
                    IFeatureClass existing = fws.OpenFeatureClass(name);
                    ((IDataset)existing).Delete();
                }
                catch
                {
                }

                ISpatialReference sr = ((IGeoDataset)source).SpatialReference;
                IFieldsEdit fieldsEdit = new FieldsClass();

                IFieldEdit oid = new FieldClass();
                oid.Name_2 = "OBJECTID";
                oid.Type_2 = esriFieldType.esriFieldTypeOID;
                fieldsEdit.AddField(oid);

                IGeometryDefEdit geomDef = new GeometryDefClass();
                geomDef.GeometryType_2 = esriGeometryType.esriGeometryPoint;
                geomDef.SpatialReference_2 = sr;
                geomDef.HasZ_2 = false;
                geomDef.HasM_2 = false;

                IFieldEdit shape = new FieldClass();
                shape.Name_2 = "SHAPE";
                shape.Type_2 = esriFieldType.esriFieldTypeGeometry;
                shape.GeometryDef_2 = geomDef;
                fieldsEdit.AddField(shape);

                IFeatureClass target = fws.CreateFeatureClass(
                    name, fieldsEdit, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");

                IFeatureCursor srcCur = source.Search(null, false);
                IFeatureCursor dstCur = target.Insert(true);
                int count = 0;
                try
                {
                    IFeature f;
                    while ((f = srcCur.NextFeature()) != null)
                    {
                        IGeometry geom = f.ShapeCopy;
                        if (geom == null || geom.IsEmpty)
                        {
                            continue;
                        }
                        IPoint pt = GeometryToPoint(geom);
                        if (pt == null || pt.IsEmpty)
                        {
                            continue;
                        }
                        if (sr != null)
                        {
                            pt.SpatialReference = sr;
                        }
                        IFeatureBuffer buf = target.CreateFeatureBuffer();
                        buf.Shape = pt;
                        dstCur.InsertFeature(buf);
                        count++;
                    }
                    dstCur.Flush();
                }
                finally
                {
                    if (srcCur != null) Marshal.FinalReleaseComObject(srcCur);
                    if (dstCur != null) Marshal.FinalReleaseComObject(dstCur);
                }

                if (count == 0)
                {
                    AddMsg(messages, "设施质心数量为 0。");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                AddMsg(messages, "创建质心点失败: " + ex.Message);
                return false;
            }
        }

        private static IPoint GeometryToPoint(IGeometry geom)
        {
            if (geom == null)
            {
                return null;
            }
            IArea area = geom as IArea;
            if (area != null)
            {
                return area.Centroid;
            }
            IPolyline line = geom as IPolyline;
            if (line != null)
            {
                IPoint mid = new PointClass();
                line.QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, mid);
                return mid;
            }
            IPoint pt = geom as IPoint;
            if (pt != null)
            {
                return pt;
            }
            IEnvelope env = geom.Envelope;
            if (env == null || env.IsEmpty)
            {
                return null;
            }
            IPoint c = new PointClass();
            c.PutCoords((env.XMin + env.XMax) * 0.5, (env.YMin + env.YMax) * 0.5);
            return c;
        }

        private static string BuildEuclideanFallback(
            GeoprocessorHelper gp,
            string facilitiesPath,
            string outputGdb,
            double cellSize,
            IList<string> messages)
        {
            string distRaster = OutputGdbHelper.DatasetPath(outputGdb, "road_euc_d");
            string scoreRaster = OutputGdbHelper.DatasetPath(outputGdb, "road_access");
            OutputGdbHelper.TryDeleteDataset(gp, distRaster);
            OutputGdbHelper.TryDeleteDataset(gp, scoreRaster);

            EucDistance euc = new EucDistance();
            euc.in_source_data = facilitiesPath;
            euc.out_distance_raster = distRaster;
            if (cellSize > 0)
            {
                euc.cell_size = cellSize.ToString(CultureInfo.InvariantCulture);
            }
            gp.Execute(euc, "EucDistance-road-fallback");

            string remap = BuildRemapString(DefaultBreaksMeters, DefaultScores);
            Reclassify reclass = new Reclassify();
            reclass.in_raster = distRaster;
            reclass.reclass_field = "VALUE";
            reclass.remap = remap;
            reclass.out_raster = scoreRaster;
            reclass.missing_values = "DATA";
            gp.Execute(reclass, "Reclassify-road-fallback");

            AddMsg(messages, "路网可达性（欧氏近似）栅格: " + scoreRaster);
            return scoreRaster;
        }

        private static string BuildRemapString(double[] breaks, int[] scores)
        {
            StringBuilder sb = new StringBuilder();
            double prev = 0;
            for (int i = 0; i < breaks.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(";");
                }
                sb.Append(prev.ToString(CultureInfo.InvariantCulture));
                sb.Append(" ");
                sb.Append(breaks[i].ToString(CultureInfo.InvariantCulture));
                sb.Append(" ");
                sb.Append(scores[i].ToString(CultureInfo.InvariantCulture));
                prev = breaks[i];
            }
            sb.Append(";");
            sb.Append(prev.ToString(CultureInfo.InvariantCulture));
            sb.Append(" 100000000 ");
            sb.Append(scores[scores.Length - 1].ToString(CultureInfo.InvariantCulture));
            return sb.ToString();
        }

        private static void FeatureToRasterScore(
            GeoprocessorHelper gp,
            string inFeatures,
            string outRaster,
            double cellSize)
        {
            ESRI.ArcGIS.ConversionTools.FeatureToRaster f2r =
                new ESRI.ArcGIS.ConversionTools.FeatureToRaster();
            f2r.in_features = inFeatures;
            f2r.field = "SCORE";
            f2r.out_raster = outRaster;
            f2r.cell_size = cellSize.ToString(CultureInfo.InvariantCulture);
            gp.Execute(f2r, "FeatureToRaster-road-access");
        }

        private static IFeatureClass OpenFeatureClass(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            string gdb = System.IO.Path.GetDirectoryName(path);
            string name = System.IO.Path.GetFileName(path);
            if (string.IsNullOrEmpty(gdb) || string.IsNullOrEmpty(name))
            {
                return null;
            }
            IWorkspaceFactory factory = new ESRI.ArcGIS.DataSourcesGDB.FileGDBWorkspaceFactoryClass();
            IFeatureWorkspace fws = (IFeatureWorkspace)factory.OpenFromFile(gdb, 0);
            try
            {
                return fws.OpenFeatureClass(name);
            }
            catch
            {
                return null;
            }
        }

        private static void LogGpMessages(IGPMessages messages, IList<string> outMessages)
        {
            if (messages == null || outMessages == null)
            {
                return;
            }
            try
            {
                for (int i = 0; i < messages.Count; i++)
                {
                    IGPMessage m = messages.GetMessage(i);
                    if (m == null || string.IsNullOrEmpty(m.Description))
                    {
                        continue;
                    }
                    if (m.Type == esriGPMessageType.esriGPMessageTypeError
                        || m.Type == esriGPMessageType.esriGPMessageTypeWarning
                        || m.Type == esriGPMessageType.esriGPMessageTypeAbort)
                    {
                        outMessages.Add("NA[" + m.Type + "]: " + m.Description);
                    }
                }
            }
            catch
            {
            }
        }

        private static void AddMsg(IList<string> messages, string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            if (messages != null)
            {
                messages.Add(text);
            }
            if (_onMessage != null)
            {
                _onMessage(text);
            }
        }
    }
}

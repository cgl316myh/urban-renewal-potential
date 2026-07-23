using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
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
    /// 路网可达性：基于预建 Network Dataset 做服务区分析，按通行距离赋 1–5 分。
    /// 无 Network Analyst 许可时回退为到中心点的欧氏距离重分类。
    /// </summary>
    public static class RoadNetworkAccessibilityBuilder
    {
        /// <summary>默认服务区打断距离（米），对应阻抗 Length。</summary>
        public static readonly double[] DefaultBreaksMeters = new double[] { 1000, 2000, 3000, 5000, 8000 };

        /// <summary>与 DefaultBreaksMeters 对应的得分（近→远：5→1）。</summary>
        public static readonly int[] DefaultScores = new int[] { 5, 4, 3, 2, 1 };

        /// <summary>
        /// 生成路网可达性得分栅格。路网必须事先建好；本方法不会构建 Network Dataset。
        /// </summary>
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

            // 设施转点（面/线 → 点）
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
            catch (Exception ex)
            {
                AddMsg(messages, "服务区分析失败: " + ex.Message);
                saRaster = null;
            }

            if (!string.IsNullOrEmpty(saRaster))
            {
                return saRaster;
            }

            AddMsg(messages, "路网可达性：服务区失败，回退欧氏距离近似。");
            return BuildEuclideanFallback(gp, facilities, outputGdb, cellSize, messages);
        }

        private static string BuildServiceAreaRaster(
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

            int loaded = LoadFacilities(naContext, facilitiesPath);
            AddMsg(messages, "服务区设施加载: " + loaded + " 个");
            if (loaded <= 0)
            {
                AddMsg(messages, "设施未能定位到路网，请检查设施与路网是否同坐标系且空间邻近。");
                return null;
            }

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
            int polyCount = (saFc == null) ? 0 : saFc.FeatureCount(null);
            // Engine 上 Solve 常返回 false 且无 GP 消息，但仍可能已写出 SAPolygons
            if (polyCount <= 0)
            {
                if (!ok)
                {
                    AddMsg(messages, "NA Solve 返回失败，且未生成服务区多边形。");
                }
                else
                {
                    AddMsg(messages, "服务区未生成多边形。");
                }
                return null;
            }
            if (!ok)
            {
                AddMsg(messages, "NA Solve 返回 false，但已生成服务区多边形 " + polyCount + " 个，继续使用。");
            }
            else
            {
                AddMsg(messages, "服务区多边形: " + polyCount + " 个");
            }

            string polyOut = OutputGdbHelper.DatasetPath(outputGdb, "road_sa_poly");
            OutputGdbHelper.TryDeleteDataset(gp, polyOut);
            if (!ExportFeatureClass(gp, saFc, polyOut, messages))
            {
                return null;
            }

            AssignScoresByToBreak(gp, polyOut, DefaultBreaksMeters, DefaultScores);
            string raster = OutputGdbHelper.DatasetPath(outputGdb, "road_access");
            OutputGdbHelper.TryDeleteDataset(gp, raster);
            FeatureToRasterScore(gp, polyOut, raster, cellSize);
            AddMsg(messages, "路网可达性（服务区）栅格: " + raster);
            return raster;
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
            // false：按最近设施划分，避免多中心重叠；交通可达性取到最近中心的路网距离
            sa.OverlapPolygons = false;
            sa.SplitPolygonsAtBreaks = true;
            // 保留各级打断环，便于按 ToBreak 赋 1–5 分
            sa.MergeSimilarPolygonRanges = false;

            // 将求解器参数写回上下文（否则打断距离可能不生效）
            solver.UpdateContext(context, deNd, null);

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
                // 米：设施到路网最大捕捉距离（投影坐标系下）
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

        private static bool ExportFeatureClass(
            GeoprocessorHelper gp,
            IFeatureClass source,
            string outPath,
            IList<string> messages)
        {
            try
            {
                IFeatureLayer layer = new FeatureLayerClass();
                layer.FeatureClass = source;
                layer.Name = "SAPolygons";

                ESRI.ArcGIS.ConversionTools.FeatureClassToFeatureClass convert =
                    new ESRI.ArcGIS.ConversionTools.FeatureClassToFeatureClass();
                convert.in_features = layer;
                convert.out_path = System.IO.Path.GetDirectoryName(outPath);
                convert.out_name = System.IO.Path.GetFileName(outPath);
                gp.Execute(convert, "Export-SAPolygons");
                return true;
            }
            catch (Exception ex)
            {
                AddMsg(messages, "导出服务区失败: " + ex.Message);
                try
                {
                    IFeatureLayer layer = new FeatureLayerClass();
                    layer.FeatureClass = source;
                    CopyFeatures copy = new CopyFeatures();
                    copy.in_features = layer;
                    copy.out_feature_class = outPath;
                    gp.Execute(copy, "Copy-SAPolygons");
                    return true;
                }
                catch (Exception ex2)
                {
                    AddMsg(messages, "CopyFeatures 服务区失败: " + ex2.Message);
                    return CopyFeaturesByCursor(source, outPath, messages);
                }
            }
        }

        private static bool CopyFeaturesByCursor(IFeatureClass source, string outPath, IList<string> messages)
        {
            try
            {
                string gdb = System.IO.Path.GetDirectoryName(outPath);
                string name = System.IO.Path.GetFileName(outPath);
                IWorkspaceFactory factory = new ESRI.ArcGIS.DataSourcesGDB.FileGDBWorkspaceFactoryClass();
                IFeatureWorkspace fws = (IFeatureWorkspace)factory.OpenFromFile(gdb, 0);

                // 删除已存在
                try
                {
                    IFeatureClass existing = fws.OpenFeatureClass(name);
                    ((IDataset)existing).Delete();
                }
                catch
                {
                }

                IFieldsEdit fieldsEdit = new FieldsClass();
                IFieldEdit oid = new FieldClass();
                oid.Name_2 = "OBJECTID";
                oid.Type_2 = esriFieldType.esriFieldTypeOID;
                fieldsEdit.AddField(oid);

                IGeometryDefEdit geomDef = new GeometryDefClass();
                geomDef.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
                geomDef.SpatialReference_2 = ((IGeoDataset)source).SpatialReference;
                geomDef.HasZ_2 = false;
                geomDef.HasM_2 = false;

                IFieldEdit shape = new FieldClass();
                shape.Name_2 = "SHAPE";
                shape.Type_2 = esriFieldType.esriFieldTypeGeometry;
                shape.GeometryDef_2 = geomDef;
                fieldsEdit.AddField(shape);

                IFieldEdit toBreak = new FieldClass();
                toBreak.Name_2 = "ToBreak";
                toBreak.Type_2 = esriFieldType.esriFieldTypeDouble;
                fieldsEdit.AddField(toBreak);

                IFieldEdit score = new FieldClass();
                score.Name_2 = "SCORE";
                score.Type_2 = esriFieldType.esriFieldTypeSmallInteger;
                fieldsEdit.AddField(score);

                IFeatureClass target = fws.CreateFeatureClass(
                    name, fieldsEdit, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");

                int srcToBreak = source.FindField("ToBreak");
                int dstToBreak = target.FindField("ToBreak");
                int dstScore = target.FindField("SCORE");

                IFeatureCursor srcCur = source.Search(null, false);
                IFeatureCursor dstCur = target.Insert(true);
                try
                {
                    IFeature f;
                    while ((f = srcCur.NextFeature()) != null)
                    {
                        IFeatureBuffer buf = target.CreateFeatureBuffer();
                        buf.Shape = f.ShapeCopy;
                        double tb = 0;
                        if (srcToBreak >= 0 && f.get_Value(srcToBreak) != null && f.get_Value(srcToBreak) != DBNull.Value)
                        {
                            tb = Convert.ToDouble(f.get_Value(srcToBreak), CultureInfo.InvariantCulture);
                            buf.set_Value(dstToBreak, tb);
                        }
                        buf.set_Value(dstScore, ScoreFromToBreak(tb, DefaultBreaksMeters, DefaultScores));
                        dstCur.InsertFeature(buf);
                    }
                    dstCur.Flush();
                }
                finally
                {
                    if (srcCur != null) Marshal.FinalReleaseComObject(srcCur);
                    if (dstCur != null) Marshal.FinalReleaseComObject(dstCur);
                }
                return true;
            }
            catch (Exception ex)
            {
                AddMsg(messages, "游标复制服务区失败: " + ex.Message);
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

            // FileGDB 的 CalculateField 对嵌套 IIf 不稳定，改用游标赋分
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

                // FeatureToPoint 需 Advanced 许可；改用面/线质心写点（Engine 可用）
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
            // 回退：Envelope 中心
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

            // Remap: 0-1000→5, 1000-2000→4, ... >8000→1（用 Reclassify）
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
            // Remap format: "0 1000 5;1000 2000 4;..."
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
            // beyond last break → score 1
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
                // 可能在要素数据集内：path = gdb\fd\fc 较少见；或仅 gdb\fc
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
            if (messages != null && !string.IsNullOrEmpty(text))
            {
                messages.Add(text);
            }
        }
    }
}

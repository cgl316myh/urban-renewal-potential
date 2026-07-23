using System;
using System.Runtime.InteropServices;
using System.Text;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.NetworkAnalyst;
using ESRI.ArcGIS.esriSystem;
using UrbanRenewal.GIS;

namespace NetDiag
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

            try
            {
                IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
                IFeatureWorkspace fws = (IFeatureWorkspace)factory.OpenFromFile(gdb, 0);

                IFeatureDataset fd = fws.OpenFeatureDataset("roadNet");
                PrintSr("roadNet FD", (IGeoDataset)fd);

                IFeatureClass road = null;
                try { road = fws.OpenFeatureClass("road"); } catch { }
                if (road != null) PrintSr("road FC", (IGeoDataset)road);

                IFeatureClass cbd = fws.OpenFeatureClass("开发强度高区域（CBD）");
                PrintSr("CBD", (IGeoDataset)cbd);

                INetworkDataset nd = OpenNetwork(fd, "roadNet_ND");
                IDatasetComponent component = (IDatasetComponent)nd;
                IDENetworkDataset deNd = (IDENetworkDataset)component.DataElement;

                INASolver solver = new NAServiceAreaSolverClass();
                INAContext ctx = solver.CreateContext(deNd, "diagSA");
                ((INAContextEdit)ctx).Bind(nd, null);
                ((INASolverSettings)solver).ImpedanceAttributeName = "Length";
                solver.UpdateContext(ctx, deNd, null);

                INAServiceAreaSolver2 sa = (INAServiceAreaSolver2)solver;
                IDoubleArray breaks = new DoubleArrayClass();
                breaks.Add(2000);
                breaks.Add(5000);
                sa.DefaultBreaks = breaks;
                sa.OutputPolygons = esriNAOutputPolygonType.esriNAOutputPolygonSimplified;
                sa.OverlapPolygons = false;
                sa.SplitPolygonsAtBreaks = true;
                sa.TravelDirection = esriNATravelDirection.esriNATravelDirectionFromFacility;

                // Build one point FC in memory workspace is hard; use FileGDB temp in %TEMP%
                string tempGdb = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "na_fac_diag.gdb");
                GeoprocessorHelper gp = new GeoprocessorHelper();
                if (System.IO.Directory.Exists(tempGdb))
                {
                    try { System.IO.Directory.Delete(tempGdb, true); } catch { }
                }
                tempGdb = OutputGdbHelper.EnsureExists(gp, tempGdb);
                string ptPath = OutputGdbHelper.DatasetPath(tempGdb, "fac1");
                CreateOneCentroid(cbd, ptPath);
                IFeatureClass ptFc = ((IFeatureWorkspace)factory.OpenFromFile(tempGdb, 0)).OpenFeatureClass("fac1");
                PrintSr("fac1", (IGeoDataset)ptFc);
                IFeature pf = ptFc.GetFeature(1);
                IPoint p = pf.Shape as IPoint;
                Console.WriteLine("FAC_XY=" + p.X + "," + p.Y);

                INALocator locator = ctx.Locator;
                locator.SnapTolerance = 5000;
                INALocation loc;
                IPoint after;
                double dist;
                locator.QueryLocationByPoint(p, out loc, out after, out dist);
                Console.WriteLine("SNAP dist=" + dist + " empty=" + (loc == null || loc.IsEmpty)
                    + " after=" + (after != null ? after.X + "," + after.Y : "null"));

                INAClass facClass = ctx.NAClasses.get_ItemByName("Facilities") as INAClass;
                facClass.DeleteAllRows();
                INAClassLoader loader = new NAClassLoaderClass();
                INAClassFieldMap fieldMap = new NAClassFieldMapClass();
                fieldMap.CreateMapping(facClass.ClassDefinition, ptFc.Fields);
                loader.FieldMap = fieldMap;
                loader.NAClass = facClass;
                loader.Locator = locator;
                IFeatureCursor cursor = ptFc.Search(null, false);
                int rowsIn = 0, rowsLocated = 0;
                loader.Load((ICursor)cursor, null, ref rowsIn, ref rowsLocated);
                Marshal.FinalReleaseComObject(cursor);
                Console.WriteLine("LOADED in=" + rowsIn + " located=" + rowsLocated);

                IGPMessages gpMessages = new GPMessagesClass();
                bool ok = solver.Solve(ctx, gpMessages, null);
                Console.WriteLine("SOLVE=" + ok + " msgCount=" + gpMessages.Count);
                for (int i = 0; i < gpMessages.Count; i++)
                {
                    IGPMessage m = gpMessages.GetMessage(i);
                    Console.WriteLine("GP|" + m.Type + "|" + m.ErrorCode + "|" + m.Description);
                }

                INAClass polys = ctx.NAClasses.get_ItemByName("SAPolygons") as INAClass;
                Console.WriteLine("POLY=" + ((IFeatureClass)polys).FeatureCount(null));
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
            return 0;
        }

        private static void PrintSr(string label, IGeoDataset ds)
        {
            ISpatialReference sr = ds.SpatialReference;
            Console.WriteLine(label + " SR=" + (sr != null ? sr.Name : "null")
                + " code=" + (sr != null ? sr.FactoryCode.ToString() : "-")
                + " isProj=" + (sr is IProjectedCoordinateSystem)
                + " isGeo=" + (sr is IGeographicCoordinateSystem));
        }

        private static void CreateOneCentroid(IFeatureClass cbd, string outPath)
        {
            string gdb = System.IO.Path.GetDirectoryName(outPath);
            string name = System.IO.Path.GetFileName(outPath);
            IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
            IFeatureWorkspace fws = (IFeatureWorkspace)factory.OpenFromFile(gdb, 0);
            try
            {
                IFeatureClass old = fws.OpenFeatureClass(name);
                ((IDataset)old).Delete();
            }
            catch { }

            ISpatialReference sr = ((IGeoDataset)cbd).SpatialReference;
            IFieldsEdit fields = new FieldsClass();
            IFieldEdit oid = new FieldClass();
            oid.Name_2 = "OBJECTID"; oid.Type_2 = esriFieldType.esriFieldTypeOID;
            fields.AddField(oid);
            IGeometryDefEdit gdef = new GeometryDefClass();
            gdef.GeometryType_2 = esriGeometryType.esriGeometryPoint;
            gdef.SpatialReference_2 = sr;
            IFieldEdit shape = new FieldClass();
            shape.Name_2 = "SHAPE"; shape.Type_2 = esriFieldType.esriFieldTypeGeometry;
            shape.GeometryDef_2 = gdef;
            fields.AddField(shape);
            IFeatureClass target = fws.CreateFeatureClass(name, fields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");

            IFeatureCursor cur = cbd.Search(null, false);
            IFeature f = cur.NextFeature();
            Marshal.FinalReleaseComObject(cur);
            IPoint pt = ((IArea)f.ShapeCopy).Centroid;
            pt.SpatialReference = sr;
            IFeatureBuffer buf = target.CreateFeatureBuffer();
            buf.Shape = pt;
            IFeatureCursor ins = target.Insert(true);
            ins.InsertFeature(buf);
            ins.Flush();
            Marshal.FinalReleaseComObject(ins);
        }

        private static INetworkDataset OpenNetwork(IFeatureDataset fd, string name)
        {
            IFeatureDatasetExtensionContainer extContainer = fd as IFeatureDatasetExtensionContainer;
            IFeatureDatasetExtension ext = extContainer.FindExtension(esriDatasetType.esriDTNetworkDataset);
            IDatasetContainer2 container = ext as IDatasetContainer2;
            return container.get_DatasetByName(esriDatasetType.esriDTNetworkDataset, name) as INetworkDataset;
        }
    }
}

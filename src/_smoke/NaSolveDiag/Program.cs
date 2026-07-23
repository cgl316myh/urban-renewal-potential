using System;
using System.Runtime.InteropServices;
using System.Text;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.NetworkAnalyst;
using ESRI.ArcGIS.esriSystem;
using UrbanRenewal.GIS;

namespace NaSolveDiag
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            string gdb = args.Length > 0
                ? args[0]
                : @"D:\外挂\淘宝2\城市更新动力分析与片区指引系统设计\资料\苏州更新潜力评价数据\苏州更新潜力评价数据.gdb";

            string msg;
            if (!ArcEngineBootstrap.TryInitialize(out msg))
            {
                Console.WriteLine(msg);
                return 2;
            }
            Console.WriteLine(msg);

            try
            {
                IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
                IFeatureWorkspace fws = (IFeatureWorkspace)factory.OpenFromFile(gdb, 0);
                IFeatureDataset fd = fws.OpenFeatureDataset("roadNet");
                Print("roadNet", (IGeoDataset)fd);

                IFeatureClass road = OpenRoad(fws, fd);
                if (road != null)
                {
                    Print("road", (IGeoDataset)road);
                }

                IFeatureClass cbd = fws.OpenFeatureClass("开发强度高区域（CBD）");
                Print("CBD", (IGeoDataset)cbd);

                INetworkDataset nd;
                string om;
                if (!NetworkDatasetHelper.TryOpen(gdb, "roadNet", "roadNet_ND", out nd, out om))
                {
                    Console.WriteLine(om);
                    return 3;
                }
                Console.WriteLine(om);

                IFeature f = cbd.GetFeature(1);
                IPoint pt = ((IArea)f.ShapeCopy).Centroid;
                pt.SpatialReference = ((IGeoDataset)cbd).SpatialReference;
                Console.WriteLine("PT " + pt.X + "," + pt.Y + " SR=" + pt.SpatialReference.Name);

                ISpatialReference ndSr = ((IGeoDataset)fd).SpatialReference;
                if (pt.SpatialReference != null && ndSr != null
                    && pt.SpatialReference.FactoryCode != ndSr.FactoryCode)
                {
                    pt.Project(ndSr);
                    Console.WriteLine("PT_PROJ " + pt.X + "," + pt.Y);
                }

                IDatasetComponent comp = (IDatasetComponent)nd;
                IDENetworkDataset deNd = (IDENetworkDataset)comp.DataElement;
                INASolver solver = new NAServiceAreaSolverClass();
                INAContext ctx = solver.CreateContext(deNd, "diag");
                ((INAContextEdit)ctx).Bind(nd, null);
                ((INASolverSettings)solver).ImpedanceAttributeName = "Length";

                INAServiceAreaSolver2 sa = (INAServiceAreaSolver2)solver;
                IDoubleArray breaks = new DoubleArrayClass();
                breaks.Add(1000);
                breaks.Add(2000);
                breaks.Add(5000);
                sa.DefaultBreaks = breaks;
                sa.OutputPolygons = esriNAOutputPolygonType.esriNAOutputPolygonSimplified;
                sa.OverlapPolygons = true;
                sa.SplitPolygonsAtBreaks = true;
                sa.MergeSimilarPolygonRanges = false;
                sa.TravelDirection = esriNATravelDirection.esriNATravelDirectionFromFacility;
                solver.UpdateContext(ctx, deNd, null);

                ctx.Locator.SnapTolerance = 5000;
                INALocation loc = null;
                IPoint after = null;
                double dist = 0;
                ctx.Locator.QueryLocationByPoint(pt, ref loc, ref after, ref dist);
                Console.WriteLine("SNAP dist=" + dist
                    + " after=" + (after == null ? "null" : after.X + "," + after.Y)
                    + " locNull=" + (loc == null));

                INAClass fac = ctx.NAClasses.get_ItemByName("Facilities") as INAClass;
                fac.DeleteAllRows();
                IFeatureClass naFc = fac as IFeatureClass;
                Console.WriteLine("FAC_HASZ=" + ((IFeatureClass)naFc).FindField("Shape")
                    + " geomHasZ=" + HasZ(naFc));

                // 方式 A：仅 NALocation（不写 Shape）
                IFeatureCursor cur = naFc.Insert(true);
                IFeatureBuffer buf = naFc.CreateFeatureBuffer();
                ((INALocationObject)buf).NALocation = loc;
                IPoint shapePt = after != null ? after : pt;
                shapePt = StripZM(shapePt);
                try
                {
                    buf.Shape = shapePt;
                }
                catch (Exception exShape)
                {
                    Console.WriteLine("SET_SHAPE_FAIL " + exShape.Message);
                }
                cur.InsertFeature(buf);
                cur.Flush();
                Marshal.FinalReleaseComObject(cur);
                Console.WriteLine("FAC_COUNT=" + naFc.FeatureCount(null));

                // 检查 Status 字段
                int statusIdx = naFc.FindField("Status");
                if (statusIdx >= 0)
                {
                    IFeature ff = naFc.GetFeature(1);
                    Console.WriteLine("FAC_STATUS=" + ff.get_Value(statusIdx));
                }

                IGPMessages gp = new GPMessagesClass();
                bool ok = false;
                try
                {
                    ok = solver.Solve(ctx, gp, null);
                }
                catch (Exception exSolve)
                {
                    Console.WriteLine("SOLVE_EX " + exSolve.Message);
                }
                Console.WriteLine("SOLVE=" + ok + " msgs=" + gp.Count);
                for (int i = 0; i < gp.Count; i++)
                {
                    IGPMessage m = gp.GetMessage(i);
                    Console.WriteLine("M|" + m.Type + "|" + m.ErrorCode + "|" + m.Description);
                }

                INAClass polys = ctx.NAClasses.get_ItemByName("SAPolygons") as INAClass;
                Console.WriteLine("POLY=" + ((IFeatureClass)polys).FeatureCount(null));
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("EX " + ex);
                return 4;
            }
            finally
            {
                ArcEngineBootstrap.Shutdown();
            }
        }

        private static IFeatureClass OpenRoad(IFeatureWorkspace fws, IFeatureDataset fd)
        {
            try
            {
                return fws.OpenFeatureClass("road");
            }
            catch
            {
            }

            IFeatureClassContainer c = fd as IFeatureClassContainer;
            if (c == null)
            {
                return null;
            }
            for (int i = 0; i < c.ClassCount; i++)
            {
                IFeatureClass fc = c.get_Class(i);
                string name = ((IDataset)fc).Name;
                Console.WriteLine("FC " + name);
                if (string.Equals(name, "road", StringComparison.OrdinalIgnoreCase))
                {
                    return fc;
                }
            }
            return null;
        }

        private static void Print(string label, IGeoDataset ds)
        {
            ISpatialReference sr = ds.SpatialReference;
            Console.WriteLine(label + " SR=" + (sr == null ? "null" : sr.Name)
                + " code=" + (sr == null ? "-" : sr.FactoryCode.ToString())
                + " proj=" + (sr is IProjectedCoordinateSystem)
                + " geo=" + (sr is IGeographicCoordinateSystem));
        }

        private static bool HasZ(IFeatureClass fc)
        {
            try
            {
                int shapeIdx = fc.FindField(fc.ShapeFieldName);
                IField field = fc.Fields.get_Field(shapeIdx);
                return field.GeometryDef.HasZ;
            }
            catch
            {
                return false;
            }
        }

        private static IPoint StripZM(IPoint pt)
        {
            if (pt == null)
            {
                return null;
            }
            IPoint clean = new PointClass();
            clean.PutCoords(pt.X, pt.Y);
            if (pt.SpatialReference != null)
            {
                clean.SpatialReference = pt.SpatialReference;
            }
            IZAware zAware = clean as IZAware;
            if (zAware != null)
            {
                zAware.ZAware = false;
            }
            IMAware mAware = clean as IMAware;
            if (mAware != null)
            {
                mAware.MAware = false;
            }
            return clean;
        }
    }
}

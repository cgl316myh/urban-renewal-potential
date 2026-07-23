using System;
using System.IO;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using IoPath = System.IO.Path;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// 要素类坐标系读取与投影对齐（解决 WGS84 与 CGCS2000 混用导致栅格分析失败）。
    /// </summary>
    public static class FeatureProjectionHelper
    {
        public static ISpatialReference GetSpatialReference(string featureClassPath)
        {
            IFeatureClass fc = OpenFeatureClass(featureClassPath);
            if (fc == null)
            {
                return null;
            }
            IGeoDataset gds = fc as IGeoDataset;
            return gds != null ? gds.SpatialReference : null;
        }

        public static bool IsSameSpatialReference(ISpatialReference a, ISpatialReference b)
        {
            if (a == null || b == null)
            {
                return false;
            }
            try
            {
                if (a.FactoryCode > 0 && b.FactoryCode > 0 && a.FactoryCode == b.FactoryCode)
                {
                    return true;
                }
            }
            catch
            {
            }
            string na = a.Name ?? string.Empty;
            string nb = b.Name ?? string.Empty;
            return string.Equals(na, nb, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 若输入与目标坐标系不一致则投影到工作目录 shapefile，否则返回原路径。
        /// </summary>
        public static string EnsureProjected(
            GeoprocessorHelper gp,
            string inFeatures,
            ISpatialReference targetSr,
            string workDir,
            string nameHint)
        {
            if (string.IsNullOrEmpty(inFeatures) || targetSr == null)
            {
                return inFeatures;
            }

            ISpatialReference src = GetSpatialReference(inFeatures);
            if (IsSameSpatialReference(src, targetSr))
            {
                return inFeatures;
            }

            Directory.CreateDirectory(workDir);
            string safe = Sanitize(nameHint) + "_" + StableHash(nameHint);
            string outShp = IoPath.Combine(workDir, safe + "_prj.shp");
            DeleteShapefile(outShp);

            // ArcObjects 投影：WGS84→CGCS2000 无地理变换时，按同经纬度桥接到目标地理坐标系再投影
            ProjectByArcObjects(inFeatures, outShp, src, targetSr);
            return outShp;
        }

        private static void ProjectByArcObjects(
            string inFeatures,
            string outShpPath,
            ISpatialReference srcSr,
            ISpatialReference targetSr)
        {
            IFeatureClass inFc = OpenFeatureClass(inFeatures);
            if (inFc == null)
            {
                throw new InvalidOperationException("无法打开要素类: " + inFeatures);
            }

            ISpatialReference bridgeGcs = GetGeographicFor(targetSr);
            bool needDatumBridge = NeedsDatumBridge(srcSr, targetSr);

            string folder = IoPath.GetDirectoryName(outShpPath);
            string name = IoPath.GetFileNameWithoutExtension(outShpPath);
            IWorkspaceFactory swf = new ShapefileWorkspaceFactoryClass();
            IFeatureWorkspace outWs = (IFeatureWorkspace)swf.OpenFromFile(folder, 0);

            UID clsid = new UIDClass();
            clsid.Value = "esriGeodatabase.Feature";

            IFields fields = CloneShapeAndOidFields(inFc, targetSr);
            IFeatureClass outFc = outWs.CreateFeatureClass(name, fields, clsid, null, esriFeatureType.esriFTSimple, "Shape", "");

            IFeatureCursor inCursor = inFc.Search(null, false);
            IFeature inFeat;
            int count = 0;
            try
            {
                while ((inFeat = inCursor.NextFeature()) != null)
                {
                    if (inFeat.Shape == null || inFeat.Shape.IsEmpty)
                    {
                        continue;
                    }

                    IGeometry geom = inFeat.ShapeCopy;
                    if (needDatumBridge && bridgeGcs != null)
                    {
                        geom.SpatialReference = bridgeGcs;
                    }
                    else if (srcSr != null)
                    {
                        geom.SpatialReference = srcSr;
                    }
                    geom.Project(targetSr);

                    IFeature outFeat = outFc.CreateFeature();
                    outFeat.Shape = geom;
                    outFeat.Store();
                    count++;
                }
            }
            finally
            {
                Marshal.ReleaseComObject(inCursor);
            }

            if (count == 0)
            {
                throw new InvalidOperationException("投影后无有效要素: " + inFeatures);
            }
        }

        private static bool NeedsDatumBridge(ISpatialReference src, ISpatialReference target)
        {
            if (src == null || target == null)
            {
                return true;
            }
            ISpatialReference srcGcs = GetGeographicFor(src) ?? src;
            ISpatialReference tgtGcs = GetGeographicFor(target) ?? target;
            if (IsSameSpatialReference(srcGcs, tgtGcs))
            {
                return false;
            }
            // WGS84 ↔ CGCS2000：城市尺度按同经纬度近似
            string sn = (srcGcs.Name ?? string.Empty).ToUpperInvariant();
            string tn = (tgtGcs.Name ?? string.Empty).ToUpperInvariant();
            bool srcWgs = sn.IndexOf("WGS", StringComparison.Ordinal) >= 0 || sn.IndexOf("1984", StringComparison.Ordinal) >= 0;
            bool tgtCgcs = tn.IndexOf("CGCS", StringComparison.Ordinal) >= 0 || tn.IndexOf("CHINA", StringComparison.Ordinal) >= 0 || tn.IndexOf("2000", StringComparison.Ordinal) >= 0;
            bool srcCgcs = sn.IndexOf("CGCS", StringComparison.Ordinal) >= 0 || sn.IndexOf("CHINA", StringComparison.Ordinal) >= 0;
            bool tgtWgs = tn.IndexOf("WGS", StringComparison.Ordinal) >= 0;
            return (srcWgs && tgtCgcs) || (srcCgcs && tgtWgs) || !IsSameSpatialReference(srcGcs, tgtGcs);
        }

        private static IFields CloneShapeAndOidFields(IFeatureClass source, ISpatialReference outSr)
        {
            IObjectClassDescription ocDesc = new FeatureClassDescriptionClass();
            IFields fields = ocDesc.RequiredFields;
            IFieldsEdit fieldsEdit = (IFieldsEdit)fields;

            // 替换 Shape 字段的空间参考
            for (int i = 0; i < fields.FieldCount; i++)
            {
                IField f = fields.get_Field(i);
                if (f.Type == esriFieldType.esriFieldTypeGeometry)
                {
                    IFieldEdit fe = (IFieldEdit)f;
                    IGeometryDef geomDef = f.GeometryDef;
                    IGeometryDefEdit geomEdit = (IGeometryDefEdit)geomDef;
                    geomEdit.GeometryType_2 = source.ShapeType;
                    geomEdit.SpatialReference_2 = outSr;
                    fe.GeometryDef_2 = geomDef;
                }
            }
            return fields;
        }

        private static ISpatialReference GetGeographicFor(ISpatialReference sr)
        {
            if (sr == null)
            {
                return null;
            }
            IGeographicCoordinateSystem gcs = sr as IGeographicCoordinateSystem;
            if (gcs != null)
            {
                return gcs;
            }
            IProjectedCoordinateSystem pcs = sr as IProjectedCoordinateSystem;
            if (pcs != null)
            {
                return pcs.GeographicCoordinateSystem;
            }
            return null;
        }

        private static void DeleteShapefile(string shpPath)
        {
            if (string.IsNullOrEmpty(shpPath) || !File.Exists(shpPath))
            {
                return;
            }
            string dir = IoPath.GetDirectoryName(shpPath);
            string stem = IoPath.GetFileNameWithoutExtension(shpPath);
            string[] exts = new string[] { ".shp", ".shx", ".dbf", ".prj", ".sbn", ".sbx", ".cpg", ".shp.xml" };
            for (int i = 0; i < exts.Length; i++)
            {
                string f = IoPath.Combine(dir, stem + exts[i]);
                if (File.Exists(f))
                {
                    try { File.Delete(f); }
                    catch { }
                }
            }
        }

        public static IFeatureClass OpenFeatureClass(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (path.EndsWith(".shp", StringComparison.OrdinalIgnoreCase))
            {
                string folder = IoPath.GetDirectoryName(path);
                string name = IoPath.GetFileNameWithoutExtension(path);
                IWorkspaceFactory swf = new ShapefileWorkspaceFactoryClass();
                IFeatureWorkspace fws = (IFeatureWorkspace)swf.OpenFromFile(folder, 0);
                return fws.OpenFeatureClass(name);
            }

            string gdb = IoPath.GetDirectoryName(path);
            string fcName = IoPath.GetFileName(path);
            if (string.IsNullOrEmpty(gdb) || !gdb.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            IWorkspaceFactory gwf = new FileGDBWorkspaceFactoryClass();
            IFeatureWorkspace gws = (IFeatureWorkspace)gwf.OpenFromFile(gdb, 0);
            return gws.OpenFeatureClass(fcName);
        }

        private static string Sanitize(string nameHint)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(nameHint))
            {
                for (int i = 0; i < nameHint.Length && sb.Length < 6; i++)
                {
                    char c = nameHint[i];
                    if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                    {
                        sb.Append(char.ToLowerInvariant(c));
                    }
                }
            }
            if (sb.Length == 0)
            {
                sb.Append("lyr");
            }
            return sb.ToString();
        }

        private static string StableHash(string text)
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
            return (h % 10000).ToString("0000");
        }
    }
}

using System;
using System.Collections.Generic;
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
    /// <summary>要素类坐标系读取与投影对齐（WGS84/CGCS2000 混用会导致栅格分析失败）。</summary>
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

        public static bool TryReadSpatialReference(
            string sourcePath,
            string layerName,
            out ISpatialReference spatialReference,
            out string spatialReferenceName,
            out int factoryCode,
            out string message)
        {
            spatialReference = null;
            spatialReferenceName = null;
            factoryCode = 0;
            message = null;

            if (string.IsNullOrEmpty(sourcePath))
            {
                message = "未指定坐标系来源路径。";
                return false;
            }

            try
            {
                string path = sourcePath.Trim().Trim('"');
                if (path.EndsWith(".shp", StringComparison.OrdinalIgnoreCase))
                {
                    if (!File.Exists(path))
                    {
                        message = "Shapefile 不存在: " + path;
                        return false;
                    }
                    spatialReference = GetSpatialReference(path);
                }
                else if (path.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
                {
                    if (!Directory.Exists(path))
                    {
                        message = "GDB 不存在: " + path;
                        return false;
                    }
                    if (string.IsNullOrEmpty(layerName))
                    {
                        message = "从 GDB 读取坐标系时请指定图层名。";
                        return false;
                    }
                    IWorkspaceFactory gwf = new FileGDBWorkspaceFactoryClass();
                    IFeatureWorkspace gws = (IFeatureWorkspace)gwf.OpenFromFile(path, 0);
                    IFeatureClass fc = gws.OpenFeatureClass(layerName.Trim());
                    IGeoDataset gds = fc as IGeoDataset;
                    spatialReference = gds != null ? gds.SpatialReference : null;
                }
                else
                {
                    message = "来源须为 Shapefile（*.shp）或 File GDB（*.gdb）。";
                    return false;
                }

                if (spatialReference == null)
                {
                    message = "无法读取空间参考（图层可能无坐标系定义）。";
                    return false;
                }

                spatialReferenceName = spatialReference.Name;
                try { factoryCode = spatialReference.FactoryCode; }
                catch { factoryCode = 0; }
                message = spatialReferenceName
                    + (factoryCode > 0 ? " [WKID=" + factoryCode + "]" : string.Empty);
                return true;
            }
            catch (Exception ex)
            {
                message = "读取空间参考失败: " + ex.Message;
                return false;
            }
        }

        public static bool TryReadSpatialReferenceInfo(
            string sourcePath,
            string layerName,
            out string spatialReferenceName,
            out int factoryCode,
            out string message)
        {
            ISpatialReference sr;
            return TryReadSpatialReference(
                sourcePath, layerName, out sr, out spatialReferenceName, out factoryCode, out message);
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

        public static void ProjectFeatureClassToGdb(
            string inFeatures,
            string outGdb,
            string outName,
            ISpatialReference targetSr)
        {
            if (string.IsNullOrEmpty(inFeatures) || string.IsNullOrEmpty(outGdb) || string.IsNullOrEmpty(outName)
                || targetSr == null)
            {
                throw new ArgumentException("投影参数无效。");
            }

            IFeatureClass inFc = OpenFeatureClass(inFeatures);
            if (inFc == null)
            {
                throw new InvalidOperationException("无法打开要素类: " + inFeatures);
            }

            ISpatialReference srcSr = GetSpatialReference(inFeatures);
            ISpatialReference bridgeGcs = GetGeographicFor(targetSr);
            bool needDatumBridge = NeedsDatumBridge(srcSr, targetSr);

            IWorkspaceFactory gwf = new FileGDBWorkspaceFactoryClass();
            IFeatureWorkspace outWs = (IFeatureWorkspace)gwf.OpenFromFile(outGdb, 0);

            try
            {
                IFeatureClass old = outWs.OpenFeatureClass(outName);
                ((IDataset)old).Delete();
            }
            catch
            {
            }

            IFields fields = CloneAllFields(inFc, targetSr);
            UID clsid = new UIDClass();
            clsid.Value = "esriGeodatabase.Feature";
            IFeatureClass outFc;
            try
            {
                outFc = outWs.CreateFeatureClass(
                    outName, fields, clsid, null, esriFeatureType.esriFTSimple, inFc.ShapeFieldName, "");
            }
            catch (Exception exCreate)
            {
                throw new InvalidOperationException(
                    "创建目标要素类失败 [" + outName + "]: " + exCreate.Message, exCreate);
            }

            int[] srcIdx;
            int[] dstIdx;
            BuildAttributeMap(inFc, outFc, out srcIdx, out dstIdx);

            IFeatureCursor inCursor = inFc.Search(null, false);
            IFeatureCursor outCursor = outFc.Insert(true);
            int count = 0;
            try
            {
                IFeature inFeat;
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
                    try
                    {
                        geom.Project(targetSr);
                    }
                    catch (Exception exProj)
                    {
                        throw new InvalidOperationException(
                            "几何投影失败: " + exProj.Message, exProj);
                    }

                    IFeatureBuffer buf = outFc.CreateFeatureBuffer();
                    try
                    {
                        buf.Shape = geom;
                    }
                    catch (Exception exShape)
                    {
                        // Z/M 冲突时去掉 Z/M 再写
                        IZAware zAware = geom as IZAware;
                        if (zAware != null) zAware.ZAware = false;
                        IMAware mAware = geom as IMAware;
                        if (mAware != null) mAware.MAware = false;
                        try
                        {
                            buf.Shape = geom;
                        }
                        catch
                        {
                            throw new InvalidOperationException(
                                "写入几何失败: " + exShape.Message, exShape);
                        }
                    }
                    for (int i = 0; i < srcIdx.Length; i++)
                    {
                        try
                        {
                            object v = inFeat.get_Value(srcIdx[i]);
                            buf.set_Value(dstIdx[i], v);
                        }
                        catch
                        {
                        }
                    }
                    outCursor.InsertFeature(buf);
                    count++;
                }
                outCursor.Flush();
            }
            finally
            {
                if (inCursor != null) Marshal.ReleaseComObject(inCursor);
                if (outCursor != null) Marshal.ReleaseComObject(outCursor);
            }

            if (count == 0)
            {
                throw new InvalidOperationException("投影后无有效要素: " + inFeatures);
            }
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

        private static IFields CloneAllFields(IFeatureClass source, ISpatialReference outSr)
        {
            IObjectClassDescription ocDesc = new FeatureClassDescriptionClass();
            IFields required = ocDesc.RequiredFields;
            IFieldsEdit fieldsEdit = new FieldsClass();

            for (int i = 0; i < required.FieldCount; i++)
            {
                IField f = required.get_Field(i);
                if (f.Type == esriFieldType.esriFieldTypeOID)
                {
                    fieldsEdit.AddField(CloneField(f));
                }
            }

            string shapeName = source.ShapeFieldName;
            IField shapeField = source.Fields.get_Field(source.FindField(shapeName));
            IFieldEdit shapeEdit = (IFieldEdit)CloneField(shapeField);
            shapeEdit.Name_2 = shapeName;
            IGeometryDefEdit geomEdit = new GeometryDefClass();
            geomEdit.GeometryType_2 = source.ShapeType;
            geomEdit.SpatialReference_2 = outSr;
            geomEdit.HasZ_2 = false;
            geomEdit.HasM_2 = false;
            shapeEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            shapeEdit.GeometryDef_2 = geomEdit;
            fieldsEdit.AddField(shapeEdit);

            for (int i = 0; i < source.Fields.FieldCount; i++)
            {
                IField f = source.Fields.get_Field(i);
                if (f.Type == esriFieldType.esriFieldTypeOID
                    || f.Type == esriFieldType.esriFieldTypeGeometry
                    || f.Type == esriFieldType.esriFieldTypeBlob
                    || f.Type == esriFieldType.esriFieldTypeRaster)
                {
                    continue;
                }
                if (IsReservedFieldName(f.Name) || FieldExists(fieldsEdit, f.Name))
                {
                    continue;
                }
                fieldsEdit.AddField(CloneField(f));
            }
            return fieldsEdit;
        }

        private static bool IsReservedFieldName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return true;
            }
            return string.Equals(name, "Shape_Length", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "Shape_Area", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "SHAPE_Length", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "SHAPE_Area", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "OBJECTID", StringComparison.OrdinalIgnoreCase)
                || string.Equals(name, "FID", StringComparison.OrdinalIgnoreCase);
        }

        private static bool FieldExists(IFields fields, string name)
        {
            if (fields == null || string.IsNullOrEmpty(name))
            {
                return false;
            }
            for (int i = 0; i < fields.FieldCount; i++)
            {
                if (string.Equals(fields.get_Field(i).Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private static IField CloneField(IField source)
        {
            IFieldEdit edit = new FieldClass();
            edit.Name_2 = source.Name;
            edit.AliasName_2 = source.AliasName;
            edit.Type_2 = source.Type;
            edit.Length_2 = source.Length;
            edit.Precision_2 = source.Precision;
            edit.Scale_2 = source.Scale;
            edit.IsNullable_2 = source.IsNullable;
            edit.Editable_2 = source.Editable;
            edit.DefaultValue_2 = source.DefaultValue;
            if (source.Type == esriFieldType.esriFieldTypeGeometry && source.GeometryDef != null)
            {
                IGeometryDefEdit g = new GeometryDefClass();
                g.GeometryType_2 = source.GeometryDef.GeometryType;
                g.SpatialReference_2 = source.GeometryDef.SpatialReference;
                g.HasZ_2 = source.GeometryDef.HasZ;
                g.HasM_2 = source.GeometryDef.HasM;
                edit.GeometryDef_2 = g;
            }
            return edit;
        }

        private static void BuildAttributeMap(
            IFeatureClass source,
            IFeatureClass target,
            out int[] srcIdx,
            out int[] dstIdx)
        {
            List<int> s = new List<int>();
            List<int> d = new List<int>();
            for (int i = 0; i < source.Fields.FieldCount; i++)
            {
                IField f = source.Fields.get_Field(i);
                if (f.Type == esriFieldType.esriFieldTypeOID
                    || f.Type == esriFieldType.esriFieldTypeGeometry)
                {
                    continue;
                }
                int ti = target.FindField(f.Name);
                if (ti >= 0 && target.Fields.get_Field(ti).Editable)
                {
                    s.Add(i);
                    d.Add(ti);
                }
            }
            srcIdx = s.ToArray();
            dstIdx = d.ToArray();
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

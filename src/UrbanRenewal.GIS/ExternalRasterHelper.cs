using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SpatialAnalystTools;
using UrbanRenewal.Model;
using IoPath = System.IO.Path;

namespace UrbanRenewal.GIS
{
  /// <summary>外部栅格接入：仅校验空间属性 + 裁切，不重投影/重采样。</summary>
  public static class ExternalRasterHelper
  {
    private const double CellSizeToleranceMeters = 0.01;
    private const double CellSizeRelativeTolerance = 0.001;

    public static string ResolveRasterPath(string nameOrPath, string outGdb, string inputGdb)
    {
      if (string.IsNullOrWhiteSpace(nameOrPath))
      {
        return null;
      }

      string trimmed = nameOrPath.Trim().Trim('"');
      if (trimmed.IndexOf(".gdb", StringComparison.OrdinalIgnoreCase) >= 0
          || trimmed.IndexOf('\\') >= 0
          || trimmed.IndexOf('/') >= 0)
      {
        if (RasterExists(trimmed))
        {
          return trimmed;
        }
        return trimmed;
      }

      if (!string.IsNullOrEmpty(outGdb))
      {
        string inOut = OutputGdbHelper.DatasetPath(outGdb, trimmed);
        if (RasterExists(inOut))
        {
          return inOut;
        }
      }
      if (!string.IsNullOrEmpty(inputGdb))
      {
        string inIn = OutputGdbHelper.DatasetPath(inputGdb, trimmed);
        if (RasterExists(inIn))
        {
          return inIn;
        }
      }
      return OutputGdbHelper.DatasetPath(outGdb, trimmed);
    }

    public static bool RasterExists(string path)
    {
      if (string.IsNullOrEmpty(path))
      {
        return false;
      }
      if (File.Exists(path))
      {
        return true;
      }
      string gdb = IoPath.GetDirectoryName(path);
      string name = IoPath.GetFileName(path);
      if (string.IsNullOrEmpty(gdb) || string.IsNullOrEmpty(name) || !Directory.Exists(gdb))
      {
        return false;
      }
      try
      {
        List<string> rasters = WorkspaceCatalog.ListRasterDatasetNames(gdb);
        for (int i = 0; i < rasters.Count; i++)
        {
          if (string.Equals(rasters[i], name, StringComparison.OrdinalIgnoreCase))
          {
            return true;
          }
        }
      }
      catch
      {
      }
      return false;
    }

    public static ExternalRasterCheckResult ValidateCompatibility(
        string rasterPath,
        ISpatialReference targetSr,
        double expectedCellSize)
    {
      ExternalRasterCheckResult result = new ExternalRasterCheckResult();
      result.ExpectedCellSize = expectedCellSize;
      result.ExpectedSpatialReference = DescribeSpatialReference(targetSr);

      if (string.IsNullOrEmpty(rasterPath) || !RasterExists(rasterPath))
      {
        result.SpatialReferenceMismatch = true;
        result.CellSizeMismatch = true;
        result.SummaryMessage = "外部交通栅格不存在或无法访问: " + rasterPath;
        result.IsCompatible = false;
        return result;
      }

      ISpatialReference actualSr;
      double cellX;
      double cellY;
      string readMsg;
      if (!TryReadRasterSpatialInfo(rasterPath, out actualSr, out cellX, out cellY, out readMsg))
      {
        result.SpatialReferenceMismatch = true;
        result.CellSizeMismatch = true;
        result.SummaryMessage = readMsg ?? "无法读取外部栅格空间属性。";
        result.IsCompatible = false;
        return result;
      }

      result.ActualSpatialReference = DescribeSpatialReference(actualSr);
      result.ActualCellSizeX = cellX;
      result.ActualCellSizeY = cellY;

      if (targetSr == null)
      {
        result.SpatialReferenceMismatch = true;
        result.SummaryMessage = "未确定分析坐标系，无法校验外部栅格。";
        result.IsCompatible = false;
        return result;
      }

      if (!FeatureProjectionHelper.IsSameSpatialReference(targetSr, actualSr))
      {
        result.SpatialReferenceMismatch = true;
      }

      if (!CellSizeMatches(expectedCellSize, cellX, cellY))
      {
        result.CellSizeMismatch = true;
      }

      result.IsCompatible = !result.SpatialReferenceMismatch && !result.CellSizeMismatch;
      if (!result.IsCompatible)
      {
        result.SummaryMessage = result.BuildDialogMessage();
      }
      else
      {
        result.SummaryMessage = "外部栅格空间属性校验通过。";
      }
      return result;
    }

    public static string PrepareExternalTrafficRaster(
        GeoprocessorHelper gp,
        string sourceRasterPath,
        string outputGdb,
        string outputName,
        string studyAreaFeaturePath,
        bool clipToStudyArea)
    {
      if (gp == null || string.IsNullOrEmpty(sourceRasterPath) || string.IsNullOrEmpty(outputGdb))
      {
        return null;
      }

      string outName = string.IsNullOrEmpty(outputName) ? "traf_ext" : outputName.Trim();
      string outRaster = OutputGdbHelper.DatasetPath(outputGdb, outName);
      OutputGdbHelper.TryDeleteDataset(gp, outRaster);

      if (clipToStudyArea && !string.IsNullOrEmpty(studyAreaFeaturePath))
      {
        ExtractByMask extract = new ExtractByMask();
        extract.in_raster = sourceRasterPath;
        extract.in_mask_data = studyAreaFeaturePath;
        extract.out_raster = outRaster;
        gp.Execute(extract, "ExtractByMask-ExternalTraffic");
        return outRaster;
      }

      CopyRaster copy = new CopyRaster();
      copy.in_raster = sourceRasterPath;
      copy.out_rasterdataset = outRaster;
      gp.Execute(copy, "CopyRaster-ExternalTraffic");
      return outRaster;
    }

    private static bool TryReadRasterSpatialInfo(
        string rasterPath,
        out ISpatialReference spatialReference,
        out double cellSizeX,
        out double cellSizeY,
        out string message)
    {
      spatialReference = null;
      cellSizeX = 0;
      cellSizeY = 0;
      message = null;
      IRasterDataset dataset = null;
      IRaster raster = null;
      try
      {
        dataset = OpenRasterDataset(rasterPath);
        if (dataset == null)
        {
          message = "无法打开栅格: " + rasterPath;
          return false;
        }

        IGeoDataset geo = dataset as IGeoDataset;
        if (geo != null)
        {
          spatialReference = geo.SpatialReference;
        }

        raster = dataset.CreateDefaultRaster();
        IRasterProps props = raster as IRasterProps;
        if (props == null || props.Width <= 0 || props.Height <= 0)
        {
          message = "无法读取栅格像元信息: " + rasterPath;
          return false;
        }

        if (geo != null && geo.Extent != null && !geo.Extent.IsEmpty)
        {
          cellSizeX = geo.Extent.Width / props.Width;
          cellSizeY = geo.Extent.Height / props.Height;
        }
        else
        {
          try
          {
            double m = props.MeanCellSize();
            cellSizeX = m;
            cellSizeY = m;
          }
          catch
          {
            message = "无法读取栅格像元大小: " + rasterPath;
            return false;
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        message = "读取栅格空间属性失败: " + ex.Message;
        return false;
      }
      finally
      {
        if (raster != null)
        {
          Marshal.FinalReleaseComObject(raster);
        }
        if (dataset != null)
        {
          Marshal.FinalReleaseComObject(dataset);
        }
      }
    }

    private static IRasterDataset OpenRasterDataset(string rasterPath)
    {
      string folder = IoPath.GetDirectoryName(rasterPath);
      string name = IoPath.GetFileName(rasterPath);
      if (string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(name))
      {
        return null;
      }
      if (folder.EndsWith(".gdb", StringComparison.OrdinalIgnoreCase))
      {
        IWorkspaceFactory gwf = new FileGDBWorkspaceFactoryClass();
        IWorkspace ws = gwf.OpenFromFile(folder, 0);
        IRasterWorkspaceEx rws = ws as IRasterWorkspaceEx;
        if (rws != null)
        {
          return rws.OpenRasterDataset(name);
        }
      }
      IWorkspaceFactory factory = new RasterWorkspaceFactoryClass();
      IRasterWorkspace rasterWs = (IRasterWorkspace)factory.OpenFromFile(folder, 0);
      return rasterWs.OpenRasterDataset(name);
    }

    private static string DescribeSpatialReference(ISpatialReference sr)
    {
      if (sr == null)
      {
        return "（未定义）";
      }
      try
      {
        int code = sr.FactoryCode;
        if (code > 0)
        {
          return sr.Name + " [WKID=" + code + "]";
        }
      }
      catch
      {
      }
      return sr.Name ?? "（未命名）";
    }

    private static bool CellSizeMatches(double expected, double actualX, double actualY)
    {
      if (expected <= 0 || actualX <= 0 || actualY <= 0)
      {
        return false;
      }
      return NearlyEqual(expected, actualX) && NearlyEqual(expected, actualY);
    }

    private static bool NearlyEqual(double expected, double actual)
    {
      double diff = Math.Abs(expected - actual);
      if (diff <= CellSizeToleranceMeters)
      {
        return true;
      }
      double max = Math.Max(expected, actual);
      return max > 0 && diff / max <= CellSizeRelativeTolerance;
    }
  }
}

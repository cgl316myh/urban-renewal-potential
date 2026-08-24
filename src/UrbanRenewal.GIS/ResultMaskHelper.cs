using System;
using ESRI.ArcGIS.SpatialAnalystTools;
using UrbanRenewal.Model;

namespace UrbanRenewal.GIS
{
    /// <summary>分析最终成果按 StudyArea 掩膜（Extract by Mask）。</summary>
    public static class ResultMaskHelper
    {
        public static bool IsMaskEnabled()
        {
            GlobalAppSettings settings = GlobalAppSettingsStore.Load();
            return settings == null || settings.MaskResultToStudyArea;
        }

        public static string MaskRasterToStudyArea(
            GeoprocessorHelper gp,
            string sourceRasterPath,
            string studyAreaFeaturePath,
            string outputRasterPath)
        {
            if (gp == null)
            {
                throw new ArgumentNullException("gp");
            }
            if (string.IsNullOrEmpty(sourceRasterPath))
            {
                throw new ArgumentException("源栅格路径无效。");
            }
            if (string.IsNullOrEmpty(studyAreaFeaturePath))
            {
                throw new ArgumentException("StudyArea 路径无效。");
            }
            if (string.IsNullOrEmpty(outputRasterPath))
            {
                throw new ArgumentException("输出栅格路径无效。");
            }

            OutputGdbHelper.TryDeleteDataset(gp, outputRasterPath);
            ExtractByMask extract = new ExtractByMask();
            extract.in_raster = sourceRasterPath;
            extract.in_mask_data = studyAreaFeaturePath;
            extract.out_raster = outputRasterPath;
            gp.Execute(extract, "ExtractByMask-Result");
            return outputRasterPath;
        }

        public static string MaskAndReplace(
            GeoprocessorHelper gp,
            string sourceRasterPath,
            string studyAreaFeaturePath,
            string finalRasterPath)
        {
            string tempPath = finalRasterPath + "_m";
            MaskRasterToStudyArea(gp, sourceRasterPath, studyAreaFeaturePath, tempPath);
            return FeasibilityRasterBuilder.SaveAs(gp, tempPath, finalRasterPath);
        }
    }
}

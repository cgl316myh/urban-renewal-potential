using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// File GDB 要素类/栅格枚举与按关键词匹配。
    /// </summary>
    public static class WorkspaceCatalog
    {
        public static List<string> ListFeatureClassNames(string gdbPath)
        {
            List<string> names = new List<string>();
            if (string.IsNullOrEmpty(gdbPath) || !System.IO.Directory.Exists(gdbPath))
            {
                return names;
            }

            IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
            IWorkspace workspace = factory.OpenFromFile(gdbPath, 0);
            Collect(workspace, names);
            return names;
        }

        /// <summary>枚举 File GDB 内栅格数据集名称（含要素数据集内栅格）。</summary>
        public static List<string> ListRasterDatasetNames(string gdbPath)
        {
            List<string> names = new List<string>();
            if (string.IsNullOrEmpty(gdbPath) || !System.IO.Directory.Exists(gdbPath))
            {
                return names;
            }

            IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
            IWorkspace workspace = factory.OpenFromFile(gdbPath, 0);
            CollectRasters(workspace, names);
            return names;
        }

        public static string ToRasterPath(string gdbPath, string rasterName)
        {
            if (string.IsNullOrEmpty(gdbPath) || string.IsNullOrEmpty(rasterName))
            {
                return null;
            }
            return System.IO.Path.Combine(gdbPath, rasterName);
        }

        /// <summary>读取栅格第一波段统计的最小/最大值（失败返回 false）。</summary>
        public static bool TryGetRasterMinMax(string rasterPath, out double minValue, out double maxValue)
        {
            minValue = 0;
            maxValue = 0;
            if (string.IsNullOrEmpty(rasterPath))
            {
                return false;
            }

            IRasterDataset rasterDs = null;
            try
            {
                string gdb = System.IO.Path.GetDirectoryName(rasterPath);
                string name = System.IO.Path.GetFileName(rasterPath);
                if (string.IsNullOrEmpty(gdb) || string.IsNullOrEmpty(name) || !System.IO.Directory.Exists(gdb))
                {
                    return false;
                }

                IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
                IRasterWorkspaceEx rws = factory.OpenFromFile(gdb, 0) as IRasterWorkspaceEx;
                if (rws == null)
                {
                    return false;
                }
                rasterDs = rws.OpenRasterDataset(name);
                if (rasterDs == null)
                {
                    return false;
                }

                IRasterBandCollection bands = rasterDs as IRasterBandCollection;
                if (bands == null || bands.Count < 1)
                {
                    return false;
                }
                IRasterBand band = bands.Item(0);
                IRasterStatistics stats = null;
                try
                {
                    stats = band.Statistics;
                }
                catch
                {
                    stats = null;
                }
                if (stats == null)
                {
                    try
                    {
                        band.ComputeStatsAndHist();
                        stats = band.Statistics;
                    }
                    catch
                    {
                        return false;
                    }
                }
                if (stats == null)
                {
                    return false;
                }
                minValue = stats.Minimum;
                maxValue = stats.Maximum;
                return maxValue >= minValue;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (rasterDs != null)
                {
                    Marshal.FinalReleaseComObject(rasterDs);
                }
            }
        }

        public static string FindByKeywords(IList<string> names, params string[] keywords)
        {
            if (names == null || keywords == null)
            {
                return null;
            }
            for (int i = 0; i < names.Count; i++)
            {
                string n = names[i];
                for (int k = 0; k < keywords.Length; k++)
                {
                    if (n.IndexOf(keywords[k], StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return n;
                    }
                }
            }
            return null;
        }

        public static string ToFeatureClassPath(string gdbPath, string featureClassName)
        {
            if (string.IsNullOrEmpty(gdbPath) || string.IsNullOrEmpty(featureClassName))
            {
                return null;
            }
            return System.IO.Path.Combine(gdbPath, featureClassName);
        }

        private static void Collect(IWorkspace workspace, List<string> names)
        {
            IEnumDataset enumFc = workspace.get_Datasets(esriDatasetType.esriDTFeatureClass);
            if (enumFc != null)
            {
                enumFc.Reset();
                IDataset ds = enumFc.Next();
                while (ds != null)
                {
                    names.Add(ds.Name);
                    ds = enumFc.Next();
                }
            }

            IEnumDataset enumFd = workspace.get_Datasets(esriDatasetType.esriDTFeatureDataset);
            if (enumFd != null)
            {
                enumFd.Reset();
                IDataset fd = enumFd.Next();
                while (fd != null)
                {
                    IFeatureDataset featureDataset = fd as IFeatureDataset;
                    if (featureDataset != null)
                    {
                        IEnumDataset subsets = featureDataset.Subsets;
                        if (subsets != null)
                        {
                            subsets.Reset();
                            IDataset child = subsets.Next();
                            while (child != null)
                            {
                                if (child.Type == esriDatasetType.esriDTFeatureClass)
                                {
                                    names.Add(child.Name);
                                }
                                child = subsets.Next();
                            }
                        }
                    }
                    fd = enumFd.Next();
                }
            }
        }

        private static void CollectRasters(IWorkspace workspace, List<string> names)
        {
            IEnumDataset enumRs = workspace.get_Datasets(esriDatasetType.esriDTRasterDataset);
            if (enumRs != null)
            {
                enumRs.Reset();
                IDataset ds = enumRs.Next();
                while (ds != null)
                {
                    names.Add(ds.Name);
                    ds = enumRs.Next();
                }
            }

            IEnumDataset enumFd = workspace.get_Datasets(esriDatasetType.esriDTFeatureDataset);
            if (enumFd != null)
            {
                enumFd.Reset();
                IDataset fd = enumFd.Next();
                while (fd != null)
                {
                    IFeatureDataset featureDataset = fd as IFeatureDataset;
                    if (featureDataset != null)
                    {
                        IEnumDataset subsets = featureDataset.Subsets;
                        if (subsets != null)
                        {
                            subsets.Reset();
                            IDataset child = subsets.Next();
                            while (child != null)
                            {
                                if (child.Type == esriDatasetType.esriDTRasterDataset)
                                {
                                    names.Add(child.Name);
                                }
                                child = subsets.Next();
                            }
                        }
                    }
                    fd = enumFd.Next();
                }
            }
        }
    }
}

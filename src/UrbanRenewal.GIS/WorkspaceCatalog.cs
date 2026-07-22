using System;
using System.Collections.Generic;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// File GDB 要素类枚举与按关键词匹配。
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
    }
}

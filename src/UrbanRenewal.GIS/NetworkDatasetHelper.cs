using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geodatabase;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// 打开已预建的网络数据集（不负责构建路网）。
    /// </summary>
    public static class NetworkDatasetHelper
    {
        public const string DefaultFeatureDataset = "roadNet";
        public const string DefaultNetworkName = "roadNet_ND";
        public const string DefaultImpedance = "Length";

        public static bool TryOpen(
            string gdbPath,
            string featureDatasetName,
            string networkName,
            out INetworkDataset networkDataset,
            out string message)
        {
            networkDataset = null;
            message = null;
            if (string.IsNullOrEmpty(gdbPath) || !System.IO.Directory.Exists(gdbPath))
            {
                message = "GDB 路径无效。";
                return false;
            }

            string fdName = string.IsNullOrEmpty(featureDatasetName) ? DefaultFeatureDataset : featureDatasetName;
            string ndName = string.IsNullOrEmpty(networkName) ? DefaultNetworkName : networkName;

            try
            {
                IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
                IWorkspace workspace = factory.OpenFromFile(gdbPath, 0);
                IFeatureWorkspace fws = (IFeatureWorkspace)workspace;

                IFeatureDataset fd;
                try
                {
                    fd = fws.OpenFeatureDataset(fdName);
                }
                catch (Exception ex)
                {
                    message = "未找到路网要素数据集「" + fdName + "」（路网可达性分析需要预先建好路网）。详情: " + ex.Message;
                    return false;
                }

                networkDataset = OpenNetwork(fd, ndName);
                if (networkDataset == null)
                {
                    message = "要素数据集「" + fdName + "」中未找到网络数据集「" + ndName
                        + "」。请先在 ArcGIS 中构建 Network Dataset 后再运行可达性分析。";
                    return false;
                }

                message = "已打开路网: " + fdName + "\\" + ndName;
                return true;
            }
            catch (Exception ex)
            {
                message = "打开网络数据集失败: " + ex.Message;
                return false;
            }
        }

        public static string FindCostAttributeName(INetworkDataset nd, string preferred)
        {
            if (nd == null)
            {
                return preferred;
            }
            if (!string.IsNullOrEmpty(preferred))
            {
                for (int i = 0; i < nd.AttributeCount; i++)
                {
                    INetworkAttribute attr = nd.get_Attribute(i);
                    if (attr != null && string.Equals(attr.Name, preferred, StringComparison.OrdinalIgnoreCase)
                        && attr.UsageType == esriNetworkAttributeUsageType.esriNAUTCost)
                    {
                        return attr.Name;
                    }
                }
            }
            for (int i = 0; i < nd.AttributeCount; i++)
            {
                INetworkAttribute attr = nd.get_Attribute(i);
                if (attr != null && attr.UsageType == esriNetworkAttributeUsageType.esriNAUTCost)
                {
                    return attr.Name;
                }
            }
            return preferred;
        }

        /// <summary>
        /// 枚举 GDB 中的网络数据集（featureDataset\networkName）。
        /// </summary>
        public static List<string> ListNetworkDatasets(string gdbPath)
        {
            List<string> list = new List<string>();
            if (string.IsNullOrEmpty(gdbPath) || !System.IO.Directory.Exists(gdbPath))
            {
                return list;
            }

            try
            {
                IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
                IWorkspace workspace = factory.OpenFromFile(gdbPath, 0);
                IEnumDataset enumFd = workspace.get_Datasets(esriDatasetType.esriDTFeatureDataset);
                if (enumFd == null)
                {
                    return list;
                }
                enumFd.Reset();
                IDataset fdDs = enumFd.Next();
                while (fdDs != null)
                {
                    IFeatureDataset fd = fdDs as IFeatureDataset;
                    if (fd != null)
                    {
                        CollectNetworksInFeatureDataset(fd, list);
                    }
                    fdDs = enumFd.Next();
                }
            }
            catch
            {
            }
            return list;
        }

        public static string BuildIntegrityReport(string gdbPath)
        {
            StringBuilder sb = new StringBuilder();
            List<string> nets = ListNetworkDatasets(gdbPath);
            if (nets.Count == 0)
            {
                sb.AppendLine("[警告] 未检测到网络数据集。路网可达性分析需要预先在要素数据集中构建 Network Dataset（如 roadNet\\roadNet_ND）。");
            }
            else
            {
                sb.AppendLine("[通过] 网络数据集 " + nets.Count + " 个: " + string.Join(", ", nets.ToArray()));
                sb.AppendLine("[提示] 路网须提前构建完成；分析程序不会自动创建 Network Dataset。");
            }
            return sb.ToString();
        }

        private static void CollectNetworksInFeatureDataset(IFeatureDataset fd, List<string> list)
        {
            IFeatureDatasetExtensionContainer extContainer = fd as IFeatureDatasetExtensionContainer;
            if (extContainer != null)
            {
                IFeatureDatasetExtension ext = extContainer.FindExtension(esriDatasetType.esriDTNetworkDataset);
                IDatasetContainer2 container = ext as IDatasetContainer2;
                if (container != null)
                {
                    try
                    {
                        int count = container.get_DatasetCount(esriDatasetType.esriDTNetworkDataset);
                        for (int i = 0; i < count; i++)
                        {
                            IDataset ds = container.get_Dataset(esriDatasetType.esriDTNetworkDataset, i);
                            if (ds != null)
                            {
                                list.Add(fd.Name + "\\" + ds.Name);
                            }
                        }
                        return;
                    }
                    catch
                    {
                    }
                }
            }

            IEnumDataset subsets = fd.Subsets;
            if (subsets == null)
            {
                return;
            }
            subsets.Reset();
            IDataset child = subsets.Next();
            while (child != null)
            {
                if (child.Type == esriDatasetType.esriDTNetworkDataset)
                {
                    list.Add(fd.Name + "\\" + child.Name);
                }
                child = subsets.Next();
            }
        }

        private static INetworkDataset OpenNetwork(IFeatureDataset fd, string name)
        {
            IFeatureDatasetExtensionContainer extContainer = fd as IFeatureDatasetExtensionContainer;
            if (extContainer != null)
            {
                IFeatureDatasetExtension ext = extContainer.FindExtension(esriDatasetType.esriDTNetworkDataset);
                IDatasetContainer2 container = ext as IDatasetContainer2;
                if (container != null)
                {
                    try
                    {
                        return container.get_DatasetByName(esriDatasetType.esriDTNetworkDataset, name) as INetworkDataset;
                    }
                    catch
                    {
                    }
                }
            }

            IEnumDataset subsets = fd.Subsets;
            if (subsets == null)
            {
                return null;
            }
            subsets.Reset();
            IDataset ds = subsets.Next();
            while (ds != null)
            {
                if (ds.Type == esriDatasetType.esriDTNetworkDataset
                    && string.Equals(ds.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return ds as INetworkDataset;
                }
                ds = subsets.Next();
            }
            return null;
        }
    }
}

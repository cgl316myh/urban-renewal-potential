using System;
using System.Runtime.InteropServices;
using System.Threading;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Geodatabase;

namespace UrbanRenewal.GIS
{
    /// <summary>File GDB 独占删除与 COM 占用释放。</summary>
    public static class FileGdbLockHelper
    {
        public static void ForceComRelease()
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch
            {
            }
            try
            {
                Thread.Sleep(200);
            }
            catch
            {
            }
        }

        /// <summary>独占删除要素类；不存在视为成功。</summary>
        public static bool TryDeleteFeatureClassExclusive(string gdbPath, string featureClassName, out string message)
        {
            message = null;
            if (string.IsNullOrEmpty(gdbPath) || string.IsNullOrEmpty(featureClassName)
                || !System.IO.Directory.Exists(gdbPath))
            {
                message = "路径无效";
                return false;
            }

            IWorkspace workspace = null;
            IFeatureClass fc = null;
            try
            {
                IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
                workspace = factory.OpenFromFile(gdbPath, 0);
                IFeatureWorkspace fws = (IFeatureWorkspace)workspace;
                try
                {
                    fc = fws.OpenFeatureClass(featureClassName);
                }
                catch
                {
                    // 已不存在
                    message = "目标不存在，无需删除";
                    return true;
                }

                IDataset ds = fc as IDataset;
                if (ds == null || !ds.CanDelete())
                {
                    message = "要素类不可删除（可能被占用或为网络附属）";
                    return false;
                }

                ISchemaLock schemaLock = ds as ISchemaLock;
                if (schemaLock != null)
                {
                    try
                    {
                        schemaLock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);
                    }
                    catch (Exception exLock)
                    {
                        message = "无法取得独占锁: " + exLock.Message;
                        return false;
                    }
                }

                ds.Delete();
                message = "已独占删除";
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
            finally
            {
                if (fc != null)
                {
                    try { Marshal.FinalReleaseComObject(fc); }
                    catch { }
                }
                if (workspace != null)
                {
                    try { Marshal.FinalReleaseComObject(workspace); }
                    catch { }
                }
                ForceComRelease();
            }
        }

        /// <summary>独占删除栅格（含 VAT）；不存在视为成功。</summary>
        public static bool TryDeleteRasterExclusive(string gdbPath, string rasterName, out string message)
        {
            message = null;
            if (string.IsNullOrEmpty(gdbPath) || string.IsNullOrEmpty(rasterName)
                || !System.IO.Directory.Exists(gdbPath))
            {
                message = "路径无效";
                return false;
            }

            IWorkspace workspace = null;
            IRasterDataset rasterDs = null;
            try
            {
                IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
                workspace = factory.OpenFromFile(gdbPath, 0);
                IRasterWorkspaceEx rws = workspace as IRasterWorkspaceEx;
                if (rws == null)
                {
                    message = "工作空间不支持栅格";
                    return false;
                }

                try
                {
                    rasterDs = rws.OpenRasterDataset(rasterName);
                }
                catch
                {
                    message = "目标不存在，无需删除";
                    return true;
                }

                IDataset ds = rasterDs as IDataset;
                if (ds == null || !ds.CanDelete())
                {
                    message = "栅格不可删除（可能被占用）";
                    return false;
                }

                ISchemaLock schemaLock = ds as ISchemaLock;
                if (schemaLock != null)
                {
                    try
                    {
                        schemaLock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);
                    }
                    catch (Exception exLock)
                    {
                        message = "无法取得栅格独占锁: " + exLock.Message;
                        return false;
                    }
                }

                ds.Delete();
                message = "已独占删除栅格";
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
            finally
            {
                if (rasterDs != null)
                {
                    try { Marshal.FinalReleaseComObject(rasterDs); }
                    catch { }
                }
                if (workspace != null)
                {
                    try { Marshal.FinalReleaseComObject(workspace); }
                    catch { }
                }
                ForceComRelease();
            }
        }

        /// <summary>要素类是否仍存在。</summary>
        public static bool FeatureClassExists(string gdbPath, string featureClassName)
        {
            if (string.IsNullOrEmpty(gdbPath) || string.IsNullOrEmpty(featureClassName)
                || !System.IO.Directory.Exists(gdbPath))
            {
                return false;
            }

            IWorkspace workspace = null;
            IFeatureClass fc = null;
            try
            {
                IWorkspaceFactory factory = new FileGDBWorkspaceFactoryClass();
                workspace = factory.OpenFromFile(gdbPath, 0);
                fc = ((IFeatureWorkspace)workspace).OpenFeatureClass(featureClassName);
                return fc != null;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (fc != null)
                {
                    try { Marshal.FinalReleaseComObject(fc); }
                    catch { }
                }
                if (workspace != null)
                {
                    try { Marshal.FinalReleaseComObject(workspace); }
                    catch { }
                }
            }
        }
    }
}

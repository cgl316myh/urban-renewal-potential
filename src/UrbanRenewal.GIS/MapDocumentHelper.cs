using System;
using System.IO;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using UrbanRenewal.Model;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// 工程地图文档（*.mxd）保存与加载。
    /// </summary>
    public static class MapDocumentHelper
    {
        /// <summary>默认工程 MXD：与 app_settings.xml 同目录下的 CurrentProject.mxd</summary>
        public static string GetDefaultProjectMxdPath()
        {
            string configDir = GlobalAppSettingsStore.GetConfigDirectory();
            Directory.CreateDirectory(configDir);
            return Path.Combine(configDir, "CurrentProject.mxd");
        }

        /// <summary>
        /// 保存当前地图到 MXD。可传入 AxMapControl 或 IMapControl3。
        /// </summary>
        public static bool SaveMapToMxd(object mapControlOrAx, string mxdPath, out string message)
        {
            message = null;
            IMapControl3 mapControl;
            IMxdContents contents;
            ResolveMapControl(mapControlOrAx, out mapControl, out contents);

            if (mapControl == null)
            {
                message = "地图控件无效。";
                return false;
            }
            if (string.IsNullOrEmpty(mxdPath))
            {
                message = "未指定 MXD 路径。";
                return false;
            }
            if (!mxdPath.EndsWith(".mxd", StringComparison.OrdinalIgnoreCase))
            {
                mxdPath = mxdPath + ".mxd";
            }

            IMapDocument mapDoc = null;
            try
            {
                string dir = Path.GetDirectoryName(mxdPath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                if (File.Exists(mxdPath))
                {
                    try
                    {
                        File.Delete(mxdPath);
                    }
                    catch (Exception exDel)
                    {
                        message = "无法覆盖已有 MXD（可能被占用）: " + exDel.Message;
                        return false;
                    }
                }

                mapDoc = new MapDocumentClass();
                mapDoc.New(mxdPath);

                if (contents != null)
                {
                    mapDoc.ReplaceContents(contents);
                }
                else
                {
                    // 回退：逐层复制到新文档
                    if (!CopyLayersToMapDocument(mapControl, mapDoc, out message))
                    {
                        return false;
                    }
                }

                mapDoc.Save(true, true);
                message = "工程地图已保存: " + mxdPath;
                return true;
            }
            catch (Exception ex)
            {
                message = "保存 MXD 失败: " + ex.Message;
                return false;
            }
            finally
            {
                if (mapDoc != null)
                {
                    try { mapDoc.Close(); }
                    catch { }
                    try { Marshal.FinalReleaseComObject(mapDoc); }
                    catch { }
                }
            }
        }

        public static bool LoadMxdToMap(object mapControlOrAx, string mxdPath, out string message)
        {
            message = null;
            IMapControl3 mapControl;
            IMxdContents unused;
            ResolveMapControl(mapControlOrAx, out mapControl, out unused);

            if (mapControl == null)
            {
                message = "地图控件无效。";
                return false;
            }
            if (string.IsNullOrEmpty(mxdPath) || !File.Exists(mxdPath))
            {
                message = "工程 MXD 不存在: " + mxdPath;
                return false;
            }

            try
            {
                mapControl.LoadMxFile(mxdPath, Type.Missing, Type.Missing);
                try
                {
                    mapControl.Extent = mapControl.FullExtent;
                }
                catch
                {
                }
                mapControl.Refresh();
                message = "已加载工程地图: " + mxdPath;
                return true;
            }
            catch (Exception ex)
            {
                message = "加载 MXD 失败: " + ex.Message;
                return false;
            }
        }

        public static bool TryDeleteMxd(string mxdPath)
        {
            if (string.IsNullOrEmpty(mxdPath) || !File.Exists(mxdPath))
            {
                return true;
            }
            try
            {
                File.Delete(mxdPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void ResolveMapControl(
            object mapControlOrAx,
            out IMapControl3 mapControl,
            out IMxdContents contents)
        {
            mapControl = null;
            contents = null;
            if (mapControlOrAx == null)
            {
                return;
            }

            contents = mapControlOrAx as IMxdContents;
            mapControl = mapControlOrAx as IMapControl3;
            if (mapControl != null && contents != null)
            {
                return;
            }

            try
            {
                System.Reflection.PropertyInfo pi = mapControlOrAx.GetType().GetProperty("Object");
                if (pi != null)
                {
                    object inner = pi.GetValue(mapControlOrAx, null);
                    if (mapControl == null)
                    {
                        mapControl = inner as IMapControl3;
                    }
                    if (contents == null)
                    {
                        contents = inner as IMxdContents;
                    }
                }
            }
            catch
            {
            }

            // AxMapControl 本身有时也可作为 IMxdContents
            if (contents == null)
            {
                contents = mapControl as IMxdContents;
            }
        }

        private static bool CopyLayersToMapDocument(IMapControl3 mapControl, IMapDocument mapDoc, out string message)
        {
            message = null;
            try
            {
                IMap destMap = mapDoc.get_Map(0);
                if (destMap == null)
                {
                    message = "无法创建地图文档内容。";
                    return false;
                }

                while (destMap.LayerCount > 0)
                {
                    destMap.DeleteLayer(destMap.get_Layer(0));
                }

                IMap srcMap = mapControl.Map;
                if (srcMap == null)
                {
                    message = "当前地图为空。";
                    return false;
                }

                for (int i = 0; i < srcMap.LayerCount; i++)
                {
                    ILayer lyr = srcMap.get_Layer(i);
                    if (lyr != null)
                    {
                        destMap.AddLayer(lyr);
                    }
                }

                try
                {
                    destMap.SpatialReference = srcMap.SpatialReference;
                }
                catch
                {
                }

                return true;
            }
            catch (Exception ex)
            {
                message = "复制图层到 MXD 失败: " + ex.Message;
                return false;
            }
        }
    }
}

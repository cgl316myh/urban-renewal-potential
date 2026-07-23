using System;
using System.Globalization;
using System.IO;
using System.Text;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using IoPath = System.IO.Path;

namespace UrbanRenewal.GIS
{
    /// <summary>
    /// Geoprocessor 封装（ArcEngine 10.2）。
    /// </summary>
    public sealed class GeoprocessorHelper
    {
        private readonly Geoprocessor _gp;

        public GeoprocessorHelper()
        {
            _gp = new Geoprocessor();
            _gp.OverwriteOutput = true;
            _gp.AddOutputsToMap = false;
        }

        /// <summary>
        /// 配置工作空间、像元大小与输出坐标系。
        /// 注意：不要在投影阶段设置 extent（米制范围会让 WGS84 图层 Project 报 invalid extent）。
        /// </summary>
        public void ConfigureAnalysis(string workDir, string extentDataset, double cellSize, object outputCoordinateSystem)
        {
            TrySetEnv("workspace", workDir);
            TrySetEnv("scratchWorkspace", workDir);

            // 投影阶段使用默认范围；栅格分析前再调用 SetExtent
            TrySetEnv("extent", "DEFAULT");

            if (cellSize > 0)
            {
                TrySetEnv("cellSize", cellSize.ToString(CultureInfo.InvariantCulture));
            }

            object ocs = ToCoordinateSystemEnvValue(outputCoordinateSystem, workDir);
            if (ocs != null)
            {
                TrySetEnv("outputCoordinateSystem", ocs);
            }
        }

        public void SetExtent(string extentDataset)
        {
            if (string.IsNullOrEmpty(extentDataset))
            {
                TrySetEnv("extent", "DEFAULT");
                return;
            }
            TrySetEnv("extent", extentDataset);
        }

        private static object ToCoordinateSystemEnvValue(object outputCoordinateSystem, string workDir)
        {
            if (outputCoordinateSystem == null)
            {
                return null;
            }

            if (outputCoordinateSystem is string)
            {
                return outputCoordinateSystem;
            }

            ISpatialReference sr = outputCoordinateSystem as ISpatialReference;
            if (sr == null)
            {
                return null;
            }

            try
            {
                if (sr.FactoryCode > 0)
                {
                    return sr.FactoryCode.ToString(CultureInfo.InvariantCulture);
                }
            }
            catch
            {
            }

            // 无 FactoryCode 时导出 .prj
            if (string.IsNullOrEmpty(workDir))
            {
                return null;
            }
            Directory.CreateDirectory(workDir);
            string prjPath = IoPath.Combine(workDir, "analysis_target.prj");
            try
            {
                int bytes;
                string buffer;
                IESRISpatialReferenceGEN gen = sr as IESRISpatialReferenceGEN;
                if (gen != null)
                {
                    gen.ExportToESRISpatialReference(out buffer, out bytes);
                    File.WriteAllText(prjPath, buffer, Encoding.ASCII);
                    return prjPath;
                }
            }
            catch
            {
            }
            return null;
        }

        private void TrySetEnv(string name, object value)
        {
            if (value == null)
            {
                return;
            }
            string s = value as string;
            if (s != null && s.Length == 0)
            {
                return;
            }
            try
            {
                _gp.SetEnvironmentValue(name, value);
            }
            catch (Exception ex)
            {
                // 个别环境项失败不阻断；投影对齐仍由 Project 工具保证
                System.Diagnostics.Debug.WriteLine("SetEnvironmentValue " + name + " skipped: " + ex.Message);
            }
        }

        public void Execute(IGPProcess process, string stepName)
        {
            try
            {
                _gp.Execute(process, null);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(stepName + " 失败: " + ex.Message + "\r\n" + GetMessages(), ex);
            }

            if (_gp.MessageCount > 0)
            {
                for (int i = 0; i < _gp.MessageCount; i++)
                {
                    if (_gp.GetMessage(i).Contains("ERROR") || _gp.GetMessage(i).Contains("Error"))
                    {
                        throw new InvalidOperationException(stepName + " 报错: " + GetMessages());
                    }
                }
            }
        }

        public string GetMessages()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _gp.MessageCount; i++)
            {
                sb.AppendLine(_gp.GetMessage(i));
            }
            return sb.ToString();
        }
    }
}

using System;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;

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

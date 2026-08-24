using System;
using System.Globalization;
using System.IO;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using IoPath = System.IO.Path;

namespace UrbanRenewal.GIS
{
    /// <summary>Geoprocessor 封装（ArcEngine 10.2）；GP 消息回传到分析日志。</summary>
    public sealed class GeoprocessorHelper
    {
        private readonly Geoprocessor _gp;
        private Action<string, string> _messageSink;
        private string _currentStep;
        private int _loggedMessageCount;
        private bool _receivedMessageEvents;

        public GeoprocessorHelper()
            : this(null)
        {
        }

        public GeoprocessorHelper(Action<string, string> messageSink)
        {
            _messageSink = messageSink;
            _gp = new Geoprocessor();
            _gp.OverwriteOutput = true;
            _gp.AddOutputsToMap = false;
            _gp.MessagesCreated += OnMessagesCreated;
        }

        public void SetMessageSink(Action<string, string> messageSink)
        {
            _messageSink = messageSink;
        }

        public void BindToProgress(Action<string, int> progress, Func<int> currentPercent)
        {
            _messageSink = delegate(string level, string text)
            {
                if (progress == null || string.IsNullOrEmpty(text))
                {
                    return;
                }
                int pct = 0;
                if (currentPercent != null)
                {
                    try
                    {
                        pct = currentPercent();
                    }
                    catch
                    {
                        pct = 0;
                    }
                }
                progress("GP[" + level + "] " + text, pct);
            };
        }

        /// <summary>配置工作空间、像元大小与输出坐标系；投影阶段勿设 extent（WGS84 Project 会 invalid extent）。</summary>
        public void ConfigureAnalysis(string workDir, string extentDataset, double cellSize, object outputCoordinateSystem)
        {
            TrySetEnv("workspace", workDir);
            TrySetEnv("scratchWorkspace", workDir);

            // 投影阶段保持 DEFAULT；栅格分析前再 SetExtent
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
                System.Diagnostics.Debug.WriteLine("SetEnvironmentValue " + name + " skipped: " + ex.Message);
            }
        }

        public void Execute(IGPProcess process, string stepName)
        {
            _currentStep = stepName ?? string.Empty;
            _loggedMessageCount = 0;
            _receivedMessageEvents = false;
            Emit("执行", string.IsNullOrEmpty(_currentStep) ? "开始 GP…" : ("开始 " + _currentStep));

            try
            {
                _gp.Execute(process, null);
            }
            catch (Exception ex)
            {
                FlushRemainingGpMessages();
                Emit("错误", (stepName ?? "GP") + " 异常: " + ex.Message);
                throw new InvalidOperationException(stepName + " 失败: " + ex.Message + "\r\n" + GetMessages(), ex);
            }

            FlushRemainingGpMessages();

            if (_gp.MessageCount > 0)
            {
                for (int i = 0; i < _gp.MessageCount; i++)
                {
                    string m = _gp.GetMessage(i);
                    if (m != null && (m.Contains("ERROR") || m.Contains("Error")))
                    {
                        throw new InvalidOperationException(stepName + " 报错: " + GetMessages());
                    }
                }
            }

            Emit("结束", string.IsNullOrEmpty(_currentStep) ? "GP 完成" : (_currentStep + " 完成"));
        }

        private void OnMessagesCreated(object sender, MessagesCreatedEventArgs e)
        {
            _receivedMessageEvents = true;
            if (e == null)
            {
                return;
            }
            IGPMessages gpMsgs = e.GPMessages;
            EmitGpMessages(gpMsgs);
        }

        private void FlushRemainingGpMessages()
        {
            if (_receivedMessageEvents)
            {
                return;
            }
            try
            {
                for (int i = 0; i < _gp.MessageCount; i++)
                {
                    string m = _gp.GetMessage(i);
                    if (!string.IsNullOrEmpty(m))
                    {
                        Emit("信息", m);
                    }
                }
            }
            catch
            {
            }
        }

        private void EmitGpMessages(IGPMessages gpMsgs)
        {
            if (gpMsgs == null)
            {
                return;
            }
            int count = 0;
            try
            {
                count = gpMsgs.Count;
            }
            catch
            {
                return;
            }

            for (int i = _loggedMessageCount; i < count; i++)
            {
                IGPMessage msg = null;
                try
                {
                    msg = gpMsgs.GetMessage(i);
                }
                catch
                {
                    continue;
                }
                if (msg == null)
                {
                    continue;
                }

                string level = ClassifyGpMessage(msg.Type);
                string desc = null;
                try
                {
                    desc = msg.Description;
                }
                catch
                {
                }
                if (string.IsNullOrEmpty(desc))
                {
                    continue;
                }
                Emit(level, desc);
            }
            _loggedMessageCount = count;
        }

        private static string ClassifyGpMessage(esriGPMessageType type)
        {
            switch (type)
            {
                case esriGPMessageType.esriGPMessageTypeAbort:
                    return "警告";
                case esriGPMessageType.esriGPMessageTypeEmpty:
                    return "信息";
                case esriGPMessageType.esriGPMessageTypeError:
                    return "错误";
                case esriGPMessageType.esriGPMessageTypeGDBError:
                    return "错误GDB";
                case esriGPMessageType.esriGPMessageTypeInformative:
                    return "信息";
                case esriGPMessageType.esriGPMessageTypeProcessDefinition:
                    return "执行";
                case esriGPMessageType.esriGPMessageTypeProcessStart:
                    return "开始";
                case esriGPMessageType.esriGPMessageTypeProcessStop:
                    return "结束";
                case esriGPMessageType.esriGPMessageTypeWarning:
                    return "警告";
                default:
                    return "信息";
            }
        }

        private void Emit(string level, string text)
        {
            if (_messageSink == null || string.IsNullOrEmpty(text))
            {
                return;
            }
            try
            {
                _messageSink(level, text);
            }
            catch
            {
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

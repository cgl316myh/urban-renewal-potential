using System;
using System.Windows.Forms;
using UrbanRenewal.Contracts;

namespace UrbanRenewal.Host
{
    internal sealed class AppContextImpl : IAppContext
    {
        private readonly MainRibbonForm _form;

        public AppContextImpl(MainRibbonForm form)
        {
            _form = form;
        }

        public object MapControl
        {
            get { return null; }
        }

        public object TocControl
        {
            get { return null; }
        }

        public object DockManager
        {
            get { return null; }
        }

        public string GdbPath { get; set; }

        public void LogInfo(string message)
        {
            _form.AppendLog("INFO", message);
        }

        public void LogWarn(string message)
        {
            _form.AppendLog("WARN", message);
        }

        public void LogError(string message)
        {
            _form.AppendLog("ERROR", message);
        }

        public void ShowProgress(string caption, int percent)
        {
            _form.SetStatus(caption + " " + percent + "%");
        }

        public void HideProgress()
        {
            _form.SetStatus("就绪");
        }

        public void ShowMessage(string caption, string text)
        {
            MessageBox.Show(_form, text, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}

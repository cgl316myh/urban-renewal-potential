using System;
using System.Threading;
using System.Windows.Forms;
using UrbanRenewal.Model;

namespace UrbanRenewal.Host
{
    /// <summary>UI / AppDomain / Task 未处理异常捕获与落盘。</summary>
    internal static class GlobalExceptionHandler
    {
        private static bool _registered;
        private static int _showing;

        public static void Register()
        {
            if (_registered)
            {
                return;
            }
            _registered = true;

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += OnUiThreadException;
            AppDomain.CurrentDomain.UnhandledException += OnDomainUnhandledException;

            try
            {
                System.Threading.Tasks.TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            }
            catch
            {
            }
        }

        private static void OnUiThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Handle("UI线程 (Application.ThreadException)", e != null ? e.Exception : null, false);
        }

        private static void OnDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e != null ? e.ExceptionObject as Exception : null;
            bool terminating = e != null && e.IsTerminating;
            Handle("非UI/最终 (AppDomain.UnhandledException)"
                + (terminating ? " [即将终止]" : ""), ex, terminating);
        }

        private static void OnUnobservedTaskException(object sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
        {
            if (e == null)
            {
                return;
            }
            Handle("Task (UnobservedTaskException)", e.Exception, false);
            e.SetObserved();
        }

        private static void Handle(string source, Exception ex, bool terminating)
        {
            string path = null;
            try
            {
                SessionLogWriter.AppendException(source, ex);
                path = SessionLogWriter.WriteCrashReport(source, ex);
            }
            catch
            {
            }

            // 防重入弹窗死锁
            if (Interlocked.CompareExchange(ref _showing, 1, 0) != 0)
            {
                return;
            }
            try
            {
                string msg = "程序捕获到未处理异常，已写入日志，请勿忽略。\r\n\r\n"
                    + "来源: " + source + "\r\n"
                    + (ex != null ? ("类型: " + ex.GetType().FullName + "\r\n消息: " + ex.Message) : "(无详情)")
                    + (string.IsNullOrEmpty(path) ? "" : ("\r\n\r\n报告文件:\r\n" + path));

                try
                {
                    MessageBox.Show(msg, "未处理异常", MessageBoxButtons.OK,
                        terminating ? MessageBoxIcon.Error : MessageBoxIcon.Warning);
                }
                catch
                {
                }
            }
            finally
            {
                Interlocked.Exchange(ref _showing, 0);
            }
        }
    }
}

using System;
using System.Threading;
using System.Windows.Forms;

namespace UrbanRenewal.Contracts
{
    /// <summary>
    /// 在 STA 后台线程执行耗时分析（ArcObjects/GP 要求），完成后再回到 UI 线程。
    /// </summary>
    public static class StaBackgroundRunner
    {
        public static void Run<T>(
            Control syncControl,
            Func<T> work,
            Action<T> onCompleted,
            Action<Exception> onFailed)
        {
            if (work == null)
            {
                throw new ArgumentNullException("work");
            }
            if (syncControl == null)
            {
                throw new ArgumentNullException("syncControl");
            }

            Thread thread = new Thread(delegate()
            {
                try
                {
                    T result = work();
                    Post(syncControl, delegate
                    {
                        if (onCompleted != null)
                        {
                            onCompleted(result);
                        }
                    });
                }
                catch (Exception ex)
                {
                    Post(syncControl, delegate
                    {
                        if (onFailed != null)
                        {
                            onFailed(ex);
                        }
                    });
                }
            });
            thread.IsBackground = true;
            thread.Name = "UrbanRenewal.AnalysisWorker";
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        /// <summary>将 UI 更新投递到控件所属线程（非阻塞）。</summary>
        public static void Post(Control syncControl, Action action)
        {
            if (action == null || syncControl == null)
            {
                return;
            }
            if (syncControl.IsDisposed)
            {
                return;
            }
            try
            {
                if (syncControl.InvokeRequired)
                {
                    syncControl.BeginInvoke(action);
                }
                else
                {
                    action();
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }

        /// <summary>
        /// 合并频繁进度回调：后台可狂发，UI 只保留最新一条待刷新，避免消息队列堵死。
        /// </summary>
        public sealed class ProgressUiGate
        {
            private readonly Control _syncControl;
            private readonly Action<string, int> _apply;
            private readonly object _gate = new object();
            private string _text;
            private int _percent;
            private bool _scheduled;

            public ProgressUiGate(Control syncControl, Action<string, int> apply)
            {
                if (syncControl == null)
                {
                    throw new ArgumentNullException("syncControl");
                }
                if (apply == null)
                {
                    throw new ArgumentNullException("apply");
                }
                _syncControl = syncControl;
                _apply = apply;
            }

            public void Report(string text, int percent)
            {
                lock (_gate)
                {
                    _text = text;
                    _percent = percent;
                    if (_scheduled)
                    {
                        return;
                    }
                    _scheduled = true;
                }
                Post(_syncControl, Flush);
            }

            private void Flush()
            {
                string text;
                int percent;
                lock (_gate)
                {
                    text = _text;
                    percent = _percent;
                    _scheduled = false;
                }
                if (_syncControl.IsDisposed)
                {
                    return;
                }
                _apply(text, percent);
            }
        }
    }

    /// <summary>
    /// 非模态打开分析窗体，避免 ShowDialog 锁死主界面；同类型窗体已打开则激活。
    /// </summary>
    public static class ModelessFormHelper
    {
        public static void ShowOrActivate<T>(ref T openForm, Func<T> create, IWin32Window owner)
            where T : Form
        {
            if (create == null)
            {
                throw new ArgumentNullException("create");
            }
            if (openForm != null && !openForm.IsDisposed)
            {
                if (openForm.WindowState == FormWindowState.Minimized)
                {
                    openForm.WindowState = FormWindowState.Normal;
                }
                openForm.Activate();
                openForm.BringToFront();
                return;
            }

            T form = create();
            openForm = form;
            form.FormClosed += delegate(object sender, FormClosedEventArgs e)
            {
                Form f = sender as Form;
                if (f != null)
                {
                    f.Dispose();
                }
            };
            if (owner != null)
            {
                form.Show(owner);
            }
            else
            {
                form.Show();
            }
        }
    }
}

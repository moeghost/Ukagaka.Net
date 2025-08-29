using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Ukagaka
{
    public class SCInputBoxSession
    {
        private readonly SCSession session;
        private readonly string symbol;
        private readonly int timeout;

        private volatile bool flagTerm = false;
        private volatile string messageFromUser = null;
        private SCInputBoxController box;
        private Thread thread;

        public SCInputBoxSession(SCSession session, string symbol, int timeout)
        {
            this.session = session;
            this.symbol = symbol;
            this.timeout = timeout;
        }

        public void Start()
        {
            if (thread == null)
            {
                thread = new Thread(Run) { IsBackground = true };
                thread.Start();
            }
        }

        private void Run()
        {
            // Java 中的 NSAutoreleasePool.push() 在 C# 不需要
            session.SetPassiveMode(true);

            // 打开输入框（UI 必须在主线程执行）
            Application.Current.Dispatcher.Invoke(() =>
            {
                box = new SCInputBoxController(this);
                box.ShowInputBox();
            });

            // 等待用户输入或超时
            if (timeout == -1) // 永不超时
            {
                while (!flagTerm && messageFromUser == null)
                {
                    try
                    {
                        Thread.Sleep(30 * 1000);
                    }
                    catch (ThreadInterruptedException)
                    {
                        if (flagTerm) break;
                        if (messageFromUser != null) break;
                    }
                }
            }
            else
            {
                try
                {
                    Thread.Sleep(timeout);
                }
                catch (ThreadInterruptedException) { }
            }

            if (!flagTerm)
            {
                if (messageFromUser == null)
                {
                    session.DoShioriEvent("OnUserInput", new[] { symbol, "timeout" });
                }
                else
                {
                    session.DoShioriEvent("OnUserInput", new[] { symbol, messageFromUser });
                }
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                box?.CloseInputBox();
            });

            session.SetPassiveMode(false);
            // NSAutoreleasePool.pop(pool) 在 C# 不需要
        }

        public SCSession GetSession() => session;

        public void MessageFromUser(string msg)
        {
            messageFromUser = msg;
            thread?.Interrupt();
        }

        public bool IsAlive()
        {
            return thread != null && thread.IsAlive;
        }

        public void Terminate()
        {
            flagTerm = true;
            thread?.Interrupt();
            Application.Current.Dispatcher.Invoke(() => box?.CloseInputBox());
            thread?.Join();
        }
    }
}

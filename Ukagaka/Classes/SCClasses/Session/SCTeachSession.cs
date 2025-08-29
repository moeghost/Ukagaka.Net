using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ukagaka
{
    public class SCTeachSession
    {
        private readonly SCSession session;
        private volatile bool flagTerm = false;
        private volatile string lastMessageFromUser;
        private SCTeachBoxController box;
        private Thread thread;

        public SCTeachSession(SCSession session)
        {
            this.session = session;
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
            if (session.GetMasterSpirit().GetShioriProtocolVersion() != 2)
                return;

            // 在 Java 中 NSAutoreleasePool.push()，C# 不需要，忽略

            // 打开 TeachBox
            box = new SCTeachBoxController(this);
            box.Show();

            var refs = new List<string>();

            string word = null;
            while (!flagTerm)
            {
                while (!flagTerm)
                {
                    lastMessageFromUser = null;
                    try
                    {
                        Thread.Sleep(30 * 1000);
                    }
                    catch (ThreadInterruptedException)
                    {
                        if (flagTerm) break;
                        if (lastMessageFromUser != null) break; // 有输入，跳出等待
                    }
                }

                if (flagTerm) break;

                if (word == null) // 第一次输入
                {
                    word = lastMessageFromUser;
                }
                else
                {
                    refs.Add(lastMessageFromUser); // 添加到引用栈
                }

                // 构造 Shiori 请求
                var buf = new StringBuilder();
                buf.Append("TEACH SHIORI/2.4\r\n");
                buf.Append("Sender: " + SCFoundation.STRING_FOR_SENDER + "\r\n");
                buf.Append("Word: " + word + "\r\n");

                for (int i = 0; i < refs.Count; i++)
                {
                    buf.Append($"Reference{i}: {refs[i]}\r\n");
                }
                buf.Append("\r\n");

                // 调用 Shiori
                var resp = session.GetMasterSpirit().DoShioriSession(buf.ToString());
                string header = resp.GetHeader();

                if (header.Contains("200 OK"))
                {
                    session.ShioriResponceWork(resp, true);
                    break;
                }
                else if (header.Contains("311 Not Enough"))
                {
                    session.ShioriResponceWork(resp, true);
                }
                else if (header.Contains("312 Advice"))
                {
                    if (refs.Count > 0)
                        refs.RemoveAt(refs.Count - 1);
                    session.ShioriResponceWork(resp, true);
                }
                else
                {
                    // 错误，退出
                    break;
                }
            }

            box?.Close();
        }

        public SCSession GetSession()
        {
            return session;
        }

        public void MessageFromUser(string msg)
        {
            lastMessageFromUser = msg;
            thread?.Interrupt();
        }

        public void Terminate()
        {
            flagTerm = true;
            thread?.Interrupt();
            box?.Close();
            thread?.Join();
        }
        public bool IsAlive()
        {
            return thread != null && thread.IsAlive;
        }

    }
}

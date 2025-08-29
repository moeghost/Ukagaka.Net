using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Ukagaka
{

    public class SCTimeEventTriggerDaemon
    {
        private DateTime lastComparedTime;
        private volatile bool signalStop;
        private Thread thread;


        public SCTimeEventTriggerDaemon()
        {
            lastComparedTime = DateTime.Now;
            signalStop = false;
            thread = new Thread(Run);
        }

        public void Start()
        {
            thread.Start();
        }

        public void Run()
        {
            while (!signalStop)
            {
                // 获取当前时间
                DateTime currentTime = DateTime.Now;

                // 检查分钟变化
                if (lastComparedTime.Minute != currentTime.Minute)
                {
                    // OnMinuteChange: Ref0=连续运行时间(hour), Ref1=见切れ标志(0|1), Ref2=重叠标志(0|1), Ref3=cantalk标志(0|1)
                    List<SCSession> sa = SCFoundation.SharedFoundation().GetSessionsList();
                    foreach (SCSession session in sa)
                    {
                        session.DoShioriEvent(
                            "OnMinuteChange",
                            new string[] {
                            session.HoursFromBootTime().ToString(),
                            session.CheckMikire() ? "1" : "0",
                            session.CheckKasanari() ? "1" : "0"//,
                          //  session.IsInPassiveMode() || NSApplication.SharedApplication().IsHidden ? "0" : "1"
                            }
                        );
                    }

                    lastComparedTime = currentTime;
                }

                // 检查秒变化
                if (lastComparedTime.Second != currentTime.Second)
                {
                    // OnSecondChange: Ref0=连续运行时间(hour), Ref1=见切れ标志(0|1), Ref2=重叠标志(0|1), Ref3=cantalk标志(0|1)
                    List<SCSession> sa = SCFoundation.SharedFoundation().GetSessionsList();
                    foreach (SCSession session in sa)
                    {
                        session.DoShioriEvent(
                            "OnSecondChange",
                            new string[] {
                            session.HoursFromBootTime().ToString(),
                            session.CheckMikire() ? "1" : "0",
                            session.CheckKasanari() ? "1" : "0"//,
                          //  session.IsInPassiveMode() || NSApplication.SharedApplication().IsHidden ? "0" : "1"
                            }
                        );
                    }

                    lastComparedTime = currentTime;
                }

                try
                {
                    Thread.Sleep(300); // 0.3秒间隔
                }
                catch (ThreadInterruptedException) { }
            }
        }

        public void Terminate()
        {
            if (!Thread.CurrentThread.IsAlive)
            {
                return;
            }
            signalStop = true;
            Thread.CurrentThread.Interrupt();
            Thread.CurrentThread.Join();
        }
    }



}

using Cocoa.AppKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ukagaka
{

    public class SCSessionVanisher
    {
        private Thread thread;
        private volatile bool holded;
        private bool inLastEvent;

        private SCSession target;
        private string targetPath;
        private string targetName;

        private string nextPath;
        private string nextBalloonPath;
        private double nextScale;

        public SCSessionVanisher(SCSession target)
        {
            this.target = target;
            this.targetPath = target.GetGhostPath();
            this.targetName = target.GetSelfName();
            this.inLastEvent = false;
            this.holded = false;

            thread = new Thread(Run);
            thread.IsBackground = true;
        }

        public bool IsAlive()
        {
            return thread != null && thread.IsAlive;    

        }

        public void Start() => thread.Start();

        private void Run()
        {
            if (target.IsInPassiveMode())
            {
                target = null;
                return;
            }

            PostNotification(target, "vanisher", "closing");
            target.SetStatusClosing();

            // OnVanishSelected 事件
            var spirit = target.GetMasterSpirit();
            string reqSelected;
            if (spirit.GetShioriProtocolVersion() == 2)
            {
                reqSelected = $"GET Sentence SHIORI/2.0\r\nSender: {SCFoundation.STRING_FOR_SENDER}\r\nSecurityLevel: local\r\nEvent: OnVanishSelected\r\n\r\n";
            }
            else
            {
                reqSelected = $"GET SHIORI/3.0\r\nSender: {SCFoundation.STRING_FOR_SENDER}\r\nID: OnVanishSelected\r\nSecurityLevel: local\r\n\r\n";
            }

            target.ClearQueue();
            target.ForceTerminateScriptRunnder();
            inLastEvent = true;
            target.ShioriResponceWork(spirit.DoShioriSession(reqSelected), false);
            inLastEvent = false;

            // 如果被引止则中止
            if (holded)
            {
                PostNotification(target, "vanisher", "holded");
                target.ClearStatusClosing();
                target = null;
                return;
            }

            // 获取下一个 Ghost
            nextPath = SCFoundation.SharedFoundation().GetAnyGhostPathInAsleep();
            if (nextPath != null)
            {
                nextBalloonPath = SCGhostManager.SharedGhostManager().GetBalloonPathOfGhost(nextPath);
                nextScale = SCGhostManager.SharedGhostManager().GetScaleOfGhost(nextPath);
            }

            // 关闭并删除当前 Ghost
            SCFoundation.SharedFoundation().CloseSession(target);
            PostNotification(target, "vanisher", "closed");
            SCGhostManager.SharedGhostManager().SetBootFlagInDefaults(targetPath, false);

            var targetDirectory = new DirectoryInfo(Path.Combine(SCFoundation.GetParentDirOfBundle(), targetPath));
            SCFileUtils.DeleteRecursively(targetDirectory.FullName);
            PostNotification(target, "vanisher", "vanished");

            // 更新 Ghost Defaults
            var defaults = SCGhostManager.SharedGhostManager().GetGhostDefaults(targetPath);
            defaults["+"] = "false";
            if (!defaults.TryGetValue("vanish_count", out var vanishCountObj))
            {
                vanishCountObj = 0;
            }
            var vanishCount = Convert.ToInt32(vanishCountObj) + 1;
            defaults["vanish_count"] = vanishCount;
            SCGhostManager.SharedGhostManager().SetGhostDefaults(targetPath, defaults);

            // 启动下一个 Ghost
            if (nextPath != null)
            {
                // WPF UI线程定时器代替 NSTimer
                var timer = new System.Timers.Timer(10);
                timer.Elapsed += (s, e) =>
                {
                    timer.Stop();
                    SCFoundation.SharedFoundation().OpenSession(nextPath, nextBalloonPath, nextScale);
                };
                timer.Start();

                SCSession s = null;
                while (s == null)
                {
                    s = SCFoundation.SharedFoundation().GetSession(nextPath);
                    if (s == null) Thread.Sleep(200);
                }
                PostNotification(s, "vanisher", "opened");
                SCGhostManager.SharedGhostManager().SetBootFlagInDefaults(s.GetGhostPath(), true);

                // OnVanished 或 OnGhostChanged
                var nextSpirit = s.GetMasterSpirit();
                string reqVanished;
                if (nextSpirit.GetShioriProtocolVersion() == 2)
                    reqVanished = $"GET Sentence SHIORI/2.0\r\nSender: {SCFoundation.STRING_FOR_SENDER}\r\nSecurityLevel: local\r\nEvent: OnVanished\r\n\r\n";
                else
                    reqVanished = $"GET SHIORI/3.0\r\nSender: {SCFoundation.STRING_FOR_SENDER}\r\nID: OnVanished\r\nSecurityLevel: local\r\n\r\n";

                var respVanished = nextSpirit.DoShioriSession(reqVanished);
                SCShioriSessionResponce finalResp = respVanished;

                if (!respVanished.GetHeader().Contains("200 OK"))
                {
                    string reqChanged;
                    if (nextSpirit.GetShioriProtocolVersion() == 2)
                        reqChanged = $"GET Sentence SHIORI/2.0\r\nSender: {SCFoundation.STRING_FOR_SENDER}\r\nSecurityLevel: local\r\nEvent: OnGhostChanged\r\nReference0: {targetName}\r\n\r\n";
                    else
                        reqChanged = $"GET SHIORI/3.0\r\nSender: {SCFoundation.STRING_FOR_SENDER}\r\nID: OnGhostChanged\r\nSecurityLevel: local\r\n\r\n";

                    finalResp = s.GetMasterSpirit().DoShioriSession(reqChanged);
                }

                s.ShioriResponceWork(finalResp, false);
            }

            target = null;
        }

        public SCSession GetTarget() => target;
        public bool IsInLastEvent() => inLastEvent;
        public void Hold() => holded = true;

        private void PostNotification(SCSession session, string name, string subinfo)
        {
            // 用事件或自定义消息中心替代 NSNotificationCenter
           // NotificationCenter.Default.PostNotification(name, subinfo, session);
        }
    }
}
using Cocoa.AppKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ukagaka
{
    public class SCShellChangingSession
    {
        private Thread thread;
        private SCSession session;

        public SCShellChangingSession(SCSession session)
        {
            this.session = session;
            thread = new Thread(Run);
            thread.IsBackground = true;
        }

        public void Start() => thread.Start();

        public bool IsAlive()
        {

            return thread != null && thread.IsAlive;
        }

        private void Run()
        {
            if (session.IsInPassiveMode())
            {
                session = null;
                return;
            }

            PostNotification("shellchanger", "begin");

            var stat = SCFoundation.SharedFoundation().GetStatusCenter().NewStatusWindow();
            stat.SetTypeToText();
            stat.Show();
            stat.TextTypePrint("Changing the shell...\n");

            var newShell = new SCShell(session, session.GetShellDirName(), stat);

            string oldShellName = session.GetCurrentShell().GetShellName();
            string name = newShell.GetShellName();

            stat.TextTypePrint("old shell:" + oldShellName + "\n");

            var masterSpirit = session.GetMasterSpirit();
            string changingReq;
            if (masterSpirit.GetShioriProtocolVersion() == 2)
            {
                changingReq = $"GET Sentence SHIORI/2.0\r\nSender: {SCFoundation.STRING_FOR_SENDER}\r\nSecurityLevel: local\r\nEvent: OnShellChanging\r\nReference0: {name}\r\n\r\n";
            }
            else
            {
                changingReq = $"GET SHIORI/3.0\r\nSender: {SCFoundation.STRING_FOR_SENDER}\r\nSecurityLevel: local\r\nID: OnShellChanging\r\nReference0: {name}\r\n\r\n";
            }

            // 等待响应结束（阻塞）
            session.ShioriResponceWork(masterSpirit.DoShioriSession(changingReq));

            session.ChangeShell(newShell);
            session.DoShioriEvent("OnShellChanged", new string[] { name, oldShellName }); // 不阻塞

            stat.TextTypePrint("changed.\n");
            stat.CloseWindow();

            PostNotification("shellchanger", "end");
            session = null;
        }

        private void PostNotification(string name, string subinfo)
        {
            // 在C#里用事件或消息中心代替NSNotificationCenter
          //  var userInfo = new NotificationUserInfo();
        //    userInfo["session"] = session;
           // NotificationCenter.Default.PostNotification(name, subinfo, userInfo);
        }

        /// <summary>
        /// 等待线程结束（Java的terminate逻辑）
        /// </summary>
        public void Terminate()
        {
            while (thread.IsAlive)
            {
                Thread.Sleep(100);
            }
        }
    }
}

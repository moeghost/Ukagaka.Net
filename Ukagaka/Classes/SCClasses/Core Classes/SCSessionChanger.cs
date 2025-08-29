using Cocoa.AppKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ukagaka
{
    public class SCSessionChanger
    {
        SCSession before;
        String selfname_of_before;
        String nextPath;
        String nextBalloonPath;
        String nextName;
        double scale;
        bool sendOnGhostChanging; // 切り替え元にOnGhostChangingを送るかどうか。
        Thread thread;
        public SCSessionChanger(SCSession before, String nextPath, String nextBalloonPath, String nextName, double scale)
            :this(before, nextPath, nextBalloonPath, nextName, scale, true)
        {
            // OnGhostChangingを送るタイプ
            
        }
        public SCSessionChanger(SCSession before, String nextPath, String nextBalloonPath, double scale)
            :this(before, nextPath, nextBalloonPath, null, scale, false)
        {
            // OnGhostChangingを送らないタイプ。nextName不要。
            ;
        }
        private SCSessionChanger(SCSession before, String nextPath, String nextBalloonPath, String nextName, Double scale, Boolean sendOnGhostChanging)
        {
            this.before = before;
            this.selfname_of_before = before.GetMasterSpirit().GetSelfName();
            this.nextPath = nextPath;
            this.nextBalloonPath = nextBalloonPath;
            this.nextName = nextName;
            this.scale = scale;
            this.sendOnGhostChanging = sendOnGhostChanging;

            thread = new Thread(Run);
        }

        public void Start()
        {
            thread.Start();
        }

        public void Run()
        {
            if (before.IsInPassiveMode())
            {
                before = null;
                return;
            }

        //    int pool = NSAutoreleasePool.push();

            postNotification(before, "sessionchanger", "closing");
            before.SetStatusClosing();

            SCStatusWindowController stat = SCFoundation.SharedFoundation().GetStatusCenter().NewStatusWindow();
            stat.SetTypeToText();
            stat.Show();
            stat.TextTypePrint("Unmaterializing...\n");

            String name_of_ghost_to_be_unmaterialized = before.GetMasterSpirit().GetName();

            // OnGhostChanging発行
            String script_in_resp = null;
            if (sendOnGhostChanging)
            {
                SCSpirit master_spirit = before.GetMasterSpirit();
                String req;
                if (master_spirit.GetShioriProtocolVersion() == 2)
                {
                    req = "GET Sentence SHIORI/2.0\r\nSender: " + SCFoundation.STRING_FOR_SENDER + "\r\nSecurityLevel: local\r\nEvent: OnGhostChanging\r\nReference0: " + nextName + "\r\nReference1: manual\r\n\r\n";
                }
                else
                {
                    req = "GET SHIORI/3.0\r\nSender: " + SCFoundation.STRING_FOR_SENDER + "\r\nSecurityLevel: local\r\nID: OnGhostChanging\r\nReference0: " + nextName + "\r\nReference1: manual\r\n\r\n";
                }
                SCShioriSessionResponce resp = master_spirit.DoShioriSession(req);
                script_in_resp = (master_spirit.GetShioriProtocolVersion() == 2 ? resp.GetRespForKey("Sentence") : resp.GetRespForKey("Value"));
                before.ShioriResponceWork(resp, false); // 再生が終わるまでブロック。
            }

            // 閉じる
            SCFoundation.SharedFoundation().CloseSession(before);
            postNotification(before, "sessionchanger", "closed");

            stat.TextTypePrint("%[color,orange]ghost \"%[color,white]" + name_of_ghost_to_be_unmaterialized + "%[color,orange]\" unmaterialized.");
            stat.CloseWindow();

            // bootflagを消去
            SCGhostManager.SharedGhostManager().SetBootFlagInDefaults(before.GetGhostPath(), false);

            // 次のゴーストを起動
            SCSession s = SCFoundation.SharedFoundation().OpenSession(nextPath, nextBalloonPath, scale);

            postNotification(s, "sessionchanger", "opened");

            // bootflagを設定
            SCGhostManager.SharedGhostManager().SetBootFlagInDefaults(s.GetGhostPath(), true);

            // OnGhostChanged若しくはOnFirstBoot発行
            if (s.HasBootedBefore())
            {
                s.DoShioriEvent("OnGhostChanged", new String[] { selfname_of_before, script_in_resp });
            }
            else
            {
                s.DoFirstBootEvent();
            }

            before = null;
           // NSAutoreleasePool.pop(pool);
        }

        private void postNotification(SCSession session, String name, String subinfo)
        {
            NSMutableDictionary userinfo = new NSMutableDictionary();
            userinfo.SetObjectForKey(session, "session");
            //NSNotificationCenter.defaultCenter().postNotification(name, subinfo, userinfo);
        }

        private void _open()
        {
            SCFoundation.SharedFoundation().OpenSession(nextPath, nextBalloonPath, scale);
        }


    }
}

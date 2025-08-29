using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace Ukagaka
{
    public sealed class SCSessionStarter
    {
        protected static SCSessionStarter _shared_instance = null;
        List<StarterThread> starters;

        public static SCSessionStarter SharedStarter()
        {
            if (_shared_instance == null)
            {
                _shared_instance = new SCSessionStarter();
            }
            return _shared_instance;
        }

        protected SCSessionStarter()
        {
            starters = new List<StarterThread>();
        }

        public void Start(String ghost_path, String balloon_path, double scale)
        {
            new StarterThread(ghost_path, balloon_path, scale).Start();
        }

        public bool IsThisGhostStartingNow(String ghost_path)
        {
            // 指定されたゴーストが現在起動処理中か否か。
            foreach (StarterThread starter in starters)
            {
                if (starter.getGhostPath().Equals(ghost_path))
                {
                    return true;
                }
            }
            return false;
        }

        protected class StarterThread
        {
            String ghost_path, balloon_path;
            double scale;
            Thread thread;
            public StarterThread(String ghost_path, String balloon_path, double scale)
            {
                this.ghost_path = ghost_path;
                this.balloon_path = balloon_path;
                this.scale = scale;
            }
            public void Start()
            {
                Run();

            }

            public void Run()
            {
                //   int pool = NSAutoreleasePool.push();
                SCSessionStarter.SharedStarter().starters.Add(this);

                postNotification("opening");

                SCSession s = SCFoundation.SharedFoundation().OpenSession(ghost_path, balloon_path, scale);
                if (s.HasBootedBefore())
                {
                    s.DoShioriEvent("OnBoot", new String[] { s.GetCurrentShell().GetShellName() });
                }
                else
                {
                    s.DoFirstBootEvent();
                }

                postNotification("done");

                SCSessionStarter.SharedStarter().starters.Remove(this);
                //  NSAutoreleasePool.pop(pool);
            }

            public String getGhostPath()
            {
                return ghost_path;
            }

            private void postNotification(String subinfo)
            {
                // NSMutableDictionary userinfo = new NSMutableDictionary();
                //  userinfo.setObjectForKey(ghost_path, "path");
                //   NSNotificationCenter.defaultCenter().postNotification("sessionstarter", subinfo, userinfo);
            }
        }

    }
}

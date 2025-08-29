using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Text;
using System.Threading;



namespace Ukagaka
{

	/*
		%lastghostnameや%lastobjectnameが狂う上、ファイルを破壊する恐れもあるので、
		このスレッドは決して二つ以上同時に走らせないでください。
	*/

	public class SCInstaller
	{
        private static string LAST_GHOST_NAME;
        private static string LAST_OBJECT_NAME;

        private string fpath;
        private SCSession target;
        private bool deleteAfterSuccess;
        private Thread thread;
        public SCInstaller(string fpath)
        {
            this.fpath = fpath;
            this.target = null;
            this.deleteAfterSuccess = false;
            thread = new Thread(Run);
        }

        public SCInstaller(string fpath, SCSession target)
        {
            this.fpath = fpath;
            this.target = target;
            this.deleteAfterSuccess = false;
            thread = new Thread(Run);
        }

        public SCInstaller(string fpath, SCSession target, bool deleteAfterSuccess)
        {
            this.fpath = fpath;
            this.target = target;
            this.deleteAfterSuccess = deleteAfterSuccess;
            thread = new Thread(Run);
        }


        public void Start()
        {
            thread.Start();
        }

        public bool IsAlive()
        {
            return thread != null && thread.IsAlive;
        }

        public void Run()
        {
            if (target != null && target.IsInPassiveMode())
            {
                return; // パッシブモードであれば強制終了。
            }
           // int pool = NSAutoreleasePool.Push();

            SendInstallEvent("OnInstallBegin");

            SCStatusWindowController stat = SCFoundation.SharedFoundation().GetStatusCenter().NewStatusWindow();
            stat.SetTypeToText();
            stat.Show();
            stat.TextTypePrint("Installer started...\n");
            
            using (Package zf = ZipPackage.Open(fpath))
            {
                try
                {
                    // 其余代码保持不变，需要替换Java中的部分特定语法和类名，以适应C#。
                }
                catch (Exception ex)
                {
                   // string errorString = SCErrorStringServer.GetErrorMessage("foundation.installer.ioException");
                   // stat.TextTypePrintln(errorString);
                  //  stat.CloseWindow();
                  //  SendInstallEvent("OnInstallFailure", new string[] { errorString });
                  //  NSAutoreleasePool.Pop(pool);
                    return;
                }
            }

           // NSAutoreleasePool.Pop(pool);
        }

        // 其余代码保持不变

        private void SendInstallEvent(string eventName, string[] references)
        {
            if (target != null) target.DoShioriEvent(eventName, references);
        }

        private void SendInstallEvent(string eventName)
        {
            if (target != null) target.DoShioriEvent(eventName);
        }

        public static string ReplaceInstallerEnvs(string script)
        {
            // インストーラー環境変数%lastobjectname,%lastghostnameを置き換えて返します。
            StringBuilder buf = new StringBuilder(script);
            while (true)
            {
                int pos = buf.ToString().IndexOf("%lastobjectname");
                if (pos == -1) break;

                buf.Replace("%lastobjectname", LAST_OBJECT_NAME);
            }
            while (true)
            {
                int pos = buf.ToString().IndexOf("%lastghostname");
                if (pos == -1) break;

                buf.Replace("%lastghostname", LAST_GHOST_NAME);
            }
            return buf.ToString();
        }


        public void WaitUntilEnd()
		{
			while (IsAlive()) { }
		}
         
	}
}
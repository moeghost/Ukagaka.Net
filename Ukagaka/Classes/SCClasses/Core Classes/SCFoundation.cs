using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Cocoa.AppKit;
using Utils;
using System.Threading;
using System.IO;
namespace Ukagaka
{
    public class SCFoundation
    {

        /**************** GLOBAL SWITCHES *****************/
        public static bool DEBUG = false;
        public static string FIXED_GHOST_TO_BOOT = null;//"/home/ghost/Taromati2";
        public static string FIXED_BALLOON_TO_BOOT = "/home/balloon/wiz";


        public static bool LOG_SHIORI_SESSION = false; // ここをtrueにすれば全てのSHIORIが標準出力にログを吐く。
        public static bool LOG_MAKOTO_SESSION = false; // ここをtrueにすれば全てのMAKOTOが標準出力にログを吐く。
        public static bool LOCK_SHIORI_EVENTS = false; // ここをtrueにすればSHIORIにイベントが全く送られない。
        public static bool LOAD_WHOLE_SURFACES_ON_BOOTING_SURFACE_SERVER = false; // ここをtrueにすればサーフィスサーバーが起動時に全てのサーフィスをロードする。
        public static bool STOP_SERIKO = false; // ここをtrueにすればSERIKOアニメーションが動作しない。（起動中のアニメーションは止まらない。）
        public static bool COMPRESS_SURFACES_ON_MEMORY = false; // ここをtrueにすればSCSurfaceが画像をメモリ上で可逆圧縮する。
        public static bool LOCK_BALLOONS = false; // ここをtrueにすれば決して何も喋らない。
        public static bool DO_NOT_BOOT_ANY_GHOSTS_AT_STARTUP = false; // ここをtrueにすればアプリケーションの起動時にゴーストを起動しない。
        public static bool SSTP_CONFLICT_IF_ANYONE_SPEAKING = true; // trueなら、SSTPで発話中のゴーストが一人でも居た場合に、SSTP SENDに409 Conflictを返す。
        public static bool DO_NOT_DETERMINE_GHOST_DETAIL = false; // trueなら、ゴーストマネージャが各ゴーストのSHIORIやMAKOTOを調べない。
        public static bool BALLOON_USES_CLICK_THROUGH = false; // trueなら、バルーンウインドウがクリックスルーを使う。
                                                               /**************************************************/

        /**************** GLOBAL CONSTANTS ****************/
        public static int HONTAI = 1;
        public static int SAKURA = 1;
        public static int UNYUU = 2;
        public static int KERO = 2;
        public static String STRING_FOR_SENDER = "Ukagaka"; // 栞や真琴へ渡すSenderヘッダ
                                                            /**************************************************/
                                                            /*
                                                            static {
                                                            if (DEBUG) {
                                                                Logger.setLogLevelLimit(Logger.DEBUG);
                                                                DO_NOT_BOOT_ANY_GHOSTS_AT_STARTUP = true;
                                                                DO_NOT_DETERMINE_GHOST_DETAIL = true;

                                                                Logger.log(null, Logger.NOTICE, "NiseRingo was built in DEBUG mode.");
                                                            }
                                                            else {
                                                                Logger.setLogLevelLimit(Logger.NOTICE);
                                                            }
                                                            }
                                                            */
                                                            // ライトモードで一時的に起動しているセッションの最大数。
                                                            // これを越えると、最も使用頻度の低いセッションが破棄される。
        protected static int TEMPORARY_GHOST_CACHE_SIZE = 10;

        private static SCFoundation _sharedInstance = null;

        List<SCSession> sessions; // 中身はSCSession。
        List<SCSession> temporary_sessions; // <SCSession>
        SCSSTPDaemon sstpDaemon;
        SCMailCheckDaemon mailCheckDaemon;
        SCTimeEventTriggerDaemon timeEventTriggerDaemon;
        SCBannerServer bannerServer;
        SCStatusWindowCenter statusCenter;
        SCInstaller lastExecutedInstaller; // 最後に起動したインストーラ。
        SCOnlineInstaller lastExecutedOI; // 最後に起動したオンラインインストーラ。
        SCUpdateDataMaker lastExecutedUDM; // 最後に起動したDAUメーカ。
        SCNarMaker lastExecutedNM; // 最後に起動したNARメーカ。
        SCSessionVanisher lastExecutedVanisher; // 最後に起動したVanishセッション。
        SCAppQuitter quitter; // これが起動して終了すればアプリケーションが終了させられる。
        SCPluginManager pluginManager;

        private volatile bool flagTerm = false;

        private static String[] ET_TAG_EXT =
        {
            "\u30df\u30ea\u79d2", // ミリ秒
	        "\u5e74", // 年
	        "\u30c1\u30a7\u30ad", // チェキ
	        "\u30b0\u30e9\u30e0", // グラム
        };

        // constructor and accessor
        public static SCFoundation SharedFoundation()
        {
            if (_sharedInstance == null)
            {

                _sharedInstance = new SCFoundation();
            }


            return _sharedInstance;
        }

        // public NSRunLoop mainthread_runloop;
        //public NSRunLoop getRunLoopOfMainThread()
        // {
        //      return mainthread_runloop;
        //  }

        private SCFoundation()
        {
            _sharedInstance = this;

            sessions = new List<SCSession>();
            temporary_sessions = new List<SCSession>();

             timeEventTriggerDaemon = new SCTimeEventTriggerDaemon();
             timeEventTriggerDaemon.Start();

             bannerServer = new SCBannerServer();
             bannerServer.Start();

             statusCenter = new SCStatusWindowCenter();
        }

        public void PerformQuit()
        {
            // 最終的な終了処理はメインスレッドで行われるので、
            // このメソッドはサブスレッドから呼んでも構いません。


            if (quitter != null && quitter.IsAlive())
            {
                return;
            }
             quitter = new SCAppQuitter();
             quitter.Start();
        }

        public static string GetVersion()
        {
            // TODO: Implement version retrieval
            return "1.0.0";
        }




        /*
        public static String getVersion()
        {
            return NSBundle.mainBundle().localizedStringForKey(
                "CFBundleShortVersionString",
                "\"pseudo apple\" [version info not found]",
                "InfoPlist"
                );
        }
        */
        public static String PseudoHoursFromStartTime()
        {

            int select = new Random().Next(ET_TAG_EXT.Length);
            int num = new Random().Next(1000);
            return num + ET_TAG_EXT[select];
        }

        public static void Debugstr(String s)
        {
            if (s != null) Console.WriteLine(s);
        }

        public void Install(String fpath)
        {
           if (lastExecutedInstaller != null && lastExecutedInstaller.IsAlive()) return;

            lastExecutedInstaller = new SCInstaller(fpath);
            lastExecutedInstaller.Start();
 
        }

        public void Install(string fpath, SCSession target)
        {
            if (lastExecutedInstaller != null && lastExecutedInstaller.IsAlive())
            {
                return;
            }
            lastExecutedInstaller = new SCInstaller(fpath, target);
            lastExecutedInstaller.Start();
        }

        public void Install(string fpath, SCSession target, bool deleteAfterSuccess)
        {
            if (lastExecutedInstaller != null && lastExecutedInstaller.IsAlive())
            {
                return;
            }
            lastExecutedInstaller = new SCInstaller(fpath, target, deleteAfterSuccess);
            lastExecutedInstaller.Start();
        }

        public void DownloadAndInstall(string url)
        {
            if (lastExecutedOI != null && lastExecutedOI.IsAlive())
            {
                return;
            }
            lastExecutedOI = new SCOnlineInstaller(url);
            lastExecutedOI.Start();
        }

        public void DownloadAndInstall(string url, SCSession target)
        {
            if (lastExecutedOI != null && lastExecutedOI.IsAlive())
            {
                return;
            }
            lastExecutedOI = new SCOnlineInstaller(url, target);
            lastExecutedOI.Start();
        }


        public void MakeDau(String dirpath)
        {
             if (lastExecutedUDM != null && lastExecutedUDM.IsAlive()) return;

          lastExecutedUDM = new SCUpdateDataMaker(dirpath);
             lastExecutedUDM.Start();
        }

        public void MakeDau(String dirpath, SCSession target)
        {
            if (lastExecutedUDM != null && lastExecutedUDM.IsAlive()) return;

            lastExecutedUDM = new SCUpdateDataMaker(dirpath, target);
             lastExecutedUDM.Start();
        }

        public void MakeNar(String dirpath)
        {
             if (lastExecutedNM != null && lastExecutedNM.IsAlive()) return;

              lastExecutedNM = new SCNarMaker(new Utils.File(dirpath));
             lastExecutedNM.Start();
        }

        public void MakeNar(String dirpath, SCSession target)
        {
         if (lastExecutedNM != null && lastExecutedNM.IsAlive()) return;

            lastExecutedNM = new SCNarMaker(new Utils.File(dirpath), target);
             lastExecutedNM.Start();
        }

        public void StartVanishSession(SCSession session)
        {
            if (lastExecutedVanisher != null && lastExecutedVanisher.IsAlive())
            {
                return;
            }
            lastExecutedVanisher = new SCSessionVanisher(session);
           lastExecutedVanisher.Start();
        }

        public bool IsThisInVanishEvent(SCSession session)
        {
            //return true;
            if (lastExecutedVanisher == null || !lastExecutedVanisher.IsAlive())
            {
                return false;
            }
            if (lastExecutedVanisher.GetTarget() != session)
            {
                return false;
            }
           return lastExecutedVanisher.IsInLastEvent();
        }

        public void HoldVanishSession()
        {
            // 最後のイベントの終了に間に合わなければ手遅れです。
            if (lastExecutedVanisher == null || !lastExecutedVanisher.IsAlive())
            {
                return;
            }
             lastExecutedVanisher.Hold();
        }

        // delegete methods
       

        public bool ApplicationOpenFile(String filename)
        {
            Utils.File f = new Utils.File(filename);
            if (f.IsDirectory() && SCUpdateDataMaker.HasDauFile(f))
            {
                MakeDau(filename);
            }
            else if (f.IsDirectory() && SCNarMaker.HasManifestFile(f))
            {
                MakeNar(filename);
            }
            else if (f.Exists() && (f.GetPath().ToLower().EndsWith(".zip") || f.GetPath().ToLower().EndsWith(".nar")))
            {
                Install(filename);
            }
            return true;
        }
         
        public static string GetParentDirOfBundle()
        {
            string parOfBundle = Environment.CurrentDirectory;

            if (parOfBundle.Contains("/"))
            {
             //   parOfBundle = parOfBundle.Substring(0, parOfBundle.LastIndexOf("/"));
            }
            return parOfBundle;
        }

        public void CheckAndMakeSystemDirs()
        {
            // 必要なディレクトリがもし無かったら作ります。
            String parOfBundle = GetParentDirOfBundle();

            String[] dirs = {
            "home",
            "home/ghost",
	        //"home/shell",
	        "home/balloon",
            "home/plugin",
            "home/nar",
            "home/saori",
            "home/_system",
            "home/_system/banner",
            "home/_system/updating_temp",
            "home/_system/headline_hash",
            "home/_system/headline_temp"
	        //"home/_system/updatechecker_db",
	        //"home/_system/updatechecker_temp"
	        };
            for (int i = 0; i < dirs.Length; i++)
            {
                Utils.File cur_dir = new Utils.File(parOfBundle, dirs[i]);
                if (!cur_dir.IsDirectory())
                {
                    cur_dir.Mkdir();
                }
            }
        }
        /*
        public void applicationDidFinishLaunching(NSNotification notification)
        {
            mainthread_runloop = NSRunLoop.currentRunLoop();
            Thread.currentThread().setName("main"); // このメソッドは常にメインスレッドから呼ばれるので名前を付けておく。

            bootcode();
        }
        */
        public static bool IsHereMainThread()
        {
           // return false;
            // このメソッドはメインスレッドから呼ばれるとtrueを返します。
             return Thread.CurrentThread.Name != null &&  Thread.CurrentThread.Name.Equals("main");
        }

        public async Task Bootcode()
        {
            // Create necessary system directories
            CheckAndMakeSystemDirs();

            // Convert old preference keys if needed
            ConvertKeysOfDefaults();

            // Load application settings
            var settings = Properties.Settings.Default;

            // Check for abnormal previous termination
            string quitCheckPath = Path.Combine(GetParentDirOfBundle(), "home/_system/quitcheck");
            if (System.IO.File.Exists(quitCheckPath))
            {
               // Logger.Info("Last time, NiseRingo exited abnormally.");
              //  Logger.Info("Auto-boot of ghosts is disabled.");
                SCGhostManager.SharedGhostManager().ClearBootFlags();
            }

            // Create quitcheck file to detect abnormal termination
            try
            {
                System.IO.File.WriteAllText(quitCheckPath, string.Empty);
            }
            catch (Exception ex)
            {
              //  Logger.Error("Failed to create quitcheck file", ex);
            }

            // Apply settings from configuration
            if (settings.DisplayBalloonWaitRate != 0)
            {
                SCScriptRunner.SetWaitRatio(settings.DisplayBalloonWaitRate);
            }

            if (settings.BootingConverterInverseType == true)
            {
              //  await SCOldTypeConverter.ConvertAllAsync();
            }

            if (settings.BootingConverterZeropointBalloon == true)
            {
              //  await SCOldBalloonConverter.ConvertAllAsync();
            }

            if (settings.BootingConverterThumbnail == true)
            {
                //await SCGhostThumbnailMover.MoveAllAsync();
            }

            // Set global flags from settings
           // SCFoundation.LOAD_WHOLE_SURFACES_ON_BOOTING_SURFACE_SERVER = true; 
             //   settings.MiscLoadWholeSurfacesOnBootingSurfaceServer == true;
            SCFoundation.STOP_SERIKO =
                settings.MiscDisableSerikoAnimation == true;
            SCFoundation.BALLOON_USES_CLICK_THROUGH =
                settings.DisplayBalloonClickthrough != false;

            // Initialize plugin manager
            pluginManager = new SCPluginManager();

            // Start services based on settings
            if (settings.SstpSwitch == true)
            {
                StartSSTPServer(settings.SstpPort);
            }

            if (settings.MailcheckCheckEnable == true &&
                settings.MailcheckCheckAutocheck == true)
            {
                StartMailCheckDaemon();
            }

            // Boot ghosts
            if (FIXED_GHOST_TO_BOOT != null && FIXED_GHOST_TO_BOOT != "")
            {
                 SCSessionStarter.SharedStarter().Start(FIXED_GHOST_TO_BOOT, FIXED_BALLOON_TO_BOOT, 1.0);
            }
            else if (!DO_NOT_BOOT_ANY_GHOSTS_AT_STARTUP)
            {
                 SCGhostManager.SharedGhostManager().BootAllGhostsToBoot();
            }
        }
        
        public void ApplicationWillTerminate(NSNotification notification)
        {
            // アプリケーションが終了する時に呼ばれます。
            bannerServer.Close();

            // インストーラーは中断出来ない。
            if (lastExecutedInstaller != null && lastExecutedInstaller.IsAlive())
            {
                lastExecutedInstaller.WaitUntilEnd();
            }
            // OIは中断出来ない。
            if (lastExecutedOI != null && lastExecutedOI.IsAlive())
            {
                lastExecutedOI.WaitUntilEnd();
            }
            // UDMは中断出来ない。
            if (lastExecutedUDM != null && lastExecutedUDM.IsAlive())
            {
                lastExecutedUDM.WaitUntilEnd();
            }
            // NMは中断出来ない。
            if (lastExecutedNM != null && lastExecutedNM.IsAlive())
            {
                lastExecutedNM.WaitUntilEnd();
            }

            // home/_system/quitcheckを削除。
            Utils.File quitcheck = new Utils.File(GetParentDirOfBundle(), "home/_system/quitcheck");
            quitcheck.Delete();
        }

        public void ApplicationDidBecomeActive(NSNotification notification)
        {
            // アプリケーションがフロントになると呼ばれます。
            // 全てのウインドウをフロントにします。
            //NSApplication.sharedApplication().arrangeInFront(null);
        }

        public void StartSSTPServer()
        {
            // SSTP鯖が起動していなかったら起動します。
            if (sstpDaemon == null || !sstpDaemon.IsAlive())
            {
                NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults();
                int port = defaults.IntegerForKey("sstp_port");
                if (port == 0) port = 11000;

                sstpDaemon = new SCSSTPDaemon(port);
                sstpDaemon.Start();
            }
        }

        public void StartSSTPServer(int port)
        {
            // SSTP鯖が起動していなかったら起動します。
            if (sstpDaemon == null || !sstpDaemon.IsAlive())
            {
                sstpDaemon = new SCSSTPDaemon(port);
                sstpDaemon.Start();
            }
        }

        public void StopSSTPServer()
        {
            // SSTP鯖が起動していたら停止します。
            if (sstpDaemon != null && sstpDaemon.IsAlive())
            {
                sstpDaemon.Terminate();
            }
        }

        public SCSSTPDaemon GetSSTPDaemon()
        {
            return sstpDaemon;
        }

        public SCBannerServer GetBannerServer()
        {
            return bannerServer;
        }
         
        public SCStatusWindowCenter GetStatusCenter()
        {
            return statusCenter;
        }

        public void StartMailCheckDaemon()
        {
            // オートメールチェックデーモンが起動していなかったら起動させます。
            // 起動していた場合は情報の更新のみ行います。
            if (mailCheckDaemon == null || !mailCheckDaemon.IsAlive())
            {
                mailCheckDaemon = new SCMailCheckDaemon();
                mailCheckDaemon.Start();
            }
            mailCheckDaemon.ReloadPref();
        }

        public void StopMailCheckDaemon()
        {
            // オートメールチェックデーモンが起動していたら終了させます。
            if (mailCheckDaemon != null && mailCheckDaemon.IsAlive())
            {
                mailCheckDaemon.Terminate();
            }
        }

        public SCBalloonSkinServer getBalloonServer(String balloon_path)
        {
            return GetBalloonServer(balloon_path, null);
        }

        public SCBalloonSkinServer GetBalloonServer(string balloonPath, SCStatusWindowController status)
        {
            /*
               This method returns the balloon skin server specified by the home/path,
               by some means.
               The caller does not need to consider how the balloon skin server returned by this method is obtained.

               This method first searches among all running sessions for a session that already has the specified balloon server,
               returns it if found, and creates a new one if not found.

               When a session ends, it loses one reference to the balloon server it was using.
               When all sessions using a particular balloon server have ended,
               the server becomes garbage and will be garbage-collected later.
             */

            foreach (var ses in sessions)
            {
                if (ses.GetBalloonServer().GetPath() == balloonPath)
                {
                    if (status != null)
                    {
                        status.TextTypePrint(
                            $"balloon \"%[color,white]{ses.GetBalloonServer().GetName()}%[color,default]\" was already loaded.\n");
                    }
                    return ses.GetBalloonServer();
                }
            }

            foreach (var ses in temporary_sessions)
            {
                if (ses.GetBalloonServer().GetPath() == balloonPath)
                {
                    if (status != null)
                    {
                        status.TextTypePrint(
                            $"balloon \"%[color,white]{ses.GetBalloonServer().GetName()}%[color,default]\" was already loaded.\n");
                    }
                    return ses.GetBalloonServer();
                }
            }

            // Not found
            SCBalloonSkinServer balloonServer;
            if (status != null)
            {
                status.TextTypePrint("%[color,orange]loading balloon... ");
                long startTime = SystemTimer.GetTimeTickCount();  //DateTimeOffset.Now.ToUnixTimeMilliseconds();
                balloonServer = new SCBalloonSkinServer(balloonPath);
                status.TextTypePrint(
                    $"%[color,white]done.({SystemTimer.GetTimeTickCount() - startTime}ms)%[color,default]\n");
                status.TextTypePrint($"balloon loaded: %[color,yellow]{balloonServer.GetName()}%[color,default]\n");
            }
            else
            {
                balloonServer = new SCBalloonSkinServer(balloonPath);
            }

            return balloonServer;

           // return null;
        }

        public SCSession OpenSession(String ghost_path, String balloon_path)
        {
            return OpenSession(ghost_path, balloon_path, 1.0);
        }

        public SCSession OpenSession(
        String ghost_path, String balloon_path, double scale)
        {

            // どちらもhome/から始まるパス。
            SCSession s = new SCSession(ghost_path, balloon_path);
            s.Start();
           
            s.ResizeShell(scale);
            sessions.Add(s);

         //   notifyOtherGhostNames();

            // temporary_sessionsに同じゴーストが入っていたら、それを破棄する。
            int n_temporaries = temporary_sessions.Count();
            for (int i = 0; i < n_temporaries; i++)
            {
                SCSession tmp = (SCSession)temporary_sessions.ElementAt(i);
                if (tmp.GetGhostDir().Equals(s.GetGhostDir()))
                {
                    temporary_sessions.RemoveAt(i);
                    break;
                }
            }

            return s;
        }

        public SCSession OpenSessionTemporarily(
        String ghost_path, String balloon_path, double scale)
        {

            return OpenSessionTemporarily(ghost_path, null, balloon_path, scale);
        }
        public SCSession OpenSessionTemporarily(
        String ghost_path, String shelldir_name,
        String balloon_path, double scale)
        {

            SCSession s = new SCSession(ghost_path, shelldir_name, balloon_path, true);
           // s.Start();
          //  s.resizeShell(scale);

            int n_temp_sessions = temporary_sessions.Count();
            if (n_temp_sessions >= TEMPORARY_GHOST_CACHE_SIZE)
            {
                // 最後のセッションを破棄
                SCSession last = (SCSession)temporary_sessions.ElementAt(n_temp_sessions - 1);
              //  Logger.log(this, Logger.DEBUG, "cache expired: {" + last + "}");

                temporary_sessions.RemoveAt(n_temp_sessions - 1);

                last.Close();

                last = null;
               // System.gc();
            }

            temporary_sessions.Insert(0, s);
            return s;
        }

        public void CloseSession(String ghost_path)
        {
            // ghost_pathはhome/から始まるパス。
            // 指定されたセッションを終了させます。
            SCSession s = GetSession(ghost_path);
            if (s == null) return;

        //    if (s.getStatus() != SCSession.CLOSED) s.close();
            sessions.Remove(s);
            //notifyOtherGhostNames();

           // System.gc(); // ここで明示的にガベージコレクタを走らせる。
        }

        public void CloseSession(SCSession s)
        {
            // セッションを終了させます。
        //    if (s.getStatus() != SCSession.CLOSED) s.close();

            sessions.Remove(s);

          //  notifyOtherGhostNames();
            //System.gc();
        }

        public SCSession GetSession()
        {
            // 起動中のセッションのうち、どれか一つを返します。
            // 一つも起動していなかったらnullを返します。
            if (sessions.Count == 0) return null;

            int session_idx = (new Random().Next(0,sessions.Count));
            return (SCSession)sessions.ElementAt(session_idx);
        }

        public SCSession GetSession(String ghostPath)
        {
            // 起動中のセッションの中から、ゴーストパスがghostPathであるセッションを探して返します。
            // 見つからなかったらnullを返します。
            SCSession result = null;
            int n_sessions = sessions.Count;
            for (int i = 0; i < n_sessions; i++)
            {
                SCSession s = (SCSession)sessions.ElementAt(i);
                if (s.GetGhostPath().Equals(ghostPath))
                {
                    result = s;
                    break;
                }
            }
            return result;
        }
        
        public SCSession GetSession(String name, bool wantAny)
        {
             
         // name:
        //  さくら,うにゅう
       //   さくら
       //   うにゅう
       //   等、両方が指定された場合は完全一致、片方のみの場合は前方一致で検索します。

        //  wantAnyがfalseの場合は、見つからなかったらnullを返します。
       //   trueの場合は、起動しているシェルをランダムで選びますが、一つも起動していなかったらnullを返します。
           
            SCSession result =
                FindSessionInVectorWithSignature(name, sessions);

            return (result == null && wantAny ? GetSession() : result);
        }

        protected static SCSession FindSessionInVectorWithSignature(
        String signature, List<SCSession> vec)
        {

            int n_sessions = vec.Count();
            SCSession result = null;

            bool both = (signature.IndexOf(',') != -1);
            for (int i = 0; i < n_sessions; i++)
            {
                SCSession session = (SCSession)vec.ElementAt(i);
                String id = session.GetCurrentIdentification();

                if (both)
                {
                    // 両方指定された。
                    if (id.Equals(signature))
                    {
                        result = session;
                        break;
                    }
                }
                else
                {
                    // 片方の指定
                    if (id.StartsWith(signature))
                    {
                        result = session;
                        break;
                    }
                }
            }

            return result;
        }

        public SCSession GetTemporarySession(String name)
        {
            return GetTemporarySession(name, true);
        }
        public SCSession GetTemporarySession(
        String name, bool boot_if_not_in_cache)
        {

            // まずはキャッシュ内を探す。
            SCSession s = FindTemporarySessionInCache(name);

            if (boot_if_not_in_cache)
            {
                // 無ければインストールされている中から探す。
                if (s == null)
                {
                    s = FindTemporarySessionInInstalled(name);
                }
            }

            return s;
        }
        protected SCSession FindTemporarySessionInCache(String name)
        {
            SCSession s =
                FindSessionInVectorWithSignature(name, temporary_sessions);
            if (s != null)
            {
                // これをキャッシュ内の先頭に移動。
                temporary_sessions.Remove(s);
                temporary_sessions.Insert(0, s);

                //Logger.log(this, Logger.DEBUG, "temporary cache hit: {" + s + "}");
            }
            return s;
        }
        protected SCSession FindTemporarySessionInInstalled(String name)
        {
            
           //   getSession(String,bool)と同じようにマッチングを行ないますが、
           //   その対象は起動中のゴーストではなく、インストールされているゴーストです。

           //   見付からなければnullを返します。
            
           List<SCInstalledGhostsList.ListsElement> installed_list =
                SCGhostManager.SharedGhostManager().GetInstalledGhostList();
            int n_installed = installed_list.Count;

            SCInstalledGhostsList.ListsElement found = null;
            String with_special_shell_dir = null;

            bool both = (name.IndexOf(',') != -1);
            for (int i = 0; i < n_installed; i++)
            {
                SCInstalledGhostsList.ListsElement le =
                (SCInstalledGhostsList.ListsElement)installed_list.ToArray().ElementAt(i);

                if (both)
                {
                    // 両方指定された。
                    if (name.Equals(le.GetIdentification()))
                    {
                        found = le;
                        break;
                    }
                }
                else
                {
                    // 片方の指定
                    if (le.GetIdentification().StartsWith(name))
                    {
                        found = le;
                        break;
                    }
                }

                String indep_shell = le.search_id_in_independent_shells(name);
                if (indep_shell != null)
                {
                    found = le;
                    with_special_shell_dir = indep_shell;
                    break;
                }
            }

            if (found == null)
            {
                return null;
            }
            else
            {
                return OpenSessionTemporarily(
                found.GetPath(), with_special_shell_dir, found.GetBalloonPath(), found.GetScale());
            }
        }

        public bool isAnyoneSpeakingNow()
        {
            // sessionsまたはtemporary_sessionsの中に、
            // \tで喋っている最中のセッションが一つでもあればtrueを返す。
            if (isAnyoneSpeakingNow(temporary_sessions))
            {
                return true;
            }
            else if (isAnyoneSpeakingNow(sessions))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        protected bool isAnyoneSpeakingNow(List<SCSession> vec)
        {
            int size = vec.Count();
            for (int i = 0; i < size; i++)
            {
                SCSession s = (SCSession)vec.ElementAt(i);
                if (s.IsScriptRunnerProtected())
                {
                    return true;
                }
            }
            return false;
        }
       
        public String GetAnyGhostPathInAsleep()
        {
            // 起動していないゴーストの中からランダムに選んでそのパスを返します。
            // 一つも無ければnullを返します。
            ArrayList ghostsInAsleep = SCGhostManager.SharedGhostManager().GetAllGhostPathsInAsleep();
            if (ghostsInAsleep.Count == 0) return null;

            int sel = (int)((new Random().Next() % ghostsInAsleep.Count));
            return (String)ghostsInAsleep.ToArray().ElementAt(sel);
        }
        
        public List<SCSession> GetSessionsList()
        {
            // 變更しない事。
            return sessions;
        }

        public int NumOfSessions()
        {
            return sessions.Count;
        }

        internal NSThread GetRunLoopOfMainThread()
        {

            return new NSThread(null);
            //throw new NotImplementedException();
        }
      
        public SCPluginManager GetPluginManager()
        {
           return pluginManager;
        }

        private void NotifyOtherGhostNames()
        {
            // Notify all running SHIORI instances about other ghosts
            lock (sessions)
            {
                foreach (SCSession receiver in sessions)
                {
                    SCSpirit receiverSpirit = receiver.GetMasterSpirit();
                    if (receiverSpirit?.GetShiori() == null) continue;

                    var message = new StringBuilder();

                    // Build message based on SHIORI protocol version
                    if (receiverSpirit.GetShioriProtocolVersion() == 2)
                    {
                        message.AppendLine($"NOTIFY OtherGhostName SHIORI/2.3");
                        message.AppendLine($"Sender: {SCFoundation.STRING_FOR_SENDER}");
                    }
                    else
                    {
                        message.AppendLine("NOTIFY SHIORI/3.0");
                        message.AppendLine($"Sender: {SCFoundation.STRING_FOR_SENDER}");
                        message.AppendLine("ID: otherghostname");
                    }

                    // Add information about other sessions
                    foreach (SCSession otherSession in sessions)
                    {
                        if (otherSession == receiver) continue; // Skip self

                        SCSpirit otherSpirit = otherSession.GetMasterSpirit();
                        if (receiverSpirit.GetShioriProtocolVersion() == 2)
                        {
                            message.AppendLine($"Ghost: {otherSpirit.GetSelfName()}");
                            message.AppendLine($"GhostEx: {otherSpirit.GetSelfName()}\u0001" +
                                              $"{otherSession.GetSakura().BaseSurfaceId()}\u0001" +
                                              $"{otherSession.GetKero().BaseSurfaceId()}");
                        }
                        else
                        {
                            message.AppendLine($"Reference: {otherSpirit.GetSelfName()}\u0001" +
                                             $"{otherSession.GetSakura().BaseSurfaceId()}\u0001" +
                                             $"{otherSession.GetKero().BaseSurfaceId()}");
                        }
                    }

                    message.AppendLine(); // End of message
                    receiver.ForwardToShioriKernel(message.ToString());
                }
            }
        }

        protected void ConvertKeysOfDefaults()
        {
            // Convert old preference keys from shell_pref[path] to ghost.pref.[path]
            var settings = Properties.Settings.Default;
            bool dirty = false;

            // Get all settings properties via reflection
            var properties = settings.GetType().GetProperties();
            foreach (var prop in properties)
            {
                if (prop.Name.StartsWith("shell_pref"))
                {
                    string newKey = "ghost.pref." + prop.Name.Substring(10);
                    try
                    {
                        var value = prop.GetValue(settings);
                        typeof(Properties.Settings)
                            .GetProperty(newKey)?
                            .SetValue(settings, value);

                        prop.SetValue(settings, null);
                        dirty = true;
                    }
                    catch (Exception ex)
                    {
                      //  Logger.Error($"Failed to convert setting {prop.Name}", ex);
                    }
                }
            }

            if (dirty)
            {
                settings.Save();
            }
        }

        public async Task DoMailCheckAsync()
        {
            SCSession anySession = GetSession();
            if (anySession == null || anySession.IsInPassiveMode()) return;

            var settings = Properties.Settings.Default;

            string server = settings.MailcheckServer;
            string username = settings.MailcheckUsername;
            string password = settings.MailcheckPassword;

            if (string.IsNullOrEmpty(server) ||
                string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password)) return;

            await anySession.DoShioriEvent("OnBIFFBegin", null);

            var result = await SCMailChecker.Pop3CheckAsync(server, username, password);

            switch (result.Status)
            {
                case MailCheckStatus.Success:
                    try
                    {
                        /*string[] parts = result.Data.Split(',');
                        if (parts.Length >= 2)
                        {
                            await anySession.DoShioriEventAsync("OnBIFFComplete",
                                new[] { parts[0], parts[1] });
                        }*/
                    }
                    catch (Exception ex)
                    {
                        //Logger.Error("Failed to process mail check results", ex);
                    }
                    break;

                case MailCheckStatus.CannotConnectToServer:
                case MailCheckStatus.ServerReturnedError:
                case MailCheckStatus.CommunicationError:
                    await anySession.DoShioriEventAsync("OnBIFFFailure", new[] { "kick" });
                    break;

                case MailCheckStatus.PasswordWasInvalid:
                    await anySession.DoShioriEventAsync("OnBIFFFailure", new[] { "defect" });
                    break;

                case MailCheckStatus.CommandStatError:
                    await anySession.DoShioriEventAsync("OnBIFFFailure", new[] { "kick" });
                    break;

                case MailCheckStatus.CommunicationTimedOut:
                    await anySession.DoShioriEventAsync("OnBIFFFailure", new[] { "timeout" });
                    break;

                case MailCheckStatus.UnexpectedError:
                    await anySession.DoShioriEventAsync("OnBIFFFailure", new[] { "kick" });
                    break;
            }
        }

    }
}

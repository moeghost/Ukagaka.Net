using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.IO;
using System.Collections;
using System.Threading;
using Cocoa.AppKit;
using Utils;
using System.Windows.Forms;
namespace Ukagaka
{
    public class SCSession
    {
        //public static  int MAX_QUEUE_SIZE = 20;

        public Thread thread;

        public static int UNAVAILABLE = 1;
        // データのロード中等の理由で、一時的にセッションにアクセスできない状態を表します。

        public static int CLOSED = 2;
        // セッションが既に閉じられた状態を表します。この状態ではアクセスしてはいけません。

        public static int AVAILABLE = 3;
        // セッションが起動中で、外部からのアクセスを許可している状態を表します。

        volatile bool flagClosing = false; // trueならセッションが閉じられようとしています。

        SCShell currentShell;
        SCSpirit masterSpirit;
        //Vector slaveSpirits; // 中身はSCSpirit。

        SCBalloonSkinServer balserver;
        SCShellWindowController hontai, unyuu;
        SCSafetyBalloonController hontai_b, unyuu_b;
        SCScriptRunner scrrunner;
        SCScriptServer tmpScriptServer; // SSTPの選択肢用。ランナーが一度終了する毎にsweepAway()されます。
        SCCommunicateBoxController comBox;
        SCVanishWindowController vanishDialog;
        SCShellContextMenuMaker shellContextMenuMaker;
        SCMenuAppearanceServer menuAppearance;
        SCShellChangingSession lastShellChangingSession; // 最後に起動されたシェル変更セッション。
        SCNetworkUpdater lastNetworkUpdater; // 最後に起動されたネットワークアップデータ。
        SCTeachSession lastTeachSession; // 最後に起動されたTeachセッション。
        SCInputBoxSession lastInputBoxSession; // 最後に起動されたINPUTセッション。
        SCSessionCloser closer; // これが起動して終了した時にはセッションが閉じられる。

        private List<String> queue; // SHIORIフォワードポートのキュー。中身はString。

        String currentSelfName;
        String currentKeroName;

        File ghost_dir;
        String ghost_path;
        File readmeFile;

        private int status = UNAVAILABLE;
        private long boottime;
        private bool passive_mode = false;

        bool light_mode; // ライトモードで起動したかどうか。
        bool special_shell_mode; // シェル指定モード(ライトモード専用)で起動したか。

        public SCSession(String ghost_path, String balloon_path) : this(ghost_path, null, balloon_path, false)
        {
            thread = new Thread(new ThreadStart(Run));
            //this(ghost_path, null, balloon_path, false);
        }
        public SCSession(String ghost_path, String balloon_path, bool light_mode) : this(ghost_path, null, balloon_path, light_mode)
        {
            //  this(ghost_path, null, balloon_path, light_mode);
        }
        public SCSession(
        String ghost_path,
        String shelldir_name,
        String balloon_path,
         bool light_mode)
        {
            // ghost_path : home/ghost/[ghostname]
            // shelldir_name: shell/下のディレクトリ名。nullなら設定されたシェルを使う。
            // light_mode : trueならライトモードで起動する。

            long boot_begin_time = SystemTimer.GetTimeTickCount();
            this.light_mode = light_mode;

            if (shelldir_name != null)
            {
                // シェル指定はライトモード専用。
                if (light_mode)
                {
                    special_shell_mode = true;
                }
                else
                {
                    //  Logger.log(
                    //    this, Logger.WARN,
                    //     "Special shell-dir was specified in constructor while light-mode is disabled. " +
                    //     "shell-dir will be ignored.");
                    shelldir_name = null;
                    special_shell_mode = false;
                }
            }
            else
            {
                special_shell_mode = false;
            }

            SCStatusWindowController stat = null;
            if (!SCFoundation.IsHereMainThread() && !light_mode)
            {
                 stat = SCFoundation.SharedFoundation().GetStatusCenter().NewStatusWindow();
                 stat.SetTypeToText();
                 stat.Show();
                //   stat.texttype_print("%[color,yellow]Materializing ghost...%[color,default]\n");
            }

            boottime = SystemTimer.GetTimeTickCount();
            this.ghost_path = ghost_path;
            ghost_dir = new File(SCFoundation.GetParentDirOfBundle(),ghost_path);

            NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults();
            NSDictionary ghostDefaults = SCGhostManager.SharedGhostManager().GetGhostDefaults(ghost_path);

            readmeFile = new File(ghost_dir,"readme.txt");

            // master spirit 起動
            masterSpirit = new SCSpirit(this, "master", stat);

            // 本体とうにゅう用のウインドウを作成
            hontai = new SCShellWindowController(this, SCFoundation.HONTAI);
            unyuu = new SCShellWindowController(this, SCFoundation.UNYUU);

            // shell 起動
            currentShell = new SCShell(
                this,
                shelldir_name != null ? shelldir_name : GetShellDirName(),
                stat);
            currentShell.Start();
           // ResetSurfaces();
            // 位置決定
           // hontai.SetHorizLoc(GetHorizontalLocationOfShell(currentShell.GetDirName(), SCFoundation.SAKURA));
           // unyuu.SetHorizLoc(GetHorizontalLocationOfShell(currentShell.GetDirName(), SCFoundation.KERO));

            // バルーンサーバー起動
            balserver = SCFoundation.SharedFoundation().GetBalloonServer(balloon_path, stat);
             
            // バルーン
            hontai_b = new SCSafetyBalloonController(this, SCFoundation.HONTAI);
            unyuu_b = new SCSafetyBalloonController(this, SCFoundation.UNYUU);
            hontai_b.SetBalloonServer(balserver);
            unyuu_b.SetBalloonServer(balserver);
            hontai.SetBalloon(hontai_b);
            unyuu.SetBalloon(unyuu_b);
            hontai_b.Show();
            unyuu_b.Show();
            
            // バルーン透明度
            double balloon_transparency = defaults.DoubleForKey("display_slider_transparency"); // 未定義ならゼロのまま。
            hontai_b.SetTransparency(balloon_transparency);
            unyuu_b.SetTransparency(balloon_transparency);

            // バルーンフェードアウト
            if (defaults.ObjectForKey("display.balloon.fadeout") == null)
            {
                hontai_b.SetDoesFadeOut(true); // デフォルトでフェードアウト実行
                unyuu_b.SetDoesFadeOut(true);
            }
            else
            {
                bool value = (defaults.IntegerForKey("display.balloon.fadeout") == 1);
                hontai_b.SetDoesFadeOut(value);
                unyuu_b.SetDoesFadeOut(value);
            }
            
            // 位置決定
            RecalcBalloonsLoc();
             
            // レベル
            int level = defaults.IntegerForKey("display_levelselect");
            // エントリが存在しなければ0が返る→NSWindow.NormalWindowLevelも0である。
            hontai.SetLevel(level);
            unyuu.SetLevel(level);
            hontai_b.SetLevel(level);
            unyuu_b.SetLevel(level);

            // フォント
            String fontname = (String)defaults.ObjectForKey("display_fontname");
            float fontsize = defaults.FloatForKey("display_fontsize");
            if (fontname == null)
            {
                fontname = "Osaka";
            }
            if (fontsize == 0)
            {
                fontsize = 9.0f;
            }
            hontai_b.RequestChangeFont(NSFont.FontWithNameAndSize(fontname, fontsize));
            unyuu_b.RequestChangeFont(NSFont.FontWithNameAndSize(fontname, fontsize));

            // 一時スクリプト(SSTP/1.2用)
            tmpScriptServer = new SCScriptServer();

            if (!light_mode)
            {
                // COMMUNICATE BOX ロード
                comBox = new SCCommunicateBoxController(this);

                // SHIORIカーネルにNOTIFY OwnerGhostNameを送信
                // この時点ではまだFORWARD PORTが起動していないのでforwardしてはいけない。
                if (masterSpirit.GetShioriProtocolVersion() == 2)
                {
                    masterSpirit.DoShioriSession(
                        "NOTIFY OwnerGhostName SHIORI/2.3\r\nSender: " +
                        SCFoundation.STRING_FOR_SENDER + "\r\nGhost: " +
                        masterSpirit.GetSelfName() + "\r\n\r\n");
                }

                // コンテクストメニュー Look&Feel
                menuAppearance = new SCMenuAppearanceServer(this);

                // コンテクストメニュー
                shellContextMenuMaker = new SCShellContextMenuMaker(this);
            }
            else
            {
                // コンテクストメニュー Look&Feel
                menuAppearance = new SCMenuAppearanceServer(this);

                // コンテクストメニュー
                shellContextMenuMaker = new SCShellContextMenuMaker(this);
            }


            System.Windows.Controls.ContextMenu menu = hontai.GetView().MenuForEvent(null);

            MainWindow.SharedMainWindow().SetMenuItemForShellSubMenu(menu);





            long elapsed_time_to_materialize = DateTime.Now.Ticks - boot_begin_time;
            if (stat != null)
            {
                stat.TextTypePrint(
                "%[color,orange]ghost \"%[color,white]" +
                masterSpirit.GetName() +
                "%[color,orange]\" materialized.\n");
                stat.TextTypePrint(
                "%[color,orange]" +
                (elapsed_time_to_materialize / 1000.0) +
                " sec elapsed.");
                stat.CloseWindow();
            }
            else
            {
                Console.WriteLine(
                (elapsed_time_to_materialize / 1000.0) +
                " sec elapsed to materialize.");
            }
 
            queue = new List<String>();
        }

        public void Start()
        {

            thread.Start();

        }

        /*
        public String ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("path: ").Append(ghost_path);

            buf.Append("; status: ");
            if (status == UNAVAILABLE)
            {
                buf.Append("unavailable");
            }
            else if (status == CLOSED)
            {
                buf.Append("closed");
            }
            else if (status == AVAILABLE)
            {
                buf.Append("available");
            }
            else
            {
                buf.Append("*unknown*");
            }

            buf.Append("; uptime: ").Append(
                System.currentTimeMillis() - boottime).Append("ms");

            if (flagClosing)
            {
                buf.Append("; closing");
            }
            if (passive_mode)
            {
                buf.Append("; passive");
            }
            if (light_mode)
            {
                buf.Append("; light");
            }
            if (special_shell_mode)
            {
                buf.Append("; special-shell");
            }

            return buf.ToString();
        }

        protected void ize()
        {
            Logger.log(this, Logger.DEBUG, "ized");
        }
        */
        private volatile bool flagTerm = false;


        public void Sleep(int value)
        {
            Thread.Sleep(value);

        }

        public void Run()
        {
            // int pool = NSAutoreleasePool.push();

            status = AVAILABLE;

            while (!flagTerm)
            {
                if (queue?.Count() > 0)
                {
                    String req = (String)queue.ElementAt(0); // 先頭から取り出す。
                    queue.RemoveAt(0);

                    // int inner_pool = NSAutoreleasePool.push();
                    ShioriResponceWork(masterSpirit.DoShioriSession(req));
                    //  NSAutoreleasePool.pop(inner_pool);
                }
                else
                {
                    if (flagTerm) break;

                    try
                    {
                        Sleep(30 * 1000); // 適当。キューにプッシュしたらinterrupt()する事。
                    }
                    catch (ThreadInterruptedException e)
                    {
                        if (flagTerm) break;
                    }
                }
            }

            //  NSAutoreleasePool.pop(pool);
        }

        public void ForwardToShioriKernel(String request)
        {
            // SHIORI/2.xリクエストをこのセッションにフォアードします。
            // 結果は返されません。

            //if (queue.Size() < MAX_QUEUE_SIZE) {
            //if (scrrunner == null ||
            //    !scrrunner.isAlive() ||
            //    scrrunner.interruptAllowed()) {
            // クリティカルセッション中はキューに溜めない。
            queue.Add(request);
            //    System.out.println("QUEUE: "+queue.Size());
            //}
            Interrupt();
        }

        public void Interrupt()
        {
            thread.Interrupt();

        }


        public void ClearQueue()
        {
            queue.Clear();
            //System.out.println("QUEUE CLEARED");
        }

        public bool IsLightMode()
        {
            return light_mode;
        }

        public bool IsInPassiveMode()
        {
            return passive_mode;
        }
        /*
        public void setPassiveMode(bool bool)
        {
            passive_mode = bool;

            if (passive_mode)
            {
                comBox.close(); // COMMUNICATE BOXを閉じる
                if (lastTeachSession != null && lastTeachSession.isAlive()) lastTeachSession.Terminate(); // TEACHセッションが起動中なら強制終了
            }

            NSMutableDictionary userinfo = new NSMutableDictionary();
            userinfo.setObjectForKey(this, "session");
            NSNotificationCenter.defaultCenter().postNotification("passivemode", passive_mode ? "enter" : "leave", userinfo);
        }
          */
        public int HoursFromBootTime()
        {
            // セッションが起動してからの経過時間を返します。
            return (int)Math.Round((double)(SystemTimer.GetTimeTickCount() - boottime) / (1000.0 * 60.0 * 60.0));
        }
      
        public void ChangeSurface(int id, bool hontai)
        {
            SCShellWindowController iwc = (hontai ? GetHontai() : GetUnyuu());

            if (iwc.CurrentSurfaceId() != id)
            {
                iwc.ChangeSurface(currentShell.GetSurfaceServer(), id);
                currentShell.GetSeriko().SurfaceChanged(id, hontai);

                DoShioriEvent(
                "OnSurfaceChange", new String[] {
            Integer.ToString(GetHontai().CurrentSurfaceId()),
            Integer.ToString(GetUnyuu().CurrentSurfaceId())});
            }
        }
       
        public void ChangeShell()
        {
            // カレントシェルを変更します。
            // 設定はdefaultsから読み込みます。

            if (special_shell_mode)
            {
                // シェル固定。
                return;
            }

            // 今現在シェル変更セッションが起動中なら何もせずに終了。
            if (lastShellChangingSession != null && lastShellChangingSession.IsAlive())
            {
                return;
            }
            lastShellChangingSession = new SCShellChangingSession(this);
            lastShellChangingSession.Start();
        }

        public void ChangeShell(SCShell newShell)
        {
            // カレントシェルを変更します。
            // このメソッドはSHIORIにイベントを送信せずにカレントシェルを変更するのみです。
            // 実際に変更したい時にはchangeShell()を使用してください。
            if (status != AVAILABLE)
            {
                return;
            }
            status = UNAVAILABLE;
            currentShell.Close();
            currentShell = newShell;
            currentShell.Start();
            
            status = AVAILABLE;
        }

        public void DoNetworkUpdate()
        {
            if (status != AVAILABLE) return;
            if (lastNetworkUpdater != null && lastNetworkUpdater.IsAlive())
            {
                return;
            }
            lastNetworkUpdater = new SCNetworkUpdater(this);
            lastNetworkUpdater.Start();
        }
       
        public object GetGhostDefaults(string entry)
        {
            // plist内のゴーストのdefaultsからデータを取得します。
            // 見つからなければnullを返します。
            Dictionary<string, object> defaults = new Dictionary<string, object>(); //SCGhostManager.sharedGhostManager().getGhostDefaults(ghost_path);

            if (defaults.ContainsKey(entry))
            {
                return defaults[entry];
            }

            return null;

            
        }
         
        public void SetGhostDefaults(String entry, Object obj)
        {
            // plist内のゴーストのdefaultsにデータを設定します。
            NSMutableDictionary defaults = SCGhostManager.SharedGhostManager().GetGhostDefaults(ghost_path);
            defaults.SetObjectForKey(obj, entry);
            SCGhostManager.SharedGhostManager().SetGhostDefaults(ghost_path, defaults);
        }
         

        public async Task DoFirstBootEventAsync()
        {
             DoFirstBootEvent();
        }


        public void DoFirstBootEvent()
        {
            // OnFirstBootイベントを発行します。
            Dictionary<string, object> defaults = new Dictionary<string, object>(); //SCGhostManager.sharedGhostManager().getGhostDefaults(ghost_path);
            if (defaults.ContainsKey("vanish_count") == false)
            {
                return;
            }
            object vanish_count = defaults["vanish_count"];
            if (vanish_count is int)
            {
                DoShioriEvent("OnFirstBoot", new string[] { Integer.ToString(vanish_count == null ? 0 : ((int)vanish_count)) });
            }
            else
            {
                DoShioriEvent("OnFirstBoot", new string[] { Integer.ToString(vanish_count == null ? 0 : ((int)vanish_count)) });
            }
        }
       
        public void MouseEventOccuredOnShell(string type, string x, string y, string wheel, string owner, string collision)
        {
            // SCShellWindow専用。

            //if (type.equals("OnMouseDoubleClick")) {
            // ダブルクリックならキューをクリア。
            //clearQueue();
            //}

            if (SCFoundation.SharedFoundation().IsThisInVanishEvent(this) && type.Equals("OnMouseDoubleClick"))
            {
                // 引き止める。
                SCFoundation.SharedFoundation().HoldVanishSession();
                ClearQueue();
                ForceTerminateScriptRunnder();

                DoShioriEvent("OnVanishButtonHold");
            }
            else
            {
                DoShioriEvent(type, new string[] { x, y, wheel, owner, collision });
            }
        }
         
        public bool HasBootedBefore()
        {
            // このゴーストが過去に一度でも起動したことが有ればtrueを返します。
            String flag = (String)GetGhostDefaults("has_booted_before");
            return (flag != null && flag.Equals("true"));
        }
        
        public void DoTeach()
        {
            if (status != AVAILABLE)
            {
                return;
            }
            if (IsInPassiveMode())
            {
                return;
            }
            if (lastTeachSession != null && lastTeachSession.IsAlive()) return;

            lastTeachSession = new SCTeachSession(this);
            lastTeachSession.Start();
        }

        public void startInputBoxSession(String symbol, int timeout)
        {
            if (status != AVAILABLE)
            {
                return;
            }
            if (IsInPassiveMode())
            {
                return;
            }
            if (lastInputBoxSession != null && lastInputBoxSession.IsAlive()) return;

            lastInputBoxSession = new SCInputBoxSession(this, symbol, timeout);
            lastInputBoxSession.Start();
        }

        public void DoHLSensing(SCHLSensorPlugin plugin)
        {
            SCHeadlineSession s = new SCHeadlineSession(this, plugin);
            s.Start();
        }

        public void OpenVanishDialog()
        {
            if (status != AVAILABLE) return;
            if (vanishDialog != null && vanishDialog.Window().IsVisible)
            {
                return;
            }
            if (vanishDialog == null)
            {
                vanishDialog = new SCVanishWindowController(this);
            }
            vanishDialog.Show();
        }

        public SCScriptRunner RunScript(String script, bool useMakoto)
        {
            return RunScript(script, null, useMakoto);
        }
        public SCScriptRunner RunScript(String script, Hashtable options, bool useMakoto)
        {
            // まずSHIORIでトランスレートします。
            // useMakotoがfalseならそのまま、そうでなければmakotoでもトランスレートします。
            // ランナーが起動中でインタラプトが許可されていなければnullを返します。
            // アプリケーションが隠されている状態では常に喋りません。
            if (script == null || script.Length == 0)
            {
                return null;
            }
            if (NSApplication.SharedApplication.IsHidden())
            {
                return null;

            }
            String s = masterSpirit.TranslateWithShiori(script);
            if (useMakoto) s = masterSpirit.TranslateWithMakoto(s);

            if (scrrunner == null || !scrrunner.IsAlive())
            {
                scrrunner = new SCScriptRunner(this, s, options);
                scrrunner.Start();
                return scrrunner;
            }
            else if (scrrunner.InterruptAllowed())
            {
                scrrunner.Terminate();

                scrrunner = new SCScriptRunner(this, s, options);
                scrrunner.Start();
                return scrrunner;
            }
            else
            {
                return null;
            }
        }
         
        public void ForceTerminateScriptRunnder()
        {
            // スクリプトランナー実行中に、タイムクリティカルセッションに入っていようと
            // 選択肢があろうと、強制的にterminate()を実行します。
           if (scrrunner != null && scrrunner.IsAlive())
            {
                 scrrunner.Terminate();
            }
        }
         
        public void DoShioriRandomTalk()
        {
            if (masterSpirit.GetShioriProtocolVersion() == 2)
            {
               ForwardToShioriKernel("GET Sentence SHIORI/2.0\r\nSender: " + SCFoundation.STRING_FOR_SENDER + "\r\n\r\n");
            }
        }

           

        public async Task DoShioriEventAsync(String eventType)
        {
            await DoShioriEvent(eventType);
        }

        public async Task DoShioriEventAsync(String eventType, String[] references)
        {
            await DoShioriEvent(eventType,references);
        }




        public async Task DoShioriEvent(String eventType)
        {
            DoShioriEvent(eventType, null);
        }

        public async Task DoShioriEvent(String eventType, String[] references)
        {
            // SHIORIにイベントを送ります。
            if (SCFoundation.LOCK_SHIORI_EVENTS)
            {
                return;
            }
            if (status != AVAILABLE)
            {
                return;
            }
            if (masterSpirit.GetShiori() == null)
            {
                return; // SHIORIが起動していなければ何もしない。
            }
            // まずはリクエストを作成。
            StringBuilder sb = new StringBuilder();
            if (masterSpirit.GetShioriProtocolVersion() == 2)
            {
                sb.Append("GET Sentence SHIORI/2.0\r\nSender: " + SCFoundation.STRING_FOR_SENDER + "\r\n");
                sb.Append("SecurityLevel: local\r\n");
                sb.Append("Event: " + eventType + "\r\n");
            }
            else
            {
                sb.Append("GET SHIORI/3.0\r\nSender: " + SCFoundation.STRING_FOR_SENDER + "\r\n");
                sb.Append("ID: " + eventType + "\r\n");
            }

            if (references != null)
            {
                for (int i = 0; i < references.Length; i++)
                {
                    sb.Append("Reference" + i + ": " + references[i] + "\r\n");
                }
            }
            sb.Append("\r\n"); // リクエスト完成。

            ForwardToShioriKernel(sb.ToString());
        }

        public void DoShioriCommunicateFromUser(String message)
        {
            // SHIORIにSender: UserでSentence付きGET Sentenceを送ります。
            if (masterSpirit.GetShioriProtocolVersion() == 2)
                ForwardToShioriKernel("GET Sentence SHIORI/2.0\r\nSender: User\r\nSentence: " + message + "\r\n\r\n");
            else
                ForwardToShioriKernel("GET SHIORI/3.0\r\nSender: " + SCFoundation.STRING_FOR_SENDER + "\r\nID: OnCommunicate\r\nReference0: User\r\nReference1: " + message + "\r\n\r\n");
        }

        public void ShioriResponceWork(SCShioriSessionResponce resp)
        {
            ShioriResponceWork(resp, true);
        }

        public void ShioriResponceWork(SCShioriSessionResponce resp, bool wait)
        {
            // 与 Java 一样的逻辑：仅由 SHIORI FORWARD PORT、SCShellChangingSession、SCTeachSession 调用
            if (status != AVAILABLE)
            {
                return;
            }
            if (resp == null)
            {
                return;
            }
            if (masterSpirit.GetShiori() == null)
            {
                return;
            }
            string header = resp.GetHeader();
            if (!header.Contains("200 OK") &&
                !header.Contains("311 Not Enough") &&
                !header.Contains("312 Advice")) return;

            string script = header.StartsWith("SHIORI/2.")
                ? resp.GetRespForKey("Sentence")
                : resp.GetRespForKey("Value");

            SCScriptRunner runner = RunScript(script, true);
            if (runner == null) return;

            // 等待脚本执行结束
            while (runner.IsAlive() &&
                   runner.GetStatus() != SCScriptRunner.DONE &&
                   runner.GetStatus() != SCScriptRunner.TERMINATED)
            {
                try
                {
                    Thread.Sleep((int)(0.25 * 1000)); // 每0.25秒检查一次状态
                }
                catch (ThreadInterruptedException)
                {
                    if (flagTerm) break;
                }
            }

            if (runner.GetStatus() == SCScriptRunner.TERMINATED)
            {
                // 被强制结束直接退出
                return;
            }

            if (!wait)
            {
                // 至少等待 0.5 秒，避免过快
                try
                {
                    Thread.Sleep(500);
                }
                catch { }
            }

            // 转发通信逻辑
            string to, age, sender;
            if (header.StartsWith("SHIORI/2."))
            {
                to = resp.GetRespForKey("To");
                age = resp.GetRespForKey("Age");
                sender = resp.GetRespForKey("Sender");
            }
            else
            {
                to = resp.GetRespForKey("Reference0");
                age = null;
                sender = masterSpirit.GetSelfName();
            }

            if (to != null)
            {
                SCSession destSession = SCFoundation.SharedFoundation().GetSession(to, false);
                if (destSession != null && destSession != this)
                {
                    var buf = new System.Text.StringBuilder();
                    if (destSession.GetMasterSpirit().GetShioriProtocolVersion() == 2)
                    {
                        // SHIORI 2.x
                        buf.Append("GET Sentence SHIORI/2.3\r\n");
                        buf.Append("Sender: " + sender + "\r\n");
                        buf.Append("Age: " + age + "\r\n");
                        buf.Append("Surface: " + hontai.BaseSurfaceId() + "," + unyuu.BaseSurfaceId() + "\r\n");
                        buf.Append("Sentence: " + script + "\r\n");
                        for (int i = 0; i <= 7; i++)
                        {
                            string refStr = resp.GetRespForKey("Reference" + i);
                            if (refStr != null)
                                buf.Append("Reference" + i + ": " + refStr + "\r\n");
                        }
                        buf.Append("\r\n");
                    }
                    else
                    {
                        // SHIORI 3.0
                        buf.Append("GET SHIORI/3.0\r\n");
                        buf.Append("Sender: " + SCFoundation.STRING_FOR_SENDER + "\r\n");
                        buf.Append("ID: OnCommunicate\r\n");
                        buf.Append("Reference0: " + sender + "\r\n");
                        buf.Append("Reference1: " + script + "\r\n");
                        buf.Append("\r\n");
                    }
                    destSession.ForwardToShioriKernel(buf.ToString());
                }
            }
        }

        public String GetWordFromShiori(String type)
        {
            // typeに指定するのは￥ms,￥mz等の形式。
            // ￥はバックスラッシュではなく、\u00a5で指定して下さい。
            // 何らかの理由で単語が取得出来なければnullを返します。
            if (masterSpirit.GetShioriProtocolVersion() != 2) return null; // SHIORI/3.0では廃止された。

            SCShioriSessionResponce resp = masterSpirit.DoShioriSession(
                    "GET Word SHIORI/2.0\r\nSender: " + SCFoundation.STRING_FOR_SENDER + "\r\nType: " + type + "\r\n\r\n");
            if (resp == null) return null;
            if (resp.GetHeader().IndexOf("200 OK") == -1) return null;

            return (String)resp.GetResponce()["Word"];
        }

        public String GetStringFromShiori(String id)
        {
            // SHIORIにGET Stringを発行します。
            // 取得に失敗したらnullを返します。
            return masterSpirit.GetStringFromShiori(id);
        }

        public void ResetSurfaces()
        {
            ChangeSurface(-1, true);
            ChangeSurface(-1, false);

            ChangeSurface(0, true);
            ChangeSurface(10, false);
        }

        public void HideBalloons()
        {
           // hontai_b.hide();
         //   unyuu_b.hide();
        }

        public void ShowCommunicateBox()
        {
            if (IsInPassiveMode()) return;
            //comBox.show();
        }

        public void SetStatusClosing()
        {
            // SCSessionChanger,SCSessionVanisher以外は決して呼んではなりません。


            //  本当はこんなことをせずにSCSessionChangerをセッション権限にすることも出来るが、
            //   しかしSCSessionChangerは所有者であるセッションを終了してしまうばかりか
            //   他のセッションを起動するという動作を行うので、セッション権限にすることは不適当である。
            //   ならばSCSessionChangerがNotificationを発行するという手もあるが、observerの登録/解除が面倒である。

            flagClosing = true;

            // 初回起動時処理
            if (!HasBootedBefore())
            {
                //setGhostDefaults("has_booted_before", "true");
            }
        }

        public void ClearStatusClosing()
        {
            // SCSessionChanger,SCSessionVanisher以外は決して呼んではなりません。
            flagClosing = false;
        }

        public bool IsStatusClosing()
        {
            return flagClosing;
        }

        public void PerformClose()
        {
            // OnCloseを発行して喋り終わってからセッションを閉じます。
           // if (closer != null && closer.IsAlive())
            {
            //    return;
            }
            // 初回起動時処理
            if (!HasBootedBefore())
            {
                 SetGhostDefaults("has_booted_before", "true");
            }

            flagClosing = true;
           // closer = new SCSessionCloser(this);
           // closer.Start();
        }

        public void Close()
        {
            // カーネルを終了させて全てのウインドウを閉じます。
            if (status == CLOSED)
            {
                return;
            }
            status = CLOSED;
            /*
                        flagTerm = true; interrupt(); while (isAlive()) { }
                        masterSpirit.close(); masterSpirit = null;
                        currentShell.close(); currentShell = null;
                        balserver = null;
                        if (scrrunner != null && scrrunner.isAlive())
                        {
                            scrrunner.Terminate();
                        }
                        scrrunner = null;
                        tmpScriptServer = null;
                        if (comBox != null)
                        {
                            comBox.close();
                        }
                        comBox = null;
                        if (vanishDialog != null)
                        {
                            vanishDialog.close();
                            vanishDialog = null;
                        }
                        menuAppearance = null;
                        if (lastShellChangingSession != null && lastShellChangingSession.isAlive())
                        {
                            lastShellChangingSession.Terminate();
                        }
                        lastShellChangingSession = null;
                        if (lastNetworkUpdater != null && lastNetworkUpdater.isAlive())
                        {
                            lastNetworkUpdater.Terminate();
                        }
                        lastNetworkUpdater = null;
                        if (lastTeachSession != null && lastTeachSession.isAlive())
                        {
                            lastTeachSession.Terminate();
                        }
                        lastTeachSession = null;
                        if (lastInputBoxSession != null && lastInputBoxSession.isAlive())
                        {
                            lastInputBoxSession.Terminate();
                        }
                        lastInputBoxSession = null;
                        hontai.close(); hontai = null;
                        unyuu.close(); hontai = null;
                        hontai_b.close(); hontai_b = null;
                        unyuu_b.close(); unyuu_b = null;
                        // SCSessionCloserはここでは放って置く。
                        queue.clear();
                        */
        }
        /*
        public int getStatus()
        {
            return status;
        }
        */
        public void RecalcBalloonsLoc()
        {
            // バルーンの位置を再計算して移動させます。
            int x, y;
            int screenBottom = (int)NSScreen.MainScreen.VisibleFrame.Y().Value;
             
            hontai_b.CheckWentOut();
            x = SimulateHorizLoc(true, hontai_b.IsAtLeft());
            y = (int)hontai.Height() - currentShell.GetDescManager().GetIntValue("sakura.balloon.offsety"); // MacOS Xの座標系は一般的なPCのそれとY軸が逆だから。

            if (y < (int)hontai_b.Height() + screenBottom) y = (int)hontai_b.Height() + screenBottom; // 下にはみ出さないように。
            hontai_b.SetLoc(new NSPoint(x, y));

            unyuu_b.CheckWentOut();
            x = SimulateHorizLoc(false, unyuu_b.IsAtLeft());
            y = (int)unyuu.Height() - currentShell.GetDescManager().GetIntValue("kero.balloon.offsety");
            if (y < (int)unyuu_b.Height() + screenBottom) y = (int)unyuu_b.Height() + screenBottom;
            unyuu_b.SetLoc(new NSPoint(x, y)); 
        }
        
        public int SimulateHorizLoc(bool isHontai, bool left)
        {
            // バルーンを左に置いた場合（true）または右に置いた場合（false）での
            // バルーンのx座標を計算して返します。
            if (isHontai)
            {
                if (left)
                    return (int)hontai.X() - (int)hontai_b.Width() + currentShell.GetDescManager().GetIntValue("sakura.balloon.offsetx");
                else
                    return (int)hontai.X() + (int)hontai.Width() - currentShell.GetDescManager().GetIntValue("sakura.balloon.offsetx");
            }
            else
            {
                if (left)
                    return (int)unyuu.X() - (int)unyuu_b.Width() + currentShell.GetDescManager().GetIntValue("kero.balloon.offsetx");
                else
                    return (int)unyuu.X() + (int)unyuu.Width() - currentShell.GetDescManager().GetIntValue("kero.balloon.offsetx");
            }
        }

        public void RestartBalloonSkinServer(String balloon_path)
        {
            // balloon_path : home/balloon/...
            SCStatusWindowController stat = null;
            if (!SCFoundation.IsHereMainThread())
            {
                stat = SCFoundation.SharedFoundation().GetStatusCenter().NewStatusWindow();
                stat.SetTypeToText();
                stat.Show();
                stat.TextTypePrint("restarting balloon server...\n");
            }

            NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults();

            // バルーンスキンサーバーを再起動して、二つのバルーンにsetBalloonServerを送ります。
            balserver = SCFoundation.SharedFoundation().GetBalloonServer(balloon_path, stat);
            hontai_b.SetBalloonServer(balserver);
            unyuu_b.SetBalloonServer(balserver);

            RecalcBalloonsLoc();

            if (stat != null)
            {
                stat.TextTypePrint("%[color,white]restarted.%[color,default]");
                stat.CloseWindow();
            }
        }
         
        public void ResizeShell(double ratio)
        {
            currentShell.GetSurfaceServer().resizeSurfaces(ratio);

            hontai.SurfaceResized(currentShell.GetSurfaceServer(), ratio);
            unyuu.SurfaceResized(currentShell.GetSurfaceServer(), ratio);

            RecalcBalloonsLoc();
        }
         
        public bool IsScriptRunnerProtected()
        {
            // もし現在スクリプトランナーが起動していて、しかもタイムクリティカルセッション中だったら、
            // 動作変更不可としてtrueを返します。

            if (scrrunner != null)
            {
                return (scrrunner.IsAlive() && !scrrunner.InterruptAllowed());
            }

            return false;
        }
         
        public bool CheckMikire()
        {
            // 本体側サーフィスが画面から半分以上はみ出していたらtrueを返します。
            if (hontai == null)
            {
                return false;
            }
            int screenWidth = (int)NSScreen.MainScreen.Width;
            return (hontai.X() < hontai.Width() / 2 || hontai.X() + hontai.Width() - screenWidth > hontai.Width() / 2);
        }

        public bool CheckKasanari()
        {
            // 重なり部分の幅が本体又はうにゅうの小さい側の80%を越えていたらtrueを返します。
            if (hontai == null || unyuu == null) return false;

            float slimmer_width = (hontai.Width() > unyuu.Width() ? unyuu.Width() : hontai.Width());
            NSRect intersection = hontai.Frame().RectByIntersectingRect(unyuu.Frame());
            if (intersection == NSRect.ZeroRect) return false;

            return (intersection.Width().Value > slimmer_width * (80.0 / 100.0));
        }

        public void ForceMovingNextToAnother(int type)
        {
            // typeで指定されたサーフィスをもう一方の隣の位置まで強制移動する。
            // ￥4及び￥5の機能。というか何故この二つは別れているのだろう。
            SCShellWindowController target = (type == SCFoundation.HONTAI ? hontai : unyuu);
            SCShellWindowController against = (type == SCFoundation.HONTAI ? unyuu : hontai);

            // とりあえず左に動かしてみて、見切れるなら右に移動。
            float x = against.X() - target.Width();
            if (x < target.Width() / 2)
            {
                x = against.X() + against.Width();
            }
            target.SetHorizLoc(x);

            // 移動を確定
            ShellWindowMoved(target);
        }

        public void ShellWindowMoved(SCShellWindowController window)
        {
            // シェルウインドウのどちらかがドラッグされた時に呼ばれます。
            //String entry_name = (window == hontai ? "shell_window hontai x" : "shell_window unyuu x");
            String entry_name = "shell_window." + currentShell.GetDirName() + "." + (window == hontai ? "sakura" : "kero") + ".x";

            SCGhostManager ghostManager = SCGhostManager.SharedGhostManager();
            NSMutableDictionary ghostDefaults = ghostManager.GetGhostDefaults(GetGhostPath());

            ghostDefaults.SetObjectForKey((int)window.X(), entry_name);

            ghostManager.SetGhostDefaults(GetGhostPath(), ghostDefaults);
        }

        public String GetGhostMasterDirPath()
        {
            return "";

            //FileInfo ghost_root = new FileInfo(SCFoundation.GetParentDirOfBundle(),ghost_path);
            //FileInfo f = new FileInfo(ghost_root, "ghost/master");
            //return f.GetPath();
        }
         
        public String GetShellDirName()
        {
            NSMutableDictionary ghostDefaults = SCGhostManager.SharedGhostManager().GetGhostDefaults(ghost_path);

            String shelldir = null;

            shelldir = (String)ghostDefaults.ObjectForKey("shell_dirname");
            File ghost_root = new File(SCFoundation.GetParentDirOfBundle(), ghost_path);


            if (shelldir == null || shelldir.Length == 0)
            {
                File master = new File(ghost_root.GetPath(),"shell/master");
                if (master.IsDirectory())
                {
                    return "master";
                }
                else
                { // shell/masterが無いので適当な物を一つ選ぶ。
                    File shell = new File(ghost_root.GetPath(), "shell");
                    string[] shells = shell.List();//Directory.GetFileSystemEntries(shell.FullName);

                    int choice;
                    while (true)
                    {
                        choice = new Random().Next(0, shells.Length);
                        File fi = new File(shells[choice]);

                        if (fi.IsDirectory())
                        {
                            break;
                        }
                    }
                    return shells[choice];
                }
            }
            else
            {
                File dir = new File(ghost_root,"shell/" + shelldir);
                if (!dir.IsDirectory())
                {
                    File master = new File(ghost_root,"shell/master");
                    if (master.IsDirectory())
                    {
                        return "master";
                    }
                    else
                    { // shell/masterが無いので適当な物を一つ選ぶ。

                        File shell = new File(ghost_root, "shell");
                        string[] shells = shell.List();//Directory.GetFileSystemEntries(shell.FullName);

                        int choice;
                        while (true)
                        {
                            choice = new Random().Next(0, shells.Length);
                            File fi = new File(shells[choice]);

                            if (fi.IsDirectory())
                            {
                                break;
                            }
                        }
                        return shells[choice];
                    }
                }
                else
                {
                    return shelldir;
                }
            }
        }
       
        public int GetHorizontalLocationOfShell(String shellDirName, int type)
        {
            // shellDirName: 水平位置を取得したいシェルのディレクトリ名
            // type: SCFoundation.SAKURA or SCFoundation.KERO
            // このメソッドはdefaultsから「shell_window.${shellDirName}.{sakura|kero}.x」を読みますが、
            // それが見付からなければ「shell_window {hontai|kero} x」を、
            // それも無ければ現在のスクリーンの幅から算出した値を返します。
            NSMutableDictionary ghostDefaults = SCGhostManager.SharedGhostManager().GetGhostDefaults(ghost_path);
            Object int_x = ghostDefaults.ObjectForKey("shell_window." + shellDirName + "." + (type == SCFoundation.SAKURA ? "sakura" : "kero") + ".x");
            if (int_x != null)
            {
                return (int_x is Integer ? Integer.ParseInt(int_x.ToString()): (int)long.Parse(int_x.ToString()));
            }
            else if (type == SCFoundation.SAKURA)
            {
                return (int)(NSScreen.MainScreen.Frame.Width().Value - hontai.Width());
            }
            else
            {
                return (int)(GetHorizontalLocationOfShell(shellDirName, SCFoundation.SAKURA) - unyuu.Width()); // 再帰
            }
        }

        public void ChangeToOtherGhostAtRandom()
        {
            // ランダムに他のゴーストと替わる。
            // 他に一つも無ければただ終了する。
            String nextPath = SCFoundation.SharedFoundation().GetAnyGhostPathInAsleep();
            if (nextPath == null)
            {
                SCGhostManager.SharedGhostManager().forceHaltGhost(this);
            }
            else
            {
                String nextBalloonPath = SCGhostManager.SharedGhostManager().GetBalloonPathOfGhost(nextPath);
                double nextScale = SCGhostManager.SharedGhostManager().GetScaleOfGhost(nextPath);

                SCSessionChanger sc = new SCSessionChanger(this, nextPath, nextBalloonPath, nextScale);
                sc.Start();
            }
        }
        
        public bool IsShellChangingSessionRunningNow()
        {
            return (lastShellChangingSession != null && lastShellChangingSession.IsAlive());
        }

        public bool IsNetworkUpdaterRunningNow()
        {
            return (lastNetworkUpdater != null && lastNetworkUpdater.IsAlive());
        }
        
        // public bool isSessionCloserRunningNow()
        // {
        //     return (closer != null && closer.isAlive());
        // }

        public SCScriptServer GetTempScriptServer()
        {
            return tmpScriptServer;
        }

        public SCSafetyBalloonController GetHontaiBalloon()
        {
            return hontai_b;
        }
        public SCSafetyBalloonController GetSakuraBalloon()
        {
            return hontai_b;
        }

        public SCSafetyBalloonController GetUnyuuBalloon()
        {
            return unyuu_b;
        }
        public SCSafetyBalloonController GetKeroBalloon()
        {
            return unyuu_b;
        }

        public SCShellWindowController GetSakura()
        {
            return hontai;
        }
        public SCShellWindowController GetHontai()
        {
            return hontai;
        }

        public SCShellWindowController GetKero()
        {
            return unyuu;
        }
        public SCShellWindowController GetUnyuu()
        {
            return unyuu;
        }

        public SCScriptRunner GetScriptRunner()
        {
            return scrrunner;
        }

        public SCBalloonSkinServer GetBalloonServer()
        {
            return balserver;
        }

        public String GetGhostPath()
        {
            return ghost_path;
        }

        public File GetGhostDir()
        {
            return ghost_dir;
        }

        public SCSpirit GetMasterSpirit()
        {
            return masterSpirit;
        }

        public SCShell GetCurrentShell()
        {
            return currentShell;
        }

        public SCShellContextMenuMaker GetShellContextMenuMaker()
        {
            return shellContextMenuMaker;
        }

        public SCMenuAppearanceServer GetMenuAppearance()
        {
            return menuAppearance;
        }

        public String GetSelfName()
        {
            // シェルでオーバーライドされていたらそれを、されていなければmaster ghostから取得して返す。
            return (currentShell.GetSelfName() == null ? masterSpirit.GetSelfName() : currentShell.GetSelfName());
        }

        public String GetKeroName()
        {
            return (currentShell.GetKeroName() == null ? masterSpirit.GetKeroName() : currentShell.GetKeroName());
        }

        public String GetCurrentIdentification()
        {
            // 現在使用中のシェルが獨自のIDを持つてゐればそれを、
            // 無ければゴーストのIDを返す。
            String shell_id = currentShell.GetIdentification();
            if (shell_id != null)
            {
                return shell_id;
            }
            return masterSpirit.GetIdentification();
        }

        public File GetReadmeFile()
        {
            return readmeFile;
        }
 

        internal void SetPassiveMode(bool v)
        {
             
        }

       
        internal void StartInputBoxSession(string symbol, int timeout)
        {
             
        }

       
         
    }
}

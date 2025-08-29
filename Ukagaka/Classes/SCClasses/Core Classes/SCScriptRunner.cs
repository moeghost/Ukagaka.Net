using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;
using System.Reflection;
namespace Ukagaka
{
    public class SCScriptRunner
    {

        public Thread thread;
        public static int DEFAULT_ALTERNATIVE_TIMEOUT_SEC = 20;

        SCSession session;
        StringBuffer script;
        String master_script;
        String curHandlerName; // 現在実行中のハンドラの名前。偽AI等の場合でハンドラが存在しない場合はnull。

        // オプション

        // \sタグを使わずに喋ろうとしたら強制的に\s[0]または\s[10]を実行する。
        // オプション名は"force-reset-surfaces-before-talk"で、型はbool。
        bool force_reset_surfaces_before_talk = false;
        // どちらかが喋ろうとした時、その相方がまだ\sタグを使っていなかったら、相方のシェルも表示する。
        // オプション名は"show-partner-before-talk-if-not-used-yen-s"で、型はbool。
        bool show_partner_before_talk_if_not_used_yen_s = false;
        // URLでない選択肢が選ばれた時、バルーンを閉じる以外の事を何もしない。
        // オプション名は"do-nothing-but-close-when-normal-alternative-selected"で、型はbool。
        bool do_nothing_but_close_when_normal_alternative_selected = false;

        volatile bool interrupt_forbidden = false;
        volatile bool flagTerm = false;
        int wait;
        bool quicksession = false;
        bool synchronizedsession = false;
        bool changedBalloon = false;

        static double wait_ratio = 1.0; // \wや\_wの倍率。

        volatile bool speed_up = false;
        double speed_up_ratio = 0.1;

        int scope = 0; // 現在のスコープ。ゼロなら本体、1ならうにゅう。
        bool enyee = false; // えんいーに達したかどうか。
        bool sakura_yen_s_used = false; // これまでにsakuraスコープで\sが使われたか。
        bool kero_yen_s_used = false; // こちらはkeroスコープ

        ArrayList unselected_urls = null; // [SCBalloonAlternative] まだ選択されていないURL。

        public static int RUNNING = 1; // 実行中。
        public static int SCRIPT_END = 2; // スクリプト再生が終了した。
        public static int DONE = 3; // スクリプト再生終了後一定時間が経過した。（またスレッドは終了せずにウインドウも開いている）
                                    // もしくは選択肢が選択された。
        public static int TERMINATED = 4; // DONEになる前にterminate()された。
        private volatile int status = RUNNING;

        private volatile int alternative_timeout_sec = DEFAULT_ALTERNATIVE_TIMEOUT_SEC;
        private volatile bool hasAlternative = false; // 選択肢がある場合にtrueになります。
        private volatile SCBalloonAlternative selectedAlt = null; // 選択肢があった場合、statusがDONEになった後に選択された選択肢が入ります。
                                                                  // タイムアウトした場合はnullのままです。

        public SCScriptRunner(SCSession s, String param, Hashtable options) //: this(s, param)
        {
            if (options == null)
            {
                return;
            }
            bool? frsbt = (bool)options["force-reset-surfaces-before-talk"];
            if (frsbt != null && frsbt.Value)
            {
                force_reset_surfaces_before_talk = true;
            }

            bool? spbtinuys = (bool)options["show-partner-before-talk-if-not-used-yen-s"];
            if (spbtinuys != null && spbtinuys.Value)
            {
                show_partner_before_talk_if_not_used_yen_s = true;
            }

            bool? dnbcwnas = (bool)options["do-nothing-but-close-when-normal-alternative-selected"];
            if (dnbcwnas != null && dnbcwnas.Value)
            {
                do_nothing_but_close_when_normal_alternative_selected = true;
            }
            thread = new Thread(Run);
        }
         
        
        public SCScriptRunner(SCSession s, string param)
        {
            // sには現在有効なセッションを渡してください。
            // paramが#で始まっていたらそのエントリを探し、そうでなければparamをスクリプトとしてそのまま実行します。
            wait = 40;
            session = s;
            quicksession = interrupt_forbidden = changedBalloon = false;

            if (StringExtension.CharAt(param, 0) == '#')
            {
                curHandlerName = param;

                string new_entry = session.GetTempScriptServer().FindEntry(param);
                if (new_entry == null)
                {
                    script = new StringBuffer("\\h\\s[4]Handler \"" + param + "\" was not found...\\e");
                    return;
                }

                script = new StringBuffer(new_entry);
            }
            else
            {
                script = new StringBuffer(param);
            }

            // バックスラッシュを￥に置き換える
            if (script.ToString().IndexOf('\\') != -1)
            {
                int pos;
                while ((pos = script.ToString().IndexOf('\\')) != -1)
                {
                    script.Insert(pos, '\u00a5');

                }
            }
            thread = new Thread(new ThreadStart(Run));

            // \tや\qを含むスクリプトはインタラプト禁止。
            //if (script.ToString().IndexOf("\u00a5t") != -1 ||
            //    script.ToString().IndexOf("\u00a5q") != -1) {
            //    interrupt_forbidden = true;
            //}
        }


        public void Start()
        {
            thread?.Start();
        }

        public void Run()
        {
            //int pool = NSAutoreleasePool.push();

            master_script = script.ToString();

            session.GetHontaiBalloon().Hide();
            session.GetUnyuuBalloon().Hide();
            session.GetHontaiBalloon().CleanUp();
            session.GetUnyuuBalloon().CleanUp();
            session.GetHontaiBalloon().SetCallbackTarget(this);
            session.GetUnyuuBalloon().SetCallbackTarget(this);

            try
            {
                while (!enyee && !flagTerm && script.Length() > 0)
                { // えんいーに達したり、インタラプトされたり、実行すべき残りのスクリプトが無くなったりしたら終了。
                   
                   // スクリプトを読んで処理を行う。
                   // タグや環境変数は一つ一つ分けて、その他の文字（表示する）はまるごと一つにする。
                   

                    char firstchar = StringExtension.CharAt(script.ToString(), 0);
                    char secondChar;
                    //char secondChar;
                 //     char thirdChar;
                    switch (firstchar)
                    {
                        case '\u00a5':
                            {
                                // 次に続く文字が%か￥だったらエスケープして表示。
                                secondChar = StringExtension.CharAt(script.ToString(), 1);
                                if (secondChar == '\u00a5' || secondChar == '%')
                                {
                                    String escaped = script.ToString().Substring(1, 2);
                                    script.Remove(0, 2); // 先頭の二文字を消す。

                                    SCShellWindowController swc = (scope == 0 ? session.GetHontai() : session.GetUnyuu());
                                    swc.Show();

                                    if (synchronizedsession)
                                    {
                                        session.GetHontaiBalloon().Show();
                                        session.GetUnyuuBalloon().Show();
                                        session.GetHontaiBalloon().AddText(escaped, 0);
                                        session.GetUnyuuBalloon().AddText(escaped, quicksession ? 0 : wait);
                                    }
                                    else
                                    {
                                        SCSafetyBalloonController bc = (scope == 0 ? session.GetHontaiBalloon() : session.GetUnyuuBalloon());
                                        bc.Show();
                                        bc.AddText(escaped, quicksession ? 0 : wait);
                                    }
                                }
                                else
                                {
                                    // ￥で始まるタグだった。
                                    switch (secondChar)
                                    {
                                        case 'e': // えんいー
                                            script.SetLength(0);
                                            enyee = true;
                                            break; // ここで終了。

                                        case 'a': // random talk
                                            status = DONE;
                                            script.SetLength(0);
                                            interrupt_forbidden = false;
                                            enyee = true;
                                            session.DoShioriRandomTalk();
                                            // NSAutoreleasePool.pop(pool);
                                            return;

                                        case '-': // 終了
                                            script.SetLength(0);
                                            enyee = true;
                                            break;

                                        case '+': // ゴーストランダムチェンジ
                                            script.SetLength(0);
                                            enyee = true;
                                            session.ChangeToOtherGhostAtRandom();
                                            break;

                                        case 'h': // 本体スコープへ
                                        case '0':
                                            scope = 0;
                                            script.Remove(0, 2); // 先頭の二文字を消去。残りが押し出される。
                                            break;

                                        case 'u': // うにゅうスコープへ
                                        case '1':
                                            scope = 1;
                                            script.Remove(0, 2);
                                            break;

                                        case '4': // カレントスコープをもう一方の隣まで移動。見切れないように考慮する。
                                        case '5':
                                            session.ForceMovingNextToAnother(scope == 0 ? SCFoundation.HONTAI : SCFoundation.UNYUU);
                                            script.Remove(0, 2);
                                            break;

                                        case 'n': // カレントスコープのバルーン内で改行
                                            DoReturn(script);
                                            break;

                                        case 't': // タイムクリティカルセッションに入る
                                            interrupt_forbidden = true;
                                            script.Remove(0, 2);
                                            break;

                                        case 'i': // SERIKOシーケンス強制起動
                                            ExecuteSerikoSequence(script);
                                            break;

                                        case 's': // カレントスコープのサーフィス変更
                                            DoSurfaceChanging(script);
                                            break;

                                        case 'w': // ウエイト
                                            DoWaitCommand(script);
                                            break;

                                        case 'b': // バルーン
                                            DoBalloonChanging(script);
                                            break;

                                        case 'j': // 他のエントリ又はブラウザでURLにジャンプ
                                            DoUniversalJumpCommand(script);
                                            break;

                                        case 'c':
                                            { // カレントスコープのバルーンをクリア
                                                if (synchronizedsession)
                                                {
                                                    //Sleep(quicksession ? CLEAR_WAIT_IN_QUICK_SESSION : 0);
                                                    session.GetSakuraBalloon().BeEmpty();
                                                    session.GetKeroBalloon().BeEmpty();
                                                }
                                                else
                                                {
                                                    //Sleep(quicksession ? CLEAR_WAIT_IN_QUICK_SESSION : 0);
                                                    SCSafetyBalloonController bc = (scope == 0 ? session.GetHontaiBalloon() : session.GetUnyuuBalloon());
                                                    bc.BeEmpty();
                                                }
                                                script.Remove(0, 2); // \cの二文字を消す。
                                            }
                                            break;

                                        case 'x': // マウスがクリックされるまで一時停止
                                                  // 未実装です。
                                            script.Remove(0, 2); // \xを消す
                                            break;

                                        case '!': // 汎用コマンドシンボル
                                            ExecuteUniversalCommand(script);
                                            break;

                                        case '_':
                                            { // _s,_w[],_q,_e,_c[],_m[],_u[],_c[],_a[]__c,__tの疑いあり
                                                if (StringExtension.CharAt(script.ToString(), 2) == 's')
                                                { // シンクロナイズドセッション
                                                    synchronizedsession = !synchronizedsession;
                                                    script.Remove(0, 3);
                                                }
                                                else if (StringExtension.CharAt(script.ToString(), 2) == 'w')
                                                { // 高精度ウエイト
                                                    DoStrictWaitCommand(script);
                                                }
                                                else if (StringExtension.CharAt(script.ToString(), 2) == 'm')
                                                { // ASCIIコード埋め込み
                                                    DoAsciiCodeImplantation(script);
                                                }
                                                else if (StringExtension.CharAt(script.ToString(), 2) == 'u')
                                                { // UCS-2コード埋め込み
                                                    DoUCS2CodeImplantation(script);
                                                }
                                                else if (StringExtension.CharAt(script.ToString(), 2) == 'q')
                                                { // クイックセッション
                                                    quicksession = !quicksession; // toggle
                                                    script.Remove(0, 3);
                                                }
                                                else if (StringExtension.CharAt(script.ToString(), 2) == 'e')
                                                { // 現在のスコープのバルーンを消す
                                                    SCSafetyBalloonController bc = (scope == 0 ? session.GetHontaiBalloon() : session.GetUnyuuBalloon());
                                                    bc.Hide();
                                                    script.Remove(0, 3);
                                                }
                                                else if (StringExtension.CharAt(script.ToString(), 2) == 'c')
                                                {
                                                    // []で囲まれた範囲をGET Sentence With Sentenceでリクエストして、結果をタグ位置に挿入。
                                                    DoShioriCommunicateImplantation(script);
                                                }
                                                else if (StringExtension.CharAt(script.ToString(), 2) == 'a')
                                                { // アンカー
                                                    DoPuttingAnchor(script);
                                                }
                                                else if (StringExtension.CharAt(script.ToString(), 2) == 'b')
                                                { // バルーンイメージ表示
                                                    DoBalloonImageCommand(script);
                                                }
                                                else if (StringExtension.CharAt(script.ToString(), 2) == '_')
                                                { // __c,__tの疑い有り。
                                                    if (StringExtension.CharAt(script.ToString(), 3) == 'c')
                                                    {
                                                        session.ShowCommunicateBox();
                                                        script.Remove(0, 4);
                                                    }
                                                    else if (StringExtension.CharAt(script.ToString(), 3) == 't')
                                                    {
                                                        session.DoTeach();
                                                        script.Remove(0, 4);
                                                    }
                                                    else
                                                    {
                                                        script.Remove(0, 4);
                                                    }
                                                }
                                                else
                                                {
                                                    script.Remove(0, 2);
                                                }
                                            }
                                            break;

                                        case 'v':
                                            { // フォアグラウンドへ
                                              //   NSApplication.sharedApplication().activateIgnoringOtherApps(true);
                                                script.Remove(0, 2);
                                            }
                                            break;

                                        case 'q': // 選択肢
                                            DoPuttingAlternativeCommand(script);
                                            break;

                                        default:
                                            { // 未定義￥タグ

                                                script.Replace(0, 1, "\u00a5\u00a5");
                                                // script.Replace(script.ToString().Substring(0, 1), "\u00a5\u00a5");
                                                //   script.Replace(0, 1, "\u00a5\u00a5"); // \\にする。
                                            }
                                            break;
                                    } // end switch (secondChar)
                                } // end if (script.charAt(1) == '\u00a5' || script.charAt(1) == '%')
                            }
                            break;

                        case '%':
                            {
                                secondChar = StringExtension.CharAt(script.ToString(), 1);
                                switch (secondChar)
                                {
                                    case 'j': // インクルード
                                        DoHandlerImplantation(script);
                                        break;

                                    case 's':
                                        { // %selfname,%second,...
                                            String script_str = script.ToString();
                                            if (script_str.StartsWith("%selfname"))
                                            {
                                                script.Replace(script.ToString().Substring(0, 9), session.GetSelfName());
                                            }
                                            else if (script_str.StartsWith("%second"))
                                            {
                                                DateTime gdate = new DateTime();
                                                script.Replace(script.ToString().Substring(0, 7), "" + gdate.Second);
                                            }
                                            else
                                            {
                                                // 無効。\%にする。
                                                script.Replace(script.ToString().Substring(0, 1), "\u00a5%");
                                            }
                                        }
                                        break;

                                    case 'k':
                                        { // %keroname
                                            if (script.ToString().StartsWith("%keroname"))
                                            { // うにゅうの名前
                                                script.Replace(script.ToString().Substring(0, 9), session.GetKeroName());
                                            }
                                            else
                                            {
                                                // 無効。\%にする。
                                                script.Replace(script.ToString().Substring(0, 1), "\u00a5%");
                                            }
                                        }
                                        break;

                                    case 'm':
                                        { // %month,%minute,...
                                            if (script.ToString().StartsWith("%month"))
                                            {
                                                DateTime gdate = new DateTime();
                                                script.Replace(script.ToString().Substring(0, 6), "" + gdate.Month);
                                            }
                                            else if (script.ToString().StartsWith("%minute"))
                                            {
                                                DateTime gdate = new DateTime();
                                                script.Replace(script.ToString().Substring(0, 7), "" + gdate.Minute);
                                            }
                                            else if (script.ToString().StartsWith("%ms"))
                                            {
                                                String word = session.GetWordFromShiori("\u00a5ms");
                                                script.Replace(script.ToString().Substring(0, 3), (word == null ? "" : word));
                                            }
                                            else if (script.ToString().StartsWith("%mz"))
                                            {
                                                String word = session.GetWordFromShiori("\u00a5mz");
                                                script.Replace(script.ToString().Substring(0, 3), (word == null ? "" : word));
                                            }
                                            else if (script.ToString().StartsWith("%mc"))
                                            {
                                                String word = session.GetWordFromShiori("\u00a5mc");
                                                script.Replace(script.ToString().Substring(0, 3), (word == null ? "" : word));
                                            }
                                            else if (script.ToString().StartsWith("%mh"))
                                            {
                                                String word = session.GetWordFromShiori("\u00a5mh");
                                                script.Replace(script.ToString().Substring(0, 3), (word == null ? "" : word));
                                            }
                                            else if (script.ToString().StartsWith("%mt"))
                                            {
                                                String word = session.GetWordFromShiori("\u00a5mt");
                                                script.Replace(script.ToString().Substring(0, 3), (word == null ? "" : word));
                                            }
                                            else if (script.ToString().StartsWith("%me"))
                                            {
                                                String word = session.GetWordFromShiori("\u00a5me");
                                                script.Replace(script.ToString().Substring(0, 3), (word == null ? "" : word));
                                            }
                                            else if (script.ToString().StartsWith("%mp"))
                                            {
                                                String word = session.GetWordFromShiori("\u00a5mp");
                                                script.Replace(script.ToString().Substring(0, 3), (word == null ? "" : word));
                                            }
                                            else if (script.ToString().StartsWith("%m?"))
                                            {
                                                String word = session.GetWordFromShiori("\u00a5m?");
                                                script.Replace(script.ToString().Substring(0, 3), (word == null ? "" : word));
                                            }
                                            else
                                            {
                                                script.Replace(script.ToString().Substring(0, 1), "\u00a5%");
                                            }
                                        }
                                        break;

                                    case 'd':
                                        { // %day,%dms
                                            if (script.ToString().StartsWith("%day"))
                                            {
                                                DateTime gdate = new DateTime();
                                                script.Replace(script.ToString().Substring(0, 4), "" + gdate.Day);
                                            }
                                            else if (script.ToString().StartsWith("%dms"))
                                            {
                                                String word = session.GetWordFromShiori("\u00a5dms");
                                                script.Replace(script.ToString().Substring(0, 4), (word == null ? "" : word));
                                            }
                                            else
                                            {
                                                script.Replace(script.ToString().Substring(0, 1), "\u00a5%");
                                            }
                                        }
                                        break;

                                    case 'h':
                                        { // %hour
                                            if (script.ToString().StartsWith("%hour"))
                                            {
                                                DateTime gdate = new DateTime();
                                                script.Replace(script.ToString().Substring(0, 5), "" + gdate.Hour);
                                            }
                                            else
                                            {
                                                script.Replace(script.ToString().Substring(0, 1), "\u00a5%");
                                            }
                                        }
                                        break;

                                    case 'u':
                                        { // %username
                                            if (script.ToString().StartsWith("%username"))
                                            {
                                                String username = session.GetStringFromShiori("username");
                                                if (username == null)
                                                {
                                                    script.Remove(0, 9);
                                                }
                                                else
                                                {
                                                    script.Replace(script.ToString().Substring(0, 9), username);
                                                }
                                            }
                                            else
                                            {
                                                script.Replace(script.ToString().Substring(0, 1), "\u00a5%");
                                            }
                                        }
                                        break;

                                    case 'e':
                                        { // %exh , %et
                                            if (script.ToString().StartsWith("%exh"))
                                            { // valid
                                                script.Replace(script.ToString().Substring(0, 4), Integer.ToString(session.HoursFromBootTime()));
                                            }
                                            else if (script.ToString().StartsWith("%et"))
                                            { // invalid
                                                script.Replace(script.ToString().Substring(0, 3), SCFoundation.PseudoHoursFromStartTime());
                                            }
                                            else
                                            {
                                                script.Replace(script.ToString().Substring(0, 1), "\u00a5%");
                                            }
                                        }
                                        break;

                                    default:
                                        { // 未定義%タグ
                                            script.Replace(script.ToString().Substring(0, 1), "\u00a5%");
                                        }
                                        break;
                                }
                                break;
                            }
                        default: // 制御コードでなかった
                                 // 次にエスケープされていない￥か%が現れるまで、もしくはスクリプトの終わりに達するまでを直接表示ブロックとする。
                            {
                                int script_len = script.Length();
                                int blockend = 0;
                                while (blockend <= script_len - 1)
                                {
                                    if (StringExtension.CharAt(script.ToString(), blockend) == '\u00a5')
                                    {
                                        if (script_len - 1 > blockend)
                                        {
                                            if (StringExtension.CharAt(script.ToString(), blockend + 1) == '\u00a5' || StringExtension.CharAt(script.ToString(), blockend + 1) == '%')
                                            {
                                                script.Remove(blockend, 1);
                                                script_len--;
                                            }
                                            else
                                            {
                                                blockend--;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            blockend--;
                                            break;
                                        }
                                    }
                                    else if (StringExtension.CharAt(script.ToString(), blockend) == '%')
                                    {
                                        blockend--;
                                        break;
                                    }

                                    // \じゃないのでサーチを続ける
                                    blockend++;
                                }
                                if (blockend > script_len - 1)
                                {
                                    blockend = script_len - 1;
                                }
                                String block = script.ToString().Substring(0, blockend + 1);
                                script.Remove(0, blockend + 1);

                                if (synchronizedsession)
                                { // シンクロナイズドセッション
                                    session.GetSakura().Show();
                                    session.GetKero().Show();

                                    if (force_reset_surfaces_before_talk)
                                    {
                                        // 両方とも必要なら初期化。
                                        if (!sakura_yen_s_used)
                                        {
                                            script.Insert(0, "\u00a5s[0]");

                                            int scope_backup = scope;
                                            scope = 0;
                                            DoSurfaceChanging(script);
                                            scope = scope_backup;
                                        }
                                        if (!kero_yen_s_used)
                                        {
                                            script.Insert(0, "\u00a5s[10]");

                                            int scope_backup = scope;
                                            scope = 1;
                                            DoSurfaceChanging(script);
                                            scope = scope_backup;
                                        }
                                    }

                                    SCSafetyBalloonController hbal = session.GetHontaiBalloon();
                                    SCSafetyBalloonController ubal = session.GetUnyuuBalloon();
                                    hbal.Show();
                                    ubal.Show();

                                    int n_chars = block.Length;
                                    for (int i = 0; i < n_chars; i++)
                                    {
                                        hbal.AddChar(block.ToCharArray().ElementAt(i));
                                        ubal.AddChar(block.ToCharArray().ElementAt(i));

                                        Sleep(quicksession ? 0 : (int)(wait * (speed_up ? speed_up_ratio : 1) * wait_ratio));
                                    }
                                }
                                else if (scope == 0)
                                { // 本体
                                    session.GetSakura().Show();

                                    if (force_reset_surfaces_before_talk)
                                    {
                                        // 必要なら初期化
                                        if (!sakura_yen_s_used)
                                        {
                                            script.Insert(0, "\u00a5s[0]");
                                            DoSurfaceChanging(script);
                                        }
                                    }

                                    if (show_partner_before_talk_if_not_used_yen_s)
                                    {
                                        // 必要なら表示
                                        if (!kero_yen_s_used)
                                        {
                                            script.Insert(0, "\u00a5s[10]");
                                            scope = 1; DoSurfaceChanging(script); scope = 0;
                                            session.GetKero().Show();
                                        }
                                    }

                                    session.GetHontaiBalloon().Show();
                                    session.GetHontaiBalloon().AddText(
                                        block,
                                        quicksession ? 0 : (long)(wait * (speed_up ? speed_up_ratio : 1) * wait_ratio));
                                }
                                else if (scope == 1)
                                { // うにゅう
                                    session.GetKero().Show();

                                    if (force_reset_surfaces_before_talk)
                                    {
                                        // 必要なら初期化
                                        if (!kero_yen_s_used)
                                        {
                                            script.Insert(0, "\u00a5s[10]");
                                            DoSurfaceChanging(script);
                                        }
                                    }

                                    if (show_partner_before_talk_if_not_used_yen_s)
                                    {
                                        // 必要なら表示
                                        if (!sakura_yen_s_used)
                                        {
                                            script.Insert(0, "\u00a5s[0]");
                                            scope = 0; DoSurfaceChanging(script); scope = 1;
                                            session.GetSakura().Show();
                                        }
                                    }

                                    session.GetUnyuuBalloon().Show();
                                    session.GetUnyuuBalloon().AddText(
                                        block,
                                        quicksession ? 0 : (long)(wait * (speed_up ? speed_up_ratio : 1) * wait_ratio));
                                }

                                break;
                            }
                    } // switch (firstchar)
                } // while(!isInterrupted() && script.Length > 0)
            } // try
            catch (ThreadInterruptedException e)
            {
                flagTerm = true;
            }
            catch (SCScriptErrorException e)
            {
                System.Console.Write("SCScriptRunner : Script Error.");
                System.Console.Write("   " + e.Message);
                System.Console.Write(" : " + master_script);
            }
            catch (Exception e)
            {
                System.Console.Write("SCScriptRunner : Exception occured in running script.");

                System.Console.Write(e.StackTrace);

            }

            if (flagTerm)
            {
                status = TERMINATED;
            }
            else
            {
                status = SCRIPT_END;

                if (hasAlternative)
                {
                    bool interrupted = false;
                    // タイムアウト時間は途中で變はる可能性がある。
                    long wait_start = SystemTimer.GetTimeTickCount();
                    while (true)
                    {
                        if (SystemTimer.GetTimeTickCount() - wait_start > alternative_timeout_sec * 1000)
                        {
                            break;
                        }
                        try
                        {
                            Sleep(1000);
                        }
                        catch (ThreadInterruptedException e)
                        {
                            interrupted = true; // 選択されたか、或いは単にインタラプトされた。
                            break;
                        }
                    }

                    if (interrupted && selectedAlt == null)
                    {
                        // 単にインタラプトされた。
                        status = TERMINATED;
                    }
                    if (!interrupted && selectedAlt == null)
                    {
                        // (．□．)オイムアウト(．□．)
                        if (!do_nothing_but_close_when_normal_alternative_selected)
                        {
                            session.DoShioriEvent("OnChoiceTimeout", new String[] { master_script });
                        }
                        status = DONE;
                    }
                    else
                    {
                        // どれか一つが選ばれた。
                        String value = selectedAlt.GetRefcon();
                        if (IsURL(value))
                        {
                            // URLを開く。
                            //try{ NSWorkspace.sharedWorkspace().openURL(new URL(value)); }catch(Exception e){}
                            // URLなら既にalternativeSelectedが開いている。
                        }
                        else
                        {
                            if (!do_nothing_but_close_when_normal_alternative_selected)
                            {
                                session.DoShioriEvent("OnChoiceSelect", new String[] { value });
                            }
                        }
                        status = DONE;
                    }

                    //selectedAlt = session.getHontaiBalloon().alternativeTask(!session.isInPassiveMode());
                    //status = DONE;
                    CleanUp(true);
                    //NSAutoreleasePool.pop(pool);
                    return;
                }

                // 一定時間経ってからウインドウを閉じる。その間にインタラプトされたら即座に終了する。
                // ウエイトには前半と後半があり、前半は必ず待たなければならないが後半はインタラプトでカットしても良い。
                try
                {
                    Sleep(1500);
                }
                catch (ThreadInterruptedException e)
                {
                    status = TERMINATED;
                    CleanUp(false);
                    // NSAutoreleasePool.pop(pool);
                    return;
                }

                interrupt_forbidden = false;
                status = DONE;

                if (!IsInterrupted())
                {
                    try
                    {
                        Sleep(6000);
                    }
                    catch (ThreadInterruptedException e)
                    {
                        CleanUp(false);
                        // NSAutoreleasePool.pop(pool);
                        return;
                    }
                }
            }

            CleanUp(true);
            //  NSAutoreleasePool.pop(pool);
        } // public void Run()

        private bool IsInterrupted()
        {
            return thread.IsAlive;
        }

        private void CleanUp(bool hideBalloons)
        {
            // hideBalloonsは無視。trueでもfalseでもバルーンは隠す。

            //sendSurfaceRestoreEvent();
            session.GetHontaiBalloon().CleanUp();
            session.GetHontaiBalloon().MakeSSTPMsgEmpty();
            session.GetUnyuuBalloon().CleanUp();
            session.GetUnyuuBalloon().MakeSSTPMsgEmpty();
            if (changedBalloon)
            {
                session.GetHontaiBalloon().SetType(0);
                session.GetUnyuuBalloon().SetType(0);
                session.RecalcBalloonsLoc();
            }
            //if (hideBalloons)
            //{
            session.GetHontaiBalloon().Hide();
            session.GetUnyuuBalloon().Hide();
            //}

            // このsessionがライトモードで起動していたら、
            // ここでシェルも非表示とする。
            if (session.IsLightMode())
            {
                session.GetSakura().Hide();
                session.GetKero().Hide();
            }

            if (unselected_urls != null)
            {
                unselected_urls.Clear();
            }

            session = null;

            // Logger.log(this, Logger.DEBUG, "cleaned up");
        }

        public static bool IsURL(String str)
        {
            return str.StartsWith("http://");
        }

        public void AlternativeSelected(SCBalloonAlternative alt)
        {
            // SCSafetyBalloonControllerだけが呼ぶメソッド。
            // 選択肢が選ばれた。

            // この選択肢がURLであり、且つ選ばれていないURL選択肢が他にもあるなら、
            // URLを開くだけ開いて選択されなかった事にする。
            // 同時にタイムアウトまでの秒数を増やす。
            if (IsURL(alt.GetRefcon()))
            {
                try
                {
                  //  NSWorkspace.sharedWorkspace().openURL(new URL(alt.getRefcon()));
                }
                catch (Exception e) { }

                unselected_urls.Remove(alt);
                if (unselected_urls.Count > 0)
                {
                    alternative_timeout_sec += DEFAULT_ALTERNATIVE_TIMEOUT_SEC;
                    return;
                }
            }

            selectedAlt = alt;
            Interrupt();

            //session.clearQueue();
        }

        public void Interrupt()
        {
            thread.Interrupt();

        }


        public void Sleep(int value)
        {
            Thread.Sleep(value);
        }




        public void AnchorSelected(SCBalloonAlternative anchor)
        {
            // SCSafetyBalloonControllerだけが呼ぶメソッド。
            // アンカーが選ばれた。
            session.DoShioriEvent("OnAnchorSelect", new String[] { anchor.GetRefcon() });
            Interrupt();

            //session.clearQueue();
        }

        protected void DoReturn(StringBuffer script)
        {
            String code; // \n 又は \u0002
            if (script.Length() >= 3 && StringExtension.CharAt(script.ToString(), 2) == '[') // 付加情報有り
            {
                int blockstart = 2;
                int blockend = script.ToString().IndexOf(']', blockstart);

                if (blockend == -1)
                {
                    throw new SCScriptErrorException("Illegal return symbol \u00a5n[] : missing ']'.");
                }

                String enn_arg = script.ToString().Substring(blockstart + 1, blockend);
                if (enn_arg.Equals("half"))
                {
                    code = "\u0002";
                }
                else if (enn_arg.Equals(""))
                {
                    code = "\n";
                }
                else
                {
                    Warning("Undefined return symbol \u00a5n[] parameter : \"" + enn_arg + "\".");
                    code = "\n";
                }
                script.Remove(0, blockend + 1); // \n[]消去
            }
            else
            {
                code = "\n";
                script.Remove(0, 2); // \nの二文字を消す。
            }

            if (synchronizedsession)
            {
                //session.getHontaiBalloon().show();
                //session.getUnyuuBalloon().show();
                session.GetHontaiBalloon().AddText(code, 0);
                session.GetUnyuuBalloon().AddText(code, 0);
            }
            else
            {
                SCSafetyBalloonController bc = (scope == 0 ? session.GetHontaiBalloon() : session.GetUnyuuBalloon());
                //bc.show();
                bc.AddText(code, 0);
            }
        }

        protected void ExecuteSerikoSequence(StringBuffer script)
        {
            if (script.Length() < 3)
            {
                throw new SCScriptErrorException("Illegal SERIKO sequence execution command \u00a5i[] : missing '['.");
            }

            int blockstart = 2;

            //if (script.ToString().ToArray().ElementAt(blockstart) != '[')
            if (StringExtension.CharAt(script.ToString(), blockstart) != '[')
            {
                throw new SCScriptErrorException("Illegal SERIKO sequence execution command \u00a5i[] : missing '['.");
            }

            int blockend = script.ToString().IndexOf(']');
            if (blockend == -1)
            {
                throw new SCScriptErrorException("Illegal SERIKO sequence execution command \u00a5i[] : missing ']'.");
            }

            // String content = script.Substring(blockstart + 1, blockend);
            String content = StringExtension.Substring(script.ToString(),blockstart + 1, blockend);

            int id;
            try
            {
                id = Integer.ParseInt(content);
            }
            catch (FormatException e)
            {
                throw new SCScriptErrorException("Illegal SERIKO sequence execution command \u00a5i[] : parameter \"" + content + "\" was not a number.");
            }
            session.GetCurrentShell().GetSeriko().Execute(id, scope == 0);

            script.Remove(0, blockend + 1);
        }

        protected void DoSurfaceChanging(StringBuffer script)
        {
            if (script.Length() < 3)
            {
                throw new SCScriptErrorException("Illegal surface changing symbol \u00a5s[] or \u00a5s? : missing parameter.");
            }

            int id;

            if (script.CharAt(2) == '[')
            { // 括弧型なら
                int blockstart = 2;
                int blockend = script.ToString().IndexOf(']');
                if (blockend == -1)
                {
                    throw new SCScriptErrorException("Illegal surface changing symbol \u00a5s[] : missing ']'.");
                }
                String content = script.Substring(blockstart + 1, blockend);

                // \s[]の中で\エスケープなど行われるわけがないので、エスケープ処理は省略。
                // まずはエイリアスネームテーブルから拾ってみる。
                id = session.GetCurrentShell().GetAliasNameTable().lookup(
                scope == 0 ? "sakura.surface.alias" : "kero.surface.alias",
                content);
                if (id == -1)
                { // テーブル内に定義が無かったら
                    try
                    {
                        id = Integer.ParseInt(content); // 数値と見なす
                    }
                    catch (FormatException e)
                    {
                        throw new SCScriptErrorException("Illegal surface changing symbol \u00a5s[] : parameter \"" + content + "\" was neither defined as an alias nor a number.");
                    }
                }

                script.Remove(0, blockend + 1);
            }
            else
            {
                String param = "" + script.CharAt(2);
                try
                {
                    id = Integer.ParseInt(param);
                }
                catch (FormatException e)
                {
                    throw new SCScriptErrorException("Illegal surface changing symbol \u00a5s? : parameter \"" + param + "\" was not a number.");
                }
                script.Remove(0, 3); // ￥s?の三文字を消す。
            }

           //if (id == -1) {
           //   SCShellWindowController swc = (scope == 0 ? session.getHontai() : session.getUnyuu());
           //   swc.hide();
            //  }
            //else {
            SCShellWindowController swc = (scope == 0 ? session.GetHontai() : session.GetUnyuu());
            session.ChangeSurface(id, scope == 0);
            swc.Show();
            //}

            if (scope == 0)
            {
                sakura_yen_s_used = true;
            }
            else
            {
                kero_yen_s_used = true;
            }
        }

        protected void DoWaitCommand(StringBuffer script)
        {
            if (script.Length() < 3)
            {
                throw new SCScriptErrorException("Illegal wait symbol \u00a5w? : missing parameter.");
            }

            int param;
            try
            {
                param = Integer.ParseInt("" + script.CharAt(2));
            }
            catch
            {
                throw new SCScriptErrorException("Illegal wait symbol \u00a5w? : parameter \"" + script.CharAt(2) + "\" was not a number.");
            }

            session.GetHontaiBalloon().FlushWindow();
            session.GetKeroBalloon().FlushWindow();

            // 50倍してウエイトをかける。ただしspeed_up中なら減る。
            // さらに環境設定のウェイト倍率もかける。
            Sleep((int)(param * 50 * (speed_up ? speed_up_ratio : 1) * wait_ratio));

            script.Remove(0, 3); // \w?の三文字を消す。
        }

        protected void DoBalloonChanging(StringBuffer script)
        {
            if (script.Length() < 3)
            {
                throw new SCScriptErrorException("Illegal balloon changing symbol \u00a5b[] or \u00a5b? : missing parameter.");
            }

            int id;
            if (script.CharAt(2) == '[') // 括弧型なら
            {
                int blockstart = 2;
                int blockend = script.ToString().IndexOf(']');
                if (blockend == -1)
                {
                    throw new SCScriptErrorException("Illegal balloon changing symbol \u00a5b[] : missing ']'.");
                }
                String content = script.Substring(blockstart + 1, blockend);

                // \b[]の中で\エスケープなど行われるわけがないので、エスケープ処理は省略。
                try
                {
                    id = Integer.ParseInt(content);
                }
                catch
                {
                    throw new SCScriptErrorException("Illegal balloon changing symbol \u00a5b[] : parameter \"" + content + "\" was not a number.");
                }

                script.Remove(0, blockend + 1);
            }
            else
            {
                String param = "" + script.CharAt(2);
                try
                {
                    id = Integer.ParseInt(param);
                }
                catch
                {
                    throw new SCScriptErrorException("Illegal balloon changing symbol \u00a5b? : parameter \"" + param + "\" was not a number.");
                }
                script.Remove(0, 3); // ￥b?の三文字を消す。
            }

            if (id >= 0)
            {
                SCSafetyBalloonController bc = (scope == 0 ? session.GetHontaiBalloon() : session.GetUnyuuBalloon());
                bc.SetType(id);
                session.RecalcBalloonsLoc();
                changedBalloon = true;
            }
            else if (id == -1) // hide
            {
                SCSafetyBalloonController bc = (scope == 0 ? session.GetHontaiBalloon() : session.GetUnyuuBalloon());
                bc.Hide();
            }
        }

        protected void DoUniversalJumpCommand(StringBuffer script)
        {
            if (script.Length() < 3)
            {
                throw new SCScriptErrorException("Illegal universal jump symbol \u00a5j[] : missing '['.");
            }

            int blockstart = 2;
            if (script.CharAt(blockstart) != '[')
            {
                throw new SCScriptErrorException("Illegal universal jump symbol \u00a5j[] : missing '['.");
            }
            int blockend = FindCharWithoutEscape(script.ToString(), ']', 3); // ４文字目から閉じ括弧のサーチ開始。
            if (blockend == -1)
            {
                throw new SCScriptErrorException("Illegal universal jump symbol \u00a5j[] : missing ']'.");
            }
            String content = script.Substring(blockstart + 1, blockend);


            if (content.StartsWith("http://"))
            {
                // 指定されたURLを開く。
                try
                {
                   // NSWorkspace.sharedWorkspace().openURL(new URL(content));
                }
                catch (Exception e)
                {
                    throw new SCScriptErrorException("Couldn't open the specified url " + content + " because of " + e.ToString());
                }
            }
            else
            {
                // deprecated //
            }

            script.Remove(0, blockend + 1);
        }

        protected String EscapeCommaInDoubleQuotation(String src)
        {
            StringBuffer result = new StringBuffer();

            bool in_quote = false;
            int len = src.Length;
            for (int i = 0; i < len; i++)
            {
                char c = src.ToArray().ElementAt(i);
                if (c == '"')
                {
                    in_quote = !in_quote;
                }
                else if (c == ',')
                {
                    if (in_quote)
                    {
                        result.Append("###_COMMA_###");
                    }
                    else
                    {
                        result.Append(c);
                    }
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }
        protected String UnescapeComma(String src)
        {
            StringBuffer buf = new StringBuffer(src);
            int pos;
            while ((pos = buf.ToString().IndexOf("###_COMMA_###")) != -1)
            {
                buf.Replace(buf.ToString().Substring(pos, pos + 13), ",");
            }
            return buf.ToString();
        }
        protected void ExecuteUniversalCommand(StringBuffer script)
        {
            if (script.Length() < 3)
            {
                throw new SCScriptErrorException("Illegal universal command symbol \u00a5![] : missing parameter.");
            }

            int blockstart = 2;
            if (script.CharAt(blockstart) != '[')
            {
                throw new SCScriptErrorException("Illegal universal command symbol \u00a5![] : missing '['.");
            }

            int blockend = FindCharWithoutEscape(script.ToString(), ']', 3);
            if (blockend == -1)
            {
                throw new SCScriptErrorException("Illegal universal command symbol \u00a5![] : missing ']'.");
            }

            // ダブルクォートに囲まれたカンマをエスケープする。
            String content = EscapeCommaInDoubleQuotation(script.Substring(blockstart + 1, blockend));
            script.Remove(0, blockend + 1);

            StringTokenizer st = new StringTokenizer(content, ",");
            String command = st.NextToken();
            ArrayList argv = new ArrayList();
            while (st.HasMoreTokens())
            {
                argv.Add(UnescapeComma(st.NextToken()));
            }
            ExecuteUniversalCommand(command, argv);
        }
        protected void ExecuteUniversalCommand(String command, ArrayList argv)
        {
            // 各処理メソッドが投げた例外はそのまま呼び出し元にリダイレクトします。
            try
            {
                MethodInfo method = this.GetType().GetMethod("univcommand_" + command);




                //       Method method = this.getClass().getMethod("univcommand_" + command, new Class[] { Vector.class});
                method.Invoke(this, new Object[] { argv });
            }
            catch (MissingMethodException e)
            {
                Warning("Undefined command of universal command symbol \u00a5![] : " + command);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public void Univcommand_enter(ArrayList argv)
        {
            // passivemode
            if (argv.Count == 0)
            {
                throw new SCScriptErrorException("Illegal \u00a5![enter,#id#] command : missing parameter id.");
            }

            String arg1 = (String)argv.ToArray().ElementAt(0);
            if (arg1.Equals("passivemode"))
            {
                if (argv.Count > 1)
                {
                    Warning("Illegal \u00a5![enter,passivemode] command : too many paramters.");
                }

                session.SetPassiveMode(true);
            }
            else
            {
                Warning("Undefined parameter id of \u00a5![enter,#id#] command : " + arg1);
            }
        }
        public void Univcommand_leave(ArrayList argv)
        {
            // passivemode
            if (argv.Count == 0)
            {
                throw new SCScriptErrorException("Illegal \u00a5![leave,#id#] command : missing parameter id.");
            }

            String arg1 = (String)argv.ToArray().ElementAt(0);
            if (arg1.Equals("passivemode"))
            {
                if (argv.Count > 1)
                {
                    Warning("Illegal \u00a5![leave,passivemode] command : too many paramters.");
                }

                session.SetPassiveMode(false);
            }
            else
            {
                Warning("Undefined parameter id of \u00a5![leave,#id#] command : " + arg1);
            }
        }
        public void Univcommand_lock(ArrayList argv)
        {
            // repaint
        }
        public void Univcommand_unlock(ArrayList argv)
        {
            // repaint
        }
        public void Univcommand_open(ArrayList argv)
        {
            // inputbox,symbol,limittime
            // browser,url
            // mailer,address
            // teachbox
            // communicatebox
            if (argv.Count == 0)
            {
                throw new SCScriptErrorException("Illegal \u00a5![open,#object#] command : missing parameter object.");
            }

            String arg1 = (String)argv.ToArray().ElementAt(0);
            if (arg1.Equals("browser"))
            {
                if (argv.Count < 2)
                {
                    throw new SCScriptErrorException("Illegal \u00a5![open,browser,url] command : missing parameter url.");
                }
                else if (argv.Count > 2)
                {
                    Warning("Illegal \u00a5![open,browser,url] command : too many paramters.");
                }

                String arg2 = (String)argv.ToArray().ElementAt(1);
                // 指定されたURLを開く。
                try
                {
                   // NSWorkspace.sharedWorkspace().openURL(new URL(arg2));
                }
                catch (Exception e)
                {
                    throw new SCScriptErrorException("Couldn't open the specified url " + arg2 + " because of " + e.ToString());
                }
            }
            else if (arg1.Equals("teachbox"))
            {
                if (argv.Count > 1)
                {
                    Warning("Illegal \u00a5![open,teachbox] command : too many paramters.");
                }

                session.DoTeach();
            }
            else if (arg1.Equals("communicatebox"))
            {
                if (argv.Count > 1)
                {
                    Warning("Illegal \u00a5![open,communicatebox] command : too many paramters.");
                }

                session.ShowCommunicateBox();
            }
            else if (arg1.Equals("inputbox"))
            {
                if (argv.Count < 2)
                {
                    Warning("Illegal \u00a5![open,inputbox,#symbol#,#timeout#] command : missing parameter event.");
                }

                String symbol = (String)argv.ToArray().ElementAt(1);
                int timeout = -1;
                if (argv.Count > 3)
                    timeout = Integer.ParseInt((String)argv.ToArray().ElementAt(2));
                session.StartInputBoxSession(symbol, timeout);
            }
        }
        public void univcommand_raise(ArrayList argv)
        {
            if (argv.Count == 0)
            {
                throw new SCScriptErrorException("Illegal \u00a5![raise,#name#,#ref0#,#ref1#,...] command : missing parameter name.");
            }

            String eventName = (String)argv.ToArray().ElementAt(0);

            int n_refs = argv.Count - 1;
            String[] refs = new String[n_refs];
            for (int i = 0; i < n_refs; i++)
            {
                refs[i] = (String)argv.ToArray().ElementAt(i + 1);
            }

            session.DoShioriEvent(eventName, refs);
        }

        protected void DoStrictWaitCommand(StringBuffer script)
        {
            if (script.Length() < 4)
            {
                throw new SCScriptErrorException("Illegal strict wait symbol \u00a5_w[] : missing '['.");
            }

            int blockstart = 3;
            if (script.CharAt(blockstart) != '[')
            {
                throw new SCScriptErrorException("Illegal strict wait symbol \u00a5_w[] : missing '['.");
            }
            int blockend = script.ToString().IndexOf(']');
            if (blockend == -1)
            {
                throw new SCScriptErrorException("Illegal strict wait symbol \u00a5_w[] : missing ']'.");
            }
            String content = script.Substring(blockstart + 1, blockend);

            int waitparam;
            try
            {
                waitparam = Integer.ParseInt(content);
            }
            catch
            {
                throw new SCScriptErrorException("Illegal strict wait symbol \u00a5_w[] : parameter \"" + content + "\" was not a number.");
            }
            if (waitparam < 0)
            {
                throw new SCScriptErrorException("Illegal strict wait symbol \u00a5_w[] : parameter \"" + content + "\" was negative.");
            }

            session.GetHontaiBalloon().FlushWindow();
            session.GetKeroBalloon().FlushWindow();

            Sleep((int)(waitparam * (speed_up ? speed_up_ratio : 1) * wait_ratio));

            script.Remove(0, blockend + 1);
        }

        protected void DoAsciiCodeImplantation(StringBuffer script)
        {
            if (script.Length() < 4)
            {
                throw new SCScriptErrorException("Illegal ascii code implantation symbol \u00a5_m[] : missing '['.");
            }

            int blockstart = 3;
            if (script.CharAt(blockstart) != '[')
            {
                throw new SCScriptErrorException("Illegal ascii code implantation symbol \u00a5_m[] : missing '['.");
            }
            int blockend = script.ToString().IndexOf(']');
            if (blockend == -1)
            {
                throw new SCScriptErrorException("Illegal ascii code implantation symbol \u00a5_m[] : missing ']'.");
            }
            String content = script.Substring(blockstart + 1, blockend);

            // 0x[0-9a-fA-F]{2}の形になっているかどうかをチェック
            if (content.Length != 4 || !content.StartsWith("0x"))
            {
                throw new SCScriptErrorException("Illegal ascii code implantation symbol \u00a5_m[] : format error in parameter \"" + content + "\". It has to be 0x[0-9a-fA-F]{2} in regex for example 0x2a.");
            }

            String result;
            try
            {
                int code = Integer.ParseInt(content.Substring(2), 16); // 0xの次
                char c = (char)(code & 0x00ff);
                result = "" + c;
            }
            catch
            {
                throw new SCScriptErrorException("Illegal ascii code implantation symbol \u00a5_m[] : parameter \"" + content + "\" was not a hex number.");
            }

            script.Replace(0, blockend + 1, result);
        }

        protected void DoUCS2CodeImplantation(StringBuffer script)
        {
            if (script.Length() < 4)
            {
                throw new SCScriptErrorException("Illegal UCS-2 code implantation symbol \u00a5_u[] : missing '['.");
            }

            int blockstart = 3;
            if (script.CharAt(blockstart) != '[')
            {
                throw new SCScriptErrorException("Illegal UCS-2 code implantation symbol \u00a5_u[] : missing '['.");
            }
            int blockend = script.ToString().IndexOf(']');
            if (blockend == -1)
            {
                throw new SCScriptErrorException("Illegal UCS-2 code implantation symbol \u00a5_u[] : missing ']'.");
            }
            String content = script.Substring(blockstart + 1, blockend);

            // 0x[0-9a-fA-F]{4}の形になっているかどうかをチェック
            if (content.Length != 6 || !content.StartsWith("0x"))
            {
                throw new SCScriptErrorException("Illegal UCS-2 code implantation symbol \u00a5_u[] : format error in parameter \"" + content + "\". It has to be 0x[0-9a-fA-F]{4} in regex for example 0x00b2.");
            }

            String result;
            try
            {
                int code = Integer.ParseInt(content.Substring(2), 16); // 0xの次
                char c = (char)(code & 0xffff);
                result = "" + c;
            }
            catch
            {
                throw new SCScriptErrorException("Illegal UCS-2 code implantation symbol \u00a5_u[] : parameter \"" + content + "\" was not a hex number.");
            }

            script.Replace(0, blockend + 1, result);
        }

        protected void DoPuttingAnchor(StringBuffer script)
        {
            if (script.Length() < 4)
            {
                throw new SCScriptErrorException("Illegal anchor definition symbol \u00a5_a[]...\u00a5_a : missing '['.");
            }

            int blockstart = 3;
            if (script.CharAt(blockstart) != '[')
            {
                throw new SCScriptErrorException("Illegal anchor definition symbol \u00a5_a[]...\u00a5_a : missing '['.");
            }
            int blockend = FindCharWithoutEscape(script.ToString(), ']', 4); // 5文字目から閉じ括弧のサーチ開始。
            if (blockend == -1)
            {
                throw new SCScriptErrorException("Illegal anchor definition symbol \u00a5_a[]...\u00a5_a : missing ']'.");
            }
            String anchor_id = script.Substring(blockstart + 1, blockend);

            // アンカー終了タグ\_aを探す。
            int end_tag_pos = script.ToString().IndexOf("\u00a5_a", blockend + 1);
            if (end_tag_pos == -1)
            {
                throw new SCScriptErrorException("Illegal anchor definition symbol \u00a5_a[]...\u00a5_a : missing end-symbol \u00a5_a.");
            }
            String anchor_title = script.Substring(blockend + 1, end_tag_pos);

            script.Remove(0, end_tag_pos + 3);

            SCSafetyBalloonController balloon = (scope == 0 ? session.GetSakuraBalloon() : session.GetKeroBalloon());
            SCShellWindowController swc = (scope == 0 ? session.GetSakura() : session.GetKero());

            balloon.Show();
            swc.Show();

            balloon.AddAnchor(anchor_title, anchor_id);
        }

        protected void DoShioriCommunicateImplantation(StringBuffer script)
        {
            if (script.Length() < 4)
            {
                throw new SCScriptErrorException("Illegal SHIORI communicate implantation symbol \u00a5_c[] : missing '['.");
            }

            int blockstart = 3;
            if (script.CharAt(blockstart) != '[')
            {
                throw new SCScriptErrorException("Illegal SHIORI communicate implantation symbol \u00a5_c[] : missing '['.");
            }
            int blockend = FindCharWithoutEscape(script.ToString(), ']', 4); // 5文字目から閉じ括弧のサーチ開始。
            if (blockend == -1)
            {
                throw new SCScriptErrorException("Illegal SHIORI communicate implantation symbol \u00a5_c[] : missing ']'.");
            }

            // SHIORI/3.0では廃止された模様。
            if (session.GetMasterSpirit().GetShioriProtocolVersion() != 2)
            {
                script.Remove(0, blockend + 1);
            }
            else
            {
                String content = script.Substring(blockstart + 1, blockend);

                StringBuffer buf = new StringBuffer("GET Sentence SHIORI/2.0\r\n");
                buf.Append("Sender: " + SCFoundation.STRING_FOR_SENDER + "\r\n");
                buf.Append("Sentence: " + content + "\r\n");
                buf.Append("\r\n"); // リクエスト完成
                SCShioriSessionResponce resp = session.GetMasterSpirit().DoShioriSession(buf.ToString());
                String responce = (String)resp.GetResponce()["Sentence"];

                String translated = (responce == null ? "" : session.GetMasterSpirit().TranslateWithShiori(responce));
                script.Replace(0, blockend + 1, session.GetMasterSpirit().TranslateWithMakoto(translated));
            }
        }

        protected void DoBalloonImageCommand(StringBuffer script)
        {
            if (script.Length() < 4)
            {
                throw new SCScriptErrorException("Illegal balloon image \u00a5_b[s,x,y] : missing parameter.");
            }

            int blockstart = 3;
            int blockend = FindCharWithoutEscape(script.ToString(), ']', 4); // \_b[s,x,y]タイプ
            if (blockend == -1)
            {
                throw new SCScriptErrorException("Illegal balloon image \u00a5_b[s,x,y] : missing ']'.");
            }
            String content = script.Substring(blockstart + 1, blockend);

            int comma_pos1 = content.IndexOf(',', 0);
            if (comma_pos1 == -1)
            {
                throw new SCScriptErrorException("Illegal balloon image \u00a5_b[s,x,y] : missing ','.");
            }
            String imageFile = content.Substring(0, comma_pos1);

            int comma_pos2 = content.IndexOf(',', comma_pos1 + 1);
            if (comma_pos1 == -1)
            {
                throw new SCScriptErrorException("Illegal balloon image \u00a5_b[s,x,y] : missing ','.");
            }
            String sx = content.Substring(comma_pos1 + 1, comma_pos2);
            String sy = content.Substring(comma_pos2 + 1);

            script.Remove(0, blockend + 1);

            // REDFLAG: x,y が centerx,centery だったらセンターポイントを取得。未實裝。
            if (sx.Equals("centerx") || sy.Equals("centery"))
            {
                System.Console.Write("SCScriptRunner: centerx/centery used in \u00a5_b tag. This is unimplemented yet.");
                sx = sy = "0";
            }

            SCSafetyBalloonController bc =
                scope == 0 ? session.GetHontaiBalloon() : session.GetUnyuuBalloon();
            bc.AddImage(imageFile, Integer.ParseInt(sx), Integer.ParseInt(sy));
        }

        protected void DoPuttingAlternativeCommand(StringBuffer script)
        {
            if (script.Length() < 3)
            {
                throw new SCScriptErrorException(
                "Illegal putting alternative symbol \u00a5q?[id][title] or \u00a5q[title,id] : missing parameter.");
            }

            String title, id;
            if (script.CharAt(2) == '[')
            { // \q[title,id]タイプ
                int blockstart = 2;
                int blockend = FindCharWithoutEscape(script.ToString(), ']', 3); // 4文字目から閉じ括弧のサーチ開始。
                if (blockend == -1)
                {
                    throw new SCScriptErrorException("Illegal putting alternative symbol \u00a5q[title,id] : missing ']'.");
                }
                String content = script.Substring(blockstart + 1, blockend);

                int comma_pos = content.IndexOf(',');
                if (comma_pos == -1)
                {
                    throw new SCScriptErrorException("Illegal putting alternative symbol \u00a5q[title,id] : missing ','.");
                }

                title = content.Substring(0, comma_pos);
                id = content.Substring(comma_pos + 1);

                script.Remove(0, blockend + 1); // 閉じ括弧までを消す。
            }
            else
            { // \q?[id][title]タイプ
                if (!Char.IsDigit(script.CharAt(2)))
                {
                    throw new SCScriptErrorException(
                        "Illegal putting alternative symbol \u00a5q?[id][title] :" +
                        " missing index number. This is not important at all but illegal format.");
                }

                int blockstart, blockend;

                blockstart = 3;
                if (script.CharAt(blockstart) != '[')
                {
                    throw new SCScriptErrorException(
                        "Illegal putting alternative symbol \u00a5q?[id][title] : missing '[' of [id].");
                }
                blockend = FindCharWithoutEscape(script.ToString(), ']', 4);
                if (blockend == -1)
                {
                    throw new SCScriptErrorException(
                        "Illegal putting alternative symbol \u00a5q?[id][title] : missing ']' of [id].");
                }
                id = script.Substring(blockstart + 1, blockend);

                blockstart = blockend + 1;
                if (script.CharAt(blockstart) != '[')
                {
                    throw new SCScriptErrorException(
                        "Illegal putting alternative symbol \u00a5q?[id][title] : missing '[' of [title].");
                }
                blockend = FindCharWithoutEscape(script.ToString(), ']', blockstart + 1); // [id]の次の次の文字から閉じ括弧のサーチを開始。
                if (blockend == -1)
                {
                    throw new SCScriptErrorException(
                        "Illegal putting alternative symbol \u00a5q?[id][title] : missing ']' of [title].");
                }
                title = script.Substring(blockstart + 1, blockend);

                script.Replace(0, blockend + 1, "\u00a5n"); // 閉じ括弧までを消して\nを入れる。
            }

            SCSafetyBalloonController balloon = (scope == 0 ? session.GetSakuraBalloon() : session.GetKeroBalloon());
            SCShellWindowController swc = (scope == 0 ? session.GetSakura() : session.GetKero());

            balloon.Show();
            swc.Show();

            SCBalloonAlternative alt = balloon.AddAlternative(title, id);
            hasAlternative = true; // 選択肢有り
            interrupt_forbidden = true; // 選択肢があるのでタイムクリティカルセッションに強制移行。

            if (IsURL(id))
            {
                // ツールチップを付ける。
                alt.SetToolTip(id);

                if (unselected_urls == null)
                {
                    //unselected_urls = Collections.synchronizedSet(new HashSet());
                }
                unselected_urls.Add(alt);
            }
        }

        protected void DoHandlerImplantation(StringBuffer script)
        {
            if (script.Length() < 3)
            {
                throw new SCScriptErrorException("Illegal handler implantation symbol %j[] : missing '['.");
            }

            int blockstart = 2;
            if (script.CharAt(blockstart) != '[')
            {
                throw new SCScriptErrorException("Illegal handler implantation symbol %j[] : missing '['.");
            }
            int blockend = FindCharWithoutEscape(script.ToString(), ']', 3);
            if (blockend == -1)
            {
                throw new SCScriptErrorException("Illegal handler implantation symbol %j[] : missing ']'.");
            }

            // deprecated //
            script.Remove(0, blockend + 1);
        }

        protected void Warning(String msg)
        {
            System.Console.Write("SCScriptRunner Warning " + msg);
        }


        public int FindCharWithoutEscape(String src, char chr, int startpos)
        {
            return FindCharWithoutEscape(src, chr, startpos, '\u00a5');
        }
        public int FindCharWithoutEscape(String src, char chr, int startpos, char escape)
        {
            // srcのstartpos文字目(0オリジン)からchrを探すが、chrがescapeでエスケープされていたら次のchrを探す。
            // 結局見付からなかったら-1を返す。
            int len = src.Length;
            int pos = startpos;
            while (pos < len)
            {
                if (src.ToArray().ElementAt(pos) == chr && !(pos > 0 && src.ToArray().ElementAt(pos - 1) == escape))
                {
                    return pos;
                }
                pos++;
            }
            return -1;
        }

        public bool InterruptAllowed()
        {
            if (status != RUNNING && status != SCRIPT_END)
            {
                return true;
            }
            else
            {
                return !interrupt_forbidden;
            }
        }

        public int GetStatus()
        {
            return status;
        }

        public bool HasAlternative()
        {
            // 選択肢コマンドが一つでも評価されたらされた時にtrueになります。
            // ランナーを作成した時点では常にfalseであることに注意して下さい。
            return hasAlternative;
        }

        public SCBalloonAlternative GetSelectedAlternative()
        {
            // つまり選択肢が選ばれる前は、常にnullを返します。
            return selectedAlt;
        }

        public void Terminate()
        {
            if (!IsAlive()) return;

            Interrupt();
            while (IsAlive())
            {
                try
                {
                    Sleep(100);
                }
                catch (Exception e) { }
            }
        }

        public bool IsAlive()
        {
            return thread.IsAlive;

        }




        public void SpeedUp()
        {
            speed_up = true;
        }

        public void SpeedDown()
        {
            speed_up = false;
        }

        private void SendSurfaceRestoreEvent()
        {
            if (session.GetHontai().CurrentSurfaceId() != 0 || session.GetUnyuu().CurrentSurfaceId() != 10)
            {
                ///session.doShioriEvent("OnSurfaceRestore",new String[] {
                //  Integer.ToString(session.getHontai().currentSurfaceId()),
               //   Integer.ToString(session.getUnyuu().currentSurfaceId())
               //   } );
                session.ChangeSurface(0, true);
                session.ChangeSurface(10, false);
            }
        }

        public static void SetWaitRatio(double ratio)
        {
            wait_ratio = ratio;
        }

        protected void Ize()
        {
            //Logger.log(this, Logger.DEBUG, "ized");
        }

        public String ToString()
        {
            StringBuffer buf = new StringBuffer();
            buf.Append("session: {").Append(session).Append('}');
            //buf.Append("; script: ").Append(master_script);
            if (interrupt_forbidden)
            {
                buf.Append("; interrupt forbidden");
            }
            buf.Append("; status: ");
            if (status == RUNNING)
            {
                buf.Append("running");
            }
            else if (status == SCRIPT_END)
            {
                buf.Append("script-end");
            }
            else if (status == DONE)
            {
                buf.Append("done");
            }
            else if (status == TERMINATED)
            {
                buf.Append("terminated");
            }
            else
            {
                buf.Append("*unknown*");
            }
            if (hasAlternative)
            {
                buf.Append("; has alternative");
            }
            if (selectedAlt != null)
            {
                buf.Append("; selected alt: {").Append(selectedAlt).Append('}');
            }
            return buf.ToString();
        }

        
    }
}

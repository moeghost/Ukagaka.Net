using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Cocoa.AppKit;
namespace Ukagaka
{
    public class SCSerikoSequenceRunner
    {
        Thread thread;
        SCShell shell;
        SCSeriko seriko;
        SCSerikoSequence seq;
        SCShellWindowController target;
        SCSession session;
        bool end_and_quit; // trueならシーケンス終了後にランナーが停止します。runonce等で使われます。
        int exec_probability = 1; // 発動前の待ち時間（秒毎の発動確率）です。random(rarely等のマクロ含む）の場合のみ意味を持ちます。そうでなければ1になります。
        SCShellLayer layer; // overlay用レイヤーです。シーケンス内で初めてoverlayが出てきた時に作成されます。
        bool hontai;

        private volatile bool signal_stop = false;

        /*
          ランナーは独立したスレッドです。

          次のタイプは常時起動しています。シーケンス終了後、次の発動タイミングまでスリープします。
          random (sometimes,rarely)
          always
          talk
          yen-e

          次のタイプは必要な時に起動され、シーケンス終了後にスレッドが停止します。
          runonce
          never

          talk,yen-eは常時起動していますが、実際に動き出すタイミングは不定です。
          なので、発動タイミングは外部から通知してもらう必要があります。

          常時起動タイプは、厳密にはスリープしてから発動、スリープしてから発動、という動作を行います。
          これはシーケンスグループが切り替わった際にrunonceとsometimesとrarelyとalwaysが一度に全て動きだすのを防ぐためです。
        */

        public SCSerikoSequenceRunner(SCSeriko seriko, SCShell shell, SCSerikoSequence seq, bool hontai, SCSession session)
        {
            this.shell = shell;
            this.seriko = seriko;
            this.hontai = hontai;
            this.seq = seq;
            this.session = session;
            target = (hontai ? session.GetHontai() : session.GetUnyuu());

            String interval_type = seq.Interval().type();
            if (interval_type.Equals("runonce") || interval_type.Equals("never"))
            {
                end_and_quit = true;
            }

            if (interval_type.Equals("random"))
            {
                exec_probability = seq.Interval().probability();
            }
            thread = new Thread(new ThreadStart(Run));
        }


        public void Start()
        {

            thread.Start();

        }

        public void Sleep(int value)
        {
            Thread.Sleep(value);

        }

        public void Run()
        {
            // exclusive時に復帰しなくなるのでこのメソッド内では決してreturnを使わないように。
            // int pool = NSAutoreleasePool.push();
            bool exc_lock = (seq.Option() != null ? seq.Option().Type().Equals("exclusive") : false);

            while (!signal_stop)
            {
                //int inner_pool = NSAutoreleasePool.push();
                // 発動前ウエイトをかける。秒毎の発動確率。

                /*
                  x回以上連続して発動してはならない。
                  ただしウエイトがy回以内だった場合は連続していると見なす。

                  とりあえずxは3、yは2としておく。
                */
                try
                {
                    int x = 3, y = 2;
                    int repeat = 0, wait_times_since_break = 0;

                    while (true)
                    {
                        if (exec_probability == 1)
                        {
                            break;
                        }
                        else
                        {
                            if (new Random().Next(0, exec_probability) == 0)
                            {
                                if (repeat < x)
                                {
                                    repeat++;
                                    break; // ウエイト終了
                                } // x回以上連続していたら強制的にスリープ。
                            }

                            Sleep(1 * 1000);
                            wait_times_since_break++;

                            if (wait_times_since_break > y)
                            { // 必要なウエイトを行ったら
                                repeat = wait_times_since_break = 0; // リセット
                            }
                        }
                    }
                }
                catch (ThreadInterruptedException e)
                {
                    // 終了指示が出ていたらスレッド終了。
                    signal_stop = true;
                    continue;
                }

                // 走らせる。
                List<SCSerikoSeqPatternEntry> patterns = seq.Patterns();

                /*
                  必要ならexclusiveロック。
                  talkやyen-eごと止まるとまずいからスレッド自体は止めないが、
                  それだとalwaysなんかが他に動いていたらそれのブロックが出来ない。
                  ま、これはそのうちなんとかします。まだ実家の仕様も固まってないし。
                */
                if (exc_lock)
                {
                    seriko.SetExclusiveLocked(true);
                    seriko.InterruptAllRunners(hontai);
                    seriko.TerminateAllRunnersOverlay(hontai);

                    // Logger.log(this, Logger.DEBUG, "Exclusive Locked");
                }

                int n_patterns = patterns.Count();
                for (int i = 0; i < n_patterns; i++)
                {
                    if (signal_stop)
                    {
                        break; // 終了指示が出ていたら終了する。
                    }
                    // 他のスレッドによつてexclusiveロックされてゐたら解除を待つ。
                    if (!exc_lock)
                    {
                        waitForUnlocked();
                    }

                    Object obj = patterns.ElementAt(i);
                    if (obj == null)
                    {
                        continue; // パディングされてた
                    }
                    SCSerikoSeqPatternEntry pat = (SCSerikoSeqPatternEntry)obj;

                    // メソッド判別
                    String method = pat.Method();
                    if (method.Equals("overlay") || method.Equals("overlayfast"))
                    { // どちらも同じ動作
                      //  if (pat.Surfaceid() == -1)

                        if (pat.Surfaceid() == -1)
                        { // Terminate
                            if (layer == null)
                            {
                                
                                layer.SetVis(false);
                                target.SetViewsNeedDisplay(false);
                                target.GetWindow().Display();
                               
                            }
                           
                        }
                        else if (pat.Surfaceid() == -2)
                        { // Terminate all
                            seriko.TerminateAllRunnersOverlay(hontai);
                        }
                        else
                        {
                            if (layer == null)
                            {
                                layer = target.GetView().AddLayer(
                                shell.GetSurfaceServer(),
                                pat.Surfaceid(),
                                seq.id(),
                                new NSPoint(pat.Offsetx(), pat.Offsety()),
                                true);
                            }
                            else
                            {
                                layer.SetImage(shell.GetSurfaceServer(), pat.Surfaceid());
                                layer.SetLoc(new NSPoint(pat.Offsetx(), pat.Offsety()));
                                layer.SetVis(true);
                            }

                          
                            target.GetWindow().Display();
                            target.SetViewsNeedDisplay(true);
                        }
                    }
                    else if (method.Equals("move"))
                    {
                        double ratio = target.GetRatio();
                        target.SetOffset((float)(pat.Offsetx() * ratio), (float)(pat.Offsety() * -1 * ratio));
                    }
                    else if (method.Equals("base"))
                    {
                        if (pat.Surfaceid() == -1)
                        {
                            target.TempChangeBase(shell.GetSurfaceServer(), target.BaseSurfaceId());
                        }
                        else
                        {
                            target.TempChangeBase(shell.GetSurfaceServer(), pat.Surfaceid());
                        }
                    }
                    else if (method.Equals("start"))
                    {
                        seriko.Execute(pat.Seq_id(), hontai);
                    }
                    else if (method.Equals("alternativestart"))
                    {

                        int select = StaticMethod.Rand(pat.Entries().Count());
                         
                        seriko.Execute((pat.Entries().ElementAt(select)), hontai);
                    }

                    // ウエイトをかける
                    if (pat.Interval() > 0)
                    {
                        try
                        {
                            Sleep(pat.Interval());
                        }
                        catch (ThreadInterruptedException e)
                        {
                            if (signal_stop)
                            {
                                break; // forループ終了→whileループ終了
                            }
                        }
                    }
                }

                // 必要ならexclusive復帰。
                if (exc_lock)
                {
                    seriko.SetExclusiveLocked(false);
                    seriko.InterruptAllRunners(hontai);

              //      Logger.log(this, Logger.DEBUG, "Exclusive Unlocked");
                }

                // 終了するなら終了。
                if (!signal_stop)
                {
                    signal_stop = end_and_quit;
                }
             //   NSAutoreleasePool.pop(inner_pool);
            }

            // レイヤー作ってあったら削除
            if (layer != null)
            {
                target.GetView().RemoveLayer(layer);
                target.SetViewsNeedDisplay(false);
                target.GetWindow().Display();
               
            }
          //  NSAutoreleasePool.pop(pool);
        }

        protected void waitForUnlocked()
        {
            // ロック中なら解除されるまで待つ。
            if (seriko.IsExclusiveLocked())
            {
                // レイヤーがあれば隠す
                if (layer != null)
                {
                    layer.SetVis(false);
                    target.SetViewsNeedDisplay(true);
                }

               // Logger.log(this, Logger.DEBUG, "Waiting for exclusive unlocked...");
                while (seriko.IsExclusiveLocked())
                {
                    try
                    {
                        Sleep(200);
                    }
                    catch (Exception e) { }
                }
          //      Logger.log(this, Logger.DEBUG, "Exclusive lock is no longer locked. I'll go ahead. ");

                if (layer != null)
                {
                    layer.SetVis(true);
                    target.SetViewsNeedDisplay(true);
                }
            }
        }

        public void TerminateOverlay()
        {
            // overlayを使わないシーケンスだったら無視。

            if (layer != null)
            {
                layer.SetVis(false);
                target.GetWindow().Display();
                target.SetViewsNeedDisplay(true);
            }
        }

        public void TerminateThread()
        {
            if (!IsAlive())
            {
                    return;
            }
            signal_stop = true;
            Interrupt();
            while (IsAlive())
            {
            }
            
        }

         public bool IsAlive()
        {
            return thread != null && thread.IsAlive;

        }

        public void Interrupt()
        {
            thread.Interrupt();
            //thread = null;
        }





        public bool IsHontai()
        {
            return hontai;
        }

        public int GetSeqID()
        {
            return seq.id();
        }

        public String ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("sequence: {").Append(seq).Append('}');
            if (layer != null)
            {
                buf.Append("; with layer");
            }
            buf.Append("; scope: ").Append(hontai ? 0 : 1);
            return buf.ToString();
        }


    }
}

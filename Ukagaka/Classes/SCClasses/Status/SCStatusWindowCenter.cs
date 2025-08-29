using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;
using Cocoa.AppKit;

namespace Ukagaka
{
 

    public class SCStatusWindowCenter
    {
        public static int WAIT_BEFORE_CLOSE = 6000; // 毫秒

        private ArrayList stats; // Content: (SCStatusWindowController)状态窗口，按顺序排列。

        public static SCStatusWindowCenter shared = new SCStatusWindowCenter();

        public SCStatusWindowCenter()
        {
            stats = new ArrayList();

        }

        public SCStatusWindowController NewStatusWindow()
        {
            // 创建并注册
            SCStatusWindowController stat = null;
            // 在后台线程中调用：
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                stat = new SCStatusWindowController(this);
            });

            Hashtable refcon = new Hashtable();
            stat.SetRefcon(refcon);
            stats.Add(stat);

            // 移动
            float screentop = NSScreen.MainScreen.VisibleFrame.Y().Value + NSScreen.MainScreen.VisibleFrame.Height().Value;
            float windowheight = stat.Window.Frame().Height().Value;
            float y = (screentop - (windowheight * stats.Count)) + 1;
            refcon.Add("virtual_y", (int)y);
            stat.Window.SetFrameOrigin(new NSPoint(0, y));

            return stat;
        }

        public void CloseStatusWindow(SCStatusWindowController stat)
        {
            (new AnimationFadeOut(stat)).Start();
        }

        protected void RemoveStatAndAlignAllStats(SCStatusWindowController statToRemove)
        {
            stats.Remove(statToRemove);

            float current_top = NSScreen.MainScreen.VisibleFrame.Y().Value + NSScreen.MainScreen.VisibleFrame.Height().Value;

            int n_stats = stats.Count;
            for (int i = 0; i < n_stats; i++)
            {
                SCStatusWindowController stat = (SCStatusWindowController)stats[i];
                current_top = current_top - stat.Window.Frame().Height().Value + 1;

                Hashtable refcon = (Hashtable)stat.GetRefcon();
                if (((int)refcon["virtual_y"]) != (int)current_top)
                {
                    refcon["virtual_y"] = (int)current_top; // 内部的なウインドウ位置。実際にはアニメーションで変化する。

                    if (refcon["mover"] != null && ((AnimationMove)refcon["mover"]).IsAlive())
                    {
                        ((AnimationMove)refcon["mover"]).Terminate();
                    }

                    AnimationMove mover = new AnimationMove(stat, 0, (int)current_top);
                    refcon["mover"] = mover;
                    mover.Start();
                }
            }
        }

        protected class AnimationFadeOut
        {
            private SCStatusWindowController stat;

            private Thread thread;



            public AnimationFadeOut(SCStatusWindowController stat)
            {
                this.stat = stat;

                thread = new Thread(Run);


            }
            public void Sleep(int ms)
            {
                Thread.Sleep(ms);

            }

            public void Start()
            {

                thread.Start();
            }

            public void Run()
            {
               // int pool = NSAutoreleasePool.Push();

                try 
                { 
                    Sleep(WAIT_BEFORE_CLOSE);
                }
                catch (Exception e)
                {

                }
                SCStatusWindowCenter.shared.RemoveStatAndAlignAllStats(stat);

                int fade_out_step = 3;
                for (int i = fade_out_step; i >= 0; i--)
                {
                    stat.Window.AlphaValue = (float)i / (float)fade_out_step;
                }

                Hashtable refcon = (Hashtable)stat.GetRefcon();
                if (refcon["mover"] != null && ((AnimationMove)refcon["mover"]).IsAlive())
                {
                    ((AnimationMove)stat.GetRefcon()["mover"]).Terminate();
                }
                stat.Close();

                //NSAutoreleasePool.Pop(pool);
            }
        }

        protected class AnimationMove
        {
            private SCStatusWindowController stat;
            private int destX, destY;

            private volatile bool flagTerm = false;

            private Thread thread;


            public AnimationMove(SCStatusWindowController stat, int destX, int destY)
            {
                this.stat = stat;
                this.destX = destX;
                this.destY = destY;
                thread = new Thread(Run);
            }

            public void Start()
            {
                thread.Start();
            }

            public bool IsAlive()
            {
                return thread.IsAlive;
            }



            // 只执行从下到上的动画。
            public void Run()
            {
                //int pool = NSAutoreleasePool.Push();

                NSWindow targetWindow = stat.Window;
                NSPoint point = new NSPoint();
                bool done = false;
                int Y = (int)stat.Window.Frame().Y().Value;
                int speedY = 0;
                int turnY = (int)(Y + ((destY - Y) / 2)); // この地点を過ぎたら加速度が逆になる。
                while (!flagTerm && !done)
                {
                    int accY = (Y > turnY ? -1 : 1);
                    speedY += accY;

                    int nextY = Y + speedY;
                    if (nextY <= Y) nextY = Y + 1;
                    if (nextY >= destY)
                    {
                        nextY = destY;
                        done = true;
                    }

                    point.SetX(destX);
                    point.SetY(nextY);
                    targetWindow.SetFrameOrigin(point);

                    Y = nextY;
                }

               // NSAutoreleasePool.Pop(pool);
            }

            public void Terminate()
            {
                flagTerm = true;
               // while (IsAlive) { }
            }
        }
    }

}

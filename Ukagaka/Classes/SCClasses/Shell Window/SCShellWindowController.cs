using System;
 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
 
using System.Windows.Input;
using Cocoa.AppKit;
using Ukagaka;

namespace Ukagaka
{
    public class SCShellWindowController:NSWindowController
    {

        //public string shellPath = "/ghost/Taromati2/shell/Remilia Scarlet";


        public Dictionary<int, SCSurface> surfaces;

        public NSImage baseImg;
        public int currentSurfaceID = 0;
        // public Dictionary<int, SCSerikoSeqPatternEntry> currentAnimation = new Dictionary<int, SCSerikoSeqPatternEntry>;
        // var queue:OperationQueue=OperationQueue()
        public NSImage currentimg;
        public int scope;

        public SCShellWindowController(int scope)
        {
           
            Init(scope);
        }


        public void Init(int scope)
        {
            this.scope = scope;
            LoadSurfaceImage();
            LoadDescript();
        }


        private void LoadSurfaceImage()
        {



        }


        private void LoadDescript()
        {



        }






        SCShellView view;
        SCShellWindow window;
        SCSession session;
        SCSurface surface = null;
        SCSurface basesurface = null; // changeSurfaceでは変化するがtempChangeBaseでは変化しない。
        double ratio = 1.0;
        float baseHorizLoc; // SERIKOのmoveで動かされない、ドラッグで決定された横位置。
        int type; // 本体かうにゅうか。



        public SCShellWindowController(SCSession s, int type)
        {
            // super("ShellWindow");
            //  window();

            window = new SCShellWindow();
          //  base.Window = window;
            session = s;
            this.type = type;
            // this.VisualOffset = new Vector(X(), Y());
            view = new SCShellView(new NSRect());
            // 
            //Window.initialize(s, this);

            window.initialize(s, this);
            window.SetFrameOrigin(new NSPoint(X(), 0));
       //      window.setAutodisplay(true);

            // 
            view.Initialize(this);
            

            window.View.Children.Add(view);
        }

      


        /*   public SCShellWindowController(SCSession s, int type)
           {
               super("ShellWindow");
               window();

               session = s;
               this.type = type;

               window().setFrameOrigin(new NSPoint(x(), 0));
               window().setAutodisplay(true);

               ((SCShellWindow)window()).initialize(s, this);
               view.initialize(this);
           }
           */
        public void ChangeSurface(SCSurfaceServer sserver, int id)
        {
            // idは-1である可能性もある。
            if (id == -1)
            {
                // 表示領域のサイズは前回のものを引繼ぐ。
                view.ChangeSurface(null);
                basesurface = surface = null;
            }
            else
            {
                basesurface = surface = sserver.findSurface(id, type);
                if (surface == null)
                {
                    return;
                }
                NSImage image = surface.GetImage();
                if (image == null)
                {
                    return;
                }
                // 座標にbaseHorizLocを使用する事で、offsetを無視する。
                NSRect newrect = new NSRect((int)baseHorizLoc, 0, image.Size().Width().IntValue(), image.Size().Height().IntValue());
                
                window.SetFrame(newrect);


                Application.Current.Dispatcher.Invoke(() =>
                {
                    window.Show();
                });

                view.ChangeSurface(image);
            }

            view.Display();
           
           // window.image.Source = view.Source;
          
            // session.recalcBalloonsLoc();
            //  window.setHasShadow(false);
        }

   


        public void TempChangeBase(SCSurfaceServer sserver, int id)
        {
            if (sserver == null)
            {
                return;
            }

            // SERIKOのbaseメソッドで使われます。
            // ベースサーフィス情報が変わらずに、一時的にベースサーフィスが差し替えられます。
            surface = sserver.findSurface(id, type);
            if (surface == null)
            {
                return;
            }
            NSImage image = surface.GetImage();
            if (image == null)
            {
                return;
            }
            //NSRect newrect = new NSRect(NSPoint.ZeroPoint,image.Size());
            //NSRect oldrect = window().frame();
            //newrect = new NSRect(newrect.rectByOffsettingRect(oldrect.x(),oldrect.y()));
            // offsetを継承する。
            NSRect newrect = new NSRect(window.Frame().Origin(), image.Size());

            window.SetFrame(newrect);
            view.ChangeSurface(image);

            view.Display();
          //  session.recalcBalloonsLoc();
          //  window.setHasShadow(false);
        }

        public void ReloadSurface(SCSurfaceServer sserver)
        {
            ChangeSurface(sserver, surface.GetID());
        }

        public void SurfaceResized(SCSurfaceServer sserver, double r)
        {
            // リサイズされた時に呼んでください。
            ratio = r;
            if (surface != null)
            {
                ChangeSurface(sserver, surface.GetID());
            }
             
        }

        public void SetLevel(int level)
        {
          //  window.setLevel(level);
        }
        
        public void Show()
        {
            if (window.IsVisible) 
            {
                return;
            }
            // ここらへんは、もう何というか、魔術。  
            //showWindow(null);
            /*
            NSTimer open_timer = new NSTimer(
                0.0,
                this,
                new NSSelector("showWindow", null),
	    null,
	    false);*/
            //SCFoundation.SharedFoundation().GetRunLoopOfMainThread().addTimerForMode(open_timer, NSRunLoop.DefaultRunLoopMode);
            if (!Thread.CurrentThread.Name.Equals("main"))
            {
                while (!window.IsVisible)
                {
                } // 待機
            }
    // SERIKO再開
    //session.getCurrentShell().getSeriko().restart(
    //    type == SCFoundation.SAKURA);

    // 確實にredraw
            window.SetViewsNeedDisplay(true);
    }

        public void Hide()
        {
            if (!window.IsVisible)
            {
                return;
            }
            // マルチスレッドのAppKitの罠 
            window.OrderOut(null);

            /*
            NSTimer close_timer = new NSTimer(
              0.0,
              window(),
              new NSSelector("orderOut", new Class[] { NSNull.class}),
	  new Object[] {NSNull.nullValue()
},
	  false);
SCFoundation.sharedFoundation().getRunLoopOfMainThread().addTimerForMode(close_timer, NSRunLoop.DefaultRunLoopMode);

*/
            if (!Thread.CurrentThread.Name.Equals("main"))
            {
                while (window.IsVisible) 
                {

                } // 待機 
            }
// SERIKOのスレッドを止める。
            session.GetCurrentShell().GetSeriko().Stop(
             type == SCFoundation.SAKURA);

            basesurface = surface = null;
        }

        public override void Close()
        {
            base.Close();

            view.CleanUp();
            view = null;
            session = null;
            surface = null;
            basesurface = null;
        }

        public NSRect Frame()
        {
            return window.Frame();
        }

        public float Width()
        {
            return (float)window.Frame().Width().Value;
        }

        public float Height()
        {
            return (float)window.Frame().Height().Value;
        }

        public float X()
        {
            return (float)window.Frame().X().Value;
        }

        public float Y()
        {
            return (float)window.Frame().Y().Value;
        }

        public void SetHorizLoc(float x)
        {
            // x座標を設定します。
            baseHorizLoc = x;
            //  window.VisualOffset = new Vector(x, Y());


            window.SetFrameOrigin(new NSPoint(x, Y()));
        }

        public void AbsoluteSetLoc(float x, float y)
        {
            // 指定されたとおりにx,y座標を設定します。
            baseHorizLoc = x;
            // this.VisualOffset = new Vector(x, y);
            window.SetFrameOrigin(new NSPoint(x, y));
        }

        public void SetOffset(float x, float y)
        {
            // SERIKO move専用。
            // moveに無関係に決定された位置からの差分で座標変更します。
            window.SetFrameOrigin(new NSPoint(baseHorizLoc + x, y)); // yのベースは常にゼロ。

            // this.VisualOffset = new Vector(baseHorizLoc + x, y);

        }

        public void relativeSetLoc(float x, float y)
        {
            // パラメータを現在の値からの相対値として解釈します。
            window.SetFrameOrigin(new NSPoint(X() + x, Y() + y));
        }

        public void SetViewsNeedDisplay(bool Bool)
        {
            // NSWindow#setViewsNeedDisplay()が何故か機能しないので自分で実装する。
            // 実装するといっても一行で済んでしまうが。
            window.ContentView().SetNeedsDisplay(Bool);
        }

        public int CurrentSurfaceId()
        {
            return (surface == null ? -1 : surface.GetID());
        }

        public SCSurface CurrentSurface()
        {
            return surface;
        }

        public int BaseSurfaceId()
        {
            return (basesurface == null ? -1 : basesurface.GetID());
        }

        public SCSurface BaseSurface()
        {
            return basesurface;
        }

        public void SetBalloon(SCSafetyBalloonController balloon)
        {
            window.SetBalloon(balloon);
            // base.setBalloon(balloon);
        }

        public double GetRatio()
        {
            return ratio;
        }

        public SCShellWindow GetWindow()
        {
            return window;
            //return (SCShellWindow)this;
        }

        public SCShellView GetView()
        {
            return view;
        }

        public int GetType()
        {
            return type;
        }

        public SCSession Session()
        {
            return session;
        }

        public String ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("session: {").Append(session).Append('}');
            buf.Append("; ratio: ").Append(ratio);
            buf.Append("; type: ").Append(type);
            return buf.ToString();
        }

        protected void Finalize()
        {
            // Logger.log(this, Logger.DEBUG, "finalized");
        }











    }
}

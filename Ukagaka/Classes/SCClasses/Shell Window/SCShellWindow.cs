using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Cocoa.AppKit;
using System.Threading;
using Utils;
using System.Windows.Threading;
namespace Ukagaka
{
    /// <summary>
    /// SCShellWindowController.xaml オトスササ．ツ゜シュ
    /// </summary>
    public partial class SCShellWindow : NSWindow
    {




        private SCSafetyBalloonController balloonBelongsTo = null; // このシェルウインドウに所属するバルーン。一緒に動かすから。
        private int initial_shell_horiz_offset, initial_shell_vert;

        private static long INTERVAL_BETWEEN_DOWN_AND_UP = 500; // msec
        private static long INTERVAL_BETWEEN_CLICK_AND_CLICKEVENT = 200; // msec

        SCSession session;
        SCShellWindowController controller;
        SCShellView view;
        bool hasToNotifyMoving = false;

        long lastMouseDown, lastMouseUp;
        NSTimer clickTimer; // 本当はNSTimerの方がコードが短くて済むが、動作が不安定ですぐ落ちる。

        

        Thread thread;
        public SCShellWindow()
        {
            InitializeComponent();
            this.ShowInTaskbar = false;
            this.Owner = Window.GetWindow(MainWindow.SharedMainWindow());
            SetOpaque(false);
            //  this.image = new Image();
        }



        public SCShellWindow(NSRect contentRect, int styleMask, int backingType, bool defer)
        {

            // setBackgroundColor(NSColor.clearColor());
            //   setOpaque(false);
            //  setHasShadow(false);
        }

        public bool canBecomeKeyWindow()
        {
            return true;
        }

        public void initialize(SCSession s, SCShellWindowController c)
        {
            session = s;
            controller = c;
            view = c.GetView();
            //    this.Grid.Children.Add(view);
            clickTimer = new NSTimer();

            /*setAcceptsMouseMovedEvents(controller.getType() == SCFoundation.HONTAI); // 何故か２つ以上のウインドウにこれを設定しても
            // 最後の一つにしか送られない。*/
            //  setAcceptsMouseMovedEvents(true);

            // NSMutableArray pboardTypes = new NSMutableArray();
            //  pboardTypes.addObject(NSPasteboard.FilenamesPboardType);
            //  pboardTypes.addObject(NSPasteboard.StringPboardType);
            //  registerForDraggedTypes(pboardTypes);
        }

        public void Display()
        {

            view.Display();

        }


        public void Close()
        {
            // base.close();

            balloonBelongsTo = null;
            session = null;
            controller = null;
            clickTimer = null;
            //timerClickTask = null;
        }

        public void SetBalloon(SCSafetyBalloonController balloon)
        {
            balloonBelongsTo = balloon;
        }




        /*
        public override void SetFrameOrigin(NSPoint point)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {

                this.VisualOffset = new Vector(point.X().Value, point.Y().Value);
            }));
        }
       
        public void SetFrameSize(NSSize size)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {

                this.width = size.Width().Value;
                this.height = size.Height().Value;
            }));
        }


        public void SetFrame(NSRect rect)
        {
            SetFrameOrigin(rect.Origin);
            SetFrameSize(rect.Size);
        }




         */


        public override void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            lastMouseDown = SystemTimer.GetTimeTickCount();

            // ウインドウ移動準備。
            // NSPoint initial_pt =  e.GetPosition((IInputElement)sender)  //theEvent.window().convertBaseToScreen(theEvent.locationInWindow());

            NSPoint initial_pt = this.Frame().Origin();

            initial_shell_horiz_offset = (int)(initial_pt.X().Value - this.Margin.Left);
            initial_shell_vert = (int)this.Margin.Top;
        }



        public override void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }


        public override void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        { 
            if (this.IsDoubleClicked)
            {
                this.IsDoubleClicked = false;
                
                DoubleClicked(sender, e);
                e.Handled = true;//这里注意e.Handled = true，添加事件处理不然会与DragMove();冲突引发异常
            }











            //if (SystemTimer.GetTimeTickCount() - lastMouseDown < INTERVAL_BETWEEN_DOWN_AND_UP)
            { 
              /*  if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
                {
                    DoubleClicked(sender, e);
                    e.Handled = true;//这里注意e.Handled = true，添加事件处理不然会与DragMove();冲突引发异常
                }
                 */
            }

            if (hasToNotifyMoving)
            {
                //session.shellWindowMoved(controller);
                hasToNotifyMoving = false;
            }
        }



        private void Clicked(object sender, MouseButtonEventArgs e)
        {
            //timerClickTask = null; // クリーンアップ

            SendMouseEvent("OnMouseClick", sender,e);
        }

        private void DoubleClicked(object sender, MouseButtonEventArgs e)
        {

            SendMouseEvent("OnMouseDoubleClick", sender, e);

        }

        public SCShellView ContentView()
        {
            return view;

        }
        private void SendMouseEvent(string type, object sender, MouseButtonEventArgs e)
        {
            double ratio = controller.GetRatio();
            NSPoint localLocForWindow = new NSPoint(this.Margin.Left, this.Margin.Top);
            NSPoint localLocForSurface = new NSPoint((localLocForWindow.X().Value / ratio), (localLocForWindow.Y().Value / ratio));
            string strX = localLocForSurface.X().ToString();
            string strY = (controller.CurrentSurface().GetOriginalSize().Height().Value - localLocForSurface.Y().Value).ToString();
            String wheel = "";
            String owner = (controller.GetType() == SCFoundation.HONTAI ? "0" : "1");
            String collision = controller.CurrentSurface().GetCollisionNameAt(localLocForSurface);
            if (collision == null)
            {
                collision = "";
            }
            //session.doShioriEvent(type,new String[] {strX,strY,wheel,owner,collision});
            session.MouseEventOccuredOnShell(type, strX, strY, wheel, owner, collision);
        }

        /*
        public int draggingEntered(NSDraggingInfo draggingInfo)
        {
            // 複数の項目をDnDしようとしていたら蹴る。
            // passive modeでも蹴る。
            if (session.isInPassiveMode()) return NSDraggingInfo.DragOperationNone;

            NSArray plist_files = (NSArray)draggingInfo.draggingPasteboard().propertyListForType(NSPasteboard.FilenamesPboardType);
            if (plist_files != null)
            {
                if (plist_files.count() == 1)
                {
                    return NSDraggingInfo.DragOperationGeneric;
                }
                else if (plist_files.count() > 1)
                {
                    return NSDraggingInfo.DragOperationNone;
                }
            }

            String str = draggingInfo.draggingPasteboard().stringForType(NSPasteboard.StringPboardType);
            if (str != null)
            {
                return (str.startsWith("http://") ? NSDraggingInfo.DragOperationGeneric : NSDraggingInfo.DragOperationNone);
            }

            return NSDraggingInfo.DragOperationNone;
        }
     



        public int draggingUpdated(NSDraggingInfo draggingInfo)
        {
            return NSDraggingInfo.DragOperationGeneric;
        }

        public void draggingExited(NSDraggingInfo draggingInfo)
        {

        }

        public bool prepareForDragOperation(NSDraggingInfo aDraggingInfo)
        {
            NSArray plist = (NSArray)aDraggingInfo.draggingPasteboard().propertyListForType(NSPasteboard.FilenamesPboardType);
            if (plist != null)
            {
                String fpath = (String)plist.objectAtIndex(0);
                File f = new File(fpath);

                if (f.isFile() && (fpath.toLowerCase().endsWith(".zip") || fpath.toLowerCase().endsWith(".nar")))
                {
                    SCFoundation.sharedFoundation().install(fpath, controller.session());
                }
                else if (f.isDirectory() && SCNarMaker.hasManifestFile(f))
                {
                    SCFoundation.sharedFoundation().makeNar(fpath, controller.session());
                }
                else if (f.isDirectory() && SCUpdateDataMaker.hasDauFile(f))
                {
                    SCFoundation.sharedFoundation().makeDau(fpath, controller.session());
                }
                return true;
            }

            String str = aDraggingInfo.draggingPasteboard().stringForType(NSPasteboard.StringPboardType);
            if (str != null)
            {
                SCFoundation.sharedFoundation().downloadAndInstall(str, controller.session());
            }
            return false;
        }
           


        public bool performDragOperation(NSDraggingInfo aDraggingInfo)
        {
            return true;
        }

        public void concludeDragOperation(NSDraggingInfo draggingInfo)
        {

        }
       


        public void mouseDown(NSEvent theEvent)
        {
            lastMouseDown = System.currentTimeMillis();

            // ウインドウ移動準備。
            NSPoint initial_pt = theEvent.window().convertBaseToScreen(theEvent.locationInWindow());

            initial_shell_horiz_offset = (int)(initial_pt.x() - theEvent.window().frame().x());
            initial_shell_vert = (int)theEvent.window().frame().y();
        }

        public void mouseDragged(NSEvent theEvent)
        {
            if (System.currentTimeMillis() - lastMouseDown < INTERVAL_BETWEEN_DOWN_AND_UP)
            {
                // クリック判定期間中ならドラッグは行わない。
                return;
            }

            NSWindow window = theEvent.window();

            // ウインドウを移動。ただし水平方向のみ。
            hasToNotifyMoving = true;
            NSPoint new_loc = window.convertBaseToScreen(window.mouseLocationOutsideOfEventStream());

            int new_x;
            new_x = (int)(new_loc.x()) - initial_shell_horiz_offset;
            controller.absoluteSetLoc(new_x, initial_shell_vert);

            session.recalcBalloonsLoc();
        }

        public void mouseUp(NSEvent theEvent)
        {
            lastMouseUp = System.currentTimeMillis();
            if (System.currentTimeMillis() - lastMouseDown < INTERVAL_BETWEEN_DOWN_AND_UP)
            {
                if (timerClickTask == null)
                { // クリックされた。
                  // ダブルクリック認識期間を過ぎた後にクリック判定が確定する。
                    timerClickTask = new ClickTimerTask(theEvent);
                    clickTimer.schedule(timerClickTask, INTERVAL_BETWEEN_CLICK_AND_CLICKEVENT);
                   //NSTimer open_timer = new NSTimer(
                    //  INTERVAL_BETWEEN_CLICK_AND_CLICKEVENT / 1000.0, // 単位をsecに。
                   //   this,
                  //    new NSSelector("clicked",new Class[] {NSEvent.class}),
                  //    new Object[] {theEvent},
                  //    false);
                  //    SCFoundation.sharedFoundation().getRunLoopOfMainThread().addTimerForMode(open_timer,NSRunLoop.DefaultRunLoopMode);
                }
                else
                { // ダブルクリックされた。
                    doubleClicked(theEvent);
                }
            }

            if (hasToNotifyMoving)
            {
                session.shellWindowMoved(controller);
                hasToNotifyMoving = false;
            }
        }

        public void mouseMoved(NSEvent theEvent)
        {
            // 本体とうにゅうのどちらがキーウインドウになっていても、キーでない側でのmouseMovedに反応出来るhack。
            // 本来はResponderChainの根本たるNSApplicationのdelegateとしてmouseMovedを処理すべきかも知れないが
            // NSApplicationのdelegateには何故かイベントが到達しない！
            //if (session.getHontai().getWindow().frame().containsPoint(NSEvent.mouseLocation(),false))
            //  {
           //   sendMouseMovedEvent(SCFoundation.HONTAI);
           //   }
           //   else if (session.getUnyuu().getWindow().frame().containsPoint(NSEvent.mouseLocation(),false))
           //   {
           //   sendMouseMovedEvent(SCFoundation.UNYUU);
           //   }
            if (controller == null) return;
            if (!frame().containsPoint(NSEvent.mouseLocation(), false)) return;

            switch (controller.getType())
            {
                case SCFoundation.SAKURA:
                    sendMouseMovedEvent(SCFoundation.SAKURA);
                    break;

                case SCFoundation.KERO:
                    sendMouseMovedEvent(SCFoundation.KERO);
                    break;
            }
        }
        private void sendMouseMovedEvent(int windowType)
        {
            SCShellWindowController wc = (windowType == SCFoundation.HONTAI ? session.getHontai() : session.getUnyuu());
            double ratio = wc.getRatio();
            NSPoint localLocForWindow = wc.getWindow().convertScreenToBase(NSEvent.mouseLocation());
            NSPoint localLocForSurface = new NSPoint((float)(localLocForWindow.x() / ratio), (float)(localLocForWindow.y() / ratio));
            String strX = Integer.ToString((int)localLocForSurface.x());
            String strY = Integer.ToString((int)wc.currentSurface().getOriginalSize().Height() - (int)localLocForSurface.y());
            String wheel = "";
            String owner = (wc.getType() == SCFoundation.HONTAI ? "0" : "1");
            String collision = wc.currentSurface().getCollisionNameAt(localLocForSurface);
            if (collision == null) collision = "";
            //session.doShioriEvent("OnMouseMove",new String[] {strX,strY,wheel,owner,collision});
            session.mouseEventOccuredOnShell("OnMouseMove", strX, strY, wheel, owner, collision);
        }

        public void scrollWheel(NSEvent theEvent)
        {
            if (controller == null) return;
            if (!frame().containsPoint(NSEvent.mouseLocation(), false)) return;

            switch (controller.getType())
            {
                case SCFoundation.SAKURA:
                    sendMouseWheelEvent(SCFoundation.SAKURA, theEvent.deltaY());
                    break;

                case SCFoundation.KERO:
                    sendMouseWheelEvent(SCFoundation.KERO, theEvent.deltaY());
                    break;
            }
        }
        private void sendMouseWheelEvent(int windowType, float deltaY)
        {
            if ((int)Math.abs(Math.round(deltaY)) <= 1) return; // 絶対値が1以下なら無視する。

            SCShellWindowController wc = (windowType == SCFoundation.HONTAI ? session.getHontai() : session.getUnyuu());
            double ratio = wc.getRatio();
            NSPoint localLocForWindow = wc.getWindow().convertScreenToBase(NSEvent.mouseLocation());
            NSPoint localLocForSurface = new NSPoint((float)(localLocForWindow.x() / ratio), (float)(localLocForWindow.y() / ratio));
            String strX = Integer.ToString((int)localLocForSurface.x());
            String strY = Integer.ToString((int)wc.currentSurface().getOriginalSize().Height() - (int)localLocForSurface.y());
            String wheel = Integer.ToString((int)Math.round(deltaY));
            String owner = (wc.getType() == SCFoundation.HONTAI ? "0" : "1");
            String collision = wc.currentSurface().getCollisionNameAt(localLocForSurface);
            if (collision == null) collision = "";

            //System.out.println(strX+","+strY+" wheel:"+wheel+" owner:"+owner+" collision:"+collision);
            //session.doShioriEvent("OnMouseWheel",new String[] {strX,strY,wheel,owner,collision});
            session.mouseEventOccuredOnShell("OnMouseWheel", strX, strY, wheel, owner, collision);
        }

        private void clicked(NSEvent theEvent)
        {
            timerClickTask = null; // クリーンアップ

            sendMouseEvent("OnMouseClick", theEvent);
        }

        private void doubleClicked(NSEvent theEvent)
        {
            timerClickTask.cancel();
            //timerClickTask.invalidate();
            timerClickTask = null;

            if ((theEvent.modifierFlags() & NSEvent.AlternateKeyMask) != 0)
            {
                // TEACHセッション開始
                session.DoTeach();
            }
            else if ((theEvent.modifierFlags() & NSEvent.CommandKeyMask) != 0)
            {
                // COMMUNICATEボックスを開く
                session.showCommunicateBox();
            }
            else
            {
                sendMouseEvent("OnMouseDoubleClick", theEvent);
            }
        }

        private void sendMouseEvent(String type, NSEvent nsevent)
        {
            double ratio = controller.getRatio();
            NSPoint localLocForWindow = nsevent.locationInWindow();
            NSPoint localLocForSurface = new NSPoint((float)(localLocForWindow.x() / ratio), (float)(localLocForWindow.y() / ratio));
            String strX = Integer.ToString((int)localLocForSurface.x());
            String strY = Integer.ToString((int)controller.currentSurface().getOriginalSize().Height() - (int)localLocForSurface.y());
            String wheel = "";
            String owner = (controller.getType() == SCFoundation.HONTAI ? "0" : "1");
            String collision = controller.currentSurface().getCollisionNameAt(localLocForSurface);
            if (collision == null) collision = "";
            //session.doShioriEvent(type,new String[] {strX,strY,wheel,owner,collision});
            session.mouseEventOccuredOnShell(type, strX, strY, wheel, owner, collision);
        }

        private class ClickTimerTask extends TimerTask
        {
            NSEvent eventToPost;

            public ClickTimerTask(NSEvent eventToPost) {
                super();
                this.eventToPost = eventToPost;
            }

            public void Run()
        {
            int pool = NSAutoreleasePool.push();
            clicked(eventToPost);
            NSAutoreleasePool.pop(pool);
        }

   
    }
     */



         

    }
}

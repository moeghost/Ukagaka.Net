using System;
using System.Linq;
using System.Windows;
using Cocoa.AppKit;
using Ukagaka;
using System.Threading;
using System.Collections;
using System.Windows.Input;
using Utils;
using System.Text;
namespace Ukagaka
{
    public class SCSafetyBalloonController : NSWindowController
    {
        bool do_fade_out = true;
        int fade_out_step = 5;

        SCSession session;
        bool atLeft = true; // trueならシェルの左にある。

        int type; // 本体かうにゅうか。SCFoundationの定数を使用。
        int num; // 現在のバルーン番号。

        SCBalloonSkinServer bsserver;
        SCBalloonSkin skin; // 現在のバルーン。これがnullの時はプリミティブバルーンが使用される。

        SCSafetyBalloonBackgroundView bgview;

        NSScrollView textScrollView;
        SCBalloonTextView textview;
        SCBalloonScrollArrow arrow_up, arrow_down;

        SCScriptRunner callback_target_runner;

        ArrayList alternatives;
        bool alternative_draw_underline;
        bool alternative_draw_square;
        NSColor alternative_pen_color;
        NSColor alternative_brush_color;
        // nullのままだったら、その時のフォント色の各色を
        // 1.0fから引いたものが用いられる。
        NSColor alternative_font_color;

        ArrayList anchors;
        NSColor anchor_color;


        SCBorderlessWindow window;


        public SCSafetyBalloonController(SCSession s, int t)
        {
            // SCSessionはnullであるかも知れない。
            // バルーンプレビュー時にはさうである。
            // super("SafetyBalloon");
            bgview = new SCSafetyBalloonBackgroundView();
            // 在后台线程中调用：
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                window = new SCBorderlessWindow();
            });
            //   window.setAcceptsMouseMovedEvents(true);
            //  window.setDelegate(this);
            //  window.setAutodisplay(true);
            //   window.useOptimizedDrawing(false);
            //textScrollView.SetDrawsBackground(false);

            textScrollView = new NSScrollView();

             arrow_up = new SCBalloonScrollArrow(this, SCBalloonSkinServer.ARROW_UP);
             arrow_down = new SCBalloonScrollArrow(this, SCBalloonSkinServer.ARROW_DOWN);
            window.ContentView().AddSubview(arrow_up);
             window.ContentView().AddSubview(arrow_down);

            //bgview.addSubview(arrow_up);
            //bgview.addSubview(arrow_down);
            // textview.initialize(arrow_up, arrow_down);
            // textview.updateArrowsVisibilities();
            textview = new SCBalloonTextView(window.Frame());
            session = s;
            type = t;
            window.View.Children.Add(bgview);
            SetType(0);
        }

        public void Close()
        {
            //base.close();
            session = null;
            bsserver = null;
            skin = null;
            arrow_up.CleanUp(); arrow_up = null;
            arrow_down.CleanUp(); arrow_down = null;
            callback_target_runner = null;
            if (alternatives != null)
            {
                alternatives.Clear();
            }
            if (anchors != null)
            {
                anchors.Clear();
            }
        }

        public void SetIgnoresMouseEvents(bool value)
        {
            if (SCFoundation.BALLOON_USES_CLICK_THROUGH || value == false)
            {
                window.SetIgnoresMouseEvents(value);
            }
        }

        public void FlushWindow()
        {
            textview.WaitUntilFlushed();
            window.FlushWindow();
        }

        public void ScrollWheel(NSEvent Event)
        {
            Scroll((float)Math.Round(Event.DeltaY()));
        }

        public void Scroll(float deltaY)
        {
            NSClipView clip = (NSClipView)textScrollView.ContentView();

            NSRect oldrect = clip.DocumentVisibleRect();
            NSPoint oldpoint = oldrect.Origin();
            float old_x = oldpoint.X().Value;
            float old_y = oldpoint.Y().Value;
            float max_vert_point = clip.DocumentRect().Height().Value - oldrect.Height().Value;

            float x = old_x;
            float y = old_y + deltaY;

            if (y < 0) y = 0;
            else if (y > max_vert_point) y = max_vert_point;

            clip.ScrollToPoint(new NSPoint(x, y));
            textview.UpdateArrowsVisibilities();
        }

        public override void MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (callback_target_runner != null)
            {
                // ダブルクリック(もしくはそれ以上)か？
                if (window.IsDoubleClicked)
                {
                    window.IsDoubleClicked = false;
                    callback_target_runner.Terminate(); // 強制停止
                }

                else
                {
                    callback_target_runner.SpeedUp(); // 再生速度を上げる
                }
            }
        }

        public override void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (callback_target_runner != null)
            {
                callback_target_runner.SpeedDown(); // 再生速度を下げる
            }
        }

        // 馬鹿トラッキング機構。このクラスはwindowのdelegateに指定されている。
        public override void MouseMoved(object sender, MouseEventArgs e)
        {
            if (!window.Frame().ContainsPoint(e.GetPosition((e.Source as FrameworkElement))))
            {
                return;
            }
            // 本来個々のNSViewからNSWindowまで辿るはずのイベントレスポンダーチェインだが
            // ここではわざわざ逆に投げる。
            if (alternatives != null)
            {
                int n_alternatives = alternatives.Count;
                for (int i = 0; i < n_alternatives; i++)
                {
                    NSView alternative = (NSView)alternatives.ToArray().ElementAt(i);

                    alternative.MouseMoved(sender, e);
                }
            }
            if (anchors != null)
            {
                int n_anchors = anchors.Count;
                for (int i = 0; i < n_anchors; i++)
                {
                    NSView anchor = (NSView)anchors.ToArray().ElementAt(i);

                    anchor.MouseMoved(sender, e);
                }
            }
        }

        public SCBalloonAlternative AddAlternative(string title, string param)
        {
            if (alternatives == null)
            { // これが一つ目の選択肢だったら
                alternatives = new ArrayList();
            }

            // 追加
            SCBalloonAlternative alt = new SCBalloonAlternative(
                title, textview.GetFont(), textview.GetMainColor(), alternative_font_color,
                alternative_pen_color, alternative_brush_color, alternative_draw_underline, alternative_draw_square, true);

            //new NSSelector("alterAction", new Class[] { Object.class})
            alt.SetAction(new NSSelector("alterAction", new Object()));
            alt.SetTarget(this);
            alt.SetRefcon(param);
            alternatives.Add(alt);

            textview.AddSubviewAtCurrentLoc(alt);

            SetIgnoresMouseEvents(false);

            return alt;
        }

        public void AddAnchor(string title, string id)
        {
            if (anchors == null)
            {
                anchors = new ArrayList();
            }
            SCBalloonAlternative anc = new SCBalloonAlternative(title, textview.GetFont(), anchor_color, anchor_color,
                                        anchor_color, anchor_color, true, false, false);


            //new NSSelector("anchorAction", new Class[] { Object.class})
            anc.SetAction(new NSSelector("anchorAction", new Object()));
            anc.SetTarget(this);
            anc.SetRefcon(id);
            anchors.Add(anc);

            textview.AddSubviewAtCurrentLoc(anc);

            SetIgnoresMouseEvents(false);
        }

        public void SetCallbackTarget(SCScriptRunner runner)
        {
            // 選択肢やアンカーがクリックされた時にコールバックを行うスクリプトランナーを設定する。
            callback_target_runner = runner;
        }

        public void AlterAction(Object sender)
        {
            // このメソッドは直接呼び出してはなりません。
            // 選択肢のActionとしてコールされます。
            callback_target_runner.AlternativeSelected((SCBalloonAlternative)sender);
        }

        public void AnchorAction(Object sender)
        {
            callback_target_runner.AnchorSelected((SCBalloonAlternative)sender);
        }

        private void RemoveAllAlternatives()
        {
            if (alternatives == null)
            {
                return;
            }
            int n_alts = alternatives.Count;
            for (int i = 0; i < n_alts; i++)
            {
                SCBalloonAlternative alt = (SCBalloonAlternative)alternatives.ToArray().ElementAt(i);
                alt.RemoveFromSuperview();
            }

            alternatives = null;
        }

        private void RemoveAllAnchors()
        {
            if (anchors == null) return;

            int n_ancs = anchors.Count;
            for (int i = 0; i < n_ancs; i++)
            {
                SCBalloonAlternative anc = (SCBalloonAlternative)anchors.ToArray().ElementAt(i);
                anc.RemoveFromSuperview();
            }

            anchors = null;
        }

        public void AddImage(string name, int x, int y)
        {
            // FIXME: 画像を直接描くよりはsubviewにした方が良い。後で。

            //System.out.println("adding balloon image to [" + x + "," + y + "]");
            string dir = session.GetGhostMasterDirPath();
            File f = new File(dir, name);
            NSImage img = new NSImage(f.GetPath(), true);
            //System.out.println("file name is " + f.GetPath());
            //System.out.println("file object is " + img.tostring());
            NSPoint p = new NSPoint(x, y);
            textview.DrawImage(img, p);
        }

        public void SetType(int num)
        {
            this.num = num;
            if (bsserver == null)
            {
                return;
            }
            // 左にあれば左用に、右にあれば右用に、それぞれ数値を修正。
            if (atLeft)
            {
                num = (num % 2 == 0 ? num : num - 1);
            }
            else
            {
                num = (num % 2 == 0 ? num + 1 : num);
            }

            skin = bsserver.findSkin(num, type);
            if (skin != null)
            {
                NSImage background = skin.getImage();
                window.SetContentSize(background.Size());
                SetBalloonAttributesFromDescription();
                //  window.SetHasShadow(false);
                bgview.SetBGImage(background);
                
                NSRect newrect = new NSRect((int)X(), Y(), background.Size().Width().IntValue(), background.Size().Height().IntValue());

                window.SetFrame(newrect);

                // 在后台线程中调用：
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    window.Show();

                });
                // textview.changedSize();
                //textview.display();
                //  textview.setNeedsDisplay(true);
            }
            else
            {
                bgview.SetBGImage(null);
                //   window.setHasShadow(true);

                NSSize newSize;
                if (type == SCFoundation.HONTAI)
                {
                    if (num == 0 || num == 1)
                    {
                        newSize = new NSSize(326, 169);
                    }
                    else
                    {
                        newSize = new NSSize(326, 384);
                    }
                }
                else
                {
                    newSize = new NSSize(326, 96);
                }
                window.SetContentSize(newSize);
                //  SetBalloonAttributesFromDescription();
                bgview.Redraw();
                textview.ChangedSize();
                 textview.Display();
                 textview.SetNeedsDisplay(true);
            }
        }

        public void SetBalloonServer(SCBalloonSkinServer bsserver)
        {
            this.bsserver = bsserver;
            SetType(num); // リロード。
        }

        public void SetTransparency(double t)
        {
            bgview.SetTransparency(t);
        }

        public void SetDoesFadeOut(bool b)
        {
            do_fade_out = b;
        }

        public void BeEmpty()
        {
            textview.BeEmpty();
            SetIgnoresMouseEvents(true);
        }

        public void MakeSSTPMsgEmpty()
        {
            bgview.SetSSTPMessage("");
            bgview.Redraw();
        }

        public void AddText(string text, long wait)
        {
            // waitミリ秒のウエイトをかけつつtextを一文字ずつ追加します。
            // ウエイトをかけるのでメインスレッド内から呼び出したりしないで下さい。
            // また、このメソッドを呼び出したスレッドにインタラプトが入った場合は、動作を中止します。
            int n_chars = text.Length;

            for (int i = 0; i < n_chars; i++)
            {
                AddChar(text.ToCharArray().ElementAt(i));
                if (wait > 0)
                {
                    Thread.Sleep((int)wait);
                }
            }
        }

        public void AddChar(char c)
        {
            textview.AddChar(c);
        }

        public void ShowSSTPMessage(string msg)
        {
            bgview.SetSSTPMessage(msg);
            bgview.Redraw();
        }

        public void SetLoc(NSPoint loc)
        {
            window.SetFrameTopLeftPoint(loc);
            CheckWentOut();
        }

        public void SetOrigin(NSPoint loc)
        {
            window.SetFrameOrigin(loc);
            CheckWentOut();
        }

        public void CheckWentOut()
        {
            // バルーンが見切れていれば反対側にまわす。
            // が、反対側にまわしても見切れていれば、見切れの度合いが少ないほうを選ぶ。
            if (session != null)
            {
                int screenWidth = (int)NSScreen.MainScreen.Frame.Width().IntValue();

                // 見切れていれば正の値。
                int left_mikire_width = -1 *
                session.SimulateHorizLoc(type == SCFoundation.HONTAI, true);
                // 見切れていれば正の値。
                int right_mikire_width = -1 *
                (screenWidth -
                 (session.SimulateHorizLoc(type == SCFoundation.HONTAI, false) +
                  (int)Width()));

                if (X() < 0)
                {
                    if (right_mikire_width <= 0 || left_mikire_width > right_mikire_width)
                    {
                        SetToRight();
                    }
                }
                else if (X() + Width() > screenWidth)
                {
                    if (left_mikire_width <= 0 || left_mikire_width < right_mikire_width)
                    {
                        SetToLeft();
                    }
                }
            }
        }

        public void SetToLeft()
        {
            if (atLeft)
            {
                return;
            }

            atLeft = true;
            SetType((num % 2 == 1 ? num - 1 : num));

            if (session != null)
            {
                session.RecalcBalloonsLoc();
            }
        }

        public void SetToRight()
        {
            if (!atLeft)
            {
                return;
            }

            atLeft = false;
            SetType((num % 2 == 0 ? num + 1 : num));

            if (session != null)
            {
                session.RecalcBalloonsLoc();
            }
        }

        public bool IsAtLeft()
        {
            return atLeft;
        }

        public float Width()
        {
            return window.Frame().Width().Value;
        }

        public float Height()
        {
            return window.Frame().Height().Value;
        }

        public float X()
        {
            return window.Frame().X().Value;
        }

        public float Y()
        {
            return window.Frame().Y().Value;
        }

        public NSSize Size()
        {
            return window.Frame().Size();
        }

        public void Show()
        {
            if (SCFoundation.LOCK_BALLOONS)
            {
                return;
            }
            if (window.IsVisible)
            {
                return;
            }
            //showWindow(null);
            NSTimer open_timer = new NSTimer(
            0.0,
            null,
            new NSSelector("showWindow", null),
            null,
            false);
            // SCFoundation.SharedFoundation().GetRunLoopOfMainThread().AddTimerForMode(open_timer, NSRunLoop.DefaultRunLoopMode);



            if (Thread.CurrentThread.Name != null)
            {

                if (!Thread.CurrentThread.Name.Equals("main"))
                {
                    while (!window.IsVisible)
                    {

                    } // 待機

                }
            }
            BeEmpty();
            bgview.Redraw();
        }

        public void Hide()
        {
            if (!window.IsVisible)
            {
                return;
            }
            if (do_fade_out)
            {
                for (int i = fade_out_step - 1; i >= 1; i--)
                {
                    window.SetAlphaValue((float)i / (float)fade_out_step);
                }
                window.OrderOut(null);
                //makeMainThreadCallOrderOut();
                window.SetAlphaValue(1.0f);
            }
            else
            {
                window.OrderOut(null);
                //makeMainThreadCallOrderOut();
            }

            //beEmpty();
            //makeSSTPMsgEmpty();

            // ここで選択肢を消してしまうと、いろいろ問題が起こる。

            CleanUp();
        }
        private void MakeMainThreadCallOrderOut()
        {
            NSTimer close_timer = new NSTimer(
            0.0,
            null,
            new NSSelector("orderOut", null),
             null,
            false);
            //SCFoundation.SharedFoundation().getRunLoopOfMainThread().addTimerForMode(close_timer, NSRunLoop.DefaultRunLoopMode);
            if (!Thread.CurrentThread.Name.Equals("main"))
            {
                while (window.IsVisible)
                {

                } // 待機

            }
        }

        public void CleanUp()
        {
            RemoveAllAlternatives();
            RemoveAllAnchors();
            BeEmpty();
            //makeSSTPMsgEmpty();
            bgview.Redraw();
            callback_target_runner = null; // これはもう不要なので参照を消す。

            // アンカーが消えない場合があるので、念の爲textviewの全サブクラスを消す。
            foreach (NSView enu in textview.Subviews())
            {
                enu.RemoveFromSuperview();
            }

          
             
        }

        public bool IsVisible()
        {
            return window.IsVisible;
        }

        public NSScrollView GetTextScrollView()
        {
            return textScrollView;
        }

        public void SetLevel(int level)
        {
            window.SetLevel(level);
        }

        public void RequestChangeFont(NSFont newfont)
        {
            textview.SetFont(newfont);
        }

        public void SetFontColor(NSColor color)
        {
            textview.SetMainColor(color);
        }

        public void SetShadowColor(NSColor color)
        {
            textview.SetMainShadowColor(color);
        }

        public void SetSSTPMessageColor(NSColor color)
        {
            bgview.SetSSTPMessageColor(color);
        }

        public void SetSSTPMessageLoc(NSPoint point)
        {
            bgview.SetSSTPMessageLoc(point);
        }

        public void SetSSTPMarkerLoc(NSPoint point)
        {
            bgview.SetSSTPMarkerLoc(point);
        }

        public void SetSSTPMarkerImage(NSImage img)
        {
            bgview.SetSSTPMarkerImage(img);
        }

        public float GetSSTPMessageFontHeight()
        {
            return bgview.GetSSTPMessageFontHeight();
        }

        public SCSafetyBalloonBackgroundView GetBGView()
        {
            return bgview;
        }

        private void SetBalloonAttributesFromDescription()
        {

            float r, g, b;
            // 実体を持つバルーンスキンならそれの属性を読み込む。
            SCDescription desc = (skin == null ? bsserver.GetDescription() : skin.getDescription());

            // originとwordwrappointを取得し、textScrollViewの位置とサイズを決定
            int origin_x, origin_y; // 逆座標系での左上座標
            if (desc.Exists("origin.x"))
            {
                origin_x = desc.GetIntValue("origin.x");
                origin_y = desc.GetIntValue("origin.y");
            }
            else
            {
                origin_x = origin_y = 10;
            }
            if (origin_x < 0) origin_x = 0;
            if (origin_y < 0) origin_y = 0;
            int tvWidth, tvHeight;
            if (desc.Exists("wordwrappoint.x"))
            {
                int wwp_x = desc.GetIntValue("wordwrappoint.x");
                tvWidth = (wwp_x > 0 ? wwp_x - origin_x : (int)Width() + wwp_x - origin_x);
            }
            else
            {
                tvWidth = (int)Width() - 2 - SCBalloonScrollArrow.DEFAULT_WIDTH - 2 - origin_x;
            }
            if (tvWidth < 0)
            {
                tvWidth = 0;
            }
            if (origin_x + tvWidth > Width())
            {
                tvWidth = (int)Width() - origin_x - 1;
            }
            if (desc.Exists("validrect.bottom"))
            {
                int vr_b = desc.GetIntValue("validrect.bottom");
                if (vr_b > (int)Height()) vr_b = (int)Height();
                tvHeight = (vr_b > 0 ? vr_b - origin_y : (int)Height() + vr_b - origin_y);
            }
            else
            {
                tvHeight = (int)Height() - 15 - origin_y;
            }
            if (tvHeight < 0)
            {
                tvHeight = 0;
            }
            if (origin_y + tvHeight > Height())
            {
                tvHeight = (int)Height() - origin_y - 1;
            }
            textScrollView.SetFrame(new NSRect(origin_x, Height() - origin_y - tvHeight, tvWidth, tvHeight));

            // フォントの色
            r = desc.GetIntValue("font.color.r") / 255.0f;
            g = desc.GetIntValue("font.color.g") / 255.0f;
            b = desc.GetIntValue("font.color.b") / 255.0f;
            SetFontColor(NSColor.ColorWithCalibratedRGB(r, g, b, 1.0f));

            // 影の色
            if (desc.Exists("font.shadowcolor.r"))
            {
                if (desc.GetIntValue("font.shadowcolor.r") == -1)
                {
                    // 影無し 
                    SetShadowColor(null);
                }
                else
                {
                    r = desc.GetIntValue("font.shadowcolor.r") / 255.0f;
                    g = desc.GetIntValue("font.shadowcolor.g") / 255.0f;
                    b = desc.GetIntValue("font.shadowcolor.b") / 255.0f;
                    SetShadowColor(NSColor.ColorWithCalibratedRGB(r, g, b, 1.0f));
                }
            }
            else
            {
                // setShadowColor(NSColor.colorWithCalibratedWhite(0.0f,0.3f));
                SetShadowColor(null);
            }

            // SSTPメッセージの色
            if (!desc.Exists("sstpmessagecolor.r"))
            {
                r = g = b = 0.5f;
            }
            else
            {
                r = desc.GetIntValue("sstpmessagecolor.r") / 255.0f;
                g = desc.GetIntValue("sstpmessagecolor.g") / 255.0f;
                b = desc.GetIntValue("sstpmessagecolor.b") / 255.0f;
            }
            SetSSTPMessageColor(NSColor.ColorWithCalibratedRGB(r, g, b, 1.0f));

            // SSTPメッセージ位置
            if (desc.Exists("sstpmessage.x"))
            {
                int horiz = desc.GetIntValue("sstpmessage.x");
                int vert = desc.GetIntValue("sstpmessage.y");

                SetSSTPMessageLoc(new NSPoint(horiz, (vert > 0 ? Height() - vert : -1 * vert) - (int)GetSSTPMessageFontHeight()));
            }
            else
            {
                SetSSTPMessageLoc(new NSPoint(2, 2));
            }

            // SSTPマーカー画像と位置
            NSImage marker = bsserver.GetSstpMarkerImage();
            SetSSTPMarkerImage(marker);
            if (marker != null)
            {
                if (desc.Exists("sstpmarker.x"))
                {
                    int horiz = desc.GetIntValue("sstpmarker.x");
                    int vert = desc.GetIntValue("sstpmarker.y");

                    SetSSTPMarkerLoc(new NSPoint(horiz, (vert > 0 ? Height() - vert : -1 * vert) - (int)marker.Size().Height().Value));
                }
                else
                {
                    SetSSTPMarkerLoc(new NSPoint(2, 2));
                }
            }

            // 選択肢
            if (!desc.Exists("cursor.style"))
            {
                alternative_draw_underline = alternative_draw_square = true;
            }
            else
            {
                string style_str = desc.GetStrValue("cursor.style");
                if (style_str.Equals("square"))
                {
                    alternative_draw_square = true;
                }
                else if (style_str.Equals("underline"))
                {
                    alternative_draw_underline = true;
                    alternative_draw_square = false;
                }
                else if (style_str.Equals("square+underline"))
                {
                    alternative_draw_underline = alternative_draw_square = true;
                }
            }

            if (!desc.Exists("cursor.pen.color.r"))
            {
                r = g = b = 10.0f / 255.0f;
            }
            else
            {
                r = desc.GetIntValue("cursor.pen.color.r") / 255.0f;
                g = desc.GetIntValue("cursor.pen.color.g") / 255.0f;
                b = desc.GetIntValue("cursor.pen.color.b") / 255.0f;
            }
            alternative_pen_color = NSColor.ColorWithCalibratedRGB(r, g, b, 1.0f);

            if (!desc.Exists("cursor.brush.color.r"))
            {
                r = g = b = 0.5f;
            }
            else
            {
                r = desc.GetIntValue("cursor.brush.color.r") / 255.0f;
                g = desc.GetIntValue("cursor.brush.color.g") / 255.0f;
                b = desc.GetIntValue("cursor.brush.color.b") / 255.0f;
            }
            alternative_brush_color = NSColor.ColorWithCalibratedRGB(r, g, b, 1.0f);

            alternative_font_color = null;
            if (desc.Exists("cursor.font.color.r"))
            {
                string blendmethod = desc.GetStrValue("cursor.blendmethod");
                if ((blendmethod != null && blendmethod.Equals("none")) || blendmethod == null)
                {
                    r = desc.GetIntValue("cursor.font.color.r") / 255.0f;
                    g = desc.GetIntValue("cursor.font.color.g") / 255.0f;
                    b = desc.GetIntValue("cursor.font.color.b") / 255.0f;

                    alternative_font_color = NSColor.ColorWithCalibratedRGB(r, g, b, 1.0f);
                }
            }

            // スクロールアロー
            NSImage image_arrow_up = bsserver.GetArrowImage(SCBalloonSkinServer.ARROW_UP);
            NSImage image_arrow_down = bsserver.GetArrowImage(SCBalloonSkinServer.ARROW_DOWN);
            //Logger.log(this, Logger.DEBUG, "arrow up: {"+image_arrow_up+"}");
            arrow_up.SetImage(image_arrow_up);
            arrow_down.SetImage(image_arrow_down);
            float x, y;
            if (!desc.Exists("arrow0.x"))
            {
                x = Width() - 2 - arrow_up.Width();
                y = Height() - 2 - arrow_up.Height();
            }
            else
            {
                x = desc.GetIntValue("arrow0.x");
                y = desc.GetIntValue("arrow0.y");
                if (x < 0) x = Width() + x;
                if (y < 0)
                    y = -1 * y - arrow_up.Height();
                else
                    y = Height() - y - arrow_up.Height();
            }
            arrow_up.SetFrameOrigin(x, y);
            //Logger.log(this, Logger.DEBUG, "width: "+width()+"; arrow_up: {"+arrow_up+"}");

            if (!desc.Exists("arrow1.x"))
            {
                x = Width() - 2 - arrow_down.Width();
                y = 2;
            }
            else
            {
                x = desc.GetIntValue("arrow1.x");
                y = desc.GetIntValue("arrow1.y");
                if (x < 0) x = Width() + x;
                if (y < 0)
                    y = -1 * y - arrow_down.Height();
                else
                    y = Height() - y - arrow_down.Height();
            }
            arrow_down.SetFrameOrigin(x, y);

            // アンカー
            if (!desc.Exists("anchor.font.color.r"))
            {
                r = b = 0.0f;
                g = 0.8f;
            }
            else
            {
                r = desc.GetIntValue("anchor.font.color.r") / 255.0f;
                g = desc.GetIntValue("anchor.font.color.g") / 255.0f;
                b = desc.GetIntValue("anchor.font.color.b") / 255.0f;
            }
            anchor_color = NSColor.ColorWithCalibratedRGB(r, g, b, 1.0f);

        }

        public string ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("session: {").Append(session).Append('}');
            buf.Append("; type: ").Append(type);
            buf.Append("; skin id: ").Append(num);
            if (skin != null)
            {
                buf.Append("; skin: {").Append(skin).Append('}');
            }
            return buf.ToString();
        }

        protected void finalize()
        {
            //Logger.log(this, Logger.DEBUG, "finalized");
        }



    }
}

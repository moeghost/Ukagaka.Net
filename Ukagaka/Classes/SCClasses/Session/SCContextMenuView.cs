using Cocoa.AppKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cocoa.AppKit;
namespace Ukagaka
{
    public class SCContextMenuView: NSMenuView
    {
        //private static boolean isTransparent = true;
        //private static float transparency = 0.5f; // 不透明にする場合は1.0fを指定して下さい。

        SCSession session;
        bool hasSideBar;
        int type; // SCFoundation.HONTAI || SCFoundation.UNYUU

        SCMenuAppearanceServer mas;
        int widthOfSideBar;

        public SCContextMenuView(SCSession session, int type, bool hasSideBar)
        {
           // super();
            this.session = session;
            this.type = type;
            this.hasSideBar = hasSideBar;

            mas = session.GetMenuAppearance();
            if (mas.HasImage() && hasSideBar)
            {
                widthOfSideBar = (int)(type == SCFoundation.HONTAI ? mas.GetSakuraSideImage() : mas.GetKeroSideImage()).Size().Width().Value;
            }
            else
            {
                this.hasSideBar = false;
                widthOfSideBar = 0;
            }
        }

        public SCContextMenuView(SCSession session, int type)
        {
         //   super();
            this.session = session;
            this.type = type;
            this.hasSideBar = false;

            mas = session.GetMenuAppearance();
            widthOfSideBar = 0;
        }

        public bool IsOpaque()
        {
            return true;
        }

        public int GetSidebarWidth()
        {
            return widthOfSideBar;
        }

        /*NSMutableRect _sizeToFit = null;
        public void sizeToFit()
        {
            base.sizeToFit();
            if (hasSideBar)
            {
                if (_sizeToFit == null)
                {
                    _sizeToFit = new NSMutableRect();
                }
                NSRect frame = Frame;
                _sizeToFit.SetX(frame.x());
                _sizeToFit.SetY(frame.y());
                _sizeToFit.SetWidth(frame.Width().Value+widthOfSideBar-base.imageAndTitleOffSet());
                _sizeToFit.SetHeight(frame.Height().Value);
                //SetFrame(new NSRect(Frame.x(),Frame.y(),Frame.Width().Value+widthOfSideBar-base.imageAndTitleOffSet(),Frame.Height().Value));
                SetFrame(_sizeToFit);
            }
        }

        NSMutableRect _innerRect = null;
        public NSRect innerRect()
        {
            NSRect r = base.innerRect();
            if (!hasSideBar ||  r.Width().Value < widthOfSideBar)
            {
                return r;
            }
            else
            {
                if (_innerRect == null)
                {
                    _innerRect = new NSMutableRect();
                }
                _innerRect.SetX(r.x()+widthOfSideBar);
                _innerRect.SetY(r.y());
                _innerRect.SetWidth(r.Width().Value-widthOfSideBar);
                _innerRect.SetHeight(r.Height().Value);
                return _innerRect;
            }
        }*/

        public float StateImageOffSet()
        {
            return (hasSideBar ? 0 : base.StateImageOffSet());
        }

        public float ImageAndTitleOffSet()
        {
            return (hasSideBar ? 5 : base.ImageAndTitleOffSet());
        }

        /*public float imageAndTitleWidth()
        {
            return (hasSideBar ? base.imageAndTitleWidth() - widthOfSideBar : base.imageAndTitleWidth());
        }*/

        public void drawRect(NSRect r)
        {
            if (!mas.HasImage()) // デフォルトのメニュー
            {
                base.DrawRect(r);
                return;
            }

            /*if (isTransparent)
            {
                NSColor.clearColor().Set();
                NSGraphics.FillRectList(new NSRect[] {r});
            }*/

            // サイドバー
            if (hasSideBar)
            {
                NSImage image = (type == SCFoundation.HONTAI ? mas.GetSakuraSideImage() : mas.GetKeroSideImage());
                NSColor topLeftColor = (type == SCFoundation.HONTAI ? mas.GetTopLeftColorOfSakuraSideImage() : mas.GetTopLeftColorOfKeroSideImage());
                NSColor bottomRightColor = (type == SCFoundation.HONTAI ? mas.GetBottomRightColorOfSakuraSideImage() : mas.GetBottomRightColorOfKeroSideImage());

                float heightOfSideBar = image.Size().Height().Value;
                switch (mas.GetAlignmentOfSide())
                {
                    case SCMenuAppearanceServer.ALIGN_TOP:
                        {
                            float height = (heightOfSideBar > Frame.Height().Value ? Frame.Height().Value : heightOfSideBar);
                            float srcY = (Frame.Height().Value > heightOfSideBar ? 0 : heightOfSideBar - Frame.Height().Value);
                            NSRect srcRect = new NSRect(0, srcY, widthOfSideBar, height);
                            /*if (isTransparent)
                            {
                            image.dissolveToPointFromRect((IsFlipped() ? new NSPoint(0,height) : new NSPoint(0,Frame.Height().Value - height)),srcRect,transparency);
                            }
                            else
                            {*/
                            image.CompositeToPointFromRect((IsFlipped() ? new NSPoint(0, height) : new NSPoint(0, Frame.Height().Value - height)), srcRect, image.GetBitmap());
                            //}

                            if (Frame.Height().Value > heightOfSideBar)
                            {
                                NSRect rectToFill = new NSRect(0, (IsFlipped() ? heightOfSideBar : 0), widthOfSideBar, Frame.Height().Value - heightOfSideBar);
                                bottomRightColor.Set();
                                NSBezierPath.FillRect(rectToFill);
                            }
                        }
                        break;

                    case SCMenuAppearanceServer.ALIGN_BOTTOM:
                        {
                            float height = (heightOfSideBar > Frame.Height().Value ? Frame.Height().Value : heightOfSideBar);
                            NSRect srcRect = new NSRect(0, 0, widthOfSideBar, height);
                            /*if (isTransparent)
                            {
                            image.dissolveToPointFromRect((IsFlipped() ? new NSPoint(0,Frame.Height().Value) : NSPoint.ZeroPoint),srcRect,transparency);
                            }
                            else
                            {*/
                            image.CompositeToPointFromRect((IsFlipped() ? new NSPoint(0, Frame.Height().Value) : NSPoint.ZeroPoint), srcRect, NSImage.CompositeCopy);
                            //}

                            if (Frame.Height().Value > heightOfSideBar)
                            {
                                NSRect rectToFill = new NSRect(0, (IsFlipped() ? 0 : heightOfSideBar), widthOfSideBar, Frame.Height().Value - heightOfSideBar);
                                topLeftColor.Set();
                                NSBezierPath.FillRect(rectToFill);
                            }
                        }
                        break;
                }
            }

            // バックグラウンド
            float widthOfBg = mas.GetBgImage().Size().Width().Value;
            float heightOfBg = mas.GetBgImage().Size().Height().Value;
            float destwidth = Frame.Width().Value - widthOfSideBar;
            switch (mas.GetAlignmentOfBg())
            {
                case SCMenuAppearanceServer.ALIGN_LEFTTOP:
                    {
                        float width = (widthOfBg > destwidth ? destwidth : widthOfBg);
                        float height = (heightOfBg > Frame.Height().Value ? Frame.Height().Value : heightOfBg);
                        float y = (Frame.Height().Value > heightOfBg ? 0 : heightOfBg - Frame.Height().Value);
                        NSRect srcRect = new NSRect(0, y, width, height);
                        NSPoint destPoint = (IsFlipped() ? new NSPoint(widthOfSideBar, height) : new NSPoint(widthOfSideBar, Frame.Height().Value - height));
                        /*if (isTransparent)
                        {
                            mas.GetBgImage().dissolveToPointFromRect(destPoint,srcRect,transparency);
                        }
                        else
                        {*/
                        mas.GetBgImage().CompositeToPointFromRect(destPoint, srcRect, NSImage.CompositeCopy);
                        //}

                        // 右の余白
                        if (destwidth > widthOfBg)
                        {
                            NSRect region = new NSRect(widthOfSideBar + widthOfBg,
                                            (IsFlipped() ? 0 : Frame.Height().Value - height),
                                            destwidth - widthOfBg,
                                            height);
                            NSRect rectToFill = region.RectByIntersectingRect(r);
                            if (rectToFill != NSRect.ZeroRect)
                            {
                                mas.GetBottomRightColorOfBgImage().Set();
                                NSBezierPath.FillRect(rectToFill);
                            }
                        }

                        // 下の余白
                        if (Frame.Height().Value > heightOfBg)
                        {
                            NSRect region = new NSRect(widthOfSideBar,
                                        (IsFlipped() ? heightOfBg : Frame.Height().Value - heightOfBg),
                                        destwidth,
                                        Frame.Height().Value - heightOfBg);
                            NSRect rectToFill = region.RectByIntersectingRect(r);
                            if (rectToFill != NSRect.ZeroRect)
                            {
                                mas.GetBottomRightColorOfBgImage().Set();
                                NSBezierPath.FillRect(rectToFill);
                            }
                        }
                    }
                    break;

                case SCMenuAppearanceServer.ALIGN_RIGHTTOP:
                    {
                        float width = (widthOfBg > destwidth ? destwidth : widthOfBg);
                        float height = (heightOfBg > Frame.Height().Value ? Frame.Height().Value : heightOfBg);
                        float x = (destwidth > widthOfBg ? 0 : widthOfBg - destwidth);
                        float y = (Frame.Height().Value > heightOfBg ? 0 : heightOfBg - Frame.Height().Value);
                        NSRect srcRect = new NSRect(x, y, width, height);
                        float destX = Frame.Width().Value - width;
                        NSPoint destPoint = (IsFlipped() ? new NSPoint(destX, height) : new NSPoint(destX, Frame.Height().Value - height));
                        /*if (isTransparent)
                        {
                            mas.GetBgImage().dissolveToPointFromRect(destPoint,srcRect,transparency);
                        }
                        else
                        {*/
                        mas.GetBgImage().CompositeToPointFromRect(destPoint, srcRect, NSImage.CompositeCopy);
                        //}

                        // 左の余白
                        if (destwidth > widthOfBg)
                        {
                            NSRect region = new NSRect(widthOfSideBar,
                                            (IsFlipped() ? 0 : Frame.Height().Value - height),
                                            destwidth - widthOfBg,
                                            height);
                            NSRect rectToFill = region.RectByIntersectingRect(r);
                            if (rectToFill != NSRect.ZeroRect)
                            {
                                mas.GetBottomRightColorOfBgImage().Set();
                                NSBezierPath.FillRect(rectToFill);
                            }
                        }

                        // 下の余白
                        if (Frame.Height().Value > heightOfBg)
                        {
                            NSRect region = new NSRect(widthOfSideBar,
                                        (IsFlipped() ? heightOfBg : Frame.Height().Value - heightOfBg),
                                        destwidth,
                                        Frame.Height().Value - heightOfBg);
                            NSRect rectToFill = region.RectByIntersectingRect(r);
                            if (rectToFill != NSRect.ZeroRect)
                            {
                                mas.GetBottomRightColorOfBgImage().Set();
                                NSBezierPath.FillRect(rectToFill);
                            }
                        }
                    }
                    break;

                case SCMenuAppearanceServer.ALIGN_LEFTBOTTOM:
                    {
                        float width = (widthOfBg > destwidth ? destwidth : widthOfBg);
                        float height = (heightOfBg > Frame.Height().Value ? Frame.Height().Value : heightOfBg);
                        NSRect srcRect = new NSRect(0, 0, width, height);
                        NSPoint destPoint = new NSPoint(widthOfSideBar, (IsFlipped() ? Frame.Height().Value : 0));
                        /*if (isTransparent)
                        {
                            mas.GetBgImage().dissolveToPointFromRect(destPoint,srcRect,transparency);
                        }
                        else
                        {*/
                        mas.GetBgImage().CompositeToPointFromRect(destPoint, srcRect, NSImage.CompositeCopy);
                        //}

                        // 右の余白
                        if (destwidth > widthOfBg)
                        {
                            NSRect region = new NSRect(widthOfSideBar + widthOfBg,
                                            (IsFlipped() ? 0 : Frame.Height().Value - height),
                                            destwidth - widthOfBg,
                                            height);
                            NSRect rectToFill = region.RectByIntersectingRect(r);
                            if (rectToFill != NSRect.ZeroRect)
                            {
                                mas.GetTopLeftColorOfBgImage().Set();
                                NSBezierPath.FillRect(rectToFill);
                            }
                        }

                        // 上の余白
                        if (Frame.Height().Value > heightOfBg)
                        {
                            NSRect region = new NSRect(widthOfSideBar,
                                        (IsFlipped() ? 0 : heightOfBg),
                                        destwidth,
                                        Frame.Height().Value - heightOfBg);
                            NSRect rectToFill = region.RectByIntersectingRect(r);
                            if (rectToFill != NSRect.ZeroRect)
                            {
                                mas.GetTopLeftColorOfBgImage().Set();
                                NSBezierPath.FillRect(rectToFill);
                            }
                        }
                    }
                    break;

                case SCMenuAppearanceServer.ALIGN_RIGHTBOTTOM:
                    {
                        float width = (widthOfBg > destwidth ? destwidth : widthOfBg);
                        float height = (heightOfBg > Frame.Height().Value ? Frame.Height().Value : heightOfBg);
                        float srcX = (destwidth > widthOfBg ? 0 : widthOfBg - destwidth);
                        NSRect srcRect = new NSRect(srcX, 0, width, height);
                        float destX = Frame.Width().Value - width;
                        NSPoint destPoint = new NSPoint(destX, (IsFlipped() ? Frame.Height().Value : 0));
                        /*if (isTransparent)
                        {
                            mas.GetBgImage().dissolveToPointFromRect(destPoint,srcRect,transparency);
                        }
                        else
                        {*/
                        mas.GetBgImage().CompositeToPointFromRect(destPoint, srcRect, NSImage.CompositeCopy);
                        //}

                        // 左の余白
                        if (destwidth > widthOfBg)
                        {
                            NSRect region = new NSRect(widthOfSideBar,
                                            (IsFlipped() ? 0 : Frame.Height().Value - height),
                                            destwidth - widthOfBg,
                                            height);
                            NSRect rectToFill = region.RectByIntersectingRect(r);
                            if (rectToFill != NSRect.ZeroRect)
                            {
                                mas.GetTopLeftColorOfBgImage().Set();
                                NSBezierPath.FillRect(rectToFill);
                            }
                        }

                        // 上の余白
                        if (Frame.Height().Value > heightOfBg)
                        {
                            NSRect region = new NSRect(widthOfSideBar,
                                        (IsFlipped() ? 0 : heightOfBg),
                                        destwidth,
                                        Frame.Height().Value - heightOfBg);
                            NSRect rectToFill = region.RectByIntersectingRect(r);
                            if (rectToFill != NSRect.ZeroRect)
                            {
                                mas.GetTopLeftColorOfBgImage().Set();
                                NSBezierPath.FillRect(rectToFill);
                            }
                        }
                    }
                    break;
            }

            // 項目
            int n_items = Menu().NumberOfItems();
            for (int i = 0; i < n_items; i++)
            {
                NSMenuItemCell mic = MenuItemCellForItemAtIndex(i);
                NSRect region = RectOfItemAtIndex(i);

                if (r.IntersectsRect(region))
                {
                    mic.DrawWithFrameInView(region, this);
                }
            }
        }

        public static void PrintSuperRecursively(NSView view)
        {
            System.Console.Write(view.ToString() + " : " + view.IsFlipped());
            if (view.Superview != null)
            {
                PrintSuperRecursively(view.Superview);
            }
        }

        internal void SetMenuItemCellForItemAtIndex(SCContextMenuItemCell mic, int v)
        {
            throw new NotImplementedException();
        }
    }
}

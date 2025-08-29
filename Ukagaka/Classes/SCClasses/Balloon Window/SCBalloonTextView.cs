using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

using Cocoa.AppKit;
 
using Ukagaka;

public class SCBalloonTextView : NSView
{
    private static readonly NSRange FIRSTCHAR_RANGE = new NSRange(0, 1);
    private static readonly int SHADOW_INTERVAL = 2;
    private static readonly int LINE_INTERVAL = 0;

    private NSFont font;
    private float fontHeight;
    private NSColor mainColor, mainShadowColor;

    private float x, y;
    private NSImage offscreen;

    private StringBuilder buffer;
    private List<NSView> subviewQueue;

    private SCBalloonScrollArrow arrow_up, arrow_down;

   
    public SCBalloonTextView(NSRect r) : base(r)
    {
        buffer = new StringBuilder();
        subviewQueue = new List<NSView>();

        SetFont(NSFont.SystemFontOfSize(10.0f));
        SetMainColor(NSColor.Black);
        SetMainShadowColor(null);

        offscreen = new NSImage(new NSSize(10, 10)); // Any size to enable lockFocus().
        // offscreen.Flipped = true;
        BeEmpty(); // Ensure correct size here.
    }

    public void Initialize(SCBalloonScrollArrow arrowUp, SCBalloonScrollArrow arrowDown)
    {
        arrow_up = arrowUp;
        arrow_down = arrowDown;
    }

    public void UpdateArrowsVisibilities()
    {
        // Initialize() might not have been called before this method.
        arrow_up?.UpdateVisibility();
        arrow_down?.UpdateVisibility();
    }

    public void AddSubviewAtCurrentLoc(NSView view)
    {
        subviewQueue.Add(view);
        buffer.Append('\u0003');
        Display();
    }

    protected void AddSubviewAtCurrentLoc()
    {
        NSView view = subviewQueue[0];
        subviewQueue.RemoveAt(0);

        NSRect viewFrame = view.Frame;
        CGFloat viewWidth = viewFrame.Width();
        CGFloat viewHeight = viewFrame.Height();

        view.Frame = new NSRect(x, y, viewWidth.Value, viewHeight.Value);
        AddSubview(view);
        x += viewWidth.Value;
    }

    public void ChangedSize()
    {
        // Call this method whenever the size of the view itself is changed.
        if (Superview == null)
        {
            return;
        }


        NSRect frame = Superview.Frame;
        Frame = frame;
        offscreen.SetSize(frame.Size());
    }

    public void SetFont(NSFont f)
    {
        font = f;
        fontHeight = (float)Math.Round(font.BoundingRectForFont.Height().Value);
        // fontHeight = (float)Math.Round(font.Ascender - font.Descender);
    }

    public NSFont GetFont()
    {
        return font;
    }

    public void SetMainColor(NSColor c)
    {
        mainColor = c;
    }

    public NSColor GetMainColor()
    {
        return mainColor;
    }

    public void SetMainShadowColor(NSColor c)
    {
        // If null, no shadow will be cast.
        mainShadowColor = c;
    }

    public void WaitUntilFlushed()
    {
        while (buffer.Length > 0)
        {
            try
            {
                System.Threading.Thread.Sleep(50);
            }
            catch (Exception)
            {
            }
        }
    }

    public void FlushBuffer()
    {
        if (buffer.Length > 0)
        {
            offscreen.LockFocus();
            while (buffer.Length > 0)
            {
                char c = buffer[0];
                buffer.Remove(0, 1);
                if (c == '\u0001') // Clear
                {
                    // Be cautious, calling display() within this block might re-enter flushBuffer().
                    CleanUp();
                }
                else if (c == '\u0003') // Add subview
                {
                    AddSubviewAtCurrentLoc();
                }
                else
                {
                    DrawChar(c);
                }
            }
            offscreen.UnlockFocus();
        }
    }

    public override void DrawRect(NSRect r)
    {
        FlushBuffer();
        offscreen.CompositeToPointFromRect(r.Location(), r, NSImage.CompositeSourceOver);
    }

    public void DrawImage(NSImage image, NSPoint point)
    {
        // FIXME: Images might be better handled as subviews, but deferring for now.
        image.CompositeToPoint(point, image.GetBitmap());
        NSRect r = new NSRect(0, 0, image.Size().Width().Value, image.Size().Height().Value);
        offscreen.CompositeToPointFromRect(r.Location(), r, NSImage.CompositeSourceOver);
        FlushBuffer();
    }

    private void DrawChar(char c)
    {
        NSMutableAttributedString astr = new NSMutableAttributedString(c.ToString());
        astr.AddAttribute(NSAttributedString.FontAttributeName, font, FIRSTCHAR_RANGE);
        NSMutableAttributedString shadowstr = new NSMutableAttributedString(astr); // Copy

        astr.AddAttribute(NSAttributedString.ForegroundColorAttributeName, mainColor, FIRSTCHAR_RANGE);
        if (mainShadowColor != null)
        {
            shadowstr.AddAttribute(NSAttributedString.ForegroundColorAttributeName, mainShadowColor, FIRSTCHAR_RANGE);
        }

        CGFloat widthOfCurrentChar = NSGraphics.SizeOfAttributedString(astr).Width();
        // If not a newline character and drawing this character exceeds the width, insert a newline.
        if (c != '\n' && c != '\u0002' && x + widthOfCurrentChar.Value > Frame.Width().Value)
        {
            x = 0;
            y -= (c == '\u0002' ? (float)Math.Round(fontHeight / 2.0) : fontHeight) + LINE_INTERVAL + SHADOW_INTERVAL;

            CGFloat screenHeight = offscreen.Size().Height();
            //if (y > screenHeight) // If y goes below the lower limit, extend the frame for scrolling.
            if (y < 0)
            {
                CGFloat deltaY = new CGFloat(-1 * y);

                offscreen.UnlockFocus();
                NSRect oldFrame = Frame;
                NSRect newFrame = new NSRect(0, 0, oldFrame.Width().Value, oldFrame.Height().Value + deltaY.Value); // Expand by the deficit
                Frame = newFrame;
                offscreen.SetSize(newFrame.Size());
                offscreen.LockFocus();

                UpdateArrowsVisibilities();

                // Push the current screen content upwards since the screen extends upwards.
                offscreen.CompositeToPoint(new NSPoint(0, deltaY.Value), offscreen.GetBitmap());

                List<NSView> subviews = Subviews();
                int nSubviews = (int)subviews.Count;
                for (int i = 0; i < nSubviews; i++)
                {
                    NSView subview = subviews.ToArray().ElementAt(i);
                    NSRect subviewOldFrame = subview.Frame;
                    subview.Frame = new NSRect(subviewOldFrame.X(), subviewOldFrame.Y() + deltaY, subviewOldFrame.Width(), subviewOldFrame.Height());
                }

                NSColor.ClearColor.Set();
                NSGraphics.RectFillList(new NSRect[] { new NSRect(0, 0, newFrame.Width().Value, deltaY.Value) });

                //if (mainShadowColor == null)
                //{
                //    y = 0;
                //}
                //else
                //{
                //    y = SHADOW_INTERVAL;
                //}
                y = SHADOW_INTERVAL;
            }
        }
        if (c != '\n' && c != '\u0002')
        {
            if (mainShadowColor != null) // Do not cast a shadow if mainShadowColor is null.
            {
                NSGraphics.DrawStringWithAttributes(shadowstr, new NSPoint(x + SHADOW_INTERVAL, y - SHADOW_INTERVAL));
            }
            NSGraphics.DrawStringWithAttributes(astr, new NSPoint(x, y));
            x += widthOfCurrentChar.Value;
        }
    }

    private void CleanUp()
    {
        // Reset the view and offscreen sizes to their initial values.
        offscreen.UnlockFocus();
        NSRect frame = Superview.Frame;
        Frame = frame;
        offscreen.SetSize(frame.Size());
        offscreen.LockFocus();

        UpdateArrowsVisibilities();

        NSColor.ClearColor.Set();
        NSGraphics.RectFillList(new NSRect[] { Frame });

        ResetLocation();
    }

    public void AddChar(char c)
    {
        buffer.Append(c);
        //flushBuffer();
        //offscreen.LockFocus();
        //drawChar(c);
        //offscreen.UnlockFocus();

        Display();
    }

    public void BeEmpty()
    {
        buffer.Append('\u0001'); // Byte value 1 means cleanup here.
        //flushBuffer();
        /*NSRect frame = superview().frame();
        setFrame(frame);
        offscreen.setSize(frame.Size());

        offscreen.lockFocus();
        NSColor.clearColor().set();
        NSGraphics.fillRectList(new NSRect[] {frame()});
        offscreen.unlockFocus();

        updateArrowsVisibilities();

        resetLocation();*/

        Display();
    }

    private void ResetLocation()
    {
        x = 0;
        y = Frame.Height().Value - fontHeight;
        //y = 0;
    }

   
}

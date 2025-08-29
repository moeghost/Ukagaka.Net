using System;
using System.Windows.Controls;
using System.Windows.Input;
using Cocoa.AppKit;
using Ukagaka;

public class SCBalloonScrollArrow : NSView
{
    public const float ScrollStep = 10f;
    public static int DEFAULT_WIDTH = 15;
    public static int DEFAULT_HEIGHT = 15;

    private SCSafetyBalloonController controller;
    private NSClipView target;
    private int type; // SCBalloonSkinServer.ARROW_UP or SCBalloonSkinServer.ARROW_DOWN
    private NSImage image;
    private NSColor bgColor, arrowColor; // Valid when image is null.
    private NSTimer scrollTimer;
    private bool isVisible = false;
    private bool isDrawn = false;

    private NSImage offscreen;
    private NSRect[] rectlistBounds = null;

    public SCBalloonScrollArrow(SCSafetyBalloonController controller, int type) : base(new NSRect(0, 0, DEFAULT_WIDTH, DEFAULT_HEIGHT))
    {
        this.controller = controller;
        this.target = (NSClipView)controller.GetTextScrollView().ContentView();
        this.type = type;
        this.image = null;
        this.bgColor = NSColor.ColorWithCalibratedRGB(0.5f, 0.5f, 0.5f, 0.8f);
        this.arrowColor = NSColor.ColorWithCalibratedRGB(1.0f, 1.0f, 1.0f, 0.8f);
        this.offscreen = new NSImage(new NSSize(DEFAULT_WIDTH, DEFAULT_HEIGHT));
    }

    public string Description => $"type: {(type == SCBalloonSkinServer.ARROW_UP ? "up" : "down")}; {(image != null ? "has image" : "")}; {(isVisible ? "visible" : "invisible")}; location: {Frame.X()}, {Frame.Y()}; size: {Frame.Width()}, {Frame.Height()}";

    public void CleanUp()
    {
        controller = null;
    }

    public void SetImage(NSImage image)
    {
        // image can be null. Stop using the image.
        this.image = image;

        if (image == null)
        {
            SetFrameSize(new NSSize(DEFAULT_WIDTH, DEFAULT_HEIGHT));
            offscreen.SetSize(new NSSize(DEFAULT_WIDTH, DEFAULT_HEIGHT));
        }
        else
        {
            SetFrameSize(image.Size());
            offscreen.SetSize(image.Size());
        }
        rectlistBounds = new NSRect[] { Bounds };
    }

    protected void UpdateOffscreen()
    {
        // Always lock/unlock offscreen before/after.
        if (SCFoundation.DEBUG)
        {
            // Display frame for debugging.
            // NSColor.Red.Set();
            // NSGraphics.FrameRect(Bounds);
        }

        /*
          If the image changes in the middle, you must erase the old image even if isVisible is true.
          The same applies if there is an alpha channel.
        */

        if (!isVisible)
        {
            if (isDrawn)
            {
                NSColor.Clear.Set();
                NSGraphics.FillRects(rectlistBounds);
                isDrawn = false;
            }
            return;
        }

        if (image == null)
        {
            bgColor.Set();
            NSGraphics.FillRects(rectlistBounds);

            arrowColor.Set();
            if (type == SCBalloonSkinServer.ARROW_UP)
            {
                NSBezierPath.StrokeLine(new NSPoint(Frame.Width().Value / 2f, 1), new NSPoint(Frame.Width().Value / 2f, Frame.Height().Value - 1));
                NSBezierPath.StrokeLine(new NSPoint(Frame.Width().Value / 3f, Frame.Height().Value / 2f), new NSPoint(Frame.Width().Value / 2f, 1));
                NSBezierPath.StrokeLine(new NSPoint(Frame.Width().Value * (2f / 3f), Frame.Height().Value / 2f), new NSPoint(Frame.Width().Value / 2f, 1));
            }
            else
            {
                NSBezierPath.StrokeLine(new NSPoint(Frame.Width().Value / 2f, Frame.Height().Value - 1), new NSPoint(Frame.Width().Value / 2f, 1));
                NSBezierPath.StrokeLine(new NSPoint(Frame.Width().Value / 3f, Frame.Height().Value / 2f), new NSPoint(Frame.Width().Value / 2f, 1));
                NSBezierPath.StrokeLine(new NSPoint(Frame.Width().Value * (2f / 3f), Frame.Height().Value / 2f), new NSPoint(Frame.Width().Value / 2f, 1));
            }
        }
        else
        {
            image.CompositeToPoint(new NSPoint(), image.GetBitmap());
        }
        isDrawn = true;
    }

    public override void DrawRect(NSRect r)
    {
        offscreen.LockFocus();
        UpdateOffscreen();
        offscreen.UnlockFocus();

        if (isVisible)
        {
            offscreen.CompositeToPointFromRect(r.Location(), r, offscreen.GetBitmap());
        }
    }

    public override void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        scrollTimer = NSTimer.CreateRepeatingTimer(0.03, new EventHandler(Scroll), null);


        scrollTimer.Start();

       // NSRunLoop.Main.AddTimer(scrollTimer, NSRunLoopMode.Default);
    }

    public override void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        scrollTimer?.Invalidate();
    }

    private void Scroll(object sender, EventArgs e)
    {
        controller.Scroll(type == SCBalloonSkinServer.ARROW_UP ? ScrollStep : -1 * ScrollStep);
    }

    public void UpdateVisibility()
    {
        NSRect currentVisibleRect = target.DocumentVisibleRect();
        CGFloat currentVertPoint = currentVisibleRect.Y();

        if (type == SCBalloonSkinServer.ARROW_UP)
        {
            CGFloat maxVertPoint = target.DocumentRect().Height() - currentVisibleRect.Height();
            SetVisibility(currentVertPoint < maxVertPoint);
        }
        else
        {
            SetVisibility(currentVertPoint.Value > 0);
        }
    }

    protected void SetVisibility(bool isVisible)
    {
        if (this.isVisible != isVisible)
        {
            this.isVisible = isVisible;

            // This should work with just setNeedsDisplay(true). If this occultism is no longer needed in future AppKit versions, abandon it.
            // macOS X 10.3.2 (2004/02/05)
            Action<Object, EventArgs> a = (o, ea) => { SetNeedsDisplay(true); };
            EventHandler e = a.Invoke;
            
            NSTimer displayTimer = NSTimer.CreateTimer(0.0,e ,null);
            SCFoundation.SharedFoundation().GetRunLoopOfMainThread().AddTimer(displayTimer);
        }

        if (isVisible)
        {
            controller.SetIgnoresMouseEvents(false);
        }
    }

    public new float Width()
    {
       return  Frame.Width().Value;
    }
    public new float Height()
    {
        return Frame.Height().Value;
    }

    public void SetFrameOrigin(float x, float y)
    {
        SetFrameOrigin(new NSPoint(x, y));
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Cocoa.AppKit;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
namespace Ukagaka
{
   // using NSFont = System.Drawing.Font;
   // using NSColor = System.Drawing.Color;
   // using FontStyle = System.Drawing.FontStyle;

    public class SCSafetyBalloonBackgroundView : NSView
    {
        double transparency; // 0なら0で良い。
        NSImage bgimage;
        NSFont sstpMessageFont;
        NSColor sstpMessageColor;
        string sstpmessage = null;
        NSPoint sstpMessageLoc;
        NSImage sstpMarkerImage;
        NSPoint sstpMarkerLoc;
        // SCBalloonScrollArrow arrow_up, arrow_down;
        NSRect frame;
        NSImage buffer;
        volatile bool bufIsDirty = true;
        bool isNeedsDisplay;



        public SCSafetyBalloonBackgroundView(NSRect r)
        {
            // super(r);
            frame = r;
             buffer = new NSImage(r.Size());
            sstpMessageFont = new NSFont("", 10.0f);
            sstpMessageColor = NSColor.Gray;
            sstpMessageLoc = NSPoint.ZeroPoint;
        }

        public SCSafetyBalloonBackgroundView()
        {
            frame = new NSRect() ;
            buffer = new NSImage(frame.Size());
            sstpMessageFont = new NSFont("", 10.0f);
            sstpMessageColor = NSColor.Gray;
            sstpMessageLoc = NSPoint.ZeroPoint;
        }



            public bool IsOpaque()
        {
            return false;
        }

        public override void DrawRect(NSRect r)
        {
            if (bufIsDirty)
            {
                buffer.LockFocus();
                RedrawBuffer();
                buffer.UnlockFocus();

                bufIsDirty = false;
            }

            if (transparency == 0)
            {
                buffer.CompositeToPointFromRect(r.Origin(), r, buffer.GetBitmap());
               // buffer.compositeToPointFromRect(r.Origin(), r, NSImage.CompositeCopy);
                buffer.SetFrame(r);
                
               // buffer.GetBitmap().MakeTransparent
            }
            else
            {
               // buffer.CompositeToPointFromRect(r.Origin, r, buffer.GetBitmap());
                //buffer.SetFrame(r);
                buffer.DissolveToPointFromRect(r.Origin(), r, (float)(1.0 - transparency));
            }
        //    this.Image = buffer;
        }
        private void RedrawBuffer()
        {
        //    NSColor.clearColor().set();
         
           // NSGraphics.fillRectList(new NSRect[] { frame() });

            if (bgimage != null)
            {


                // Graphics g = bgimage.LockFocus();

                bgimage.LockFocus();
                bgimage.CompositeToPoint(frame.Origin(), bgimage.GetBitmap());
                //this.Image = bgimage;

                if (isNeedsDisplay)
                {

                    this.Image.Source = bgimage.GetBitmapImage();
                     
                }
                else
                {
                    this.Image.Source = bgimage.GetOriginBitmap();
                
                }



                // g.DrawImage(bgimage.GetBitmap(), frame.Origin.ToPoint());

                // g.Dispose();


            }
            else
            {
               // NSColor.whiteColor().set();
              //  NSBezierPath.fillRect(frame());
            }

            if (sstpmessage != null)
            {
                if (sstpMarkerImage != null)
                {
                    // アルファチャンネル付きpngを考慮する。
                    //  sstpMarkerImage.compositeToPoint(
                    //      sstpMarkerLoc, NSImage.CompositeSourceOver);


                    sstpMarkerImage.LockFocus();
                    sstpMarkerImage.CompositeToPoint(frame.Origin(), sstpMarkerImage.GetBitmap());


                    // Graphics g = sstpMarkerImage.lockFocus();

                    // g.DrawImage(sstpMarkerImage.GetBitmap(), frame.Origin.ToPoint());

                    // g.Dispose();

                }

              //  NSGraphics.drawAttributedString(sstpmessage, sstpMessageLoc);
            }
         
        }

        public void SetBGImage(NSImage img)
        {
            bgimage = img;
            
            Redraw();
        }

        public void Redraw()
        {
            bufIsDirty = true;
            //display();
            SetNeedsDisplay(true);
        }

        public void ResizeWithOldSuperviewSize(NSSize s)
        {
            //base.resizeWithOldSuperviewSize(s);
            buffer.SetSize(frame.Size());
            bufIsDirty = true;
            //display();
            SetNeedsDisplay(true);
        }

        public void SetTransparency(double t)
        {
            transparency = t;
            bufIsDirty = true;
            //display();
            SetNeedsDisplay(true);
        }

        public float GetSSTPMessageFontHeight()
        {
            return 0;
            //return sstpMessageFont.boundingRectForFont().Height();
            //return sstpMessageFont.ascender() - sstpMessageFont.descender();
        }

        public void SetSSTPMessageColor(NSColor c)
        {
            sstpMessageColor = c;
            bufIsDirty = true;
        }

        public void SetSSTPMessageLoc(NSPoint point)
        {
            sstpMessageLoc = point;
            bufIsDirty = true;
        }

        public void SetSSTPMarkerImage(NSImage img)
        {
            sstpMarkerImage = img;
            bufIsDirty = true;
        }

        public void SetSSTPMarkerLoc(NSPoint point)
        {
            sstpMarkerLoc = point;
            bufIsDirty = true;
        }

        public NSImage GetSSTPMarkerImage()
        {
            return sstpMarkerImage;
        }

        public void SetSSTPMessage(String str)
        {
            if (str == null || str.Length == 0)
            {
                sstpmessage = null;
            }
            else
            {
                sstpmessage = str;
                CharacterRange wholeStrRange = new CharacterRange(0, str.Length);
              /*  sstpmessage.addAttributeInRange(
                NSAttributedString.FontAttributeName,
                sstpMessageFont,
                wholeStrRange);
                sstpmessage.addAttributeInRange(
                NSAttributedString.ForegroundColorAttributeName,
                sstpMessageColor,
                wholeStrRange);*/
            }
            bufIsDirty = true;
            //display();
            SetNeedsDisplay(true);
        }

        public override void SetNeedsDisplay(bool value)
        {
            isNeedsDisplay = value;
            if (value)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    DrawRect(frame);
                }));
            }

        }

    }
}

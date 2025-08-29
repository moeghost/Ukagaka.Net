using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cocoa.AppKit;
namespace Ukagaka
{
    public class SCStatusProgressBarView : NSView
    {
        public static NSColor BACKGROUND_COLOR = NSColor.ColorWithCalibratedRGB(0.3f, 0.3f, 0.3f, 0.7f);

        NSPoint locText1, locText2, locBar, locBarContent;
        NSSize sizeBar, sizeBarContent;
        NSRect frameBar, frameBarContent;
        NSColor colorBarFrame, colorBarLeft, colorBarRight;

        NSFont font;
        NSColor fontColor;
        NSMutableAttributedString text1, text2;
        double val;

        static NSRect mrect = new NSRect();
        static NSRect[] rectarray = new NSRect[] { mrect };




        public SCStatusProgressBarView(NSRect r) : base(r)
        {
            font = NSFont.SystemFontOfSize(11.0f);
            fontColor = NSColor.White;
            text1 = text2 = null;
            val = 0.0;

            float height = r.Height().Value;
            float width = r.Width().Value;

            float fontHeight = font.BoundingRectForFont.Height().Value;

            float barWidth = width - 40;
            float barHeight = 20;
            float barTop = height / 2 + barHeight / 2;
            float barBottom = height / 2 - barHeight / 2;
            float overBarHeight = height - barTop;
            float underBarHeight = overBarHeight;

            locText1 = new NSPoint(7, barTop + overBarHeight * (1.0f / 3.0f));
            locText2 = new NSPoint(7, barBottom - underBarHeight * (1.0f / 3.0f) - fontHeight / 2);
            locBar = new NSPoint(20, barBottom);
            locBarContent = new NSPoint(locBar.X().Value + 2, locBar.Y().Value + 2);
            sizeBar = new NSSize(barWidth, barHeight);
            sizeBarContent = new NSSize(sizeBar.Width().Value - 4, sizeBar.Height().Value - 4);
            colorBarFrame = NSColor.ColorWithCalibratedRGB(1.0f, 1.0f, 1.0f, 0.8f);
            colorBarLeft = NSColor.ColorWithCalibratedRGB(0.9f, 0.0f, 0.0f, 0.5f);
            colorBarRight = NSColor.ColorWithCalibratedRGB(1.0f, 1.0f, 1.0f, 0.2f);
            frameBar = new NSRect(locBar, sizeBar);
            frameBarContent = new NSRect(locBar.X().Value + 2, locBar.Y().Value + 2, sizeBar.Width().Value - 4, sizeBar.Height().Value - 4);
        }

        public void SetText1(string str) // can be null
        {
            if (str == null)
            {
                text1 = null;
            }
            else
            {
                text1 = new NSMutableAttributedString(str);
                NSRange wholeStrRange = new NSRange(0, str.Length);
                text1.AddAttribute(NSAttributedString.FontAttributeName, font, wholeStrRange);
                text1.AddAttribute(NSAttributedString.ForegroundColorAttributeName, fontColor, wholeStrRange);
            }

            Display();
        }

        public void SetText2(string str)
        {
            if (str == null)
            {
                text2 = null;
            }
            else
            {
                text2 = new NSMutableAttributedString(str);
                NSRange wholeStrRange = new NSRange(0, str.Length);
                text2.AddAttribute(NSAttributedString.FontAttributeName, font, wholeStrRange);
                text2.AddAttribute(NSAttributedString.ForegroundColorAttributeName, fontColor, wholeStrRange);
            }

            Display();
        }

        public void SetVal(double d) // 0.0 ... 1.0
        {
            val = d;
            Display();
        }

        public override void DrawRect(NSRect r)
        {
            //  BACKGROUND_COLOR.Set();
            // NSBezierPath.FillRect(r);

            //   if (text1 != null) NSGraphics.DrawString(text1, locText1);
            //  if (text2 != null) NSGraphics.DrawString(text2, locText2);

            if (val >= 0.0 && val <= 1.0)
            {
                //   colorBarFrame.Set();
                //   NSGraphics.FrameRect(frameBar);

                float x, y, w, h;
                int pos = (int)((sizeBar.Width().Value - 4) * val);
                mrect.X().Value = locBar.X().Value + 2;
                mrect.Y().Value = locBar.Y().Value + 2;
                mrect.Width().Value = pos;
                mrect.Height().Value = sizeBar.Height().Value - 4;
                //colorBarLeft.Set();
               // NSGraphics.FillRectList(rectarray);

                mrect.X().Value = locBar.X().Value + 2 + pos;
                mrect.Width().Value = sizeBar.Width().Value - 4 - pos;
                //colorBarRight.Set();
                //NSGraphics.FillRectList(rectarray);
            }
            else
            {
                mrect.x = locBarContent.x;
                mrect.width = sizeBarContent.width;
                mrect.Height().Value = 1;
                int top = (int)locBarContent.Y().Value + (int)sizeBarContent.Height().Value;
                for (int y = (int)locBarContent.Y().Value; y < top; y++)
                {
                    float white = (float)(((new Random().Next() * int.MaxValue) % 70) / 100.0f);
                    float alpha = (float)(((new Random().Next() * int.MaxValue) % 70) / 100.0f);
                   // NSColor.ColorWithCalibratedWhite(white, alpha).Set();

                    mrect.Y().Value = y;
                  //  NSGraphics.FrameRect(mrect);
                }
            }
        }
    }

}

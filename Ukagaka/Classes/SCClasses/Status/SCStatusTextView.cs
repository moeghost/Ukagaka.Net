using Cocoa.AppKit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ukagaka 
{
    
    public class SCStatusTextView : NSView
    {
        private static readonly NSRange FIRSTCHAR_RANGE = new NSRange(0, 1);
        private const int LINE_INTERVAL = 0;

        public static NSColor BACKGROUND_COLOR = NSColor.ColorWithCalibratedRGB(0.3f, 0.3f, 0.3f, 0.7f);

        NSFont font;
        float fontHeight;
        NSColor defaultColor;
        NSColor currentColor;

        float x, y;
        NSImage offscreen;

        StringBuilder buffer;

        public SCStatusTextView(NSRect r) : base(r)
        {
            buffer = new StringBuilder();

            SetFont(NSFont.SystemFontOfSize(10.0f));
            defaultColor = currentColor = NSColor.Green;

            offscreen = new NSImage(r.Size());

            x = 0;
            y = r.Height().Value - fontHeight;
        }

        public void SetFont(NSFont f)
        {
            font = f;
            fontHeight = (float)Math.Round(font.BoundingRectForFont.Height().Value);
        }

        public void SetColor(NSColor c)
        {
            currentColor = c;
        }

        public override void DrawRect(NSRect r)
        {
            BACKGROUND_COLOR.Set();
            NSBezierPath.FillRect(r);

            // Flush the buffer
            if (buffer.Length > 0)
            {
                // Graphics g = offscreen.LockFocus();

                offscreen.LockFocus();
                while (buffer.Length > 0)
                {
                    char c = buffer[0];
                    if (c == '%') // Control command
                    {
                        int blockStart = 1;
                        int blockEnd = buffer.ToString().IndexOf(']');

                        try
                        {
                            ExecuteControlCommand(new StringTokenizer(buffer.ToString().Substring(blockStart + 1, blockEnd - blockStart), ","));
                            buffer.Remove(0, blockEnd + 1);
                        }
                        catch (Exception e)
                        {
                            Console.Error.WriteLine("SCStatusTextView: control command error.");
                            Console.Error.WriteLine(e.StackTrace);
                            buffer.Remove(0, 1);
                        }
                    }
                    else
                    {
                        DrawChar(c);
                        buffer.Remove(0, 1);
                    }
                }
                offscreen.UnlockFocus();
            }

            offscreen.CompositeToPointFromRect(r.Origin(), r, offscreen.GetBitmap());
        }

        private void DrawChar(char c)
        {
            NSMutableAttributedString astr = new NSMutableAttributedString(c.ToString());
            astr.AddAttribute(NSAttributedString.FontAttributeName, font, FIRSTCHAR_RANGE);
            astr.AddAttribute(NSAttributedString.ForegroundColorAttributeName, currentColor, FIRSTCHAR_RANGE);


            float widthOfCurrentChar = (float)NSGraphics.SizeOfAttributedString(astr).Width().Value;
            // If not a newline character, check if drawing this character exceeds the frame width and move to a new line
            if (c == '\n' || x + widthOfCurrentChar > Frame.Width().Value)
            {
                x = 0;
                y -= fontHeight + LINE_INTERVAL;

                if (y < 0) // Scroll if y has gone below the lower limit (0)
                {
                    float deltaY = -y;

                    // Push the current content of the screen upwards
                    offscreen.CompositeToPoint(new NSPoint(0, deltaY), offscreen.GetBitmap());
                    NSColor.ClearColor.Set();
                   // NSGraphics.FillRectList(new NSRect[] { new NSRect(0, 0, Frame.Width, fontHeight + LINE_INTERVAL) });

                    y = LINE_INTERVAL;
                }
            }
            if (c != '\n')
            {
              // NSGraphics.DrawString(astr, new NSPoint(x, y));
                x += widthOfCurrentChar;
            }
        }

        private void ExecuteControlCommand(StringTokenizer args)
        {
            string type = args.NextToken();
            if (type.Equals("color"))
            {
                string name = args.NextToken();
                if (name.Equals("default"))
                {
                    currentColor = defaultColor;
                }
                else if (name.Equals("black"))
                {
                    currentColor = NSColor.Black;
                }
                else if (name.Equals("blue"))
                {
                    currentColor = NSColor.Blue;
                }
                else if (name.Equals("brown"))
                {
                    currentColor = NSColor.Brown;
                }
                else if (name.Equals("clear"))
                {
                    currentColor = NSColor.Clear;
                }
                else if (name.Equals("darkgray"))
                {
                    currentColor = NSColor.DarkGray;
                }
                else if (name.Equals("gray"))
                {
                    currentColor = NSColor.Gray;
                }
                else if (name.Equals("green"))
                {
                    currentColor = NSColor.Green;
                }
                else if (name.Equals("lightgray"))
                {
                    currentColor = NSColor.LightGray;
                }
                else if (name.Equals("magenta"))
                {
                    currentColor = NSColor.Magenta;
                }
                else if (name.Equals("orange"))
                {
                    currentColor = NSColor.Orange;
                }
                else if (name.Equals("purple"))
                {
                    currentColor = NSColor.Purple;
                }
                else if (name.Equals("red"))
                {
                    currentColor = NSColor.Red;
                }
                else if (name.Equals("yellow"))
                {
                    currentColor = NSColor.Yellow;
                }
                else if (name.Equals("white"))
                {
                    currentColor = NSColor.White;
                }
                else
                {
                    string r = name;
                    string g = args.NextToken();
                    string b = args.NextToken();
                    try
                    {
                        currentColor = NSColor.ColorWithCalibratedRGB(
                            float.Parse(r) / 255.0f,
                            float.Parse(g) / 255.0f,
                            float.Parse(b) / 255.0f,
                            1.0f);
                    }
                    catch (Exception e) { }
                }
            }
        }

        public void PrintStr(string str)
        {
            buffer.Append(str);
            Display();
        }
    }

}

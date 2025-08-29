using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Windows;
namespace Cocoa.AppKit
{
    public class NSRect
    {
        public CGFloat x;
        public CGFloat y;
        public CGFloat width;
        public CGFloat height;

        public NSPoint origin;
        public NSSize size;

        public static NSRect ZeroRect { get; internal set; }
        public NSPoint location { get; internal set; }


        public NSPoint Location()
        {
            return location;
        }

        public NSRect()
        {

            this.x = new CGFloat(0);
            this.y = new CGFloat(0);
            this.width = new CGFloat(0);
            this.height = new CGFloat(0);
            this.origin = new NSPoint(x, y);
            this.size = new NSSize(width, height);
            this.location = this.origin;
        }
        public NSRect(NSPoint Origin, NSSize Size)
        {
            this.origin = Origin;
            this.size = Size;
            this.x = Origin.x;
            this.y = Origin.y;
            this.width = Size.Width();
            this.height = Size.Height();
            this.location = this.origin;
        }

        public Rectangle ToRectangle()
        {

            return new Rectangle(this.X().IntValue(), this.Y().IntValue(), this.Width().IntValue(), this.Height().IntValue());

        }

        public Rect ToRect()
        {
            return new Rect(this.X().IntValue(), this.Y().IntValue(), this.Width().IntValue(), this.Height().IntValue());

        }


        public bool ContainsPoint(System.Windows.Point point)
        {

            return ToRect().Contains(point);

        }

        public bool ContainsPoint(System.Drawing.Point point)
        {

            return ToRectangle().Contains(point);

        }

        public bool ContainsPoint(NSPoint point)
        {

            return ToRectangle().Contains(point.ToPoint());

        }



        public NSRect(int x, int y, int width, int height)
        {

            this.x = new CGFloat(x);
            this.y = new CGFloat(y);
            this.width = new CGFloat(width);
            this.height = new CGFloat(height);
            this.origin = new NSPoint(this.x, this.y);
            this.size = new NSSize(this.width, this.height);
            this.location = this.origin;
        }

        public NSRect(double x, double y, double width, double height)
        {
            this.x = new CGFloat(x);
            this.y = new CGFloat(y);
            this.width = new CGFloat(width);
            this.height = new CGFloat(height);
            this.origin = new NSPoint(this.X(), this.Y());
            this.size = new NSSize(this.Width(), this.Height());

            this.location = this.Origin();
        }




        public NSRect(float x, float y, float width, float height)
        {
            this.x = new CGFloat(x);
            this.y = new CGFloat(y);
            this.width = new CGFloat(width);
            this.height = new CGFloat(height);
            this.origin = new NSPoint(this.X(), this.Y());
            this.size = new NSSize(this.Width(), this.Height());

            this.location = this.Origin();
        }
        public NSRect(CGFloat x,CGFloat y,CGFloat width,CGFloat height)
        {

            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.origin = new NSPoint(x, y);
            this.size = new NSSize(width, height);
            this.location = this.origin;
        }

        public CGFloat X()
        {
            return x;
        }

        public CGFloat Y()
        {
            return y;
        }
        public CGFloat Width()
        {
            return width;
        }
        public CGFloat Height()
        {
            return height;
        }

        public NSPoint Origin()
        {
            return origin;

        }

        public NSSize Size()
        {
            return size;
        }

        internal NSRect RectByIntersectingRect(NSRect nSRect)
        {
            return new NSRect();
            //throw new NotImplementedException();
        }

        internal bool IntersectsRect(NSRect region)
        {
            throw new NotImplementedException();
        }

        internal CGFloat MaxX()
        {
            throw new NotImplementedException();
        }
    }
}

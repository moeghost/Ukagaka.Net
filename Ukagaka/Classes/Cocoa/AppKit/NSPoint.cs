using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using Ukagaka.Classes.Cocoa.AppKit;
namespace Cocoa.AppKit
{
    public class NSPoint:NSObject
    {
        public CGFloat x;
        public CGFloat y;
        public static NSPoint ZeroPoint = new NSPoint();


        public NSPoint()
        {
            x = new CGFloat(0);
            y = new CGFloat(0); ;

        }


        public NSPoint(CGFloat x, CGFloat y)
        {
            this.x = x;
            this.y = y;
        }


        public NSPoint(float x, float y)
        {
            this.x = new CGFloat(x);
            this.y = new CGFloat(y);
        }


        public NSPoint(double x, double y)
        {
            this.x = new CGFloat(x);
            this.y = new CGFloat(y);
        }
        public NSPoint(int x, int y)
        {
            this.x = new CGFloat(x);
            this.y = new CGFloat(y);
        }





        public Point ToPoint()
        {
            return new Point(this.X().IntValue(), this.Y().IntValue());

        }

        public void SetX(int x)
        {
            this.x = new CGFloat(x);
        }

        public void SetY(int y)
        {
            this.y= new CGFloat(y);
        }

        public CGFloat X()
        {
            return x;

        }

        public CGFloat Y()
        {
            return y;
        }

      

    }
}

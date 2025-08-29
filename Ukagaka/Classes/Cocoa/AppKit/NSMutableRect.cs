using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cocoa.AppKit
{
    public class NSMutableRect : NSRect
    {
        public void SetOrigin(NSPoint point)
        {
            this.origin = point;
        }

        public void SetSize(NSSize size)
        {
            this.size = size;
        }

        public void SetWidth(int width)
        {
            this.width = new CGFloat(width);
        }

        public void SetHeight(int height)
        {
            this.height = new CGFloat(height); ;
        }



        public void SetWidth(CGFloat width)
        {
            this.width = width;
        }

        public void SetHeight(CGFloat height)
        {
            this.height = height;
        }

        public void SetX(CGFloat x)
        {
            this.x = x;
        }


        public void SetY(CGFloat y)
        {
            this.y = y;
        }



        public void SetX(int x)
        {
            this.x = new CGFloat(x); ;
        }


        public void SetY(int y)
        {
            this.y = new CGFloat(y); ;
        }
    }
}

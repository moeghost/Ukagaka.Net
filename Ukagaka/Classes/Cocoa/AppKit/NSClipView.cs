using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cocoa.AppKit
{
    public class NSClipView:NSView
    {

        public NSRect DocumentVisibleRect()
        {
            return new NSRect();
        }

        public NSRect DocumentRect()
        {
            return new NSRect();
        }

        internal void ScrollToPoint(NSPoint nSPoint)
        {
           // throw new NotImplementedException();
        }
    }
}

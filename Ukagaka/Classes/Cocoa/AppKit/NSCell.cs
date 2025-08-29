using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cocoa.AppKit
{
    public class NSCell:NSView
    {
        public static int OnState = 1;
        public static int OffState = 0;

        public bool IsHighlighted()
        {
            return false;
        }

        public NSRect KeyEquivalentRectForBounds(NSRect r)
        {
            return new NSRect(new NSPoint(),r.Size());
        }

    }
}

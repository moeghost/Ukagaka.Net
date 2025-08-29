using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cocoa.AppKit;
namespace Ukagaka
{
    public class NSGraphics
    {
         public static NSSize SizeOfAttributedString(NSAttributedString str)
        {
            return new NSSize(str.ToString().Length, 0);
        }

        internal static void DrawAttributedString(NSMutableAttributedString astr, object value)
        {
            throw new NotImplementedException();
        }

        internal static void DrawStringWithAttributes(NSMutableAttributedString shadowstr, NSPoint point)
        {
            throw new NotImplementedException();
        }

        internal static void FillRects(NSRect[] rectlistBounds)
        {
            //throw new NotImplementedException();
        }

        internal static void RectFillList(NSRect[] cGRects)
        {
          //  throw new NotImplementedException();
        }
    }
}

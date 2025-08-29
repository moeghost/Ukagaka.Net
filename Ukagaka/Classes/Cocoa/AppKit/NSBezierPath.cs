using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cocoa.AppKit
{
    public class NSBezierPath
    {
        public static object LineCapStyleButt { get; internal set; }

        public static void FillRect(NSRect r)
        {


        }

        internal static void SetDefaultLineCapStyle(object lineCapStyleButt)
        {
            throw new NotImplementedException();
        }

        internal static void SetDefaultLineWidth(float v)
        {
            throw new NotImplementedException();
        }

        internal static void StrokeLine(NSPoint cGPoint1, NSPoint cGPoint2)
        {
           // throw new NotImplementedException();
        }

        internal static void StrokeLineFromPoint(NSPoint nSPoint1, NSPoint nSPoint2)
        {
            throw new NotImplementedException();
        }
    }
}

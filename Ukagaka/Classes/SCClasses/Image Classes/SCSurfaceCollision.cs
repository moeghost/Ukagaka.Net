using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Cocoa.AppKit;
namespace Ukagaka
{
   
     
    public class SCSurfaceCollision
    {
        NSRect rect;
        String name;

        public SCSurfaceCollision(NSRect r, String n)
        {
            rect = r;
            name = n;
        }

        public NSRect Rect()
        {
            return rect;
        }

          


        public String Name()
        {
            return name;
        }


    }
}

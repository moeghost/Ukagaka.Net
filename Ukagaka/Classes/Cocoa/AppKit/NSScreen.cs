using Cocoa.AppKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace Cocoa.AppKit
{
    public class NSScreen
    {
        public static NSScreen MainScreen = new NSScreen();

        public NSRect VisibleFrame = new NSRect();

        public float Width = (float)System.Windows.SystemParameters.PrimaryScreenWidth;

        public NSRect Frame;


        public NSScreen() 
        {
             
            Frame = new NSRect(new NSPoint(), new NSSize(Width, Height));
            VisibleFrame = Frame;
             
        }



        public float Height = (float)System.Windows.SystemParameters.PrimaryScreenHeight;


    }
}

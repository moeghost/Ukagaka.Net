using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Cocoa.AppKit
{
 
    public class NSApplication
    {
        public static NSApplication SharedApplication { get; } = new NSApplication();

        public bool IsHidden()
        {
            Application app = Application.Current;
            if (app == null || app.MainWindow == null) return true;

            return app.MainWindow.Visibility != Visibility.Visible
                   || app.MainWindow.WindowState == WindowState.Minimized;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Cocoa.AppKit
{
    public class NSWindowController:NSWindowDelegate
    {
        public NSWindow Window;
        public NSWindowController(NSWindow Window) 
        { 
            this.Window = Window;

        }

        public NSWindowController()
        {
            // 在后台线程中调用：
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Window = new NSWindow();
            });
        }

        public virtual void Close()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                this.Window.Close();
            });
        }

        public virtual void MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {


        }

        public virtual void  MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        public virtual void MouseDown(object sender, MouseButtonEventArgs e)
        {
             
        }

        public virtual void MouseMoved(object sender, MouseEventArgs e)
        {
             
        }

        public void SetFrameAutoSaveName(string name)
        {

        }
    }
}

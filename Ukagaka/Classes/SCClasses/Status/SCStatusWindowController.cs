using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Cocoa.AppKit;
namespace Ukagaka
{
   
    public class SCStatusWindowController:NSWindowController
    {
        public static float WINDOW_WIDTH = 230; // 166
        public static float WINDOW_HEIGHT = 120; // 90

        private SCStatusWindowCenter center;
        private SCStatusTextView viewTextTextView;
        private SCStatusProgressBarView viewProgressBarView;

        private Hashtable refcon; // SCStatusWindowCenterが使用します。

        public SCStatusWindowController(SCStatusWindowCenter center) : base(new SCStatusWindow(WINDOW_WIDTH, WINDOW_HEIGHT))
        {
            viewTextTextView = new SCStatusTextView(Window.Frame());
            viewProgressBarView = new SCStatusProgressBarView(Window.Frame());

            this.center = center;
            //Window.Level = NSWindowLevel.Floating;
        }

        public void SetRefcon(Hashtable o) { refcon = o; }
        public Hashtable GetRefcon() { return refcon; }

        public void Show()
        {
            if (Window.IsVisible)
            {
                return;
            }
            /*
            NSTimer openTimer = new NSTimer(
                0.0,
                this,
                new Selector("ShowWindow", "NSObject"),
                NSNull.Null,
                false);
            SCFoundation.SharedFoundation().GetRunLoopOfMainThread().AddTimer(openTimer, NSRunLoopMode.Default);
            if (!Foundation.NSThread.CurrentThread.Name.Equals("main"))
            {
                while (!Window.IsVisible) { } // 待機
            }
            */
        }

        public void CloseWindow()
        {
            center.CloseStatusWindow(this);
        }

        public void SetTypeToText()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Window.Content = viewTextTextView;
            });
        }

        public void SetTypeToProgressBar()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Window.Content = viewProgressBarView;
            });
        }

        /************** テキストタイプ *************/
        public void TextTypePrint(string str)
        {
            viewTextTextView.PrintStr(str);
        }

        public void TextTypePrintln(string str)
        {
            viewTextTextView.PrintStr(str + '\n');
        }

        /*********** プログレスバータイプ ***********/
        public void ProgressBarTypeSetText1(string str)
        {
            viewProgressBarView.SetText1(str);
        }

        public void ProgressBarTypeSetText2(string str)
        {
            viewProgressBarView.SetText2(str);
        }

        public void ProgressBarTypeSetVal(double val)
        {
            viewProgressBarView.SetVal(val);
        }
    }

}

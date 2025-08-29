using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using Ukagaka;

namespace Cocoa.AppKit
{


    public interface NSWindowDelegate
    {



        public abstract void MouseLeftButtonDown(object sender, MouseButtonEventArgs e);



        public abstract void MouseUp(object sender, MouseButtonEventArgs e);


        public abstract void MouseDown(object sender, MouseButtonEventArgs e);



        public abstract void  MouseMoved(object sender, MouseEventArgs e);
         


    }












    /// <summary>
    /// NSWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NSWindow : Window
    {

        private NSRect frame;

        public float AlphaValue = 1.0f;

        public NSWindowDelegate Delegate;

        private int mouseDown;

        public bool IsDoubleClicked = false;
        public NSView view;

        public NSWindow()
        {
            InitializeComponent();
            view = new NSView(View);
            this.frame = new NSRect(new NSPoint(), new NSSize((int)Width, (int)Height));
           // this.Content = view;
            this.View.Children.Add(view);
        }

        public NSWindow(NSRect rect)
        {

            InitializeComponent();
            view = new NSView(View);
            this.Width = rect.Width().Value;

            this.Height = rect.Height().Value;



            this.frame = rect;

            this.VisualOffset = new Vector(rect.X().Value, rect.Y().Value);

            // this.Content = view;

            this.View.Children.Add(view);
        }








        public NSWindow(CGFloat Width, CGFloat Height)
        {

            if (!Thread.CurrentThread.IsBackground && !Thread.CurrentThread.IsThreadPoolThread)
            {
                InitializeComponent();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => InitializeComponent());
            }
            view = new NSView(View);

            this.Width = Width.Value;

            this.Height = Height.Value;

            this.frame = new NSRect(new NSPoint(), new NSSize(Width, Height));

            this.View.Children.Add(view);
        }
        public NSWindow(float Width, float Height)
        {
            if (!Thread.CurrentThread.IsBackground && !Thread.CurrentThread.IsThreadPoolThread)
            {
                InitializeComponent();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => InitializeComponent());
            }
            view = new NSView(View);

            this.Width = Width;

            this.Height = Height;

            this.frame = new NSRect(new NSPoint(), new NSSize(Width, Height));

            //this.Content = view;

            this.View.Children.Add(view);
        }

        public NSRect Frame()
        {
            NSRect rect = new NSRect();
            this.Dispatcher.Invoke((Action)(() => {
                rect = new NSRect((int)this.VisualOffset.X, (int)this.VisualOffset.Y, (int)this.Width, (int)this.Height);

            }));

            return rect;

        }

        public void SetBackgroundColor(NSColor color)
        {
            
        }

        public void SetOpaque(bool value)
        {
            if (value == false)
            {
                this.AllowsTransparency = true;

                this.Background = Brushes.Transparent;

                this.WindowStyle = WindowStyle.None;
            }
        }
        public void SetHasShadow(bool value)
        {

        } 


        public float X()
        {
            float x = 0;
            this.Dispatcher.Invoke((Action)(() =>
            {
                x = (float)this.VisualOffset.X;
            }));
            return x;
        }

        public float Y()
        {
            float y = 0;
            this.Dispatcher.Invoke((Action)(() =>
            {
                y = (float)this.VisualOffset.Y;
            }));
            return y;
        }





        public void SetFrameOrigin(NSPoint origin)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                this.VisualOffset = new Vector(origin.X().Value, origin.Y().Value);
                this.frame.origin = origin;
            }));
        }




        public void SetFrameSize(NSSize size)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {

                this.Width = size.Width().Value;
                this.Height = size.Height().Value;
            }));
        }


        public void SetFrame(NSRect rect)
        {
            SetFrameOrigin(rect.Origin());
            SetFrameSize(rect.Size());
        }

        public void SetViewsNeedDisplay(bool value)
        {
            view.SetNeedsDisplay(value);
        }

        
        internal void SetAlphaValue(float v)
        {
            this.AlphaValue = v;
        }

        internal void OrderOut(object value)
        {
             
        }
        internal void SetLevel(int level)
        {
           // throw new NotImplementedException();
        }

        public virtual void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Delegate != null)
            {
                Delegate.MouseLeftButtonDown(sender, e);
            }
        }

        public virtual NSView ContentView()
        {
            return view;
        }




        public virtual void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        { 
            mouseDown += 1;
             
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            timer.Tick += (s, e1) => { timer.IsEnabled = false; mouseDown = 0; };
            timer.IsEnabled = true;
             
            if (mouseDown % 2 ==0)
            {
                timer.IsEnabled = false;
                mouseDown = 0;
                IsDoubleClicked = true;
            }

            if (Delegate != null)
            {
                Delegate.MouseUp(sender, e);
            }
        }
        public virtual void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Delegate != null)
            {
                Delegate.MouseDown(sender, e);
            }
        }

        public virtual void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (Delegate != null)
            {
                Delegate.MouseMoved(sender, e);
            }
        }
        public void SetFrameAutoSaveName(string name)
        {

        }

        internal void Display()
        {
            throw new NotImplementedException();
        }
    }
}

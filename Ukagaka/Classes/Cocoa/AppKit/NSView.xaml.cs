using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cocoa.AppKit
{
    /// <summary>
    /// NSView.xaml 的交互逻辑
    /// </summary>
    public partial class NSView : UserControl
    {

        NSRect frame;
        NSRect bounds;
        private UIElement chid;
        bool isNeedDisplay = false;


        public NSView Superview {


            get
            {
                 return this.Parent as NSView;
            }
            
            
            
            set
            {

            }
        
        }



        public NSRect Frame
        {
            get
            {
                return frame;   
            }
            set
            { 
                frame = value;
                this.SetFrame(frame);
            }
        }

        public NSRect Bounds
        {
            get
            {
                bounds = new NSRect(new NSPoint(0,0),frame.Size());

                return bounds;
            }
            
        }


        public NSView(NSRect r)
        {
            InitializeComponent();
            frame = r;
          
            this.SetFrame(frame);
        }

        public NSView()
        {
            InitializeComponent();
            frame = new NSRect();
        }

        public NSView(UIElement chid)
        {
            this.Content = chid;
            InitializeComponent();
            this.chid = chid;
            frame = new NSRect();
        }

        public float X()
        {
            return (float)this.VisualOffset.X;
        }

        public float Y()
        {
            return (float)this.VisualOffset.Y;
        }

        public void SetFrameOrigin(NSPoint point)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {

                this.VisualOffset = new Vector(point.X().Value, point.Y().Value);

                this.frame.origin = point;
                
            }));
        }

        public void SetFrameSize(NSSize size)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {

                this.Width = size.Width().Value;
                this.Height = size.Height().Value;

                this.frame.size = size;
                this.View.Width = size.Width().Value;
                this.View.Height = size.Height().Value;

            }));
        }

        public NSSize Size()
        {
            return frame.Size();

        }


        public void SetFrame(NSRect rect)
        {
            SetFrameOrigin(rect.Origin());
            SetFrameSize(rect.Size());
        }

         public virtual void DrawRect(NSRect r)
        {
             
        }
        public virtual void DrawRect()
        {

        }

        public virtual void Display()
        {
             
        }

        public virtual NSView ContentView()
        {
            return this.Content as NSView;
        }

        internal void MouseMoved(object sender, MouseEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void RemoveFromSuperview()
        {


        }

        internal List<NSView> Subviews()
        {
            List<NSView> arrayList = new List<NSView>();
            foreach ( UIElement chid in this.View.Children)
            {
                arrayList.Add(chid as NSView);
            }

            return arrayList;
        }

        public void AddSubview(NSView view)
        {
             this.View.Children.Add(view);
        }

        public void AddSubview(UIElement view)
        {
            this.View.Children.Add(view);
        }



        public virtual void SetNeedsDisplay(bool value)
        {
            isNeedDisplay = value;
        }
        public void SetEnabled(bool isEnabled)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.IsEnabled = isEnabled;
            });

        }

        public virtual void OnMouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        public virtual void OnMouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        public NSWindow Window()
        {
            throw new NotImplementedException();
        }

        public virtual void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        internal bool IsFlipped()
        {
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Cocoa.AppKit;
using System.Threading;
namespace Ukagaka
{
    /// <summary>
    /// SCShellWindowController.xaml オトスササ．ツ゜シュ
    /// </summary>
    public partial class SCBorderlessWindow : NSWindow
    {



        public SCBorderlessWindow():base()
        {
            InitializeComponent();
            //InitializeComponent();
            this.ShowInTaskbar = false;

            this.Owner = Window.GetWindow(MainWindow.SharedMainWindow());
            SetOpaque(false);
            // SetOpaque(false);
            // this.Owner = Window.GetWindow(MainWindow.SharedMainWindow());
            //  this.image = new Image();
        }



        public SCBorderlessWindow(
           NSRect contentRect, int styleMask, int backingType, bool defer):base(contentRect)
        {

            //setBackgroundColor(NSColor.clearColor());
          //   SetOpaque(false);
            // setHasShadow(false);
        }

        public bool CanBecomeKeyWindow()
        {
            return true;
        }
        public override void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }


        public override void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }
        public override void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        public override void Grid_MouseMove(object sender, MouseEventArgs e)
        {


        }

         
        public void SetContentSize(NSSize size)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.RenderSize = new Size(size.Width().Value, size.Height().Value);
            });

        }

        public void FlushWindow()
        {

        }

        internal void SetIgnoresMouseEvents(bool value)
        {
            //throw new NotImplementedException();
        }

        internal void SetFrameTopLeftPoint(NSPoint loc)
        {
            
        }

       
    }
}

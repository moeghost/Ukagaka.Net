using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cocoa.AppKit
{
    public class NSTextField: TextBox
    {
         
        internal void SetIntValue(int n)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.Text = n.ToString();
            });
        }

        internal void SetStringValue(string v)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.Text = v;


            });
        }

        internal void SetTextColor(NSColor c)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.Foreground = new SolidColorBrush(c.GetMediaColor());
            });
        }

        internal string StringValue()
        {
             return Text;
        }

        public void SetEnabled(bool value)
        {
            if (value)
            {
                this.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.Visibility = System.Windows.Visibility.Hidden;
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Cocoa.AppKit
{
    public class NSButton:Button
    {
        string title;
        int intValue;

        internal int IntValue()
        {
            return intValue;
        }
 

        internal void SetIntValue(int value)
        {
            intValue = value;
        }

        internal void SetTitle(string title)
        {
            this.title = title;
        }

        public void SetEnabled(bool isEnabled)
        {

            Application.Current.Dispatcher.Invoke(() =>
            {
                this.IsEnabled = isEnabled;
            });

        }


    }
}

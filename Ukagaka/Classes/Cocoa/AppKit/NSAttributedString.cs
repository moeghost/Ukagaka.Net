using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cocoa.AppKit
{
     public class NSAttributedString
    {
        public static string FontAttributeName = "FontAttributeName";

        public static string ForegroundColorAttributeName = "ForegroundColorAttributeName";

        public string baseString;

        public NSAttributedString(string baseString)
        {
             this.baseString = baseString;
        }

        public string ToString() 
        {
            return baseString;
        }

    }
}

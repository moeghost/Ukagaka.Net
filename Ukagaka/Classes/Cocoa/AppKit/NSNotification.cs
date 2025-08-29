using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cocoa.AppKit
{
    public class NSNotification
    {
        NSUserDefaults userInfo;
        string name = "";
        public string Name
        {

            get
            {
                return name;
            }
            set
            {
                name = value;

            }
        }
        public NSNotification()
        {

            userInfo = new NSUserDefaults();
        }

        public object Object()
        {
            return this;
        }

        internal NSUserDefaults UserInfo()
        {
            return userInfo;
        }
    }
}

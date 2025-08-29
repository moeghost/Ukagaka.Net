using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ukagaka;

namespace Cocoa.AppKit
{
    public class NSNotificationCenter
    {
        private static NSNotificationCenter defaultCenter;
        private object obj;
        private NSSelector selector;
        private string name;
        private object description;
        public static NSNotificationCenter DefaultCenter()
        {
            if (defaultCenter == null)
            {
                defaultCenter = new NSNotificationCenter();
            }


            return defaultCenter;
        }

        public void AddObserver(Object obj, NSSelector selector, string name, object description)
        {
            this.obj = obj;
            this.selector = selector;
            this.name = name;
            this.description = description;
        }
    }
}

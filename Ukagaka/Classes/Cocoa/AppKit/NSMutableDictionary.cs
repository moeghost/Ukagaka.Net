using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ukagaka;

namespace Cocoa.AppKit
{
    [Serializable]
    public class NSMutableDictionary:NSDictionary
    {
      
        public NSMutableDictionary():base()
        {
            
        }
         
        internal void SetObjectForKey(object obj, string entry_name)
        {
            base.SetObjectForKey(obj, entry_name);
        }

        public object ObjectForKey(string key)
        {
             return base.ObjectForKey(key);
        }

    }
}

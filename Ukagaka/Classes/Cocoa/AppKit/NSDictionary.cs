using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  Cocoa.AppKit
{
    public class NSDictionary:Dictionary<string, object>
    {
         
        public NSDictionary():base()
        {
             
        }
         
        internal void SetObjectForKey(object obj, string entry_name)
        {
            this[entry_name] = obj;
        }

        public object ObjectForKey(string key)
        {
            if (this.ContainsKey(key))
            {
                return this[key];
            }

            return null;
        }

 

    }
}

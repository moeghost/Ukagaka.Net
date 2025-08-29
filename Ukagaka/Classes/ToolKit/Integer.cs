using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ukagaka.Classes.Cocoa.AppKit;

namespace Ukagaka
{
    public class Integer:Object
    {
        private int value;

        public Integer() 
        { 
            value = 0;
        }

        public static int ParseInt(string str)
        {
            return Convert.ToInt32(str);
        }

        public static String ToString(int value)
        {
            return value.ToString();
        }

        internal static int ParseInt(string v1, int v2)
        {
            throw new NotImplementedException();
        }

        public int IntValue()
        {
            return ParseInt(this.ToString());
        }
    }
}

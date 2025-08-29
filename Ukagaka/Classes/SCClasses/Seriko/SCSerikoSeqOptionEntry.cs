using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ukagaka
{
    public class SCSerikoSeqOptionEntry
    {
        String type;

        public SCSerikoSeqOptionEntry(String param)
        {
            /*
              param : [id]option,以降を指定して下さい。

              例えば
              exclusive
              等です。
            */
            type = param;
        }

        public void dump()
        {
            System.Console.WriteLine("Option : " + type);
        }

        public String toString()
        {
            return "type: " + type;
        }

        public String Type()
        {
            return type;
        }


    }
}

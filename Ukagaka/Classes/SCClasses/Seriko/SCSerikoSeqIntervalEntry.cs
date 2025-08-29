using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ukagaka
{
    public class SCSerikoSeqIntervalEntry
    {

        String Type;
        int talk_interval; // インターバルタイプがtalkでない場合は常に0が入っています。
        int exec_probability = 1; // random系でない場合は常に1が入っています。

        public SCSerikoSeqIntervalEntry(String param)
        {

            /*
              param : [id]interal,以降を指定して下さい。

              例えば
              always
              runonce
              talk,[interval]
              等です。
            */
            if (param.Equals("sometimes"))
            {
                Type = "random";
                exec_probability = 2;
            }
            else if (param.Equals("rarely"))
            {
                Type = "random";
                exec_probability = 4;
            }
            else if (param.StartsWith("random"))
            {
                Type = "random";

                try
                {
                    exec_probability = Integer.ParseInt(param.Substring(7));
                }
                catch (Exception e) { }
            }
            else if (param.StartsWith("talk"))
            {
                Type = "talk";
                try
                {
                    talk_interval = Integer.ParseInt(param.Substring(5));
                }
                catch (Exception e) { }
            }
            else
            {
                Type = param;
            }
        }

        public void dump()
        {


            System.Console.Write("Interval : " + Type + "," + exec_probability);
        }

        public String toString()
        {
            StringBuilder buf = new StringBuilder();

            buf.Append("type: ").Append(Type);
            buf.Append("; probability: ").Append(exec_probability);
            return buf.ToString();
        }

        public String type()
        {
            return Type;
        }

        public int talkInterval()
        {
            // タイプがtalkでない場合は常に0が返されます。
            return talk_interval;
        }

        public int probability()
        {
            // タイプがrandom系でない場合は常に1が返されます。
            return exec_probability;
        }
    }
}

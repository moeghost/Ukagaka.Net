using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ukagaka
{
    public class SCScriptEntry
    {

        string name; // 先頭の#も含んだ名前。
        string data; // スクリプト。

        public SCScriptEntry(string name, string data)
        {
            this.name = name;
            this.data = data;
        }

        public string GetName()
        {
            return name;
        }

        public string GetScript()
        {
            return data;
        }

    }
}

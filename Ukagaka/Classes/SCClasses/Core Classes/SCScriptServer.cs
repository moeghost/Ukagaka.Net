using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ukagaka
{

    public class SCScriptServer
    {
        ArrayList entries; // 中身はSCScriptEntryクラス。

        public SCScriptServer()
        {
            entries = new ArrayList();
        }

        public String FindEntry(String name)
        {
            // エントリを探します。見つからなければnullを返します。
            int n_entries = entries.Count;
            for (int i = 0; i < n_entries; i++)
            {
                SCScriptEntry ent = (SCScriptEntry)entries.ToArray().ElementAt(i);
                if (ent.GetName().Equals(name))
                {
                    return ent.GetScript();
                }
            }

            return null;
        }

        public void AddEntry(SCScriptEntry ent)
        {
            entries.Add(ent);
        }

        public void Dump()
        {
            // SCScriptServerのデバッグ用。実行には必要ない。
            int n_entries = entries.Count;
            for (int i = 0; i < n_entries; i++)
            {
                SCScriptEntry ent = (SCScriptEntry)entries.ToArray().ElementAt(i);
                //System.out.println("#" + ent.GetName() + "\n   " + ent.getScript() + "\n");
            }
        }

        public void SweepAway()
        {
            entries.Clear();
        }

        protected void finalize()
        {
            //Logger.log(this, Logger.DEBUG, "finalized");
        }
    }
}

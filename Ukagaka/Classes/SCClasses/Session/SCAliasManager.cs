using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.IO;
using System.Collections;
using Utils;
namespace Ukagaka
{
    public class SCAliasManager:SCDescription
    {

        public SCAliasManager(File f):base(f)
        {
            // f = alias.txt
       
        }

        public String ResolveAlias(String entry)
        {
            // entryで示されたエントリがalias.txtに存在した場合は置き換えて、存在しなければそのまま返します。
            String result = GetStrValue(entry);
            return (result == null ? entry : result);
        }

        public String InverseSearch(String data)
        {
            return (String)base.InverseSearch(data);
        }


    }
}

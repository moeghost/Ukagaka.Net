using LittleGrayCalculator.Cores;
using System.Collections;

namespace aya.Eval
{
    public class Namespace
    {
        private Hashtable hash; // {変数名 => Variable}

        public Namespace()
        {
            hash = new Hashtable();
        }

        public Namespace(Namespace parent)
        {
            // 親名前空間を継承して作成。
            hash = new Hashtable(parent.GetHashtable());
        }

        protected Hashtable GetHashtable()
        {
            return hash;
        }

        public Namespace Put(string name, Variable variable)
        {
         
            if (name =="_i")
            {
                ;
            }
            
            hash[name] = variable;

            return this;
        }

        public Variable Get(string name)
        {
            // 無ければnullを返す。
            Variable v = hash[name] as Variable;
            return v;
        }

        public Variable Define(string name)
        {
            // 無ければ作って返す。
            Variable v = hash[name] as Variable;

            if (v == null)
            {
                v = new Variable(new Value(new BigNumber("0")));
                hash[name] = v;
            }
            return v;
        }
    }
}

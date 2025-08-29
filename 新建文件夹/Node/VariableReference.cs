using aya.Eval;
using System;

namespace aya.Node
{
    public class VariableReference
    {
        private string name;
        private Expression index; // nullであれば、添字が無い。

        public VariableReference(string name)
        {
            this.name = name;
            this.index = null;
        }

        public VariableReference SetIndex(Expression index)
        {
            this.index = index;
            return this;
        }

        public bool IsGlobal()
        {
            return name[0] != '_';
        }

        public string GetName()
        {
            return name;
        }

        public Expression GetIndex()
        {
            return index;
        }

        public int EvalIndex(Namespace ns)
        {
            // 簡易配列の添字がある。
            Value idx = index.Eval(ns);
            if (idx.IsInteger())
            {
                return (int)idx.GetInteger();
            }
            else
            {
                throw new Exception("Index of pseudo-array must be numeric value : " + idx);
            }
        }

        public override string ToString()
        {
            if (index == null)
            {
                return name;
            }
            else
            {
                return name + '[' + index.ToString() + ']';
            }
        }
    }
}

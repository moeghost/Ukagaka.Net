using System;
using aya.Node;
namespace aya.Eval
{
    public class VariablePointer
    {
        private readonly Namespace ns;
        private readonly VariableReference vref;
        private readonly bool indexExists;
        private readonly int evaluatedIndex;
        private readonly bool tacit;

        public VariablePointer(Namespace ns, VariableReference vref) : this(ns, vref, false)
        {
        }

        public VariablePointer(Namespace ns, VariableReference vref, bool tacit)
        {
            this.ns = ns ?? throw new ArgumentNullException(nameof(ns));
            this.vref = vref ?? throw new ArgumentNullException(nameof(vref));
            this.tacit = tacit;

            if (vref.IsGlobal() && !(ns is GlobalNamespace))
            {
                throw new Exception("Internal Error: created VariablePointer points global variable, but its namespace is not global. The vref is " + vref.ToString());
            }

            if (vref.GetIndex() != null)
            {
                indexExists = true;
                var idx = vref.GetIndex().Eval(ns);
                if (idx.IsInteger())
                {
                    evaluatedIndex = (int)idx.GetInteger();
                }
                else
                {
                    throw new Exception("Index of pseudo-array must be a numeric value.");
                }
            }
            else
            {
                indexExists = false;
                evaluatedIndex = -1;
            }
        }

        public bool IsTacit()
        {
            return tacit;
        }

        public VariableReference GetVariableReference()
        {
            return vref;
        }

        public Namespace GetNamespace()
        {
            return ns;
        }

        public Variable GetVariable()
        {
            return ns.Define(vref.GetName());
        }

        public VariablePointer Store(Value val)
        {
            var v = ns.Define(vref.GetName());

            if (indexExists)
            {
                v.SetValue(evaluatedIndex, val);
            }
            else
            {
                v.SetValue(val);
            }

            return this;
        }

        public Value Fetch()
        {
            var v = ns.Define(vref.GetName());

            return indexExists ? v.GetValue(evaluatedIndex) : v.GetValue();
        }
    }
}

using aya.Eval;
using System;
using LittleGrayCalculator.Cores;
namespace aya.Node
{
    public class Substitution
    {
        private Aya aya;
        private VariableReference vref;
        private string oper; // 代入演算子またはインクリメント演算子。
        private Expression expr; // 代入演算子が用いられた場合の、代入する式。

        public Substitution(Aya aya, VariableReference vref)
        {
            this.aya = aya;
            this.vref = vref;
            this.oper = null;
            this.expr = null;
        }

        public override string ToString()
        {
            if (expr == null)
            {
                return vref.ToString() + oper;
            }
            else
            {
                return vref.ToString() + ' ' + oper + ' ' + expr.ToString();
            }
        }

        public Substitution SetIncrementalOper(string oper)
        {
            this.oper = oper;
            return this;
        }

        public Substitution SetSubstitutionalOper(string oper, Expression expr)
        {
            this.oper = oper;
            this.expr = expr;
            return this;
        }

        public void Eval(Namespace ns)
        {
            if (vref.GetName() == "_i")
            {
                ;
            }
            Namespace space = (vref.IsGlobal() ? aya.GetGlobalNamespace() : ns);
            Variable v = space.Define(vref.GetName());
           
            bool indexExists = false;
            int index = -1;
            if (vref.GetIndex() != null)
            {
                indexExists = true;
                index = vref.EvalIndex(ns);
            }

            Value oldVal;
            if (indexExists)
            {
                oldVal = v.GetValue(index);
            }
            else
            {
                oldVal = v.GetValue();
            }

            // oldValが文字列だった場合、次の演算子は許されない。
            // ++,--,-=,*=,/=,%=,+:=,-:=,*:=,/:=,%:=
            // 逆に言うと、許されるのは=,:=,+=のみ。
            // ただし、oldValが空文字列だった場合はゼロとして扱う。
            if (oldVal.IsString() && !(oper.Equals("=") || oper.Equals(":=") || oper.Equals("+=")))
            {

                if (oldVal.GetString().Length == 0)
                {
                    oldVal.SetBigNumber(new BigNumber("0"));
                }
                else
                {
                    throw new Exception("Illegal substitution: " + this.ToString());
                }
            }

            if (oper.Equals("++") || oper.Equals("--"))
            {
                int signal = (oper.Equals("++") ? 1 : -1);

                if (oldVal.IsInteger())
                {
                    oldVal.SetInteger(oldVal.GetInteger() + signal);
                }
                else if (oldVal.IsBigNumber())
                {
                    oldVal.SetBigNumber(oldVal.GetBigNumber() + new BigNumber(signal.ToString()));
                }
                else if (oldVal.IsReal())
                {
                    oldVal.SetReal(oldVal.GetReal() + signal);
                }
            }
            else
            {
                if (vref.GetName() == "_i")
                {
                    ;
                }

                Value val = expr.Eval(ns);
                if (val == null)
                {
                    val = new Value("");
                }

                if (val.IsNumeric() && oper.IndexOf(':') == -1)
                {
                    // 値が数値であり、且つコロンを含まない演算子なので、小数点以下を切り捨て。
                    val.SetBigNumber(val.GetBigNumber());
                }

                if (oper.Equals("=") || oper.Equals(":="))
                {
                    if (val.IsInteger())
                    {
                        oldVal.SetInteger(val.GetInteger());
                    }
                    else if (val.IsBigNumber())
                    {
                        oldVal.SetBigNumber(val.GetBigNumber());
                    }
                    else if (val.IsReal())
                    {
                        oldVal.SetReal(val.GetReal());
                    }
                    else if (val.IsString())
                    {
                        oldVal.SetString(val.GetString());
                    }
                }
                else if (oper.Equals("+=") || oper.Equals("+:="))
                {
                    if (val.IsInteger())
                    {
                        oldVal.SetInteger(oldVal.GetInteger() + val.GetInteger());
                    }
                    else if (val.IsBigNumber())
                    {
                        oldVal.SetBigNumber(oldVal.GetBigNumber() + val.GetBigNumber());
                    }
                    else if (val.IsReal())
                    {
                        oldVal.SetReal(oldVal.GetReal() + val.GetReal());
                    }
                    else if (val.IsString())
                    {
                        oldVal.SetString(oldVal.GetString() + val.GetString());
                    }
                }
                else if (oper.Equals("-=") || oper.Equals("-:="))
                {
                    if (val.IsInteger())
                    {
                        oldVal.SetInteger(oldVal.GetInteger() - val.GetInteger());
                    }
                    else if (val.IsBigNumber())
                    {
                        oldVal.SetBigNumber(oldVal.GetBigNumber() - val.GetBigNumber());
                    }
                    else if (val.IsReal())
                    {
                        oldVal.SetReal(oldVal.GetReal() - val.GetReal());
                    }
                }
                else if (oper.Equals("*=") || oper.Equals("*:="))
                {
                    if (val.IsInteger())
                    {
                        oldVal.SetInteger(oldVal.GetInteger() * val.GetInteger());
                    }
                    else if (val.IsBigNumber())
                    {
                        oldVal.SetBigNumber(oldVal.GetBigNumber() * val.GetBigNumber());
                    }
                    else if (val.IsReal())
                    {
                        oldVal.SetReal(oldVal.GetReal() * val.GetReal());
                    }
                }
                else if (oper.Equals("/=") || oper.Equals("/:="))
                {
                    if (val.IsInteger())
                    {
                        oldVal.SetInteger(oldVal.GetInteger() / val.GetInteger());
                    }
                    else if (val.IsBigNumber())
                    {
                        oldVal.SetBigNumber(oldVal.GetBigNumber() / val.GetBigNumber());
                    }
                    else if (val.IsReal())
                    {
                        oldVal.SetReal(oldVal.GetReal() / val.GetReal());
                    }
                }
                else if (oper.Equals("%=") || oper.Equals("%:="))
                {
                    if (val.IsInteger())
                    {
                        oldVal.SetInteger(oldVal.GetInteger() % val.GetInteger());
                    }
                    else if (val.IsReal())
                    {
                        oldVal.SetReal(oldVal.GetReal() % val.GetReal());
                    }
                }
            }

            if (indexExists)
            {
                v.SetValue(index, oldVal);
            }
            else
            {
                if (vref.GetName() == "_i")
                {
                    ;
                }

                v.SetValue(oldVal);
            }
            //return v;
        }
    }
}

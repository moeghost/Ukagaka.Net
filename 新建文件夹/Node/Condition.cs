using aya.Eval;
using System;
using System.Collections.Generic;

namespace aya.Node
{
    public class Condition
    {
        static protected Condition _factory = null;
        static public Condition GetFactory()
        {
            if (_factory == null)
            {
                _factory = new Condition();
            }
            return _factory;
        }

        // 条件式は、一つ以上の部分条件式(Subcondition)と、
        // (部分条件式の個数)-1個の条件結合子から構成される。
        private List<Subcondition> subconditions; // [Subcondition,...]
        private List<string> combinations; // [String,...]

        protected Condition()
        {

        }

        public Condition(Subcondition cond)
        {
            subconditions = new List<Subcondition>();
            combinations = null;

            subconditions.Add(cond);
        }

        public Condition Add(string combination, Subcondition cond)
        {
            if (combinations == null)
            {
                combinations = new List<string>();
            }

            combinations.Add(combination);
            subconditions.Add(cond);
            return this;
        }

        public bool Eval(Namespace ns)
        {
            bool result = false;

            for (int i = 0; i < subconditions.Count; i++)
            {
                Subcondition subcond = subconditions[i];
                bool booleanResult = subcond.Eval(ns);

                if (i == 0)
                {
                    // 最初なので条件結合子は見ない。
                    result = booleanResult;
                }
                else
                {
                    string cmb = combinations[i - 1];
                    if (cmb.Equals("&&"))
                    {
                        result = result && booleanResult;
                    }
                    else if (cmb.Equals("||"))
                    {
                        result = result || booleanResult;
                    }
                }
            }

            return result;
        }

        public Subcondition NewSubcondition(Condition cond)
        {
            return new ConditionalSubcondition(cond);
        }

        public Subcondition NewSubcondition(Expression left, string comparison, Expression right)
        {
            return new ExpressionalSubcondition(left, comparison, right);
        }

        public class Subcondition
        {
            public virtual bool Eval(Namespace ns)
            {
                throw new Exception("Abstract method \"Eval\" of Condition.Subcondition has been called directly.");
            }
        }

        public class ConditionalSubcondition : Subcondition
        {
            // 条件式を持つ部分条件式。'(' 条件式 ')'で生成される。
            private Condition cond;

            public ConditionalSubcondition(Condition cond)
            {
                this.cond = cond;
            }

            public override bool Eval(Namespace ns)
            {
                return cond.Eval(ns);
            }
        }

        public class ExpressionalSubcondition : Subcondition
        {
            // 二つの式(Expression)と一つの条件比較子から構成される部分条件式。
            private Expression left, right;
            private string oper;

            public ExpressionalSubcondition(Expression left, string oper, Expression right)
            {
                this.left = left;
                this.oper = oper;
                this.right = right;
            }

            public override bool Eval(Namespace ns)
            {
                // 条件比較子 := '==' | '!=' | '<' | '<=' | '>' | '>=' | '_in_' | '!_in_'
                Value lh = left.Eval(ns);
                Value rh = right.Eval(ns);

                // 左右で型が違う場合はエラー………にするのはやっぱりやめ。
                // 比較子が==か!=なら、それぞれ適切な結果を返す。
                // それ以外の比較子なら常に偽。
                if ((lh.IsNumeric() && rh.IsString()) || (lh.IsString() && rh.IsNumeric()))
                {
                    if (oper.Equals("=="))
                    {
                        return false;
                    }
                    else if (oper.Equals("!="))
                    {
                        return true;
                    }
                    else
                    {
                        //throw new Exception("Comparison of numeric value and string: " + left + ' ' + oper + ' ' + right + " (evaluated to " + lh + ' ' + oper + ' ' + rh + ")");
                        return false;
                    }
                }

                if (oper.Equals("=="))
                {
                    if (lh.IsNumeric())
                    {
                        return lh.GetReal() == rh.GetReal();
                    }
                    else if (lh.IsString())
                    {
                        return lh.GetString() == rh.GetString();
                    }
                }
                else if (oper.Equals("!="))
                {
                    if (lh.IsNumeric())
                    {
                        return lh.GetReal() != rh.GetReal();
                    }
                    else if (lh.IsString())
                    {
                        return lh.GetString() != rh.GetString();
                    }
                }
                else if (oper.Equals("<"))
                {
                    if (lh.IsNumeric())
                    {
                        return lh.GetReal() < rh.GetReal();
                    }
                    else if (lh.IsString())
                    {
                        return lh.GetString().CompareTo(rh.GetString()) < 0;
                    }
                }
                else if (oper.Equals("<="))
                {
                    if (lh.IsNumeric())
                    {
                        return lh.GetReal() <= rh.GetReal();
                    }
                    else if (lh.IsString())
                    {
                        return lh.GetString().CompareTo(rh.GetString()) <= 0;
                    }
                }
                else if (oper.Equals(">"))
                {
                    if (lh.IsNumeric())
                    {
                        return lh.GetReal() > rh.GetReal();
                    }
                    else if (lh.IsString())
                    {
                        return lh.GetString().CompareTo(rh.GetString()) > 0;
                    }
                }
                else if (oper.Equals(">="))
                {
                    if (lh.IsNumeric())
                    {
                        return lh.GetReal() >= rh.GetReal();
                    }
                    else if (lh.IsString())
                    {
                        return lh.GetString().CompareTo(rh.GetString()) >= 0;
                    }
                }
                else if (oper.Equals("_in_"))
                {
                    if (lh.IsNumeric())
                    {
                        throw new Exception("Comparison of _in_ operator to numeric value.");
                    }
                    else
                    {
                        return rh.GetString().IndexOf(lh.GetString()) != -1;
                    }
                }
                else if (oper.Equals("!_in_"))
                {
                    if (lh.IsNumeric())
                    {
                        throw new Exception("Comparison of !_in_ operator to numeric value.");
                    }
                    else
                    {
                        return rh.GetString().IndexOf(lh.GetString()) == -1;
                    }
                }
                return false; // ここには来ないはず。
            }
        }
    }
}

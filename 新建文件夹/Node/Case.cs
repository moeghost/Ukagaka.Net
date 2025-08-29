using aya.Eval;
using System;
using System.Collections.Generic;

namespace aya.Node
{
    public class Case : Statement
    {
        private Expression attention; // 比較対象となる式
        private List<Candidate> candidates; // [Candidate]

        protected static Case _factory = null;
        public static Case GetCaseFactory()
        {
            if (_factory == null)
            {
                _factory = new Case();
            }
            return _factory;
        }
        protected Case()
        {
            attention = null;
            candidates = null;
        }

        public Case(Expression attention)
        {
            this.attention = attention;
            this.candidates = new List<Candidate>();
        }

        public Case AddCandidate(Candidate cand)
        {
            candidates.Add(cand);
            return this;
        }

        public override object Eval(Namespace ns)
        {
            Value val = attention.Eval(ns);

            // まずはothers以外で候補を探す。
            foreach (var cand in candidates)
            {
                if (!cand.IsOthers && cand.Match(val))
                {
                    // マッチするなら、このブロックを評価して返す。
                    return cand.Eval(ns);
                }
            }

            // othersを見る。
            foreach (var cand in candidates)
            {
                if (cand.IsOthers)
                {
                    return cand.Eval(ns);
                }
            }

            return null;
        }

        public Candidate NewCandidate(CaseCondition condition, Block block)
        {
            return new Candidate(condition, block);
        }

        public class Candidate
        {
            // case候補には、when候補とothers候補の二つがある。
            // when候補はcase条件とブロックを、others候補はブロックのみを持つ。
            private CaseCondition condition; // nullであれば、others候補。
            private Block block;

            public Candidate(CaseCondition condition, Block block)
            {
                this.condition = condition;
                this.block = block;
            }

            public bool IsOthers => condition == null;

            public bool Match(Value attention)
            {
                return condition == null || condition.Match(attention);
            }

            public Value Eval(Namespace ns)
            {
                return block.Eval(new Namespace(ns));
            }
        }

        public CaseCondition NewCondition()
        {
            return new CaseCondition();
        }

        public class CaseCondition
        {
            // case条件は一つ以上のcase部分条件から成る。
            private List<CaseSubcondition> subconditions; // [CaseSubcondition]

            public CaseCondition()
            {
                this.subconditions = new List<CaseSubcondition>();
            }

            public CaseCondition AddSubcondition(CaseSubcondition sub)
            {
                subconditions.Add(sub);
                return this;
            }

            public bool Match(Value attention)
            {
                // case部分条件が複数あれば、どれか一つでも一致すれば真を返す。
                foreach (var sub in subconditions)
                {
                    if (sub.Match(attention))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public CaseSubcondition NewSubcondition(double num)
        {
            return new SubcondNumeric(num);
        }

        public CaseSubcondition NewSubcondition(double lower, double higher)
        {
            return new SubcondNumericRange(lower, higher);
        }

        public CaseSubcondition NewSubcondition(string str)
        {
            return new SubcondString(str);
        }

        public CaseSubcondition NewSubcondition(string lower, string higher)
        {
            return new SubcondStringRange(lower, higher);
        }

        public class CaseSubcondition
        {
            // case部分条件には、数値範囲、文字列範囲、または式の3通りの形式がある。

            public virtual bool Match(Value attention)
            {
                return false;
            }
        }

        public class SubcondNumeric : CaseSubcondition
        {
            private double num;

            public SubcondNumeric(double num)
            {
                this.num = num;
            }

            public override bool Match(Value attention)
            {
                if (attention.IsNumeric())
                {
                    return num == attention.GetReal();
                }
                else
                {
                    return false;
                }
            }
        }

        public class SubcondNumericRange : CaseSubcondition
        {
            private double lower, higher;

            public SubcondNumericRange(double lower, double higher)
            {
                this.lower = lower;
                this.higher = higher;
            }

            public override bool Match(Value attention)
            {
                if (attention.IsNumeric())
                {
                    double n = attention.GetReal();
                    return (lower <= n && n <= higher) || (higher <= n && n <= lower);
                }
                else
                {
                    return false;
                }
            }
        }

        public class SubcondString : CaseSubcondition
        {
            private string str;

            public SubcondString(string str)
            {
                this.str = str;
            }

            public override bool Match(Value attention)
            {
                if (attention.IsString())
                {
                    return attention.GetString() == str;
                }
                else
                {
                    return false;
                }
            }
        }

        public class SubcondStringRange : CaseSubcondition
        {
            private string lower, higher;

            public SubcondStringRange(string lower, string higher)
            {
                this.lower = lower;
                this.higher = higher;
            }

            public override bool Match(Value attention)
            {
                if (attention.IsString())
                {
                    string str = attention.GetString();
                    return (lower.CompareTo(str) <= 0 && str.CompareTo(higher) <= 0) ||
                           (higher.CompareTo(str) <= 0 && str.CompareTo(lower) <= 0);
                }
                else
                {
                    return false;
                }
            }
        }
    }
}

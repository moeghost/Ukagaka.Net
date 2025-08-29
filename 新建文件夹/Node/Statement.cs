using aya.Eval;
using System;
using System.Collections.Generic;
 
namespace aya.Node
{
    public class Statement
    {
        protected static Statement _factory = null;

        public static Statement GetFactory()
        {
            if (_factory == null)
            {
                _factory = new Statement();
            }
            return _factory;
        }

        protected Statement()
        {
            // for factory
        }

        public virtual object Eval(Namespace ns)
        {
            // Statement's Eval() returns null, Value, or Vector<Value>.
            throw new Exception("Abstract method \"Eval\" of Statement has been called directly.");
        }

        public Statement NewStatement(Expression expr)
        {
            return new ExpressionalStatement(expr);
        }

        public Statement NewStatement(Substitution subs)
        {
            return new SubstitutionalStatement(subs);
        }

        public Statement NewStatement(Block block)
        {
            return new BlockStatement(block);
        }

        public If NewIf(Condition cond, Block block)
        {
            return new If(cond, block);
        }

        public Switch NewSwitch(Expression expr, Block block)
        {
            return new Switch(expr, block);
        }

        public While NewWhile(Condition cond, Block block)
        {
            return new While(cond, block);
        }

        public For NewFor(Substitution init, Condition cond, Substitution alter, Block block)
        {
            return new For(init, cond, alter, block);
        }

        public Separate NewSeparate()
        {
            return new Separate();
        }

        public Break NewBreak()
        {
            return new Break();
        }

        public Continue NewContinue()
        {
            return new Continue();
        }

        public Return NewReturn()
        {
            return new Return();
        }

        public class ExpressionalStatement : Statement
        {
            private Expression expr;

            public ExpressionalStatement(Expression expr)
            {
                this.expr = expr;
            }

            public override object Eval(Namespace ns)
            {
                return expr.Eval(ns);
            }
        }

        public class SubstitutionalStatement : Statement
        {
            private Substitution subs;

            public SubstitutionalStatement(Substitution subs)
            {
                this.subs = subs;
            }

            public override object Eval(Namespace ns)
            {
                subs.Eval(ns);
                return null;
            }
        }

        public class BlockStatement : Statement
        {
            private Block block;

            public BlockStatement(Block block)
            {
                this.block = block;
            }

            public override object Eval(Namespace ns)
            {
                return block.Eval(new Namespace(ns));
            }
        }

        public class If : Statement
        {
            private Condition cond; // ifの条件
            private Block blockIf; // ifのブロック

            private List<Condition> elseifConds; // [(Condition)それぞれのelseifの条件] 一つも無ければnull
            private List<Block> elseifBlocks; // [(Block)それぞれのelseifのブロック] 一つも無ければnull

            private Block blockElse; // elseのブロック。無ければnull

            public If(Condition cond, Block blockIf)
            {
                this.cond = cond;
                this.blockIf = blockIf;
                this.elseifConds = null;
                this.elseifBlocks = null;
                this.blockElse = null;
            }

            public If AddElseIf(Condition cond, Block block)
            {
                if (elseifConds == null)
                {
                    elseifConds = new List<Condition>();
                    elseifBlocks = new List<Block>();
                }
                elseifConds.Add(cond);
                elseifBlocks.Add(block);
                return this;
            }

            public If SetElse(Block block)
            {
                blockElse = block;
                return this;
            }

            public override object Eval(Namespace ns)
            {
                if (cond.Eval(ns))
                {
                    return blockIf.Eval(new Namespace(ns));
                }

                if (elseifConds != null)
                {
                    for (int i = 0; i < elseifConds.Count; i++)
                    {
                        Condition elseifCond = elseifConds[i];
                        Block elseifBlock = elseifBlocks[i];

                        if (elseifCond.Eval(ns))
                        {
                            return elseifBlock.Eval(new Namespace(ns));
                        }
                    }
                }

                if (blockElse != null)
                {
                    return blockElse.Eval(new Namespace(ns));
                }

                return null;
            }
        }

        public class Switch : Statement
        {
            private Expression expr;
            private Block block;

            public Switch(Expression expr, Block block)
            {
                this.expr = expr;
                this.block = block;
            }

            public override object Eval(Namespace ns)
            {
                return block.Eval(new Namespace(ns), (int)expr.Eval(ns).GetInteger());
            }
        }

        public class While : Statement
        {
            private Condition cond;
            private Block block;

            public While(Condition cond, Block block)
            {
                this.cond = cond;
                this.block = block;
            }

            public override object Eval(Namespace ns)
            {
                List<Value> result = null;

                Namespace nsForLoop = new Namespace(ns);
                while (cond.Eval(nsForLoop))
                {
                    try
                    {
                        Value val = block.Eval(nsForLoop);
                        if (val != null)
                        {
                            if (result == null)
                            {
                                result = new List<Value>();
                            }
                            result.Add(val);
                        }
                    }
                    catch (BreakOccurrence)
                    {
                        // ここで終わり。
                        break;
                    }
                    catch (ContinueOccurrence)
                    {
                        // 特に何もせず、単にcatch。
                    }
                }

                return result;
            }
        }

        public class For : Statement
        {
            private Substitution init;
            private Condition cond;
            private Substitution alter;
            private Block block;

            public For(Substitution init, Condition cond, Substitution alter, Block block)
            {
                // block以外は省略可能。
                this.init = init;
                this.cond = cond;
                this.alter = alter;
                this.block = block;
            }

            public override object Eval(Namespace ns)
            {
                List<Value> result = null;

                Namespace nsForLoop = new Namespace(ns);
                for (init.Eval(nsForLoop); cond.Eval(nsForLoop); alter.Eval(nsForLoop))
                {
                    try
                    {
                        Value val = block.Eval(nsForLoop);
                        if (val != null)
                        {
                            if (result == null)
                            {
                                result = new List<Value>();
                            }
                            result.Add(val);
                        }
                    }
                    catch (BreakOccurrence)
                    {
                        // ここで終わり。
                        break;
                    }
                    catch (ContinueOccurrence)
                    {
                        // 特に何もせず、単にcatch。
                    }
                }

                return result;
            }
        }

        public class Separate : Statement { }

        public class Break : Statement { }

        public class Continue : Statement { }

        public class Return : Statement { }

        public bool IsSeparator()
        {
            return (this is Separate);
        }

        public bool IsBreak()
        {
            return (this is Break);
        }

        public bool IsContinue()
        {
            return (this is Continue);
        }

        public bool IsReturn()
        {
            return (this is Return);
        }

        public BreakOccurrence NewBreakOccurrence()
        {
            return new BreakOccurrence();
        }

        public class BreakOccurrence : Exception { }

        public ContinueOccurrence NewContinueOccurrence()
        {
            return new ContinueOccurrence();
        }

        public class ContinueOccurrence : Exception { }
    }
}

using System;
using System.Collections.Generic;
 

namespace aya.Node
{
    public class FunctionCall
    {
        protected static FunctionCall _factory = null;

        public static FunctionCall GetFactory()
        {
            if (_factory == null)
            {
                _factory = new FunctionCall("");
            }
            return _factory;
        }

        private string name; // 関数名
        private List<Argument> args; // [Argument,...] これがnullの場合は引数群そのものが無かった場合。

        public FunctionCall(string name)
        {
            this.name = name;
            this.args = null;
        }

        public string GetName()
        {
            return name;
        }

        public List<Argument> GetArgs()
        {
            return args;
        }

        public FunctionCall PrepareArgs()
        {
            args = new List<Argument>();
            return this;
        }

        public FunctionCall AddArg(Argument arg)
        {
            if (args == null)
            {
                args = new List<Argument>();
            }
            args.Add(arg);
            return this;
        }

        public Argument NewArg(Expression expr)
        {
            return new ExpressionArgument(expr);
        }

        public Argument NewArg(VariableReference vref)
        {
            return new ReferenceArgument(vref);
        }

        public class Argument
        {
        }

        public class ExpressionArgument : Argument
        {
            private Expression expr;
            public ExpressionArgument(Expression expr)
            {
                this.expr = expr;
            }
            public Expression GetExpression()
            {
                return expr;
            }
        }

        public class ReferenceArgument : Argument
        {
            private VariableReference vref;
            public ReferenceArgument(VariableReference vref)
            {
                this.vref = vref;
            }
            public VariableReference GetReference()
            {
                return vref;
            }
        }
    }
}

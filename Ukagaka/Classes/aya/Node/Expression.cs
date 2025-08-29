using aya.Eval;
using aya;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using LittleGrayCalculator.Cores;
using System.DirectoryServices.ActiveDirectory;
using System.Windows.Documents;

namespace aya.Node
{
    using Node = LittleGrayCalculator.Cores.Node;
    public class Expression
    {
        protected static Expression _factory = null;

        static LittleGrayCalculator.Cores.Expression exp = new LittleGrayCalculator.Cores.Expression();

        private bool isForloop;

        public bool IsForloop
        {
            get
            {
                return isForloop;
            }
            set
            {
                isForloop = value;
                
            }

        }



        public static Expression GetFactory()
        {
            if (_factory == null)
            {
                _factory = new Expression();
            }
            return _factory;
        }

        private Aya aya;
        private List<Term> terms; // [Term, Term, ...]
          


        protected Expression()
        {
            // constructor of factory
            this.aya = null;
            this.terms = null;
        }

        public Expression(Aya aya)
        {
            this.aya = aya;
            this.terms = new List<Term>();
        }

        public Expression AddTerm(Term t)
        {
            terms.Add(t);
            return this;
        }

        public int Size()
        {
            return terms.Count;
        }

        public VariableReference IsVariableOnly()
        {
            // This method checks if the expression consists of a single variable reference without indices.
            // If true, it returns the VariableReference; otherwise, it returns null.

            // If there are multiple terms, it's not a single variable reference.
            if (terms.Count != 1)
            {
                return null;
            }

            Term term = terms[0];

            // If the term's signal is not positive, it's not a single variable reference.
            if (term.GetSignal() != 1)
            {
                return null;
            }

            // If the term has more than one factor, it's not a single variable reference.
            if (term.Size() != 1)
            {
                return null;
            }

            Factor factor = term.FactorAt(0);

            // If the factor's signal is not positive, it's not a single variable reference.
            if (factor.GetSignal() != 1)
            {
                return null;
            }

            // If the factor is a VariableFactor, return its VariableReference.
            // If the factor is a FunctionCallFactor, check if it can be redirected to a variable reference.
            if (factor is VariableFactor)
            {
                return ((VariableFactor)factor).GetVariableReference();
            }
            else if (factor is FunctionCallFactor)
            {
                FunctionCall fcall = ((FunctionCallFactor)factor).GetFunctionCall();
                if (fcall.GetArgs() == null)
                {
                    Function f = aya.GetDictionary().GetFunction(fcall.GetName());
                    if (f == null)
                    {
                        return new VariableReference(fcall.GetName());
                    }
                }
            }

            return null;
        }

        public virtual Value Eval1(Namespace ns)
        {

            string Format = "";

            foreach (Term t in terms)
            {
                Format += t.Eval(ns).ToString();
            }


                exp.Format = Format;

            BigNumber res = exp.Calculator();

            Value result;
            result = new Value(res);
            return result;

        }

        public virtual Value Eval0(Namespace ns)
        {
            StringBuilder formatBuilder = new StringBuilder();

            foreach (Term t in terms)
            {
                // 处理符号（正负）
                if (t.GetSignal() == -1)
                {
                    formatBuilder.Append("-");
                }

                // 拼接 Term 的表达式
                Value termValue = t.Eval(ns);
                formatBuilder.Append(termValue.ToString());
            }

            // 设置表达式并计算
            exp.Format = formatBuilder.ToString();
            BigNumber res = exp.Calculator();
            return new Value(res);
        }
        public virtual Value Eval(Namespace ns)
        {
            Value result = null;
            foreach (Term t in terms)
            {
                t.IsForloop = IsForloop;
                Value val = t.Eval(ns);
                 
                if (result == null)
                {
                    result = val;
                }
                else
                {
                    // String concatenation or numeric addition/subtraction is possible.
                    if (result.IsString() && val.IsString())
                    {
                        result.SetString(result.GetString() + val.GetString());
                    }
                    else if (result.IsNumeric() && val.IsNumeric())
                    {
                        // If either is a real number, the result is a real number.
                        if (result.IsReal() || val.IsReal())
                        {
                            result.SetReal(result.GetReal() + val.GetReal());
                        }
                        else if (result.IsBigNumber() || val.IsBigNumber())
                        {
                            result.SetBigNumber(result.GetBigNumber() + val.GetBigNumber());
                        }
                        else if (result.IsBool() || val.IsBool())
                        {
                          
                            result.SetBool(result.GetBool() && val.GetBool());
                        }
                        else
                        {
                            result.SetInteger(result.GetInteger() + val.GetInteger());
                        }
                    }
                    else
                    {
                        // If either is an empty string, do nothing.
                        if ((result.IsString() && result.GetString().Length == 0) ||
                            (val.IsString() && val.GetString().Length == 0))
                        {
                            // do nothing
                        }
                        else
                        {
                            throw new Exception("Type mismatch: add/sub with string and numeric: " + result + " , " + val);
                        }
                    }
                }
            }
            return result;
        }

        // factory
        public Term NewTerm()
        {
            return new Term();
        }

        public Factor NewFactor(Expression expr)
        {
            return new ExpressionFactor(expr);
        }

        public Factor NewFactor(BigNumber num)
        {
            return new NumericFactor(num);
        }

        public Factor NewFactor(Aya aya, string str)
        {
            return new StringFactor(aya, str);
        }

        public Factor NewFactor(Aya aya, FunctionCall fcall)
        {
            return new FunctionCallFactor(aya, fcall);
        }

        public Factor NewFactor(Aya aya, VariableReference vref)
        {
            return new VariableFactor(aya, vref);
        }

        public class Term
        {
            private List<Factor> factors; // [Factor, Factor, ...]
            private List<string> combinations; // [Character, Character, ...]
            private int signal;
            private bool isForloop;
             
            public bool IsForloop
            {
                get
                {
                    return isForloop;
                }
                set
                {
                    isForloop = value;

                }

            }

            public Term()
            {
                factors = new List<Factor>();
                combinations = new List<string>();
                signal = 1;
            }

            public Term SetSignal(int signal)
            {
                if (signal == 1 || signal == -1)
                {
                    this.signal = signal;
                }
                else
                {
                    throw new Exception("Signal must be 1 or -1. " + signal + " is invalid.");
                }
                return this;
            }

            public int GetSignal()
            {
                return signal;
            }

            public void AddFactor(string combination, Factor factor)
            {
                // If this is the first factor, ignore the combination.
                if (factors.Count != 0)
                {
                    combinations.Add(combination);
                    
                }
                
                factors.Add(factor);
            }

            public int Size()
            {
                return factors.Count;
            }

            public Factor FactorAt(int i)
            {
                return factors[i];
            }


            private bool IsComparisonOperator(string op)
            {
                return op == "==" || op == "!=" || op == "<" || op == "<=" ||
                       op == ">" || op == ">=" || op == "_in_" || op == "!_in_";
            }


            public Value Eval00(Namespace ns)
            {
                Stack<string> opStack = new Stack<string>();
                Queue<string> output = new Queue<string>();

                for (int i = 0; i < factors.Count; i++)
                {
                    // 操作数直接加入输出队列
                    
                    if (i == 0)
                    {
                        Value val = factors[i].Eval(ns);
                        output.Enqueue(val.ToString());
                    }
                    else
                    {
                        // 处理运算符
                        if (i - 1 < combinations.Count)
                        {
                            string op = combinations[i - 1];

                            opStack.Push(op);

                            while (opStack.Count > 0 && GetPriority(opStack.Peek()) >= GetPriority(op))
                            {
                                output.Enqueue(opStack.Pop());
                            }
                            
                        }
                        Value val = factors[i].Eval(ns);
                        output.Enqueue(val.ToString());
                    }
                }

                 

                // 拼接后序表达式
                exp.Format = string.Join("", output);
                BigNumber res = exp.Calculator();
                return new Value(res);
            }


            public Value Eval000(Namespace ns)
            {
                // 1. 构建节点列表（中序表达式）
                List<Node> nodes = new List<Node>();

                for (int i = 0; i < factors.Count; i++)
                {
                    // 添加操作数节点
                    Value val = factors[i].Eval(ns);
                    nodes.Add(new NumberNode(val.GetBigNumber().ToString()));

                    // 添加运算符节点（如果有）
                    if (i < combinations.Count)
                    {
                        string op = combinations[i];
                        Node opNode = Lexical.CreateOperateNode(op);
                        nodes.Add(opNode);
                    }
                }

                // 2. 将中序表达式转换为后序表达式
                List<Node> postfix = new List<Node>();
                Stack<Node> opStack = new Stack<Node>();

                foreach (Node n in nodes)
                {
                    if (n is NumberNode || n is AlgebraNode || n is ConstantNode)
                    {
                        postfix.Add(n);
                    }
                    else if (n is OperateNode currentOp)
                    {
                        // 处理运算符优先级和结合性
                        while (opStack.Count > 0 && opStack.Peek() is OperateNode topOp)
                        {
                            if (topOp.Priority > currentOp.Priority ||
                               (topOp.Priority == currentOp.Priority && currentOp.Associativity == Associativity.Left))
                            {
                                postfix.Add(opStack.Pop());
                            }
                            else
                            {
                                break;
                            }
                        }
                        opStack.Push(currentOp);
                    }
                }

                // 弹出剩余运算符
                while (opStack.Count > 0)
                {
                    postfix.Add(opStack.Pop());
                }

                // 3. 构建表达式树并计算
                Node root = CreateSyntaxTree(postfix);
                exp.Head = root;
                BigNumber res = exp.Calculator();
                return new Value(res);
            }

            // 辅助方法：从后序表达式构建语法树
            private Node CreateSyntaxTree(List<Node> postfix)
            {
                Stack<Node> stack = new Stack<Node>();

                foreach (Node n in postfix)
                {
                    if (n is OperateNode op)
                    {
                        // 根据参数数量弹出操作数
                        if (op.MinParameterCount == 2)
                        {
                            Node right = stack.Pop();
                            Node left = stack.Pop();
                            op.Nexts.Add(left);
                            op.Nexts.Add(right);
                            stack.Push(op);
                        }
                        else if (op.MinParameterCount == 1)
                        {
                            Node operand = stack.Pop();
                            op.Nexts.Add(operand);
                            stack.Push(op);
                        }
                    }
                    else
                    {
                        stack.Push(n);
                    }
                }

                return stack.Pop();
            }


            public Value Eval10(Namespace ns)
            {
                // 1. 构建中序表达式列表
                List<Node> nodeList = new List<Node>();
                for (int i = 0; i < factors.Count; i++)
                {
                    Factor factor = factors[i];

                    Value val = factor.Eval(ns);
                    nodeList.Add(new NumberNode(val.ToString()));

                    // 添加运算符（如果有）
                    if (i < combinations.Count)
                    {
                        string op = combinations[i];
                        nodeList.Add(Lexical.CreateOperateNode(op));
                    }
                }

                // 2. 使用 Syntax.Analyse 转换为语法树
                Node root = Syntax.Analyse(nodeList);
               
                exp.Head = root;  // 假设 exp 支持直接设置语法树
                BigNumber res = exp.Calculator();
                return new Value(res);
            }


            private int GetPriority(string op)
            {
                switch (op)
                {
                    case "*":
                    case "/":
                    case "%":
                        return 3;
                    case "+":
                    case "-":
                        return 2;
                    case "<":
                    case ">":
                    case "<=":
                    case ">=":
                    case "==":
                    case "!=":
                        return 1;
                    default:
                        return 0;
                }
            }

            public Value Eval2(Namespace ns)
            { 
                string Format = "";
                Value currentValue = factors[0].Eval(ns);
                Format += currentValue.ToString();
                for (int i = 1; i < factors.Count; i++)
                {
                     
                    string op = combinations[i - 1];
                    Format += op;
                    Value nextValue = factors[i].Eval(ns);
                    Format += nextValue.ToString();
                }
                exp.Format = Format;

                BigNumber res = exp.Calculator();

                Value result;
                result = new Value(res);
                return result;

            }


            private bool IsChainedComparison(List<string> combinations)
            {
           
                int comparisonCount = 0;
                for (int i = 0; i < combinations.Count; i++)
                {
                    string op = combinations[i];
                    if (IsComparisonOperator(op))
                    {
                        comparisonCount++;
                        if (comparisonCount > 0 && isForloop == false)
                        {
                            return true;
                        }
                    }
                }
                return false;


 

            }

            public Value HandleChainedComparison(Namespace ns)
            {
                List<Value> values = new List<Value>();
                List<string> operators = new List<string>();
                List<string> chainOperators = new List<string>();
                string chainOp = null;
                // 收集所有操作数
                for (int i = 0; i < factors.Count; i++)
                {
                    values.Add(factors[i].Eval(ns));
                    if (i < combinations.Count)
                    {
                        operators.Add(combinations[i]);

                        if (IsComparisonOperator(combinations[i]))
                        {
                            chainOperators.Add(combinations[i]);
                        }
                    }


                }

                // 构建逻辑与表达式：a < b < c => (a < b) && (b < c)
                Value result = null;
                for (int i = 0; i < values.Count - 1; i++)
                {
                    if (i < chainOperators.Count)
                    {
                        chainOp = chainOperators[i];
                    }

                    Value left = values[i];
                    Value right = values[i + 1];
                    Value comparison = ComputeOperation(left, chainOp, right);

                    if (result == null)
                    {
                        result = comparison;
                    }
                    else
                    {
                        result = ComputeOperation(result, "&&", comparison);
                    }
                }

                // 处理符号
                if (signal == -1)
                {
                    result = ComputeOperation(new Value(new BigNumber("1")), "-", result);
                }

                return result;
                 
            }


            public Value Eval(Namespace ns)
            {
                bool isChainComparison;

                isChainComparison = IsChainedComparison(combinations);
                if (isChainComparison)
                {
                    return HandleChainedComparison(ns);

                }


                // 第一步：处理所有乘除模运算（高优先级）
                List<Value> values = new List<Value>();
                List<string> remainingOperators = new List<string>();

                // 初始值
                Value currentValue = factors[0].Eval(ns);

                for (int i = 1; i < factors.Count; i++)
                {
                    string op = combinations[i - 1];
                    Value nextValue = factors[i].Eval(ns);

                    // 处理高优先级运算符

                    OperateNode oper = Lexical.CreateOperateNode(op.ToString()) as OperateNode;

                    if (IsComparisonOperator(op.ToString()))
                    {
                        
                        if (this.isForloop == false)
                        {
                            currentValue = ComputeOperation(currentValue, op.ToString(), nextValue);
                        }
                         
                    }

                    else if (op == "+" || op == "-")
                    {

                        // 低优先级运算符暂存
                         
                        values.Add(currentValue);

                        remainingOperators.Add(op);
                        currentValue = nextValue;
                        
                      //  currentValue = ComputeOperation(currentValue, op.ToString(), nextValue);
                    }
                     
                    else
                    {
                        
                        currentValue = ComputeOperation(currentValue, op.ToString(), nextValue);
                    }
                }
                
                values.Add(currentValue);
                Value result = values[0];


                // 第二步：处理加减运算（低优先级）
                if (values.Count > 1)
                {
                    
                    for (int i = 0; i < remainingOperators.Count; i++)
                    {
                        result = ComputeOperation(result, remainingOperators[i].ToString(), values[i + 1]);
                    }
                }
                // 处理符号
                if (signal == -1)
                {
                    if (result.IsString())
                        throw new Exception("Signal of string value must be positive.");

                    if (result.IsInteger())
                        result.SetInteger(result.GetInteger() * -1);
                    else if (result.IsReal())
                        result.SetReal(result.GetReal() * -1);
                    else if (result.IsBigNumber())
                        result.SetBigNumber(result.GetBigNumber() * new BigNumber("-1"));
                }

                return result;
            }

            private Value ComputeOperation(Value left, string op, Value right)
            {
                switch (op)
                {
                    case "+":
                        if (left.IsString() || right.IsString())
                            //throw new Exception("Only operator '+' can be used with strings.");
                            return new Value(left.GetBigNumber() + right.GetBigNumber());
                        if (left.IsReal() || right.IsReal())
                            return new Value(left.GetReal() + right.GetReal());
                        else if (left.IsBigNumber() || right.IsBigNumber())
                            return new Value(left.GetBigNumber() + right.GetBigNumber());
                        else
                            return new Value(left.GetInteger() + right.GetInteger());

                    case "-":
                        if (left.IsReal() || right.IsReal())
                            return new Value(left.GetReal() - right.GetReal());
                        else if (left.IsBigNumber() || right.IsBigNumber())
                            return new Value(left.GetBigNumber() - right.GetBigNumber());
                        else
                            return new Value(left.GetInteger() - right.GetInteger());

                    case "*":
                        if (left.IsReal() || right.IsReal())
                            return new Value(left.GetReal() * right.GetReal());
                        else if (left.IsBigNumber() || right.IsBigNumber())
                            return new Value(left.GetBigNumber() * right.GetBigNumber());
                        else
                            return new Value(left.GetInteger() * right.GetInteger());

                    case "/":
                        if (left.IsReal() || right.IsReal())
                        {
                            double d = left.GetReal() / right.GetReal();
                            if (d == Math.Floor(d))
                                return new Value((long)d);
                            else
                                return new Value(d);
                        }
                        else if (left.IsBigNumber() || right.IsBigNumber())
                            return new Value(left.GetBigNumber() / right.GetBigNumber());
                        else
                            return new Value(left.GetInteger() / right.GetInteger());

                    case "%":
                        return new Value(left.GetInteger() % right.GetInteger());

                    default:

                        

                        if (Lexical.IsOperate(op.ToString()))
                        {
                            string Format = left.GetBigNumber().ToString() + op.ToString() + right.GetBigNumber().ToString();


                            exp.Format = Format;

                            BigNumber res = exp.Calculator();
                            return new Value(res);
                        }


                        else
                        {
                            throw new Exception("Unsupported operator '" + op + "'");
                        }

                        break;
                            
                }
            }



            public Value Eval0(Namespace ns)
            {
                Value result = new Value(new BigNumber("0"));
                for (int i = 0; i < factors.Count; i++)
                {
                    Factor factor = factors[i];

                    // If the factor is a string with a negative signal, throw an error.
                    if (factor is StringFactor && factor.GetSignal() != 1)
                    {
                        throw new Exception("Signal of string value must be positive.");
                    }

                    if (i == 0)
                    {
                        // The first factor has no combination.
                        Value val = factor.Eval(ns);
                      //  result = factor.eval(ns);
                         result.SetBigNumber(val.GetBigNumber());
                    }
                    else
                    {
                        // If the result is a string and there are multiple factors, throw an error.
                        if (result.IsString())
                        {
                            throw new Exception("Only operator '+' can be used with strings.");
                        }

                        Value val = factor.Eval(ns);
                        string comb = combinations[i - 1];

                        if (IsComparisonOperator(comb.ToString()))
                        {

                        }


                        else if (Lexical.IsOperate(comb.ToString()))
                        {
                            string Format = result.GetBigNumber().ToString() + comb.ToString() + val.GetBigNumber().ToString();


                            exp.Format = Format;

                            BigNumber res = exp.Calculator();
                            result.SetBigNumber(res);
                            // combination = t.GetToken.ToCharArray().ElementAt(0);
                        }

                        else
                        {
                            throw new Exception("Unsupported combinational operator '" + comb + "'.");
                        }

                        /*
                        if (comb == '*')
                        {
                            // If either result or val is a real number, the result is a real number.
                            if (result.IsReal() || val.IsReal())
                            {
                                result.SetReal(result.GetReal() * val.GetReal());
                            }
                            else if (result.IsBigNumber() || val.IsBigNumber())
                            {
                                 result.SetBigNumber(result.GetBigNumber() * val.GetBigNumber());
                                //result.SetBigNumber(val.GetBigNumber());
                            }
                            else
                            {
                                result.SetInteger(result.GetInteger() * val.GetInteger());
                            }
                        }
                        else if (comb == '/')
                        {
                            // If the result becomes an integer, it remains an integer. Otherwise, it becomes a real number.

                            if (result.IsReal() || val.IsReal())
                            {
                                double d = result.GetReal() / val.GetReal();
                                if (d == Math.Floor(d))
                                {
                                    result.SetInteger((long)d);
                                }

                                else
                                {
                                    result.SetReal(d);
                                }
                            }
                            else if (result.IsBigNumber())
                            {
                                result.SetBigNumber(result.GetBigNumber() / val.GetBigNumber());
                            }
                        }
                        else if (comb == '%')
                        {
                            // Always an integer result.
                            result.SetInteger(result.GetInteger() % val.GetInteger());
                        }
                        */
                        
                    }
                }

                if (result == null)
                {
                    return null;
                }
                else if (result.IsString() && signal == -1)
                {
                    throw new Exception("Signal of string value must be positive.");
                }
                else if (result.IsInteger() && signal == -1)
                {
                    result.SetInteger(result.GetInteger() * -1);
                }
                else if (result.IsReal() && signal == -1)
                {
                    result.SetReal(result.GetReal() * -1);
                }
                else if (result.IsBigNumber() && signal == -1)
                {
                    result.SetBigNumber(result.GetBigNumber() * new BigNumber("-1"));
                }
                return result;
            }
        }

        public class Factor
        {
            // The elements of a factor can be an expression, a numeric constant, a string constant, a function call, or a variable.
            protected int signal;
             
            public Factor()
            {
                signal = 1;
            }

            public int GetSignal()
            {
                return signal;
            }

            public Factor SetSignal(int signal)
            {
                if (signal == 1 || signal == -1)
                {
                    this.signal = signal;
                }
                else
                {
                    throw new Exception("Signal must be 1 or -1. " + signal + " is invalid.");
                }
                return this;
            }

            public virtual Value Eval(Namespace ns)
            {
                throw new Exception("Abstract method \"Eval\" of Expression.Factor has been called directly.");
            }
        }

        public class ExpressionFactor : Factor
        {
            private Expression expr;
             

            public ExpressionFactor(Expression expr)
            {
                this.expr = expr;
            }

            public override Value Eval(Namespace ns)
            {
                Value val = expr.Eval(ns);
                if (val.IsNumeric() && signal == -1)
                {
                    val.SetBigNumber(val.GetBigNumber() * new BigNumber((-1).ToString()));
                }
                return val;
            }
        }

        public class NumericFactor : Factor
        {
            private BigNumber num;
             

            public NumericFactor(BigNumber num)
            {
                this.num = num;
            }



            public override Value Eval(Namespace ns)
            {
                // If it's an integer, return as an integer.
                BigNumber n = num * new BigNumber(signal.ToString());
                /*
                if (n == Math.Floor(n))
                {
                    return new Value((long)n);
                }
                */
               // else
                {
                    return new Value(n);
                }
            }
        }
        private static string hex_num = "0x[0-9a-fA-F]+";
        private static string bin_num = "0b[01]+";
        private static string dec_num = "[0-9]*\\.[0-9]+|[0-9]+";
        private static string num = $"({hex_num}|{bin_num}|{dec_num})";
        private static Regex pat_history = new Regex("%\\[(\\d+)\\]");
        private static Regex pat_ident = new Regex("%([^\\-+*/=:!;{}%&#\"()\\[\\]<>,?\\|\\s]+)");
        private static Regex pat_args = new Regex("\\((?:\\s*" + num + "\\s*(?:,\\s*" + num + "\\s*)*)?\\)");

        private static Regex pat_index = new Regex("\\[\\s*" + num + "\\s*\\]");



        public class StringFactor : Factor
        {
            private Aya aya;
            private string str;
          



            public StringFactor(Aya aya, string str)
            {
                this.aya = aya;
                this.str = str;
            }

            public override Value Eval(Namespace ns)
            {
                StringBuilder result = new StringBuilder();
                List<string> history = null; // <string>

                int pos = 0;
                while (pos < str.Length)
                {
                    int percPos = str.IndexOf('%', pos);
                    if (percPos == -1 || percPos == str.Length - 1)
                    {
                        // No more '%' or the string ends with '%'.
                        result.Append(str.Substring(pos));
                        break;
                    }
                    result.Append(str.Substring(pos, percPos - pos));

                    // History reference?
                    Match mat = pat_history.Match(str);
                    if (mat.Success && mat.Index == percPos)
                    {
                        string hist;
                        try
                        {
                            int idx = int.Parse(mat.Groups[1].Value);
                            hist = history[idx];
                        }
                        catch (Exception)
                        {
                            hist = "";
                        }
                        result.Append(hist);
                        pos = mat.Index + mat.Length;
                        continue;
                    }

                    // Identifier?
                    mat = pat_ident.Match(str);
                    if (mat.Success)
                    {
                        StringBuilder currentIdent = new StringBuilder(mat.Groups[1].Value);
                        bool replaced = false;
                        while (currentIdent.Length > 0)
                        {
                            // Does the function exist?
                            Function f = aya.GetDictionary().GetFunction(currentIdent.ToString());
                            if (f != null)
                            {
                                // It exists.
                                // If the immediately following string has the form of arguments, use it as arguments.
                                ArrayList args = null;
                                Match matArgs = pat_args.Match(str);
                                int argsPos = percPos + 1 + currentIdent.Length;
                                if (matArgs.Success && matArgs.Index == argsPos)
                                {
                                    args = new ArrayList();
                                    for (int i = 0; i < matArgs.Groups.Count; i++)
                                    {
                                        string g = matArgs.Groups[i].Value;
                                        if (g != null)
                                        {
                                            args.Add(new Value(Parser.ParseNumericConstant(g)));
                                        }
                                    }
                                    pos = matArgs.Index + matArgs.Length;
                                }
                                else
                                {
                                    pos = percPos + currentIdent.Length + 1;
                                }

                                // Evaluate the function and add the result to the history.
                                string resultStr;
                                Value resultVal = f.Eval(args);
                                if (resultVal == null)
                                {
                                    resultStr = "";
                                }
                                else
                                {
                                    resultStr = resultVal.GetString();
                                }

                                if (history == null)
                                {
                                    history = new List<string>();
                                }
                                history.Add(resultStr);

                                result.Append(resultStr);

                                replaced = true;
                                break;
                            }

                            // Does the variable exist?
                            Namespace space = (currentIdent[0] != '_' ? aya.GetGlobalNamespace() : ns);
                            Variable v = space.Get(currentIdent.ToString());
                            if (v != null)
                            {
                                // It exists.
                                // If the immediately following string has the form of an index, use it as the index.
                                string resultStr;
                                Match matIndex = pat_index.Match(str);
                                int indexPos = percPos + 1 + currentIdent.Length;
                                if (matIndex.Success && matIndex.Index == indexPos)
                                {
                                    try
                                    {
                                        resultStr = v.GetValue(int.Parse(matIndex.Groups[1].Value)).GetString();
                                    }
                                    catch (Exception)
                                    {
                                        resultStr = "";
                                    }
                                    pos = matIndex.Index + matIndex.Length;
                                }
                                else
                                {
                                    resultStr = v.GetValue().GetString();
                                    pos = percPos + currentIdent.Length + 1;
                                }

                                // Add to the history.
                                if (history == null)
                                {
                                    history = new List<string>();
                                }
                                history.Add(resultStr);

                                result.Append(resultStr);

                                replaced = true;
                                break;
                            }

                            currentIdent.Remove(currentIdent.Length - 1, 1);
                        }
                        if (!replaced)
                        {
                            result.Append('%');
                            pos++;
                        }
                    }
                }

                return new Value(result.ToString());
            }
        }

        public class FunctionCallFactor : Factor
        {
            private Aya aya;
            private FunctionCall fcall;

             



            public FunctionCallFactor(Aya aya, FunctionCall fcall)
            {
                this.aya = aya;
                this.fcall = fcall;
            }

            public FunctionCall GetFunctionCall()
            {
                return fcall;
            }

            public override Value Eval(Namespace ns)
            {
                Function f = aya.GetDictionary().GetFunction(fcall.GetName());
                if (f != null)
                {
                    List<FunctionCall.Argument> args = fcall.GetArgs();
                    ArrayList argsForF = null;

                    if (args != null)
                    {
                        argsForF = new ArrayList();
                        foreach (FunctionCall.Argument arg in args)
                        {
                            if (arg is FunctionCall.ExpressionArgument exprArg)
                            {
                                Expression expr = exprArg.GetExpression();
                                // If this expression is a single variable reference and has no index, create an implicit variable pointer.
                                VariableReference vref = expr.IsVariableOnly();
                                if (vref != null && vref.GetIndex() == null)
                                {
                                    Namespace space = (vref.IsGlobal() ? aya.GetGlobalNamespace() : ns);
                                    argsForF.Add(new VariablePointer(space, vref, true));
                                }
                                else
                                {
                                    argsForF.Add(expr.Eval(ns));
                                }
                            }
                            else if (arg is FunctionCall.ReferenceArgument refArg)
                            {
                                VariableReference vref = refArg.GetReference();
                                argsForF.Add(new VariablePointer(ns, vref));
                            }
                            else
                            {
                                throw new Exception("Internal Error: an object of " + arg.GetType().FullName + " was in array part of Variable illegally.");
                            }
                        }
                    }
                    return f.Eval(argsForF);
                }
                else
                {
                    // Calling an undefined function with no arguments redirects to a variable reference.
                    if (fcall.GetArgs() == null)
                    {
                        Namespace space = (fcall.GetName()[0] != '_' ? aya.GetGlobalNamespace() : ns);
                        Variable v = space.Get(fcall.GetName());


                        if (fcall.GetName() == "_i")
                        {
                            ;
                        }


                        if (v != null)
                        {
                            return v.GetValue();
                        }
                    }
                    return new Value(""); // There may be room for consideration in this behavior.
                }
            }
        }

        public class VariableFactor : Factor
        {
            private Aya aya;
            private VariableReference vref;

            public VariableFactor(Aya aya, VariableReference vref)
            {
                this.aya = aya;
                this.vref = vref;
            }
             
            public VariableReference GetVariableReference()
            {
                return vref;
            }

            public override Value Eval(Namespace ns)
            {
                Namespace space = (vref.IsGlobal() ? aya.GetGlobalNamespace() : ns);
                Variable v = space.Define(vref.GetName());

                if (vref.GetIndex() != null)
                {
                    // There is a simple array index.
                    return v.GetValue(vref.EvalIndex(ns));
                }
                else
                {
                    return v.GetValue();
                }
            }
        }
    }
}

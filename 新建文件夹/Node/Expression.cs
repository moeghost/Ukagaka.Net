using aya.Eval;
using aya;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace aya.Node
{
    public class Expression
    {
        protected static Expression _factory = null;

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

        public Value Eval(Namespace ns)
        {
            Value result = null;
            foreach (Term t in terms)
            {
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

        public Factor NewFactor(double num)
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
            private List<char> combinations; // [Character, Character, ...]
            private int signal;

            public Term()
            {
                factors = new List<Factor>();
                combinations = new List<char>();
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

            public void AddFactor(char combination, Factor factor)
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

            public Value Eval(Namespace ns)
            {
                Value result = null;
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
                        result = factor.Eval(ns);
                    }
                    else
                    {
                        // If the result is a string and there are multiple factors, throw an error.
                        if (result.IsString())
                        {
                            throw new Exception("Only operator '+' can be used with strings.");
                        }

                        Value val = factor.Eval(ns);
                        char comb = combinations[i - 1];
                        if (comb == '*')
                        {
                            // If either result or val is a real number, the result is a real number.
                            if (result.IsReal() || val.IsReal())
                            {
                                result.SetReal(result.GetReal() * val.GetReal());
                            }
                            else
                            {
                                result.SetInteger(result.GetInteger() * val.GetInteger());
                            }
                        }
                        else if (comb == '/')
                        {
                            // If the result becomes an integer, it remains an integer. Otherwise, it becomes a real number.
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
                        else if (comb == '%')
                        {
                            // Always an integer result.
                            result.SetInteger(result.GetInteger() % val.GetInteger());
                        }
                        else
                        {
                            throw new Exception("Unsupported combinational operator '" + comb + "'.");
                        }
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
                    val.SetReal(val.GetReal() * -1);
                }
                return val;
            }
        }

        public class NumericFactor : Factor
        {
            private double num;

            public NumericFactor(double num)
            {
                this.num = num;
            }

            public override Value Eval(Namespace ns)
            {
                // If it's an integer, return as an integer.
                double n = num * signal;

                if (n == Math.Floor(n))
                {
                    return new Value((long)n);
                }
                else
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

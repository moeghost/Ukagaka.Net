using aya.Node;
using aya;
using System;
using LittleGrayCalculator.Cores;

namespace aya.Node
{
    public class Parser
    {
        private Aya aya;
        private LexicalAnalyzer lex;

        public bool isForloop;
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
        public Parser(Aya aya, LexicalAnalyzer lex)
        {
            this.aya = aya;
            this.lex = lex;
        }

        public Function ParseFunction()
        {
            Token t;

            string name;
            t = lex.NextToken();
            if (t.Type == LexicalAnalyzer.TYPE_IDENTIFIER)
            {
                name = t.GetToken;
            }
            else
            {
                lex.Rollback();
                return null;
            }

            t = lex.NextToken();
            string option = null;
            if (t.Type == LexicalAnalyzer.TYPE_SYMBOL && t.GetToken == ":")
            {
                Token tOption = lex.NextToken();
                if (tOption.Type == LexicalAnalyzer.TYPE_IDENTIFIER)
                {
                    option = tOption.GetToken;
                }
                else
                {
                    throw new IllegalTokenException(tOption);
                }
            }
            else
            {
                lex.Rollback();
            }

            Block b = ParseBlock();
            if (b == null)
            {
                throw new IllegalTokenException(lex.NextToken());
            }
            b.SetOption(option);

            return new Function(aya, name, b);
        }


        public Block NewBlock1()
        {
            Token t;

            
            t = lex.NextToken();
            Block b = new Block(aya);
            while (true)
            {
                Statement s = ParseStatement();
                if (s == null)
                {
                    t = lex.NextToken();
                    if (t.Type == LexicalAnalyzer.TYPE_SYMBOL && t.GetToken == ";")
                    {
                        continue;
                    }
                    else
                    {
                        lex.Rollback();
                    }

                    t = lex.NextToken();
                    if (t.Type == LexicalAnalyzer.TYPE_SYMBOL && t.GetToken == "}")
                    {
                        break;
                    }
                    else
                    {
                        throw new IllegalTokenException(t);
                    }
                }
                else
                {
                    b.Add(s);
                }
            }

            return b;



        }




        public Block ParseBlock()
        {
            Token t;

            t = lex.NextToken();
            if (t.Type != LexicalAnalyzer.TYPE_SYMBOL || t.GetToken != "{")
            {
                lex.Rollback();
                return null;
            }

            Block b = new Block(aya);
            while (true)
            {
                Statement s = ParseStatement();
                if (s == null)
                {
                    t = lex.NextToken();
                    if (t.Type == LexicalAnalyzer.TYPE_SYMBOL && t.GetToken == ";")
                    {
                        continue;
                    }
                    else
                    {
                        lex.Rollback();
                    }

                    t = lex.NextToken();
                    if (t.Type == LexicalAnalyzer.TYPE_SYMBOL && t.GetToken == "}")
                    {
                        break;
                    }
                    else
                    {
                        throw new IllegalTokenException(t);
                    }
                }
                else
                {
                    b.Add(s);
                }
            }

            return b;
        }

        public Statement ParseStatement()
        {
            Token t;

            Substitution subs = ParseSubstitution();
            if (subs != null)
            {
                return Statement.GetFactory().NewStatement(subs);
            }

            Expression expr = ParseExpression();
            if (expr != null)
            {
                return Statement.GetFactory().NewStatement(expr);
            }

            Block block = ParseBlock();
            if (block != null)
            {
                return Statement.GetFactory().NewStatement(block);
            }

            Statement.If sIf = ParseIf();
            if (sIf != null)
            {
                return sIf;
            }

            Case sCase = ParseCase();
            if (sCase != null)
            {
                return sCase;
            }

            Statement.Switch sSwitch = ParseSwitch();
            if (sSwitch != null)
            {
                return sSwitch;
            }

            Statement.While sWhile = ParseWhile();
            if (sWhile != null)
            {
                return sWhile;
            }

            Statement.For sFor = ParseFor();
            if (sFor != null)
            {
                return sFor;
            }

            t = lex.NextToken();
            if (t.Type == LexicalAnalyzer.TYPE_SYMBOL)
            {
                if (t.GetToken == "--")
                {
                    return Statement.GetFactory().NewSeparate();
                }
                else if (t.GetToken == "break")
                {
                    return Statement.GetFactory().NewBreak();
                }
                else if (t.GetToken == "continue")
                {
                    return Statement.GetFactory().NewContinue();
                }
                else if (t.GetToken == "return")
                {
                    return Statement.GetFactory().NewReturn();
                }
                else
                {
                    lex.Rollback();
                    return null;
                }
            }
            else
            {
                lex.Rollback();
                return null;
            }
        }

        public Expression ParseExpression()
        {
            Expression expr = new Expression(aya);
            bool firsttime = true;
            while (true)
            {
                int signal = 1;
                if (firsttime)
                {
                    firsttime = false;
                }
                else
                {
                    Token t = lex.NextToken();
                    if (t.IsSymbol)
                    {
                        if (t.GetToken == "+")
                        {
                            // This term has a positive sign
                        }
                        else if (t.GetToken == "-")
                        {
                            signal = -1;
                        }
                        else
                        {
                            lex.Rollback();
                            break;
                        }
                    }
                    else
                    {
                        lex.Rollback();
                        break;
                    }
                }

                Expression.Term tm = ParseTerm();
                if (tm == null)
                {
                    break;
                }
                else
                {
                    tm.SetSignal(signal);
                    expr.AddTerm(tm);
                }
            }

            if (expr.Size() == 0)
            {
                return null;
            }
            else
            {
                return expr;
            }
        }

        public Expression.Term ParseTerm()
        {
            Token t;
            Expression.Term term = Expression.GetFactory().NewTerm();

            bool firsttime = true;
            while (true)
            {
                string combination = "?";
                if (firsttime)
                {
                    firsttime = false;
                }
                else
                {
                    t = lex.NextToken();
                    if (t.Type == LexicalAnalyzer.TYPE_SYMBOL)
                    {
                         
                        if (t.GetToken == "*")
                        {
                            combination = "*";
                        }
                        else if (t.GetToken == "/")
                        {
                            combination = "/";
                        }
                        else if (t.GetToken == "%")
                        {
                            combination = "%";
                        }
                        else if (Lexical.IsOperate(t.GetToken))
                        {
                            
                            combination = t.GetToken;
                            while (Lexical.IsOperate(combination))
                            {
                                t = lex.NextToken();

                                if (Lexical.IsOperate(t.GetToken))
                                {
                                    combination += t.GetToken;
                                }
                                else
                                {
                                    lex.Rollback();
                                    break;

                                }
                            }
                        }
                        else
                        {
                            lex.Rollback();
                            break;
                        }
                    }
                    else
                    {
                        lex.Rollback();
                        break;
                    }
                }

                t = lex.NextToken();
                int signal = 1;
                if (t.Type == LexicalAnalyzer.TYPE_SYMBOL)
                {
                    if (t.GetToken == "+")
                    {
                        // Positive sign
                    }
                    else if (t.GetToken == "-")
                    {
                        // Negative sign
                        signal = -1;
                    }
                    else
                    {
                        lex.Rollback();
                    }
                }
                else
                {
                    lex.Rollback();
                }

                Expression.Factor factor = ParseFactor();
                if (factor == null)
                {
                    return null;
                }
                factor.SetSignal(signal);

                term.AddFactor(combination, factor);
            }
            return term;
        }

        public Expression.Factor ParseFactor()
        {
            Token t;

            FunctionCall fcall = ParseFunctionCall();
            if (fcall != null)
            {
                return Expression.GetFactory().NewFactor(aya, fcall);
            }

            VariableReference vref = ParseVariable();
            if (vref != null)
            {
                return Expression.GetFactory().NewFactor(aya, vref);
            }

            t = lex.NextToken();
            if (t.Type == LexicalAnalyzer.TYPE_SYMBOL)
            {
                if (t.GetToken == "(")
                {
                    Expression expr = ParseExpression();
                    if (expr == null)
                    {
                        throw new IllegalTokenException(t);
                    }

                    t = lex.NextToken();
                    if (t.Type == LexicalAnalyzer.TYPE_SYMBOL && t.GetToken == ")")
                    {
                        return Expression.GetFactory().NewFactor(expr);
                    }
                    else
                    {
                        throw new IllegalTokenException(t);
                    }
                }
                else
                {
                    lex.Rollback();
                    return null;
                }
            }
            else if (t.Type == LexicalAnalyzer.TYPE_STRING)
            {
                return Expression.GetFactory().NewFactor(aya, t.GetToken);
            }
            else if (t.Type == LexicalAnalyzer.TYPE_NUMBER)
            {
                return Expression.GetFactory().NewFactor(ParseNumericConstant(t.GetToken));
            }
            else
            {
                lex.Rollback();
                return null;
            }
        }

        public static BigNumber ParseNumericConstant(string num)
        {
            double n;
            if (num.StartsWith("0x"))
            {
                n = long.Parse(num.Substring(2), (System.Globalization.NumberStyles)16);
            }
            else if (num.StartsWith("0b"))
            {
                n = long.Parse(num.Substring(2), (System.Globalization.NumberStyles)2);
            }
            else
            {
                //n = double.Parse(num);
                return new BigNumber(num);

            }
            return new BigNumber(n.ToString());
        }

        public FunctionCall ParseFunctionCall()
        {
            Token t;

            Token tName = t = lex.NextToken();
            string name;
            if (t.Type == LexicalAnalyzer.TYPE_IDENTIFIER)
            {
                name = t.GetToken;
            }
            else
            {
                lex.Rollback();
                return null;
            }

            FunctionCall fcall = new FunctionCall(name);
            t = lex.NextToken();
            if (t.Type == LexicalAnalyzer.TYPE_SYMBOL && t.GetToken == "(")
            {
            }
            else
            {
                if (t.Type == LexicalAnalyzer.TYPE_SYMBOL && t.GetToken == "[")
                {
                    lex.Rollback();
                    lex.Rollback();
                    return null;
                }
                else
                {
                    lex.Rollback();
                    return fcall;
                }
            }

            fcall.PrepareArgs();
            bool firsttime = true;
            while (true)
            {
                if (firsttime)
                {
                    firsttime = false;

                    t = lex.NextToken();
                    if (t.IsSymbol && t.GetToken == ")")
                    {
                        break;
                    }
                    else
                    {
                        lex.Rollback();
                    }
                }
                else
                {
                    t = lex.NextToken();
                    if (t.Type == LexicalAnalyzer.TYPE_SYMBOL && t.GetToken == ",")
                    {
                    }
                    else if (t.Type == LexicalAnalyzer.TYPE_SYMBOL && t.GetToken == ")")
                    {
                        break;
                    }
                    else
                    {
                        throw new IllegalTokenException(t);
                    }
                }

                FunctionCall.Argument arg = ParseArgument();
                if (arg == null)
                {
                    throw new IllegalTokenException(lex.NextToken());
                }
                else
                {
                    fcall.AddArg(arg);
                }
            }

            return fcall;
        }

        public FunctionCall.Argument ParseArgument()
        {
            Token t;

            Expression expr = ParseExpression();
            if (expr != null)
            {
                return FunctionCall.GetFactory().NewArg(expr);
            }

            t = lex.NextToken();
            if (t.Type == LexicalAnalyzer.TYPE_SYMBOL && t.GetToken == "&")
            {
                VariableReference vref = ParseVariable();
                if (vref == null)
                {
                    throw new IllegalTokenException(lex.NextToken());
                }
                else
                {
                    return FunctionCall.GetFactory().NewArg(vref);
                }
            }
            else
            {
                lex.Rollback();
                return null;
            }
        }

        public VariableReference ParseVariable()
        {
            Token t;

            t = lex.NextToken();
            string name;
            if (t.Type == LexicalAnalyzer.TYPE_IDENTIFIER)
            {
                name = t.GetToken;
            }
            else
            {
                lex.Rollback();
                return null;
            }
            VariableReference vref = new VariableReference(name);

            t = lex.NextToken();
            if (t.Type == LexicalAnalyzer.TYPE_SYMBOL && t.GetToken == "[")
            {
                Expression expr = ParseExpression();
                if (expr == null)
                {
                    return null;
                }
                else
                {
                    vref.SetIndex(expr);
                }

                t = lex.NextToken();
                if (t.Type == LexicalAnalyzer.TYPE_SYMBOL && t.GetToken == "]")
                {
                }
                else
                {
                    throw new IllegalTokenException(t);
                }
            }
            else
            {
                lex.Rollback();
            }

            return vref;
        }



        public Substitution ParseSubstitution()
        {
            // 代入文 := 変数 インクリメント演算子 | 変数 代入演算子 式
            // インクリメント演算子 := '++' | '--'
            // 代入演算子 := '='  | '+='  | '-='  | '*='  | '/='  | '%=' |
            //                 ':=' | '+:=' | '-:=' | '*:=' | '/:=' | '%:='
            lex.Push();
            VariableReference vref = ParseVariable();
            if (vref == null)
            {
                // 変数でないので、これは代入文でない。
                lex.PopNBack();
                return null;
            }
            Substitution subs = new Substitution(aya, vref);

            Token t;
            t = lex.NextToken();
            if (t.Type == LexicalAnalyzer.TYPE_SYMBOL)
            {
                string oper = t.GetToken;
                if (oper == "++" || oper == "--")
                {
                    // インクリメント
                    if (oper == "--" && t.WasSpaceBefore)
                    {
                        // --の前に空白文字があった場合、これは出力確定子の--である。
                        // これは代入文でない。
                        lex.PopNBack();
                        return null;
                    }
                    subs.SetIncrementalOper(oper);
                }
                else if (oper == "=" || oper == "+=" || oper == "-=" ||
                         oper == "*=" || oper == "/=" || oper == "%=" ||
                         oper == ":=" || oper == "+:=" || oper == "-:=" ||
                         oper == "*:=" || oper == "/:=" || oper == "%:=")
                {
                    // 代入
                    // 次に来た式を得る。
                    Expression expr = ParseExpression();
                    if (expr == null)
                    {
                        // 文法エラー。
                        throw new IllegalTokenException(lex.NextToken());
                    }
                    subs.SetSubstitutionalOper(oper, expr);
                }
                else
                {
                    // 代入文で用いられる演算子でない。これは代入文でない。
                    lex.PopNBack();
                    return null;
                }
            }
            else
            {
                // 次に来たのが演算子でない。これは代入文でない。
                lex.PopNBack();
                return null;
            }

            lex.Pop();
            return subs;
        }

        public Statement.If ParseIf()
        {
            // if文 := 'if' 条件式 ブロック (elseif 条件式 ブロック)* (else ブロック)?
            Token t;

            t = lex.NextToken();
            if (t.Type != LexicalAnalyzer.TYPE_SYMBOL || t.GetToken != "if")
            {
                // if文でない。
                lex.Rollback();
                return null;
            }

             Condition headCond = ParseCondition();
            if (headCond == null)
            {
                // ifの次に来たのが条件式でない。文法エラー。
                throw new IllegalTokenException(lex.NextToken());
            }

            Block headBlock = ParseBlock();
            if (headBlock == null)
            {
                // 次に来たのがブロックでない。文法エラー。
                throw new IllegalTokenException(lex.NextToken());
            }

            Statement.If statIf = Statement.GetFactory().NewIf(headCond, headBlock);

            while (true)
            {
                t = lex.NextToken();
                if (t.Type == LexicalAnalyzer.TYPE_SYMBOL && t.GetToken == "elseif")
                {

                    Condition cond = ParseCondition();
                    if (cond == null)
                    {
                        // elseifの次に来たのが条件式でない。文法エラー。
                        throw new IllegalTokenException(lex.NextToken());
                    }

                    Block block = ParseBlock();
                    if (block == null)
                    {
                        // 次に来たのがブロックでない。文法エラー。
                        throw new IllegalTokenException(lex.NextToken());
                    }

                    statIf.AddElseIf(cond, block);
                }
                else
                {
                    // elseifでない。ここで終わり。
                    lex.Rollback();
                    break;
                }
            }

            t = lex.NextToken();
            if (t.Type == LexicalAnalyzer.TYPE_SYMBOL && t.GetToken == "else")
            {

                Block block = ParseBlock();
                if (block == null)
                {
                    // 次に来たのがブロックでない。文法エラー。
                    throw new IllegalTokenException(lex.NextToken());
                }

                statIf.SetElse(block);
            }
            else
            {
                // elseでない。
                lex.Rollback();
            }

            return statIf;
        }



        public Condition ParseCondition()
        {
            // 条件式 := 部分条件式 (条件結合子 部分条件式)*
            // 条件結合子 := '||' | '&&'
            Token t;

            Condition.Subcondition head = ParseSubcondition();
            if (head == null)
            {
                // 条件式でない。
                return null;
            }
            Condition cond = new Condition(head);

            // 次に条件結合子が来ているか？
            while (true)
            {
                t = lex.NextToken();
                if (t.Type == LexicalAnalyzer.TYPE_SYMBOL &&
                    (t.GetToken == "||" || t.GetToken == "&&"))
                {

                    // これは条件結合子である。次に来るのは部分条件式でなければならない。
                    Condition.Subcondition sub = ParseSubcondition();
                    if (sub == null)
                    {
                        // 文法エラー。
                        throw new IllegalTokenException(lex.NextToken());
                    }

                    cond.Add(t.GetToken, sub);
                }
                else
                {
                    // 条件結合子でない。ここで条件式終わり。
                    lex.Rollback();
                    break;
                }
            }

            return cond;
        }

        public Condition.Subcondition ParseSubcondition()
        {
            // 部分条件式 := '(' 条件式 ')' | 式 条件比較子 式
            // 条件比較子 := '==' | '!=' | '<' | '<=' | '>' | '>=' | '_in_' | '!_in_'
            Token t;

            // '(' 条件式 ')'という形式は一見すると式のように見えるため(実際には違うのでエラー)
            // 先にparseExpressionを実行するわけにはいかない。

            t = lex.NextToken();
            if (t.Type == LexicalAnalyzer.TYPE_SYMBOL && t.GetToken == "(")
            {
                // 条件式かも知れないので条件式としてパースしてみる。
                Condition cond = ParseCondition();
                if (cond == null)
                {
                    // 条件式ではなかった。戻る。
                    lex.Rollback();
                }
                else
                {
                    // 条件式だ。次に来たのは')'か？
                    t = lex.NextToken();
                    if (t.Type == LexicalAnalyzer.TYPE_SYMBOL && t.GetToken == ")")
                    {
                        // 閉じ括弧だった。
                        return Condition.GetFactory().NewSubcondition(cond);
                    }
                    else
                    {
                        // 違う。文法エラー。
                        throw new IllegalTokenException(t);
                    }
                }
            }
            else
            {
                // 括弧でなかった。戻る。
                lex.Rollback();
            }

            // '(' 条件式 ')'でなかった場合のみ此処に来る。
            lex.Push();
            //lex.Rollback();
           // lex.Rollback();
            //lex.Rollback();
            Expression left = ParseExpression();
            if (left == null)
            {
                // 部分条件式でない。
                lex.PopNBack();
                return null;
            }

            string cmp;
            Expression right;

            lex.Rollback();
            lex.Rollback();
            t = lex.NextToken();
            if (t.Type == LexicalAnalyzer.TYPE_SYMBOL)
            {
                cmp = t.GetToken;

                // 誤り訂正！
                if (cmp == "=")
                {
                    cmp = "==";
                }

                if (cmp == "==" || cmp == "!=" || cmp == "<" || cmp == "<=" ||
                    cmp == ">" || cmp == ">=" || cmp == "_in_" || cmp == "!_in_")
                {
                    // これは条件比較子である。次に来るのは条件因子でなければならない。
                    right = ParseExpression();
                    if (right == null)
                    {
                        // 文法エラー
                        throw new IllegalTokenException(lex.NextToken());
                    }
                }
                else
                {
                    // 次に来たのが条件比較子でないので、これは部分条件式でない。
                    lex.PopNBack();
                    return null;
                }
            }
            else
            {
                // 次に来たのが条件比較子でないので、これは部分条件式でない。
                lex.PopNBack();
                return null;
            }

            lex.Pop();
            left.IsForloop = this.IsForloop;
            return Condition.GetFactory().NewSubcondition(left, cmp, right);
        }
         
        public Case ParseCase()
        {
            // case文 := 'case' 式 '{' case候補+ '}'
            // case候補 := 'when' case条件 ブロック | 'others' ブロック
            Token t;

            t = lex.NextToken();
            if (!t.IsSymbol || t.GetToken != "case")
            {
                // caseでない。
                lex.Rollback();
                return null;
            }

            Expression attention = ParseExpression();
            if (attention == null)
            {
                // caseの次に来たのが式でない。文法エラー。
                throw new IllegalTokenException(lex.NextToken());
            }
            Case statCase = new Case(attention);

            t = lex.NextToken();
            if (!t.IsSymbol || t.GetToken != "{")
            {
                // 式の次に来たのが'{'でない。文法エラー。
                throw new IllegalTokenException(t);
            }

            while (true)
            {
                t = lex.NextToken();

                if (t.IsSymbol && t.GetToken == "when")
                {
                    // 次に来るのはcase条件であるはず。
                    Case.CaseCondition cond = ParseConditionOfCase();
                    if (cond == null)
                    {
                        throw new IllegalTokenException(lex.NextToken());
                    }

                    // その次に来るのはブロックであるはず。
                    Block block = ParseBlock();
                    if (block == null)
                    {
                        throw new IllegalTokenException(lex.NextToken());
                    }

                    statCase.AddCandidate(statCase.NewCandidate(cond, block));
                }
                else if (t.IsSymbol && t.GetToken == "others")
                {
                    // 次に来るのはブロックであるはず。
                    Block block = ParseBlock();
                    if (block == null)
                    {
                        throw new IllegalTokenException(lex.NextToken());
                    }

                    statCase.AddCandidate(statCase.NewCandidate(null, block));
                }
                else
                {
                    // ここで終わり
                    lex.Rollback();
                    break;
                }
            }

            t = lex.NextToken();
            if (!t.IsSymbol || t.GetToken != "}")
            {
                // 次に来たのが'}'でない。文法エラー。
                throw new IllegalTokenException(t);
            }

            return statCase;
        }

        public Case.CaseCondition ParseConditionOfCase()
        {
            // case条件 := case部分条件 (',' case部分条件)*
            // case部分条件 := <数値定数> | <数値定数> '-' <数値定数> | <文字列定数> | <文字列定数> '-' <文字列定数>
            Token t;
            this.IsForloop = true;
            Case.CaseCondition cond = Case.GetCaseFactory().NewCondition();
            bool firsttime = true;
            while (true)
            {
                if (firsttime)
                {
                    firsttime = false;
                }
                else
                {
                    t = lex.NextToken();
                    if (!t.IsSymbol || t.GetToken != ",")
                    {
                        // カンマでない。ここで終わり。
                        lex.Rollback();
                        break;
                    }
                }

                t = lex.NextToken();

                if (t.IsNumber)
                {
                    // '-' 数値 と続いていれば、数値範囲。
                    // そうでなければ数値定数。
                    string lower = t.GetToken;

                    t = lex.NextToken();
                    if (t.IsSymbol && t.GetToken == "-")
                    {
                        t = lex.NextToken();
                        if (t.IsNumber)
                        {
                            string higher = t.GetToken;

                            cond.AddSubcondition(
                                Case.GetCaseFactory().NewSubcondition(
                                    ParseNumericConstant(lower), ParseNumericConstant(higher)));
                        }
                        else
                        {
                            throw new IllegalTokenException(t);
                        }
                    }
                    else
                    {
                        lex.Rollback();
                        cond.AddSubcondition(
                            Case.GetCaseFactory().NewSubcondition(ParseNumericConstant(lower)));
                    }
                }
                else if (t.IsString)
                {
                    // '-' 文字列定数 と続いていれば、文字列範囲。
                    // そうでなければ数値定数。
                    string lower = t.GetToken;

                    t = lex.NextToken();
                    if (t.IsSymbol && t.GetToken == "-")
                    {
                        t = lex.NextToken();
                        if (t.IsString)
                        {
                            string higher = t.GetToken;

                            cond.AddSubcondition(
                                Case.GetCaseFactory().NewSubcondition(lower, higher));
                        }
                        else
                        {
                            throw new IllegalTokenException(t);
                        }
                    }
                    else
                    {
                        lex.Rollback();
                        cond.AddSubcondition(
                            Case.GetCaseFactory().NewSubcondition(lower));
                    }
                }
                else
                {
                    throw new IllegalTokenException(t);
                }
            }
            this.IsForloop = false;
            return cond;
        }

        public Statement.Switch ParseSwitch()
        {
            this.IsForloop = true;
            // switch文 := 'switch' 式 ブロック
            Token t;

            t = lex.NextToken();
            if (!t.IsSymbol || t.GetToken != "switch")
            {
                // これはswitch文でない。
                lex.Rollback();
                return null;
            }

            Expression expr = ParseExpression();
            if (expr == null)
            {
                throw new IllegalTokenException(lex.NextToken());
            }

            Block block = ParseBlock();
            if (block == null)
            {
                throw new IllegalTokenException(lex.NextToken());
            }
            this.IsForloop = false;
            return Statement.GetFactory().NewSwitch(expr, block);
        }

        public Statement.While ParseWhile()
        {
            this.IsForloop = true;
            // while文 := 'while' 条件式 ブロック
            Token t;

            t = lex.NextToken();
            if (!t.IsSymbol || t.GetToken != "while")
            {
                // これはwhile文でない。
                lex.Rollback();
                return null;
            }

            Condition cond = ParseCondition();
            if (cond == null)
            {
                throw new IllegalTokenException(lex.NextToken());
            }

            Block block = ParseBlock();
            if (block == null)
            {
                throw new IllegalTokenException(lex.NextToken());
            }

            return Statement.GetFactory().NewWhile(cond, block);
        }

        public Statement.For ParseFor()
        {
            this.IsForloop = true;
            // for文 := 'for' 代入文? ';' 条件式? ';' 代入文? ブロック
            Token t;

            t = lex.NextToken();
            if (!t.IsSymbol || t.GetToken != "for")
            {
                // これはwhile文でない。
                lex.Rollback();
                return null;
            }

            Substitution init = ParseSubstitution();

            t = lex.NextToken();
            if (!t.IsSymbol || t.GetToken != ";")
            {
                throw new IllegalTokenException(t);
            }

            Condition cond = ParseCondition();
             
            t = lex.NextToken();
            if (!t.IsSymbol || t.GetToken != ";")
            {
                throw new IllegalTokenException(t);
            }

            Substitution alteration = ParseSubstitution();

            Block block = ParseBlock();
            if (block == null)
            {
                throw new IllegalTokenException(lex.NextToken());
            }

            cond.IsForloop = true;
            this.IsForloop = false;
            return Statement.GetFactory().NewFor(init, cond, alteration, block);
        }



    }
}

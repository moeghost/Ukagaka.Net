using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
 
namespace aya
{
    public class LexicalAnalyzer
    {
        public static int TYPE_IDENTIFIER = 0; // 識別子
        public static int TYPE_SYMBOL = 1; // "if" "+" ">="等
        public static int TYPE_NUMBER = 2; // 数値定数(符号無し)
        public static int TYPE_STRING = 3; // 文字列定数(ダブルクオート無し)

        private static readonly Regex SpaceOfHead = new Regex("^\\s*");
        private static readonly Regex CommentLine = new Regex("^(?m://.*$)");
        private static readonly Regex CommentBlock = new Regex("^(?s:/\\*.*?\\*/)");
        private static readonly Regex Identifier = new Regex("^([^\\-+*/=:!;{}%&#\"()\\[\\]<>,?\\|\\s]+)");
        private static readonly Regex Symbol = new Regex("^(?::=|\\+\\+|\\+:=|\\+=|\\+|--|-:=|-=|-|\\*:=|\\*=|\\*|/:=|/=|/|==|=|:=|!_in_|!=|%:=|%=|%|<=|<|>=|>|if|elseif|else|case|switch|when|others|while|for|break|continue|return|_in_|&&|\\|\\||[&,;{}()\\[\\]])");
        private static readonly Regex ConstString = new Regex("^\"(.*?)\"");
        private static readonly Regex ConstNumber = new Regex("^(0x[0-9a-fA-F]+|0b[01]+|[0-9]*\\.[0-9]+|[0-9]+)");

        private readonly StringBuilder buf;
        private readonly ArrayList rollbackStack; // [[Token, Token, ...][Token, Token, ...]...]
        private readonly ArrayList rollbacked;
        private bool spaceWasBeforeLastToken;
        private int lineNumber;

        public LexicalAnalyzer(string source)
        {
            buf = new StringBuilder(ConvertBackslashToYen(source));
            rollbackStack = new ArrayList();
            rollbacked = new ArrayList();
            spaceWasBeforeLastToken = false;
            lineNumber = 0;

            Push();
        }

        public override string ToString()
        {
            string first = buf.Length >= 100 ? buf.ToString(0, 100) : buf.ToString();
            return $"<rollbacked: {rollbacked} remaining(first 50): \"{first}\">";
        }

        public void Push()
        {
            rollbackStack.Add(new ArrayList());
        }

        public void Pop()
        {
            // Discard the top of the stack.
            rollbackStack.RemoveAt(rollbackStack.Count - 1);
        }

        public void PopNBack()
        {
            // Roll back the top of the stack and then discard it.
            // Tokens rolled back later will appear earlier.
            rollbacked.InsertRange(0, (ArrayList)rollbackStack[rollbackStack.Count - 1]);
            Pop();
        }

        public void Rollback()
        {
            // Roll back the last token of the top of the stack and then discard it.
            // Tokens rolled back later will appear earlier.
            ArrayList top = (ArrayList)rollbackStack[rollbackStack.Count - 1];
            rollbacked.Insert(0, top[top.Count - 1]);
            top.RemoveAt(top.Count - 1);
        }

        public Token NextToken()
        {
            // Throw NoMoreTokensException if there are no more tokens.
            // Are there any rolled back tokens?
            Token result = null;
            if (rollbacked.Count > 0)
            {
                result = (Token)rollbacked[0];
                rollbacked.RemoveAt(0);
            }
            else
            {
                Match matcher;
                while (true)
                {
                    // Remove leading whitespaces.
                    if ((matcher = SpaceOfHead.Match(buf.ToString())).Success)
                    {
                        spaceWasBeforeLastToken = matcher.Groups[0].Length > 0;
                        // Count the number of line breaks to increase the line number.
                        lineNumber += Regex.Matches(matcher.Groups[0].Value, "\n").Count;
                        buf.Remove(0, matcher.Groups[0].Length);
                    }

                    // Is the buffer empty?
                    if (buf.Length == 0)
                    {
                        throw new NoMoreTokensException();
                    }

                    if ((matcher = ConstString.Match(buf.ToString())).Success)
                    {
                        // Evaluate string constants. Determine here.
                        result = new Token(TYPE_STRING, matcher.Groups[1].Value, spaceWasBeforeLastToken, lineNumber);
                        buf.Remove(0, matcher.Groups[0].Length);
                        break;
                    }
                    else if ((matcher = CommentLine.Match(buf.ToString())).Success)
                    {
                        // Next, evaluate comments.
                        buf.Remove(0, matcher.Groups[0].Length);
                    }
                    else if ((matcher = CommentBlock.Match(buf.ToString())).Success)
                    {
                        buf.Remove(0, matcher.Groups[0].Length);
                    }
                    else if ((matcher = Symbol.Match(buf.ToString())).Success)
                    {
                        // Evaluate symbols.
                        result = new Token(TYPE_SYMBOL, matcher.Groups[0].Value, spaceWasBeforeLastToken, lineNumber);
                        buf.Remove(0, matcher.Groups[0].Length);
                        break;
                    }
                    else if ((matcher = Identifier.Match(buf.ToString())).Success)
                    {
                        // Evaluate identifiers.
                        string token = matcher.Groups[1].Value;
                        Match subMatcher;
                        if ((subMatcher = ConstNumber.Match(token)).Success && subMatcher.Groups[0].Length == token.Length)
                        {
                            // If it looks like an identifier but all the characters that make it up
                            // form a valid numeric constant, then it is a numeric constant.
                            result = new Token(TYPE_NUMBER, token, spaceWasBeforeLastToken, lineNumber);
                            // Identifiers are evaluated before numeric constants,
                            // as a name starting with a number is a valid identifier in Aya.
                        }
                        else
                        {
                            result = new Token(TYPE_IDENTIFIER, token, spaceWasBeforeLastToken, lineNumber);
                        }
                        buf.Remove(0, token.Length);
                        break;
                    }
                    else if ((matcher = ConstNumber.Match(buf.ToString())).Success)
                    {
                        // Evaluate numeric constants.
                        result = new Token(TYPE_NUMBER, matcher.Groups[0].Value, spaceWasBeforeLastToken, lineNumber);
                        buf.Remove(0, matcher.Groups[0].Length);
                        break;
                    }
                    else
                    {
                        int size = buf.Length >= 15 ? 15 : buf.Length;
                        throw new Exception($"Syntax error occurred near \"{buf.ToString(0, size)}\"");
                    }
                }
            }

            if (result != null)
            {
                // Register in the buffer at the top of the stack.
                ArrayList top = (ArrayList)rollbackStack[rollbackStack.Count - 1];
                top.Add(result);
            }

            return result;
        }

        public static string ConvertBackslashToYen(string src)
        {
            // Replace backslashes with yen signs
            StringBuilder result = new StringBuilder();
            int len = src.Length;
            for (int i = 0; i < len; i++)
            {
                char c = src[i];
                int n = (int)c;
                result.Append(c == 0x005c ? (char)0x00a5 : c);
            }
            return result.ToString();
        }
    }
}

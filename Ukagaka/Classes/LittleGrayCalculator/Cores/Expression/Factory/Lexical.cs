/*============================================
 * 类名 :Lexical
 * 描述 :
 *   
 * 创建时间: 2011-2-6 17:36:59
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using aya.Eval;
using System;
using System.Collections.Generic;

using System.Text;
using System.Text.RegularExpressions;

namespace LittleGrayCalculator.Cores
{
    /// <summary>词法分析</summary>
    class Lexical
    {
      
        private static Lexical shared = new Lexical();

        public Lexical() {
            //ReflectWord.Find(constants, functions, operates);
            ReflectWord.Find(constants);
            ReflectWord.Find(functions);
            ReflectWord.Find(operates);
            ReflectWord.Find(algebras);
        }
        string Value;
        List<ConstantNode> constants = new List<ConstantNode>();
        List<FunctionNode> functions = new List<FunctionNode>();
        List<OperateNode> operates = new List<OperateNode>();
        List<AlgebraNode> algebras = new List<AlgebraNode>();
         
        public static bool IsOperate(string value)
        {
            foreach (OperateNode operate in Lexical.shared.operates)
            {
                if (operate.Format == value)
                {
                    return true;
                }

            }

            return false;
        }


        int _carry;
        public int Carry
        {
            get { return _carry; }
            set
            {
                _carry = value;
                
            }

        }

        bool _isRadian;
        public bool IsRadian
        {
            get { return _isRadian; }
            set
            {

                _isRadian = value;
                
            }

        }

        /// <summary>词法分析</summary>
        public List<Node> Analyse(string value) {
            List<Node> nodes = new List<Node>();
            Value = DealSymbol(value);
            Stack<int> leftBracket = new Stack<int>(); //检查左右括号是否匹配

            for (int i = 0; i < Value.Length; i++) {
                char ch = Value[i];
                if (ch == ' ' || ch.ToString() == "\n") {
                    continue;
                }
                /*
                else if (CreateAlgebraNode(ch) != null)
                {
                    int o = i;
                    nodes.Add(CreateAlgebraNode(ch));
                    SetIndex(nodes, o);
                    i--;
                }
                */
                else if (IsWord(ch))
                {
                    int o = i;
                    nodes.Add(CreateWordNode(GetWord(ref i), o));
                    SetIndex(nodes, o);
                    i--;
                } else if (IsFirstNumber(i)) {
                    int o = i;
                    try {
                        NumberNode node = new NumberNode(GetNumber(ref i));
                        node.Carry = Carry;
                        node.IsRadian = IsRadian;
                        nodes.Add(node);
                        SetIndex(nodes, o);
                        i--;
                    } catch (ExpressionException e) {
                        throw new ExpressionException("数字出现错误" + e.Message, o, i - o + 1);
                    }
                } else if (ch == '(') {
                    leftBracket.Push(i);
                    nodes.Add(new LeftBracketCompart());
                    SetIndex(nodes, i);

                } else if (ch == ')') {
                    if (leftBracket.Count == 0)
                        throw new ExpressionException("右括号不匹配", i, 1);
                    leftBracket.Pop();
                    nodes.Add(new RightBracketCompart());
                    SetIndex(nodes, i);

                } else if (ch == ',') {
                    nodes.Add(new CommaCompart());
                    SetIndex(nodes, i);
                } 
                else if (IsFirstOperate(i)) 
                {
                    int o = i;
                    nodes.Add(CreateOperateNode(GetOperate(ref i),o));
                    SetIndex(nodes, o);
                    i--;
                }
                else if (CreateFunctionNode(ch) != null)
                {
                    nodes.Add(CreateFunctionNode(ch));
                    SetIndex(nodes, i);
                } 
                else if (ch == '.') {
                    throw new ExpressionException("小数点不能作为数字的首字符", i, 1);
                } else {
                    throw new ExpressionException("出现未定义字符", i, 1);
                }

            }
            if (leftBracket.Count != 0) {
                throw new ExpressionException("左括号不匹配", leftBracket.Peek(), 1);
            }
            Validate(nodes);
            return nodes;
        }
        static void Validate(List<Node> nodes) {
            for (int i = 0; i < nodes.Count - 1; i++) {
                if (IsNumber(nodes[i]) && IsNumber(nodes[i + 1]))
                    throw new ExpressionException("数字、常量或函数不能直接相邻", nodes[i + 1].Index, nodes[i + 1].Format.Length);
                if ((nodes[i] is LeftBracketCompart && nodes[i + 1] is RightBracketCompart) ||
                    nodes[i] is RightBracketCompart && nodes[i + 1] is LeftBracketCompart)
                    throw new ExpressionException("左右括号不能直接相邻", nodes[i].Index, 1);


                if (nodes[i] is OperateNode)
                {
                    
                    if ((nodes[i] as OperateNode).MinParameterCount > 1)
                    {
                        if (i == 0 && (!(nodes[i] is AddOperate) && !(nodes[i] is MinusOperate)))  //cal -sin(cos(PI,10)+pow(2,3))-max(2,PI)
                            throw new ExpressionException("二元操作符不能作为首字符", nodes[i].Index, 1);
                        if (nodes[i + 1] is RightBracketCompart || nodes[i + 1] is CommaCompart || nodes[i + 1] is OperateNode)
                            throw new ExpressionException("二元操作符右边的元素不合法", nodes[i + 1].Index, nodes[i + 1].Format.Length);
                        if ((nodes[i - 1] is LeftBracketCompart || nodes[i - 1] is CommaCompart) && (!(nodes[i - 1] is AddOperate) && !(nodes[i - 1] is MinusOperate)))
                            throw new ExpressionException("二元操作符左边的元素不合法", nodes[i - 1].Index, nodes[i - 1].Format.Length);
                    }
                }
            }
            if (nodes[nodes.Count - 1] is OperateNode && (nodes[nodes.Count - 1] as OperateNode).MinParameterCount > 1)
            {
                throw new ExpressionException("二元操作符不能作为尾字符", nodes[nodes.Count - 1].Index, nodes[nodes.Count - 1].Format.Length);
            }
        }
        static bool IsNumber(Node value) {
            if (value is NumberNode || value is ConstantNode || value is FunctionNode || value is AlgebraNode)
                return true;
            return false;
        }
        private static void SetIndex(List<Node> nodes, int o) {
            nodes[nodes.Count - 1].Index = o;
        }

        /// <summary>处理正负号的二义性问题</summary>
        string DealSymbol(string value) {
            value = value.Replace(" ", "");
            Regex r = new Regex(@"[\+-]");
            int start = 0;
            while (true) {
                Match mPlug = r.Match(value, start);
                if (mPlug.Success) {
                    if (CanChange(value, mPlug)) {
                        value = value.Insert(mPlug.Index + 1, "1*");

                    }
                    start = mPlug.Index + 1;
                } else
                    break;
            }
            return value;
        }

        private bool CanChange(string value, Match m) {
            return (m.Index == 0 || value[m.Index - 1] == ',' || value[m.Index - 1] == '(')
                                && (value[m.Index + 1] == '(' || IsWord(value[m.Index + 1]));
        }

        /// <summary>创建操作符节点</summary>
        Node CreateAlgebraNode(char ch) {
            for (int i = 0; i < algebras.Count; i++) {
                if (algebras[i].Format == ch.ToString())
                    return algebras[i].NewObject(ch.ToString());
            }
            return null;
        }

        /// <summary>创建操作符节点</summary>
        public Node CreateOperateNode(char ch)
        {
            for (int i = 0; i < operates.Count; i++)
            {
                if (operates[i].Format == ch.ToString())
                    return operates[i].NewObject("");
            }
            return null;
        }

        Node CreateOperateNode(string value, int index)
        {
              
            for (int i = 0; i < operates.Count; i++)
            {
                if (operates[i].Format == value)
                    return operates[i].NewObject(value);
            }

            return null;


        }

        public static Node CreateOperateNode(string value)
        {

            for (int i = 0; i < shared.operates.Count; i++)
            {
                if (shared.operates[i].Format == value)
                    return shared.operates[i].NewObject(value);
            }

            return null;


        }

        Node CreateFunctionNode(char ch)
        {
            for (int i = 0; i < functions.Count; i++)
            {
                if (functions[i].Format == ch.ToString())
                    return functions[i].NewObject("");
            }
            return null;
        }







        /// <summary>根据关键字创建节点，可以是常量也只可以为函数名</summary>
        Node CreateWordNode(string value, int index) {
            for (int i = 0; i < constants.Count; i++) {
                if (constants[i].Format == value)
                    return constants[i].NewObject(value);
            }
            for (int i = 0; i < functions.Count; i++) {
                if (functions[i].Format == value)
                    return functions[i].NewObject(value);
            }
           
            for (int i = 0; i < algebras.Count; i++)
            {
                if (algebras[i].Format == value)
                {
                   return algebras[i].NewObject(value);
                }
            } 
            throw new ExpressionException("出现未定义的关键字", index, value.Length);
        }

        /// <summary>在这个索引上获得后继的关键字，并移动索引</summary>
        string GetWord(ref  int index) {
            string result = "";
            for (; index < Value.Length; index++) {
                if (!IsAfterWord(Value[index]))
                    return result;
                result += Value[index];
            }
            return result;
        }
        /// <summary>在这个索引上获得后继的数字，并移动索引</summary>
        string GetNumber(ref int index) {
            string result = Value[index].ToString();
            index++;
            for (; index < Value.Length; index++) {
                if (!IsContinueNumber(Value[index]))
                    return result;
                result += Value[index];
            }
            return result;
        }

        string GetOperate(ref int index)
        {
            string result = "";

             
            for (; index < Value.Length; index++)
            {
                string value = "";
                value += Value[index];


               
                 

                if (IsOperate(value))
                {
                    result += value;

                    if (IsOperate(result))
                    {

                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
                
            }
            return result;
             
        }
        /// <summary>操作符的后续是否还可组合为新的操作符，可以是&&，||，>>,<<,!=,==</summary>
        int AfterOperate(ref int index, List<Node> nodes)
        {
            string value = nodes[index].Format;
            int result = 0;
            index++;
            for (; index < nodes.Count; index++)
            {
                if (IsOperate(value))
                {
                    result++;
                }
                else
                {
                    break;
                }
                value += nodes[index].Format;
            }
            return result;

        }


        /// <summary>字母关键的后续是否还可组合为关键字，可以是字母、下划线、数字</summary>
        bool IsAfterWord(char value) {
            if (IsWord(value)  //字母
                || value == '_' ||                                //下划线
               IsNumber(value)              //数字
                )
                return true;
            return false;
        }
        /// <summary>常量或函数的开始</summary>
        bool IsWord(char value) 
        {
            if (Carry == 16)
            {
                if (('a' <= value && value <= 'f') || ('A' <= value && value <= 'F'))
                {
                    return false;
                }

            }

            if (('a' <= value && value <= 'z') || ('A' <= value && value <= 'Z') || value == '_')
                return true;
            return false;
        }

        

        /// <summary>是数字</summary>
        bool IsNumber(char value) {
            if ('0' <= value && value <= '9')
            {
                return true;
            }
            if (Carry == 16)
            {
                if (('a' <= value && value <= 'f') || ('A' <= value && value <= 'F'))
                {
                    return true;
                }

            }
            return false;
        }
        /// <summary>为数字的第一个字符</summary>
        bool IsFirstNumber(int index) {
            if (IsNumber(Value[index]))
                return true;
            //考虑 + - 号的二意性问题
            if (Value[index] == '+' || Value[index] == '-') {
                if (index == 0)
                    return true;
                if (Value[index - 1] == '(' || Value[index - 1] == ',')
                    return true;
            }
            return false;
        }

        bool IsFirstOperate(int index)
        {
            int i = index;

            if (GetOperate(ref i) != "")
            {

                return true;
            }

            return false;


        }



        /// <summary>是合法的后继数字字符</summary>
        bool IsContinueNumber(char value) {
            if ((IsNumber(value) || value == '.'))
                return true;
            return false;
        }

      

    }
}

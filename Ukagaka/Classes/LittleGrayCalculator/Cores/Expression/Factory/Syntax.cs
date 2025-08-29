/*============================================
 * 类名 :Syntax
 * 描述 :语法分析类,这里经过两步：先将中序转化为后序，再将后序转化为树形
 *   
 * 创建时间: 2011-2-6 17:49:07
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary>语法分析类</summary>
    class Syntax
    {
        /// <summary>经过词法分析的中序形式,返回树形的头节点</summary>
        public static Node Analyse(List<Node> nodes) {
            return CreateSyntaxTree(nodes, 0, nodes.Count - 1);
        }

          
        static Node CreateSyntaxTree(List<Node> nodes, int first, int end)
        {
            List<Node> after = new List<Node>();
            Stack<Node> op = new Stack<Node>();

            for (int i = first; i <= end; i++)
            {
                Node n = nodes[i];

                if (n is NumberNode || n is ConstantNode || n is AlgebraNode)
                {
                    after.Add(n);
                }
                else if (n is LeftBracketCompart)
                {
                    op.Push(n);
                }
                else if (n is RightBracketCompart)
                {
                    while (op.Count > 0 && !(op.Peek() is LeftBracketCompart))
                    {
                        after.Add(op.Pop());
                    }
                    op.Pop(); // 弹出左括号
                }
                else if (n is FunctionNode)
                {
                    int funEnd = 0;
                    n.Nexts = FindParameters(nodes, i + 1, ref funEnd);
                    after.Add(n);
                    i = funEnd;
                }
                else if (n is OperateNode currentOp)
                {
                    // 处理运算符结合性
                    while (op.Count > 0 && op.Peek() is OperateNode topOp)
                    {
                        // 如果栈顶运算符优先级更高，或者优先级相同且左结合，则弹出
                        if (topOp.Priority > currentOp.Priority ||
                            (topOp.Priority == currentOp.Priority && currentOp.Associativity == Associativity.Left))
                        {
                            after.Add(op.Pop());
                        }
                        else
                        {
                            break;
                        }
                    }
                    op.Push(currentOp);
                }
            }

            // 弹出剩余运算符
            while (op.Count > 0)
            {
                after.Add(op.Pop());
            }

            return CreateSyntaxTree(after); // 转换为语法树
        }

        /// <summary>经过词法分析的中序形式,返回树形的头节点</summary>
        static Node CreateSyntaxTree1(List<Node> nodes, int first, int end)
        {
            List<Node> after = new List<Node>();
            Stack<Node> op = new Stack<Node>();
              
            for (int i = first; i <= end; i++) {
                Node n = nodes[i];

                if (n is NumberNode || n is ConstantNode || n is AlgebraNode) {
                    after.Add(n);

                } else if (n is LeftBracketCompart) {
                    op.Push(n);

                } else if (n is RightBracketCompart) {
                    while (true) {
                        Node t = op.Pop();
                        if (t is LeftBracketCompart)
                            break;
                        after.Add(t);
                    }

                } else if (n is FunctionNode) {
                    //在正在后序形式的同时，将函数的参数都放在其后序节点上
                    int funEnd = 0;
                    n.Nexts = FindParameters(nodes, i + 1, ref  funEnd);
                    
                    //将参数合并到函数名节点后，就可将其视为一个数
                    after.Add(n);
                    i = funEnd; //在for有加1的操作

                } else if (n is OperateNode) {
                    if (op.Count != 0) {
                        if (op.Peek().Priority >= n.Priority) {
                            after.Add(op.Pop());
                            op.Push(n);
                        } else {
                            op.Push(n);
                        }
                    } else {
                        op.Push(n);
                    }
                }

            }
            while (op.Count != 0)
            {
                after.Add(op.Pop());
            }
            return CreateSyntaxTree(after);
        }

        /// <summary>这个方法的参数已是后序形式，没有括号，函数的参数也合并到函数的子节点上</summary>
       public static Node CreateSyntaxTree(List<Node> after) {
            Stack<Node> tree = new Stack<Node>();
            // 如果是单节点，直接返回
            

            for (int i = 0; i < after.Count; i++) {
                Node n = after[i];
                if (n is OperateNode && (n as OperateNode).MinParameterCount > 1) {
                    Node right = tree.Pop();
                    Node left = tree.Pop();
                    n.Nexts.Add(left);
                    n.Nexts.Add(right);
                    tree.Push(n);
                }
                else  if (n is OperateNode)
                {
                    Node left = tree.Pop();
                    n.Nexts.Add(left);
                    tree.Push(n);
                }
                else
                {
                    tree.Push(n);
                }
            }
            return tree.Pop();
        }

        /// <summary>在函数的括号中,找到参数,这些参数用逗号分开，返回所以的参数。start指向左括号的位置，end返回函数的右括号</summary>
        static List<Node> FindParameters(List<Node> nodes, int start, ref int end) {
            List<Node> result = new List<Node>();

            int leftCount = 0;
            int i = start + 1;
            int subStart = i;
            if (start == nodes.Count)
                throw new ExpressionException("函数后必须先接左括号", nodes[start - 1].Index, nodes[start - 1].Format.Length);
            if (!(nodes[start] is LeftBracketCompart)) {
                throw new ExpressionException("函数后必须先接左括号", nodes[start].Index, nodes[start].Format.Length);
            }
            if (nodes[i] is CommaCompart)
                throw new ExpressionException("逗号位置不对", nodes[i].Index, 1);
            while (true) {
                if (nodes[i] is CommaCompart) {
                    if (leftCount == 0) {
                        if (subStart > i - 1) {
                            throw new ExpressionException("逗号位置不合法", subStart + 1, 1);
                        }
                        result.Add(CreateSyntaxTree(nodes, subStart, i - 1));
                        subStart = i + 1;
                    }
                } else if (nodes[i] is LeftBracketCompart) {
                    leftCount++;
                } else if (nodes[i] is RightBracketCompart) {
                    leftCount--;
                    //遇到匹配的右括号时结束
                    if (leftCount == -1) {
                        if (subStart > i - 1)
                            throw new ExpressionException("逗号位置不合法", subStart + 1, 1);
                        result.Add(CreateSyntaxTree(nodes, subStart, i - 1));
                        end = i;
                        break;
                    }
                }
                //到表达式结束时也结束
                if (i == nodes.Count - 1) {
                    if (!(nodes[i] is RightBracketCompart))
                        throw new ExpressionException("表达式中括号不匹配");
                    result.Add(CreateSyntaxTree(nodes, subStart, i - 1));
                    end = i;
                    break;
                }
                i++;

            }
            return result;
        }

    } // end class
}

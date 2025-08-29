/*============================================
 * 类名 :Node
 * 描述 :
 *   
 * 创建时间: 2011-2-5 10:57:26
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;
using System.ComponentModel;
using System.Security.Cryptography;

namespace LittleGrayCalculator.Cores
{
    /// <summary></summary>
    abstract class Node 
    {
        public Node() { }
        public Node(string v) {
            _format = v;
        }

        public Node(Node node)
        {

        }

        List<Node> nexts = new List<Node>();

        protected string _format;



        /// <summary>此节点的字符串表示形式</summary>
        [Category("Node"), Description("此节点在表达式中的字符串形式")]
        public abstract string Format { get; }

        ///// <summary>此节点的字符串长度</summary>
        //[Category("Node"), Description("在此节点下结果的长度")]
        //public int Length {
        //    get { return Value.IntLength + Value.DecimalLength; }
        //}

        /// <summary>最少的参数个数</summary>
        [Category("Node"), Description("此节点最少的子节点数")]
        public abstract int MinParameterCount { get; }

          

        /// <summary>此节点的后继节点</summary>
        [Browsable(false)]
        public List<Node> Nexts {
            get { return nexts; }
            set {
                nexts = value;
                if (value.Count < MinParameterCount)
                    throw new ExpressionException(string.Format("函数的个数没有达到{0}个", MinParameterCount), Index, Format.Length);
            }
        }

        /// <summary>此节点的值</summary>
        [Category("Node"), Description("此节点在表达式中的值")]
        public abstract BigNumber Value { get; }

        [Category("Node"), Description("此节点在计算中的优先级")]
        /// <summary>优先级</summary>
        public abstract int Priority { get; }
        public virtual bool IsRadian { get; set; }
        /// <summary>此节点的首字符在表达式中的索引位</summary>
        [Category("Node"), Description("此节点的首字符在表达式中的索引位")]
        public int Index { get; set; }

        public override string ToString() {
            return Format;
        }

         

         

        internal void ReplaceAll(string src, string dst)
        {
            

            _format = _format?.Replace(src, dst);
            foreach (Node node in nexts)
            {
                node.ReplaceAll(src, dst);
            }

        }

        internal void Restore()
        {
            _format = Format;
            foreach (Node node in nexts)
            {
                node.Restore();
            }
        }
    }

}

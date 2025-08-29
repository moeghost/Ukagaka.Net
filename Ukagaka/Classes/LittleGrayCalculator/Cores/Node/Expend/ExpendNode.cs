/*============================================
 * 类名 :ExpendNode
 * 描述 :用字母表示的节点，包括函数名和常量
 *   
 * 创建时间: 2011-2-5 10:58:55
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;
using System.ComponentModel;

namespace LittleGrayCalculator.Cores
{
    /// <summary>可进行拓展的节点，包括函数、常量、操作符</summary>
    abstract class ExpendNode : Node
    {
        ///// <summary>常量或函数名在表达式中的字符表示</summary>
        //public override abstract string Format { get; }
        /// <summary>对节点的描述</summary>
        [Browsable(false)]

        public ExpendNode() { }
        public ExpendNode(string value) : base(value) { }

            
        public abstract string Description { get; }

        /// <summary>创建一个新的节点</summary>
        public abstract Node NewObject(string value);
        //public abstract string Format { get; }
        //public abstract      string Format { get; }

        //public override string ToString()
        //{
        //    return Format;
        //}
    }
}

/*============================================
 * 类名 :ConstantNode
 * 描述 :表达内置常量的类，如e,PI
 *   
 * 创建时间: 2011-2-5 10:59:23
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary>表达内置常量的类，如e,PI</summary>
    abstract class AlgebraNode : ExpendNode
    {

        public AlgebraNode() { }
        public AlgebraNode(string value) : base(value) { }


        public override string Format
        {
            get
            {
                return _format;
            }
             
        }

       

        public override int Priority
        {
            get { return 6; }
        }

        public override int MinParameterCount
        {
            get { return 0; }
        }

        public override string ToString()
        {
            return _format;
        }

    }
}

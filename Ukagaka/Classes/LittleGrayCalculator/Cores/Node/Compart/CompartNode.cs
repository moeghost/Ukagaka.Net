/*============================================
 * 类名 :CompartNode
 * 描述 :
 *   
 * 创建时间: 2011-2-6 13:09:07
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary>表达式中的间隔，比如括号之类，只在词法分析中用到，不会出现在树形中</summary>
    abstract   class CompartNode : Node
    {


        public override BigNumber Value
        {
            get { throw new NotImplementedException(); }
        }

        public override int Priority
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinParameterCount
        {
            get { throw new NotImplementedException(); }
        }
    }
}

/*============================================
 * 类名 :RightBracketCompart
 * 描述 :
 *   
 * 创建时间: 2011-2-6 13:44:10
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary></summary>
    class RightBracketCompart :CompartNode 
    { 

        public override int Priority
        {
            get { return 0; }
        }

        public override BigNumber Value
        {
            get { throw new NotImplementedException(); }
        }

        public override string Format
        {
            get { return ")"; }
        }
    }
}

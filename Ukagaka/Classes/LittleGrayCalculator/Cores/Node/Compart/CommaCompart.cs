/*============================================
 * 类名 :CommaCompart
 * 描述 :
 *   
 * 创建时间: 2011-2-6 17:05:20
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary>逗号</summary>
    class CommaCompart :CompartNode 
    {
         
        public override int Priority
        {
            get { return 8; }
        }

        public override BigNumber Value
        {
            get { throw new NotImplementedException(); }
        }

        public override string Format
        {
            get { return ","; }
        }
    }
}

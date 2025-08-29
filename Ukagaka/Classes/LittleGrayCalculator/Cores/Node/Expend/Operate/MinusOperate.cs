/*============================================
 * 类名 :MinusOperate
 * 描述 :
 *   
 * 创建时间: 2011-2-6 16:15:37
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary>减</summary>
    class MinusOperate : OperateNode
    {

        public override string Format {
            get { return "-"; }
        }

        public override Node NewObject(string value) {
            return new MinusOperate();
        }

        public override int Priority {
            get { return 2; }
        }

        public override string Description {
            get { return "减法操作符"; }
        }

        public override BigNumber Value {
            get { return Nexts[0].Value - Nexts[1].Value; }
        }
    }
}

/*============================================
 * 类名 :MultiplyOperate
 * 描述 :
 *   
 * 创建时间: 2011-2-6 16:16:31
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary>乘法操作符</summary>
    class MultiplyOperate : OperateNode
    {
        public override string Format {
            get { return "*"; }
        }

        public override Node NewObject(string value) {
            return new MultiplyOperate();
        }

        public override int Priority {
            get { return 3; }
        }

        public override string Description {
            get { return "乘法操作符"; }
        }

        public override BigNumber Value {
            get { return Nexts[0].Value * Nexts[1].Value; }
        }

    }
}

/*============================================
 * 类名 :ExpNode
 * 描述 :
 *   
 * 创建时间: 2011/2/17 20:23:04
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary>计算e的x次幂</summary>
    class ExpNode : FunctionNode
    {
        public override string Format {
            get {
                return "exp";
            }
        }
        public override string Description {
            get { return "计算e的x次幂"; }
        }

        public override Node NewObject(string value) {
            return new ExpNode();
        }

        public override BigNumber Value {
            get {
                if (Nexts.Count >= 2)
                    return TaylorFunction.Exp(Nexts[0].Value, Int32.Parse(Nexts[1].Value.ToString()));
                return TaylorFunction.Exp(Nexts[0].Value, Nexts[0].Value.DecimalPart.Count > BigNumber.precision ? Nexts[0].Value.DecimalPart.Count : BigNumber.precision);
            }
        }

        public override int MinParameterCount {
            get { return 1; }
        }
    }
}

/*============================================
 * 类名 :PowNode
 * 描述 :求任意次幂的运算
 *   
 * 创建时间: 2011-3-9 20:14:31
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary>求任意次幂的运算</summary>
    class PowNode : FunctionNode
    {
        public override string Description {
            get { return "可以计算任意正数的任意次幂，包括小数次幂"; }
        }

        public override Node NewObject(string value) {
            return new PowNode();
        }

        public override string Format {
            get { return "pow"; }
        }

        public override int MinParameterCount {
            get { return 5; }
        }

        public override BigNumber Value {
            get {
                if (Nexts.Count >= 3) {
                    int result;
                    if (Int32.TryParse(Nexts[2].Value.ToString(), out result)) {
                        return Nexts[0].Value.Power(Nexts[1].Value, result);
                    } else
                        throw new ExpressionException("pow的第三个参数只能为正数");
                }
                return Nexts[0].Value.Power(Nexts[1].Value);
            }
        }

    }
}

/*============================================
 * 类名 :CosineHyberNode
 * 描述 :
 *   
 * 创建时间: 2011-2-5 14:03:05
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary></summary>
    class CosineHyberNode : FunctionNode
    {

        public override string Format {
            get { return "cosh"; }
        }

        public override Node NewObject(string value) {
            return new CosineHyberNode();
        }

        public override string Description {
            get { return "双曲余弦函数cosh，单位为弧度。支持重载形式:cosh(PI/6,3)，其中第二个参数指明结果精确到小数点后几位，默认情况下与参数精度相同"; }
        }

        public override BigNumber Value {
            get {
                if (Nexts.Count >= 2)
                {
                    return TaylorFunction.CosineHyper(Nexts[0].Value, Int32.Parse(Nexts[1].Value.ToString()));
                }

                return EngineerFunction.CosineHyper(Nexts[0].Value);


                return TaylorFunction.CosineHyper(Nexts[0].Value, Nexts[0].Value.DecimalPart.Count > BigNumber.precision ? Nexts[0].Value.DecimalPart.Count : BigNumber.precision);
            }
        }

        public override int MinParameterCount {
            get { return 1; }
        }
    }
}

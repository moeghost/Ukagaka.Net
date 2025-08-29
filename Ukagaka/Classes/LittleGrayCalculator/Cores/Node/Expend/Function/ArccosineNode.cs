/*============================================
 * 类名 :SineNode
 * 描述 :
 *   
 * 创建时间: 2011-2-5 14:02:20
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary></summary>
    class ArccosineNode : FunctionNode
    {

        public override Node NewObject(string value) {
            return new ArccosineNode();
            //throw new NotImplementedException();
        }

        public override string Format {
            get { return "acos"; }
        }

        public override string Description {
            get { return "反三角余弦函数acos，单位为弧度。支持重载形式:acos(PI/6,3)，其中第二个参数指明结果精确到小数点后几位，默认情况下与参数精度相同"; }
        }
    
        public override BigNumber Value {
            get {
                if (Nexts.Count >= 2) {
                    int result;
                    if (Int32.TryParse(Nexts[1].Value.ToString(), out result)) {
                         return TaylorFunction.Arccosine(Nexts[0].Value, result);
                    } else {
                        throw new ExpressionException("acos的第二个参数只能是正整数", Nexts[1].Index, Nexts[1].Format.Length);
                    }
                }

                BigNumber res =  EngineerFunction.Arccosine(Nexts[0].Value);

                if (Nexts[0].IsRadian == false)
                {
                    res = EngineerFunction.ToDegrees(res);
                }

                return res;

               

                return TaylorFunction.Arccosine(Nexts[0].Value, Nexts[0].Value.DecimalPart.Count > BigNumber.precision ? Nexts[0].Value.DecimalPart.Count : BigNumber.precision);
            }
        }
        
        public override int MinParameterCount {
            get { return 1; }
        }
    }
}

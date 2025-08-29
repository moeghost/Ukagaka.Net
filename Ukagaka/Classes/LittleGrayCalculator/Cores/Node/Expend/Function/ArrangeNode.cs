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
    class ArrangeNode : FunctionNode
    {
        public override string Format {
            get {
                return "arr";
            }
        }
        public override string Description {
            get { return "计算排列"; }
        }

        public override Node NewObject(string value) {
            return new ArrangeNode();
        }

        public override BigNumber Value {
            get {
                if (Nexts.Count >= 2)
                {
                    int result;
                    if (Int32.TryParse(Nexts[0].Value.ToString(), out result) == false || Int32.TryParse(Nexts[1].Value.ToString(), out result) == false)
                    { 
                        throw new ExpressionException("Arr的参数只能是正整数", Nexts[1].Index, Nexts[1].Format.Length);
                    }
                }
                return Statistics.Arrange(Nexts[0].Value, Nexts[1].Value);
            }
        }

        public override int MinParameterCount {
            get { return 2; }
        }
    }
}

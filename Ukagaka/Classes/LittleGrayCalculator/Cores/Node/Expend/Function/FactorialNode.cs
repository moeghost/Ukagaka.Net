/*============================================
 * 类名 :FactorialNode
 * 描述 :阶乘计算
 *   
 * 创建时间: 2011/2/6 22:38:54
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary>阶乘计算</summary>
    class FactorialNode : FunctionNode
    {
        public override string Format {
            get { return "fac"; }
        }

        public override Node NewObject(string value) {
            return new FactorialNode();
        }

        public override string Description {
            get { return "阶乘函数命令，只能对正整数进行计算，如fac(4)"; }
        }

        public override BigNumber Value {
            get {
                try {
                    return Nexts[0].Value.Factorial();
                } catch (NumberException) {
                    throw new ExpressionException("阶乘函数的参数只能是正整数", Index, Format.Length);
                }
            }
        }

        public override int MinParameterCount {
            get { return 1; }
        }
    }
}

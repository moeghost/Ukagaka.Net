/*============================================
 * 类名 :MaxNode
 * 描述 :
 *   
 * 创建时间: 2011/2/16 17:28:56
 * Blog:   http://home.cnblogs.com/xiangism
 *============================================*/
using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary></summary>
    class MaxNode : FunctionNode
    {
        public override string Format {
            get { return "max"; }
        }

        public override string Description {
            get { return "max函数用于计算参数中的最大值，可支持任意个数的参数"; }
        }

        public override Node NewObject(string value) {
            return new MaxNode();
        }

        public override BigNumber Value {
            get {
                BigNumber b = Nexts[0].Value;
                foreach (Node n in Nexts) {
                    if (n.Value.CompareTo(b) == 1)
                        b = n.Value;
                }
                return b;
            }
        }

        public override int MinParameterCount {
            get { return 2; }
        }

    }
}

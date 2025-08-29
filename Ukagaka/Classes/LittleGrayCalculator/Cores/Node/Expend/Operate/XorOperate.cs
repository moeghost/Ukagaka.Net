using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary></summary>
    class XorOperate : OperateNode
    { 
        public override string Format
        {
            get { return "∧"; }
        }

        public override Node NewObject( string value )
        {
            return new XorOperate();
        }
 
        public override int Priority
        {
            get { return 3; }
        }

        public override string Description
        {
            get { return "按位异或操作符"; }
        }

        public override BigNumber Value
        {
            get { return BigNumber.BitwiseXor(Nexts[0].Value, Nexts[1].Value);  }
        }
    }
}

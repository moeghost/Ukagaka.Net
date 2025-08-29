using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary></summary>
    class LogicXOrOperate : OperateNode
    { 
        public override string Format
        {
            get { return "⊕"; }
        }

        public override Node NewObject( string value )
        {
            return new LogicXOrOperate();
        }
 
        public override int Priority
        {
            get { return 1; }
        }

        public override string Description
        {
            get { return "逻辑异或操作符"; }
        }

        public override BigNumber Value
        {
            get 
            { 
                return BigNumber.LogicXor(Nexts[0].Value, Nexts[1].Value);
            }
        }
    }
}

using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary></summary>
    class LogicNotOperate : OperateNode
    { 
        public override string Format
        {
            get { return "¬"; }
        }

        public override Node NewObject( string value )
        {
            return new LogicNotOperate();
        }
 
        public override int Priority
        {
            get { return 1; }
        }

        public override string Description
        {
            get { return "逻辑非操作符"; }
        }

        public override BigNumber Value
        {
            get 
            { 
                return BigNumber.LogicNot(Nexts[0].Value);
            }
        }
    }
}

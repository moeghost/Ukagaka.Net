using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary></summary>
    class LogicOrOperate : OperateNode
    { 
        public override string Format
        {
            get { return "||"; }
        }

        public override Node NewObject( string value )
        {
            return new LogicOrOperate();
        }
 
        public override int Priority
        {
            get { return 1; }
        }

        public override string Description
        {
            get { return "逻辑或操作符"; }
        }

        public override BigNumber Value
        {
            get 
            { 
                return BigNumber.LogicOr(Nexts[0].Value, Nexts[1].Value);
            }
        }

        public override int MinCharCount
        {
            get { return 2; }
        }

    }
}

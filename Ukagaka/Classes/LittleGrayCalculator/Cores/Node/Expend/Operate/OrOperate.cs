using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary></summary>
    class OrOperate : OperateNode
    { 
        public override string Format
        {
            get { return "|"; }
        }

        public override Node NewObject( string value )
        {
            return new OrOperate();
        }
 
        public override int Priority
        {
            get { return 1; }
        }

        public override string Description
        {
            get { return "按位或操作符"; }
        }

        public override BigNumber Value
        {
            get {   return Nexts[0].Value | Nexts[1].Value;  }
        }
    }
}

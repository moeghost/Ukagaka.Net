using System;
using System.Collections.Generic;

using System.Text;

namespace LittleGrayCalculator.Cores
{
    /// <summary></summary>
    class NotOperate : OperateNode
    { 
        public override string Format
        {
            get { return "~"; }
        }

        public override Node NewObject( string value )
        {
            return new NotOperate();
        }
 
        public override int Priority
        {
            get { return 1; }
        }

        public override string Description
        {
            get { return "按位取反操作符"; }
        }

        public override BigNumber Value
        {
            get {   return ~Nexts[0].Value;  }
        }

        public override int MinParameterCount
        {
            get { return 1; }
        }
    }
}

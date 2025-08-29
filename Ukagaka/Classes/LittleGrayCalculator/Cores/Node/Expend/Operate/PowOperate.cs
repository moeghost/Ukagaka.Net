using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleGrayCalculator.Cores
{
    class PowOperate : OperateNode
    {

        public override string Format
        {
            get { return "^"; }
        }

        public override Node NewObject(string value)
        {
            return new PowOperate();
        }

        public override int Priority
        {
            get { return 5; }
        }

        public override string Description
        {
            get { return "乘方操作符"; }
        }

        public override BigNumber Value
        {
            get { return Nexts[0].Value.Power(Nexts[1].Value); }
        }


    }
}

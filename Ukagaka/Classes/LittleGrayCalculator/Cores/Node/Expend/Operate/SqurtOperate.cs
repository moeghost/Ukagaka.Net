using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleGrayCalculator.Cores
{
    class SqurtOperate : OperateNode
    {

        public override string Format
        {
            get { return "√"; }
        }

        public override Node NewObject(string value)
        {
            return new SqurtOperate();
        }

        public override int Priority
        {
            get { return 5; }
        }

        public override string Description
        {
            get { return "开方操作符"; }
        }

        public override BigNumber Value
        {
            get { return Nexts[1].Value.Squrt(Nexts[0].Value); }
        }


    }
}

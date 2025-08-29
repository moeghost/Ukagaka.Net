using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleGrayCalculator.Cores
{
    internal class MinuteNode: OperateNode
    {
        public override string Format
        {
            get { return "'"; }
        }

        public override Node NewObject(string value)
        {
            return new MinuteNode();
        }

        public override int Priority
        {
            get { return 3; }
        }

        public override string Description
        {
            get { return "分操作符"; }
        }

        public override BigNumber Value
        {
            get
            {
                  
                return Nexts[0].Value + Nexts[1].Value / new BigNumber("3600");

            }
        }
    }
}

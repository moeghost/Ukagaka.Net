using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace LittleGrayCalculator.Cores
{
    class logarithmNode:FunctionNode
    {
        public override string Format
        {
            get
            {
                return "log";
            }
        }
        public override string Description
        {
            get { return "计算对数"; }
        }

        public override Node NewObject(string value)
        {
            return new logarithmNode();
        }

        public override BigNumber Value
        {
            get
            {
              
                return ScienceFunction.Log(Nexts[1].Value,Nexts[0].Value, Nexts[1].Value.DecimalPart.Count > BigNumber.precision ? Nexts[1].Value.DecimalPart.Count : BigNumber.precision);

                 
            }
        }

        public override int MinParameterCount
        {
            get { return 2; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace LittleGrayCalculator.Cores
{
    class logarithmNNode:FunctionNode
    {
        public override string Format
        {
            get
            {
                return "ln";
            }
        }
        public override string Description
        {
            get { return "计算自然对数"; }
        }

        public override Node NewObject(string value)
        {
            return new logarithmNNode();
        }

        public override BigNumber Value
        {
            get
            {

                return ScienceFunction.Ln(Nexts[0].Value, Nexts[0].Value.DecimalPart.Count > BigNumber.precision ? Nexts[0].Value.DecimalPart.Count : BigNumber.precision);

                 
            }
        }

        public override int MinParameterCount
        {
            get { return 1; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace LittleGrayCalculator.Cores
{
    class logarithmGNode:FunctionNode
    {
        public override string Format
        {
            get
            {
                return "lg";
            }
        }
        public override string Description
        {
            get { return "计算10的对数"; }
        }

        public override Node NewObject(string value)
        {
            return new logarithmGNode();
        }

        public override BigNumber Value
        {
            get
            {
              
                return ScienceFunction.Log10(Nexts[0].Value, Nexts[0].Value.DecimalPart.Count > BigNumber.precision ? Nexts[0].Value.DecimalPart.Count : BigNumber.precision);

                 
            }
        }

        public override int MinParameterCount
        {
            get { return 1; }
        }

    
    }
}

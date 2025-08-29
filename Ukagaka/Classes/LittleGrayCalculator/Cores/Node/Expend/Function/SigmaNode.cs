using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LittleGrayCalculator.Cores
{
    /// <summary>求任意次幂的运算</summary>
    class SigmaNode : FunctionNode
    {
        public override string Description
        {
            get { return "可以计算任意正数的任意次幂，包括小数次幂"; }
        }

        public override Node NewObject(string value)
        {
            return new SigmaNode();
        }

        public override string Format
        {
            get { return "Σ"; }
        }

        public override int MinParameterCount
        {
            get { return 3; }
        }

        public override BigNumber Value
        {
            get
            {
                
                //Expression exp = new Expression(Nexts[2].ToString());
                return  Statistics.Sigma(Nexts[0].Value, Nexts[1].Value, Nexts[2]);
            }
        }

    }
}


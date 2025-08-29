using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleGrayCalculator.Cores
{
    internal class YNode: AlgebraNode
    {

        public override string Format
        {
            get { return "y"; }
        }
        public YNode() { }
        public YNode(string value) : base(value) { }

        public override Node NewObject(string value)
        {
            return new YNode("y");
        }

        public override string Description
        {
            get { return "表示未知数y"; }
        }

        public override BigNumber Value
        {
            get
            {
                // return new BigNumber( Math.PI.ToString() );  
                return new BigNumber(_format);
            }
        }
    }
}

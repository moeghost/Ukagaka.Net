using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleGrayCalculator.Cores
{
    internal class XNode: AlgebraNode
    {


        public override string Format
        {
            get
            { 
                return "x";
            }
             
        }

        public XNode() { }
        public XNode(string value) : base(value) { }



        public override Node NewObject(string value)
        {
            return new XNode("x");
        }

        public override string Description
        {
            get { return "表示未知数x"; }
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
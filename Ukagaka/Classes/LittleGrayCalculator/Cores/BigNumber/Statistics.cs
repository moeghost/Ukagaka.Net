using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LittleGrayCalculator.Cores
{
    internal class Statistics
    {

        public static BigNumber Sigma(BigNumber start,BigNumber end, Node node)
        {
            BigNumber sum = BigNumber.Zero;

            

            for (BigNumber i = start;i <= end;i++)
            {

                List<Node> nodes = new List<Node>();

                


                Node nodeCopy = (Node)node;
                nodeCopy.ReplaceAll("x", i.ToString());
                nodeCopy.Index = 0;
                nodes.Add(nodeCopy);
                if (node.Nexts.Count > 1)
                {
                    
                }
                else if (node.Nexts.Count > 0)
                {
                   
                }
                
                    
                sum += nodeCopy.Value;

                nodeCopy.Restore();
            }



            return sum;
        }
        public static BigNumber Sigma(BigNumber start, BigNumber end, BigNumber value)
        {
            BigNumber sum = BigNumber.Zero;
            for (BigNumber i = start; i < end; i++)
            {

                sum += value;


            }



            return sum;
        }

        internal static BigNumber Arrange(BigNumber value1, BigNumber value2)
        {
            if (value1 == value2)
            {
                return value1.Factorial();
        }
            if (value2 > value1)
            {
                return new BigNumber("0");
            }

            BigNumber n = value1.Factorial();
            BigNumber sub = value1 - value2;
            BigNumber m = sub.Factorial();
            BigNumber result = n / m;
             
            return result;
        }

        internal static BigNumber Combine(BigNumber value1, BigNumber value2)
        {
            if (value2 > value1)
            {
                return new BigNumber("1");
            }


            BigNumber n = Arrange(value1, value1);
            BigNumber m = value2.Factorial();
            BigNumber result = n / m;
            return result;
             
        }
    }
}

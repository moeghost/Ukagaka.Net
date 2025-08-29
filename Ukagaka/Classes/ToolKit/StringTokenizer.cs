using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ukagaka
{
    public class StringTokenizer
    {
        private string splitString;
        private string source;
        private string[] splitArray;
        private int splitArrayIndex;
        public StringTokenizer(string source,string splitString)
        {
            this.source = source;

            this.splitString = splitString;

            this.splitArray = this.source.Split(this.splitString.ToCharArray());

            this.splitArrayIndex = 0;
        }


        public bool HasMoreTokens()
        {
            return splitArrayIndex < this.splitString.Length;
        }

        public string NextToken()
        {
            if (splitArrayIndex < this.splitString.Length)
            {
                return this.splitArray[this.splitArrayIndex++];
            }
            return "";
        }




    }
}

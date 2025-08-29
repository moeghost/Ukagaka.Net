using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ukagaka
{
    class StringExtension
    {
        public static Char CharAt(String str,int index)
        {
            return str.Substring(index,1).Single();
        } 
         
        public static String Substring(String Str,int beginIndex, int endIndex)
        {

            return Str.Substring(beginIndex, endIndex - beginIndex);

        }

    }
}

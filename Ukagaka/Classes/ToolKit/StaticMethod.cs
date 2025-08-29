using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ukagaka
{
    public class StaticMethod
    {
        public static int Rand(int value)
        {

            return new Random().Next(0, value);

        }



    }
}

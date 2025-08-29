using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ukagaka
{
     public class SCScriptErrorException:Exception
    {
        public SCScriptErrorException():base()
        {
            
        }

        public SCScriptErrorException(String msg):base(msg)
        {
           
        }




    }
}

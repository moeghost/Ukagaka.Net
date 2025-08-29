using System;
using System.Collections.Generic;
 
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using Utils;
namespace Ukagaka
{
    public class SCUpdateDataMaker
    {
        Thread thread;

        SCSession session;
        public SCUpdateDataMaker(string path) 
        {
            thread = new Thread(Run);
        }

        public SCUpdateDataMaker(string path,SCSession session)
        {
            thread = new Thread(Run);
           // session = new SCSession(path);
        }




        public void Run()
        {


        }


        public void Start()
        {
            thread.Start();
        }
           
        public bool IsAlive()
        {

            return true;
        }

        internal static bool HasDauFile(File f)
        {
            throw new NotImplementedException();
        }

        internal void WaitUntilEnd()
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace Ukagaka
{
    public class SCOnlineInstaller
    {
        Thread thread;

        public SCOnlineInstaller(string path)
        {

            thread = new Thread(Run);
        }

        public SCOnlineInstaller(string path,SCSession session)
        {

            thread = new Thread(Run);
        }
        public bool IsAlive()
        {
            return thread != null && thread.IsAlive;

        }

        public void Start()
        {
            thread.Start();
        }

        internal void WaitUntilEnd()
        {
            throw new NotImplementedException();
        }

        private void Run()
        {


        }

    }
}

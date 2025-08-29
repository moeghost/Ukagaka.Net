using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ukagaka
{
    public class SCNetworkUpdater
    {
        private SCSession session;
        private Thread thread;
        public SCNetworkUpdater(SCSession session)
        {
            this.session = session;
            this.thread = new Thread(Run);
        }

        public void Run()
        {


        }

        internal bool IsAlive()
        {
            return thread.IsAlive;
        }

        internal void Start()
        {
            thread.Start();
        }
    }
}

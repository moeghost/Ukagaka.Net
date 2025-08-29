using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace Cocoa.AppKit
{
    public class NSThread
    {
        private Thread thread;


        public NSThread(ThreadStart start)
        {
            thread = new Thread(start);

        }

        public void Start()
        {
            thread.Start();
        }

        internal void AddTimer(NSTimer displayTimer, object @default)
        {
            throw new NotImplementedException();
        }

        internal void AddTimer(NSTimer displayTimer)
        {
            
        }
    }
}

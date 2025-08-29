using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ukagaka
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;

    public class SCSSTPDaemon
    {
        private TcpListener tcpListener;
        private Thread listenerThread;
        private bool signalStop;

        public SCSSTPDaemon(int port)
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, port);
                listenerThread = new Thread(new ThreadStart(ListenForClients));
                listenerThread.Start();

             //   SCPrefWindowController prefwindow = SCPrefWindowController.SharedPrefWindowController();
         //       prefwindow.AddStringToSSTPLog("### sstpd started listening at port " + port + " of " + Dns.GetHostName() + " ###\n\n");
            }
            catch (Exception e)
            {
                tcpListener = null; // 服务器无法启动。不可用。
            }

            signalStop = false;
        }

        private void ListenForClients()
        {
            if (tcpListener == null) return;

            while (!signalStop)
            {
                try
                {
                    tcpListener.Start();
                    TcpClient client = tcpListener.AcceptTcpClient();

                 /*   int innerPool = NSAutoreleasePool.Push();

                    SCPrefWindowController prefwindow = SCPrefWindowController.SharedPrefWindowController();
                    prefwindow.AddStringToSSTPLog("# connected from " + client.Client.RemoteEndPoint.ToString() + '\n');

                    SCServerServiceThread sst = new SCServerServiceThread(client);
                    Thread serviceThread = new Thread(new ThreadStart(sst.Run));
                    serviceThread.Start();

                    NSAutoreleasePool.Pop(innerPool);*/
                }
                catch (Exception e)
                {
                    continue;
                }
            }

            tcpListener.Stop();

          //  SCPrefWindowController prefwindow = SCPrefWindowController.SharedPrefWindowController();
          //  prefwindow.AddStringToSSTPLog("### sstpd stopped ###\n\n");
        }

        public void Terminate()
        {
            if (tcpListener == null) return;

            signalStop = true;
            listenerThread.Join();
        }

        internal bool IsAlive()
        {
            throw new NotImplementedException();
        }

        internal void Start()
        {
            throw new NotImplementedException();
        }
    }
}

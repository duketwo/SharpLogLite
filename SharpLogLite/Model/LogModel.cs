using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using SharpLogLite.Utility;
using System.Collections.Generic;

/*
 * User: duketwo - https://github.com/duketwo/
 * Date: 10.11.2016
 */

namespace SharpLogLite.Model
{
    public class LogModel : IDisposable
    {
        private static ManualResetEvent ManualResetEvent = new ManualResetEvent(false);
        public ConcurrentDictionary<uint?, FixedSizedQueue<SharpLogMessage>> Queues;
        private Socket listener;

        public LogModel()
        {
            this.Queues = new ConcurrentDictionary<uint?, FixedSizedQueue<SharpLogMessage>>();
        }

        public void StartListening()
        {
            IPEndPoint localEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3273);
            Console.WriteLine("Local address and port : {0}", localEP.ToString());

            this.listener = new Socket(localEP.Address.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                this.listener.Bind(localEP);
                this.listener.Listen(20);

                while (true)
                {
                    try
                    {
                        ManualResetEvent.Reset();
                        Console.WriteLine("Waiting for a connection...");
                        this.listener.BeginAccept(
                            new AsyncCallback(new LogModelHandler(ManualResetEvent, this.Queues).AcceptCallback),
                            this.listener);

                        ManualResetEvent.WaitOne();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(String.Format("Exception: {0}", e));
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("Closing the listener...");
        }



        public void Dispose()
        {
            this.listener.Close();
        }

    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpLogLite.Utility;
using System.Collections.Concurrent;

/*
 * User: duketwo - https://github.com/duketwo/
 * Date: 10.11.2016
 */

namespace SharpLogLite.Model
{
    class LogModelHandler : IDisposable
    {

        private ManualResetEvent ManualResetEvent;
        private uint? Pid;
        private static int VERSION = 2;
        private static int MAX_MESSAGES = 1000;
        private FixedSizedQueue<SharpLogMessage> Queue;
        private ConcurrentDictionary<uint?, FixedSizedQueue<SharpLogMessage>> Queues;
        private Socket handler;


        public LogModelHandler(ManualResetEvent manualResetEvent, ConcurrentDictionary<uint?, FixedSizedQueue<SharpLogMessage>> queues)
        {
            this.ManualResetEvent = manualResetEvent;
            this.Queues = queues;
            this.Pid = null;
            Queue = new FixedSizedQueue<SharpLogMessage>(MAX_MESSAGES);
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            Console.WriteLine("Accepting a new connection...");
            Socket listener = (Socket)ar.AsyncState;
            this.handler = listener.EndAccept(ar);
            ManualResetEvent.Set();
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(this.ReadCallback), state);
        }

        private void ReadCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            try
            {
                int read = handler.EndReceive(ar);
                if (read > 0)
                {
                    RawLogMessage msg = Helper.ByteArrayToStructure<RawLogMessage>(state.buffer);

                    if (msg.Type == MessageType.CONNECTION_MESSAGE)
                    {
                        this.Pid = msg.ConnectionMessage.Pid;

                        Console.WriteLine(String.Format("Accepted a new connection from PID {0}.", this.Pid));
                        while (!this.Queues.ContainsKey(this.Pid))
                        {
                            this.Queues.TryAdd(this.Pid, this.Queue);
                        }


                        if (msg.ConnectionMessage.Version > VERSION)
                        {
                            Console.WriteLine(String.Format("Error: Client using a newer verison: {0}."), msg.ConnectionMessage.Version);
                            this.Dispose();
                        }
                    }

                    if (this.Pid == null && msg.Type != MessageType.CONNECTION_MESSAGE)
                    {
                        Console.WriteLine("Error: Initial CONNECTION_MESSAGE message was not received.");
                        this.Dispose();
                    }

                    if (msg.Type == MessageType.SIMPLE_MESSAGE ||
                        msg.Type == MessageType.LARGE_MESSAGE)
                    {
                        state.sharpLogMessage = new SharpLogMessage(
                            Helper.Unix2DateTime(msg.TextMessage.Timestamp),
                            msg.TextMessage.Severity,
                            msg.TextMessage.Module,
                            msg.TextMessage.Channel,
                            msg.TextMessage.Message
                            );
                    }

                    if (msg.Type == MessageType.CONTINUATION_MESSAGE)
                    {
                        state.sharpLogMessage.Message += msg.TextMessage.Message;
                    }

                    if (msg.Type == MessageType.CONTINUATION_END_MESSAGE)
                    {
                        state.sharpLogMessage.Message += msg.TextMessage.Message;
                    }


                    if (msg.Type == MessageType.SIMPLE_MESSAGE ||
                        msg.Type == MessageType.CONTINUATION_END_MESSAGE)
                    {
                        Queue.Enqueue(state.sharpLogMessage);
                    }

//                    Console.WriteLine(String.Format("Queue of PID {1} has {0} items.", Queue.Count, this.Pid));
                    //                    if (Queue.Count > 0)
                    //                    {
                    //                        Console.WriteLine(Queue.Last().ToString());
                    //                    }

                    state.buffer = new byte[StateObject.BufferSize];
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
                else
                {
                    this.Dispose();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Exception: {0}", e));
                this.Dispose();
            }

        }


        public void Dispose()
        {
            if (this.handler != null)
                this.handler.Close();

            if (this.Pid != null)
            {
                while (this.Queues.ContainsKey(this.Pid))
                {
                    FixedSizedQueue<SharpLogMessage> q;
                    this.Queues.TryRemove(this.Pid, out q);
                }
            }
        }

    }
}

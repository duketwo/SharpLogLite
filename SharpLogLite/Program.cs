using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SharpLogLite.Model;
using System.Threading;

/*
 * User: duketwo
 * Date: 10.11.2016
 * 
 */

namespace SharpLogLite
{
    class Program
    {
        static void Main(string[] args)
        {

            var model = new LogModel();
            new Thread(delegate () {
                model.StartListening();
            }).Start();

            while (true)
            {
                Console.ReadKey();
                foreach (var queue in model.Queues)
                {
                    foreach (var msg in queue.Value)
                    {
                        Console.WriteLine(msg.ToString());
                    }
                }
            }

        }
    }
}

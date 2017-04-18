using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EtwStream;

namespace EtwTail
{
    class Program
    {
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            var container = new SubscriptionContainer();

            ObservableEventListener.FromTraceEvent("DocumentDBClient")
                .Buffer(TimeSpan.FromSeconds(1), 1000, cts.Token)
                .LogTo(xs =>
                {
                    var d1 = xs.LogToFile("log.txt", x => $"[{DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")}][{x.Level}]{x.DumpPayload()}", Encoding.UTF8, autoFlush: true);
                    var d2 = xs.LogToConsole();
                    var d3 = xs.LogToFile("log.json", e => e.ToJson(), Encoding.UTF8, autoFlush:true);
                    return new[] { d1, d2, d3 };
                })
                .AddTo(container);

            Console.ReadLine();

            cts.Cancel();
            container.Dispose();

        }
        static void Main2(string[] args)
        {
            // in ApplicationStart, prepare two parts.
            var cts = new CancellationTokenSource();
            var container = new SubscriptionContainer();

            // configure log
            ObservableEventListener.FromTraceEvent("DocumentDBClient")
                .Buffer(TimeSpan.FromSeconds(1), 1000, cts.Token)
                .LogTo(xs =>
                {
                    var d1 = xs.LogToFile("log.txt", x => $"[{DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss")}][{x.Level}]{x.DumpPayload()}", Encoding.UTF8, autoFlush: true);
                    var d2 = xs.LogToConsole();
                    return new[] { d1, d2 };
                })
                .AddTo(container);

            // Application Running....
            Console.ReadLine();

            // End of Application(Form_Closed/Application_End/Main's last line/etc...)
            cts.Cancel();        // Cancel publish rest of buffered events.
            container.Dispose(); // Wait finish of subscriptions's buffer event.

        }
    }
}

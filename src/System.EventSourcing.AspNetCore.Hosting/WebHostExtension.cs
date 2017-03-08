using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.EventSourcing.AspNetCore.Hosting
{
    public static class WebHostExtension
    {
        public static void Run(this IEnumerable<IWebHost> hosts)
        {
            using (var cts = new CancellationTokenSource())
            {
                Action shutdown = () =>
                {
                    if (!cts.IsCancellationRequested)
                    {
                        Console.WriteLine("Application is shutting down...");
                        cts.Cancel();
                    }
                };

                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    shutdown();
                    // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                    eventArgs.Cancel = true;
                };

                var hostTasks = hosts.Select(
                    host =>
                    {
                        return Task.Run(
                            () =>
                            {
                                host.Run(cts.Token);
                            });
                    });

                Task.WaitAll(hostTasks.ToArray());
            }
        }
    }
}

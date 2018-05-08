using Microsoft.AspNetCore.Hosting;
using System.EventSourcing.AspNetCore.Hosting;

namespace CustomRequestFactory
{
    class Program
    {
        static void Main()
        {
            var host = new WebHostBuilder()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}
using System;
using System.Diagnostics;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Example.Download.Bootstrapper.Service.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"Serving from base dir: {Directory.GetCurrentDirectory()}");

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseUrls("http://0.0.0.0:5000")
                .Build();

            host.Run();
        }
    }
}

using System;
using System.Threading;

namespace Bootstrapper.Service
{
    class Program
    {
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        [STAThread]
        static void Main(string[] args)
        {
            string startupPath = @"C:\temp\ConsoleApplication2.exe";

            var watcher = new ServiceFolder()
                .ExecutablesChanged(startupPath);

            IDisposable app = null;

            watcher.Subscribe(path =>
            {
                Console.WriteLine("Received update, unloading exising application if any.");
                app?.Dispose();

                Console.WriteLine($"Loading new application from '{path}'");
                app = ShadowDomainApplication.StartApplication(path);
            },
            completed => app?.Dispose());

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}

using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Bootstrapper.Service
{
    public class ShadowDomainApplication : IDisposable
    {
        private readonly CancellationTokenSource _cts;

        private ShadowDomainApplication(string path, ILogger startupLogger)
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(() =>
            {
                try
                {
                    CreateNewDomainAndLoadApplication(path, startupLogger, token);
                }
                catch (Exception ex)
                {
                    startupLogger.Error(ex);
                }
            }, _cts.Token);
        }

        private static void CreateNewDomainAndLoadApplication(string path, ILogger startupLogger, CancellationToken token)
        {
            AppDomainSetup setup = new AppDomainSetup
            {
                ApplicationName = "CustomServiceLoader.LoadedAssembly",
                ShadowCopyFiles = "true"
            };

            AppDomain domain = AppDomain.CreateDomain(
                "CustomServiceLoader.LoadedAssembly",
                AppDomain.CurrentDomain.Evidence,
                setup);

            using (token.Register(() => AppDomain.Unload(domain)))
            {
                if (!File.Exists(path))
                {
                    startupLogger.Error($"Configured startup file '{path}' doesnt exist, nothing to start.");
                    return;
                }

                domain.ExecuteAssembly(path);
            }
        }

        public static IDisposable StartApplication(string path, ILogger startupLogger)
        {
            return new ShadowDomainApplication(path, startupLogger);
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
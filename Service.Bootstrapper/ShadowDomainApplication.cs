using System;
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
                    AppDomainSetup setup = new AppDomainSetup
                    {
                        ApplicationName = "CustomServiceLoader.LoadedAssembly",
                        ShadowCopyFiles = "true"
                    };

                    AppDomain domain = AppDomain.CreateDomain(
                        "CustomServiceLoader.LoadedAssembly",
                        AppDomain.CurrentDomain.Evidence,
                        setup);

                    using (token.Register(
                        () => AppDomain.Unload(domain)))
                    {
                        domain.ExecuteAssembly(path);
                    }
                }
                catch (Exception ex)
                {
                    startupLogger.Error(ex);
                }
            }, _cts.Token);
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
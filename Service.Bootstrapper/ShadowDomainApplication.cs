using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bootstrapper.Service
{
    public class ShadowDomainApplication : IDisposable
    {
        private readonly CancellationTokenSource _cts;

        private ShadowDomainApplication(string path)
        {
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            Task.Run(() =>
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
            }, _cts.Token);
        }

        public static IDisposable StartApplication(string path)
        {
            return new ShadowDomainApplication(path);
        }

        public void Dispose()
        {
            _cts.Dispose();
        }
    }
}
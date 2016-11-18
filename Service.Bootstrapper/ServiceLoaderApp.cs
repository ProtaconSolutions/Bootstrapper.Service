using System;
using NLog;

namespace Bootstrapper.Service
{
    public class ServiceLoaderApp : IDisposable
    {
        private IDisposable _app;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly IObservable<string> _watcher;

        public ServiceLoaderApp(Configuration configuration, ILogger logger)
        {
            _watcher = new ServiceFolder(configuration)
                .ExecutablesChanged();

            _app = null;

            _watcher.Subscribe(path =>
                {
                    logger.Info("Unloading exising application if any.");
                    _app?.Dispose();

                    logger.Info($"Loading new application from '{path}'");
                    _app = ShadowDomainApplication.StartApplication(path, startupLogger: logger);
                },
                onError: logger.Error,
                onCompleted: () => _app?.Dispose());
        }

        public void Dispose()
        {
            _app?.Dispose();
        }
    }
}
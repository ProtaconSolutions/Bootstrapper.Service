using System;
using Bootstrapper.Service.Core.Updaters;
using NLog;

namespace Bootstrapper.Service.Core
{
    public class BootstrapperService
    {
        private ServiceLoaderApp _currentApp;
        private IDisposable _updaterApp;
        private readonly Configuration _configuration;
        private readonly ILogger _logger;

        public BootstrapperService(Configuration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void Start()
        {
            _logger.Info("Service starting.");
            _currentApp?.Dispose();
            _updaterApp?.Dispose();
            _currentApp = new ServiceLoaderApp(_configuration, _logger);
            _updaterApp = new ServiceUpdateFactory(_configuration, _logger).Create();
        }

        public void Stop()
        {
            _logger.Info("Service stopping.");
            _currentApp?.Dispose();
            _updaterApp?.Dispose();
        }

        public void ShutDown()
        {
            _logger.Info("Service shutting down.");
            _currentApp?.Dispose();
            _updaterApp?.Dispose();
        }
    }
}
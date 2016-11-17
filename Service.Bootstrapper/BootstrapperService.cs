using System;
using Bootstrapper.Service.Updaters;
using NLog;

namespace Bootstrapper.Service
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
            _currentApp?.Dispose();
            _updaterApp?.Dispose();
            _currentApp = new ServiceLoaderApp(_configuration, _logger);
            _updaterApp = new ServiceUpdaterAppLocalFolder(_configuration, _logger);
        }

        public void Stop()
        {
            _currentApp?.Dispose();
            _updaterApp?.Dispose();
        }

        public void ShutDown()
        {
            _currentApp?.Dispose();
            _updaterApp?.Dispose();
        }
    }
}
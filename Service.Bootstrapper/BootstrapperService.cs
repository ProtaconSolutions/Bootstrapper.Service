using NLog;

namespace Bootstrapper.Service
{
    public class BootstrapperService
    {
        private ServiceLoaderApp _currentApp;
        private ServiceUpdaterApp _updaterApp;
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
            _updaterApp = new ServiceUpdaterApp();
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
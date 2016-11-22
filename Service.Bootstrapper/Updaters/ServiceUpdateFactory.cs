using System;
using NLog;

namespace Bootstrapper.Service.Updaters
{
    public class ServiceUpdateFactory
    {
        private readonly Configuration _configuration;
        private readonly ILogger _logger;

        public ServiceUpdateFactory(Configuration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public IDisposable Create()
        {
            if (_configuration.RemoteServicePackageFile.StartsWith("http"))
            {
                _logger.Debug(
                    $"{_configuration.RemoteServicePackageFile} starts with 'http', using {nameof(ServiceUpdaterAppWeb)}");

                return new ServiceUpdaterAppWeb(_configuration, _logger);
            }

            _logger.Debug(
                    $"{_configuration.RemoteServicePackageFile}, using {nameof(ServiceUpdaterAppLocalFolder)}");

            return new ServiceUpdaterAppLocalFolder(_configuration, _logger);
        }
    }
}

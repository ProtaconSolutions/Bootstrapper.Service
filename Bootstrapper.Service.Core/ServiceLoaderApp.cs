﻿using System;
using NLog;

namespace Bootstrapper.Service.Core
{
    public class ServiceLoaderApp : IDisposable
    {
        private IDisposable _app;
        private readonly IDisposable _subscribe;

        public ServiceLoaderApp(Configuration configuration, ILogger logger)
        {
            var watcher = new ServiceFolder(configuration)
                .ExecutablesChanged();

            _subscribe = watcher.Subscribe(path =>
                {
                    logger.Info("Unloading exising application if any.");
                    _app?.Dispose();

                    logger.Info($"Loading new application from '{path}'");
                    _app = new ProcessContainerApplication(path, logger, configuration);
                },
                onError: logger.Error,
                onCompleted: () => _app?.Dispose());
        }

        public void Dispose()
        {
            _subscribe?.Dispose();
            _app?.Dispose();
        }
    }
}
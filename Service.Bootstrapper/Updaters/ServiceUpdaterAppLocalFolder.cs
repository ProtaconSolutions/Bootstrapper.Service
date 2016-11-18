using System;
using System.IO;
using System.Reactive.Linq;
using NLog;
using System.IO.Compression;
using Bootstrapper.Service.Util;

namespace Bootstrapper.Service.Updaters
{
    public class ServiceUpdaterAppLocalFolder : IDisposable
    {
        private readonly ILogger _logger;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly IObservable<long> _interval;
        private readonly IDisposable _routine;

        public ServiceUpdaterAppLocalFolder(Configuration configuration, ILogger logger)
        {
            _logger = logger;

            _interval = Observable.Interval(TimeSpan.FromSeconds(30));

            _routine = _interval.Subscribe(_ =>
            {
                try
                {
                    UpdateServiceBinariesRoutine(configuration);
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            });
        }

        private void UpdateServiceBinariesRoutine(Configuration configuration)
        {
            var currentSum = Utilities.CalculateMd5(configuration.CurrentServicePackageFile);
            var remoteSum = Utilities.CalculateMd5(configuration.RemoteServicePackageFile);

            _logger.Debug(
                $"Checking for updates, local '{configuration.CurrentServicePackageFile}[{currentSum}]' against '{configuration.RemoteServicePackageFile}[{remoteSum}]'");

            if (currentSum == remoteSum)
                return;

            if (File.Exists(configuration.CurrentServicePackageFile))
                File.Delete(configuration.CurrentServicePackageFile);

            _logger.Info(
                $"New version found, replacing '{configuration.RemoteServicePackageFile}'->'{configuration.CurrentServicePackageFile}'");
            File.Copy(configuration.RemoteServicePackageFile, configuration.CurrentServicePackageFile);

            _logger.Info($"Extracting package to '{configuration.ServicePath}'");

            new DirectoryInfo(configuration.ServicePath).Empty();
            ZipFile.ExtractToDirectory(configuration.CurrentServicePackageFile, configuration.ServicePath);
        }

        public void Dispose()
        {
            _routine.Dispose();
        }
    }
}
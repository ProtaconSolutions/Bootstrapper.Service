using System;
using System.IO;
using System.IO.Compression;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Bootstapper.Service.Core.Util;
using NLog;

namespace Bootstapper.Service.Core.Updaters
{
    public class ServiceUpdaterAppLocalFolder : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IDisposable _routine;

        public ServiceUpdaterAppLocalFolder(Configuration configuration, ILogger logger)
        {
            _logger = logger;

            _routine = Observable
                .Interval(TimeSpan.FromSeconds(30))
                .ObserveOn(Scheduler.Default)
                .Subscribe(_ =>
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

            _logger.Info($"Extracting package to '{configuration.ServiceBinPath}'");

            new DirectoryInfo(configuration.ServiceBinPath).Empty();
            ZipFile.ExtractToDirectory(configuration.CurrentServicePackageFile, configuration.ServiceBinPath);
        }

        public void Dispose()
        {
            _routine.Dispose();
        }
    }
}
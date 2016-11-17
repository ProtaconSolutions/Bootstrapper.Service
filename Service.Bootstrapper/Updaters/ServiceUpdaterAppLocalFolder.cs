using System;
using System.IO;
using System.Reactive.Linq;
using System.Security.Cryptography;
using NLog;
using System.IO.Compression;

namespace Bootstrapper.Service.Updaters
{
    public class ServiceUpdaterAppLocalFolder : IDisposable
    {
        private readonly Configuration _configuration;
        private readonly ILogger _logger;
        private IObservable<long> _interval;
        private IDisposable _routine;

        public ServiceUpdaterAppLocalFolder(Configuration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;

            _interval = Observable.Interval(TimeSpan.FromSeconds(30));

            _routine = _interval.Subscribe(_ =>
            {
                var currentSum = CalculateMd5(configuration.CurrentServicePackageFile);
                var remoteSum = CalculateMd5(configuration.RemoteServicePackageFile);

                _logger.Info($"Checking for updates, local '{configuration.CurrentServicePackageFile}[{currentSum}]' against '{configuration.RemoteServicePackageFile}[{remoteSum}]'");

                if (currentSum == remoteSum)
                    return;

                if(File.Exists(configuration.CurrentServicePackageFile))
                    File.Delete(configuration.CurrentServicePackageFile);

                _logger.Info($"New version found, replacing '{configuration.RemoteServicePackageFile}'->'{configuration.CurrentServicePackageFile}'");
                File.Copy(configuration.RemoteServicePackageFile, configuration.CurrentServicePackageFile);

                _logger.Info($"Extracting package to '{configuration.ServicePath}'");
                ZipFile.ExtractToDirectory(configuration.CurrentServicePackageFile, configuration.ServicePath);
            });
        }

        public void Dispose()
        {
            _routine.Dispose();
        }

        private string CalculateMd5(string file)
        {
            if (!File.Exists(file))
                return "";

            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                   return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "‌​").ToLower();
                }
            }
        }
    }
}
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive.Linq;
using System.Security.Cryptography;
using Bootstrapper.Service.Util;
using NLog;
using RestSharp;
using RestSharp.Extensions;

namespace Bootstrapper.Service.Updaters
{
    public class ServiceUpdaterAppWeb : IDisposable
    {
        private readonly ILogger _logger;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly IObservable<long> _interval;

        private readonly IDisposable _routine;

        public ServiceUpdaterAppWeb(Configuration configuration, ILogger logger)
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
            if (!configuration.RemoteServicePackageFile.StartsWith("http"))
                throw new ArgumentException(nameof(configuration.RemoteServicePackageFile));

            var currentSum = Utilities.CalculateMd5(configuration.CurrentServicePackageFile);
            var remoteSum = GetMd5FromHeaders(configuration) ?? GetMd5FromPackage(configuration) ?? "NOT_FOUND";

            _logger.Debug(
                $"Checking for updates, local '{configuration.CurrentServicePackageFile}[{currentSum}]' against '{configuration.RemoteServicePackageFile}[{remoteSum}]'");

            if (currentSum == remoteSum)
                return;

            if (File.Exists(configuration.CurrentServicePackageFile))
                File.Delete(configuration.CurrentServicePackageFile);

            _logger.Info(
                $"New version found, replacing '{configuration.RemoteServicePackageFile}'->'{configuration.CurrentServicePackageFile}'");

            DownloadNewPackage(configuration);

            _logger.Info($"Extracting package to '{configuration.ServicePath}'");

            new DirectoryInfo(configuration.ServicePath).Empty();
            ZipFile.ExtractToDirectory(configuration.CurrentServicePackageFile, configuration.ServicePath);
        }

        private void DownloadNewPackage(Configuration configuration)
        {
            var client = new RestClient(configuration.RemoteServicePackageFile);
            client.DownloadData(new RestRequest("")).SaveAs(configuration.CurrentServicePackageFile);
        }

        private string GetMd5FromHeaders(Configuration configuration)
        {
            var response = new RestClient(configuration.RemoteServicePackageFile)
                .Execute(new RestRequest("", Method.HEAD));

            if (response.Headers.Any(x => x.Name == "md5"))
                return response.Headers.Single(x => x.Name == "md5").Value.ToString();

            return null;
        }

        private string GetMd5FromPackage(Configuration configuration)
        {
            return null;
        }

        public void Dispose()
        {
            _routine.Dispose();
        }
    }
}
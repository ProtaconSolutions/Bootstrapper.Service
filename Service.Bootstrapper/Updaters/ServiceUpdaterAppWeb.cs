using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Bootstrapper.Service.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using RestSharp;
using RestSharp.Extensions;

namespace Bootstrapper.Service.Updaters
{
    public class ServiceUpdaterAppWeb : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IDisposable _routine;

        public ServiceUpdaterAppWeb(Configuration configuration, ILogger logger)
        {
            _logger = logger;
            _routine = Observable.Interval(TimeSpan.FromSeconds(30)).ObserveOn(Scheduler.Default).Subscribe(_ =>
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

            var currentVersionIdentifier = GetLocalVersionIdentifier(configuration) ?? "NO_CURRENT";
            var remoteVersionIdentifier = GetRemoteVersionIdentifier(configuration) ?? "NO_REMOTE";

            _logger.Debug(
                $"Checking for updates, local '{configuration.CurrentServicePackageFile}[{currentVersionIdentifier}]' against '{configuration.RemoteServicePackageFile}[{remoteVersionIdentifier}]'");

            if (currentVersionIdentifier == remoteVersionIdentifier)
                return;

            if (File.Exists(configuration.CurrentServicePackageFile))
                File.Delete(configuration.CurrentServicePackageFile);

            _logger.Info(
                $"New version found, replacing '{configuration.RemoteServicePackageFile}'->'{configuration.CurrentServicePackageFile}'");

            DownloadNewPackage(configuration);

            _logger.Info($"Extracting package to '{configuration.ServicePath}'");

            new DirectoryInfo(configuration.ServicePath).Empty();
            ZipFile.ExtractToDirectory(configuration.CurrentServicePackageFile, configuration.ServicePath);

            WriteCurrentVersionMeta(configuration, remoteVersionIdentifier);
        }

        private void WriteCurrentVersionMeta(Configuration configuration, string versionIdentifier)
        {
            File.WriteAllText(configuration.WebUpdaterMetaFile, JsonConvert.SerializeObject(new
            {
                CurrentVersionIdentifier = versionIdentifier
            }));
        }

        private void DownloadNewPackage(Configuration configuration)
        {
            var client = new RestClient(configuration.RemoteServicePackageFile);
            client.DownloadData(new RestRequest("")).SaveAs(configuration.CurrentServicePackageFile);
        }

        private string GetRemoteVersionIdentifier(Configuration configuration)
        {
            var response = new RestClient(configuration.RemoteServicePackageFile)
                .Execute(new RestRequest("", Method.HEAD));

            return $"{response.Headers.Single(x => x.Name == "Content-Length").Value}_{response.Headers.Single(x => x.Name == "Last-Modified").Value}";
        }

        private string GetLocalVersionIdentifier(Configuration configuration)
        {
            if (!File.Exists(configuration.WebUpdaterMetaFile))
                return "";

            dynamic meta = JObject.Parse(File.ReadAllText(configuration.WebUpdaterMetaFile));

            return meta.CurrentVersionIdentifier ?? "";
        }

        public void Dispose()
        {
            _routine.Dispose();
        }
    }
}
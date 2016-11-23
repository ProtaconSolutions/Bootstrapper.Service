using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Bootstrapper.Service.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Optional;
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
            if (!configuration.RemoteServicePackageFile.StartsWith("http"))
                throw new ArgumentException(nameof(configuration.RemoteServicePackageFile));

            var currentVersionIdentifier = GetLocalVersionIdentifier(configuration) ?? "NO_CURRENT";

            GetRemoteVersionIdentifier(configuration).Match(some: remoteVersion =>
            {
                _logger.Debug(
                    $"Checking for updates, local '{configuration.CurrentServicePackageFile}[{currentVersionIdentifier}]' against '{configuration.RemoteServicePackageFile}[{remoteVersion}]'");

                if (currentVersionIdentifier == remoteVersion)
                    return;

                if (File.Exists(configuration.CurrentServicePackageFile))
                    File.Delete(configuration.CurrentServicePackageFile);

                _logger.Info(
                    $"New version found, replacing '{configuration.RemoteServicePackageFile}'->'{configuration.CurrentServicePackageFile}'");

                DownloadNewPackage(configuration);

                _logger.Info($"Extracting package to '{configuration.ServiceBinPath}'");

                new DirectoryInfo(configuration.ServiceBinPath).Empty();
                //ZipFile.ExtractToDirectory(configuration.CurrentServicePackageFile, configuration.ServiceBinPath);

                WriteCurrentVersionMeta(configuration, remoteVersion);
            },
            none: e => _logger.Error(e));
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

        private Option<string, string> GetRemoteVersionIdentifier(Configuration configuration)
        {
            var response = new RestClient(configuration.RemoteServicePackageFile)
                .Execute(new RestRequest("", Method.HEAD));

            if (response.StatusCode != HttpStatusCode.OK)
                return Option
                    .None<string, string>($"Cannot locate remote package '{configuration.RemoteServicePackageFile}', return code '{response.StatusCode}'");

            return $"{response.Headers.Single(x => x.Name == "Content-Length").Value}_{response.Headers.Single(x => x.Name == "Last-Modified").Value}"
                .Some<string, string>();
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
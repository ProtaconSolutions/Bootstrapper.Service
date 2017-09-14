using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Bootstrapper.Service.Core.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Optional;
using RestSharp;
using RestSharp.Extensions;

namespace Bootstrapper.Service.Core.Updaters
{
    public class ServiceUpdaterAppWeb : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IDisposable _routine;

        public ServiceUpdaterAppWeb(Configuration configuration, ILogger logger)
        {
            _logger = logger;
            _routine = Observable
                .Interval(TimeSpan.FromSeconds(configuration.UpdaterInterval))
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

        public void Dispose()
        {
            _routine.Dispose();
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
                    $"New version found {remoteVersion}, replacing '{configuration.RemoteServicePackageFile}'->'{configuration.CurrentServicePackageFile}'");

                DownloadNewPackage(configuration, remoteVersion);

                _logger.Info($"Extracting package to '{configuration.ServiceBinPath}'");

                new DirectoryInfo(configuration.ServiceBinPath).Empty();
                ZipFile.ExtractToDirectory(configuration.CurrentServicePackageFile, configuration.ServiceBinPath);

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

        private void DownloadNewPackage(Configuration configuration, string version)
        {
            var client = new RestClient(configuration.RemoteServicePackageFile);
            client.DownloadData(GetRequest(version, Method.GET, configuration)).SaveAs(configuration.CurrentServicePackageFile);
        }

        private Option<string, string> GetRemoteVersionIdentifier(Configuration configuration)
        {
            var response = new RestClient(configuration.RemoteServicePackageFile)
                .Execute(GetRequest("", Method.GET, configuration));

            if (response.StatusCode != HttpStatusCode.OK)
                return Option
                    .None<string, string>($"Cannot locate remote package '{configuration.RemoteServicePackageFile}', return code '{response.StatusCode}'");

            return response.Content
                .Some<string, string>();
        }

        private string GetLocalVersionIdentifier(Configuration configuration)
        {
            if (!File.Exists(configuration.WebUpdaterMetaFile))
                return "";

            dynamic meta = JObject.Parse(File.ReadAllText(configuration.WebUpdaterMetaFile));

            return meta.CurrentVersionIdentifier ?? "";
        }

        private static RestRequest GetRequest(string resource, Method method, Configuration configuration)
        {
            var request = new RestRequest(resource, method);

            foreach (var header in configuration.RemoteServiceHeaders)
            {
                request.AddHeader(header.Key, header.Value);
            }

            return request;
        }
    }
}
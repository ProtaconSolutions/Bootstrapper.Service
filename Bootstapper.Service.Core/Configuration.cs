using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using Bootstrapper.Service.Util;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Optional;
using RestSharp;

namespace Bootstrapper.Service
{
    public class Configuration
    {
        public string ServiceBinPath { get; }
        public string CurrentServicePackageFile { get; }
        public string RemoteServicePackageFile { get; }
        public string StartupFile { get; }
        public string BootstrapperLogPath { get; }
        public string TempPath { get; }

        private Configuration()
        {
            var startupLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).NotNull();

            var configFile = ReadConfigFile();

            ServiceBinPath = Path.Combine(startupLocation, "serviceBin");

            StartupFile = Path.Combine(ServiceBinPath, configFile.GetSection("StartupFile").Value.NotNull());

            if (!Directory.Exists(ServiceBinPath))
                Directory.CreateDirectory(ServiceBinPath);

            BootstrapperLogPath = Path.Combine(startupLocation, "log") + @"\bootstrapper_service.log";

            var servicePackageFolder = Path.Combine(startupLocation, "servicePackage");

            if (!Directory.Exists(servicePackageFolder))
                Directory.CreateDirectory(servicePackageFolder);

            CurrentServicePackageFile = servicePackageFolder + @"\\current.zip";

            RemoteServicePackageFile = configFile.GetSection("RemoteServicePackageFile").Value.NotNull();

            WebUpdaterMetaFile = servicePackageFolder + @"\\web.updater.meta.json";

            TempPath = Path.Combine(startupLocation, "temp");

            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);
        }

        private static IConfigurationRoot ReadConfigFile()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();
        }

        public string WebUpdaterMetaFile { get; private set; }

        public static Option<Configuration, Exception> Create()
        {
            try
            {
                return Option.Some<Configuration, Exception>(new Configuration());
            }
            catch(Exception ex)
            {
                return Option.None<Configuration, Exception>(ex);
            } 
        }
    }
}

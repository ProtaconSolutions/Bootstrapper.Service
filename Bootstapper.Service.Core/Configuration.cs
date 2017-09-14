using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bootstapper.Service.Core.Util;
using Microsoft.Extensions.Configuration;
using Optional;

namespace Bootstapper.Service.Core
{
    public class Configuration
    {
        public string ServiceBinPath { get; }
        public string CurrentServicePackageFile { get; }
        public string RemoteServicePackageFile { get; }
        public string StartupFile { get; }
        public string StartupFileArguments { get; }
        public string BootstrapperLogPath { get; }
        public string TempPath { get; }
        public string WebUpdaterMetaFile { get; }
        public Dictionary<string, string> RemoteServiceHeaders { get; }

        private Configuration()
        {
            var startupLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).NotNull();

            var configFile = ReadConfigFile(startupLocation);

            ServiceBinPath = Path.Combine(startupLocation, "serviceBin");

            StartupFile = Path.Combine(ServiceBinPath, configFile.GetSection("StartupFile").Value.NotNull());
            StartupFileArguments = configFile.GetSection("StartupFileArguments").Value;

            if (!string.IsNullOrEmpty(StartupFileArguments))
            {
                StartupFileArguments = StartupFileArguments.Replace("{startupLocation}", startupLocation);
            }

            if (!Directory.Exists(ServiceBinPath))
                Directory.CreateDirectory(ServiceBinPath);

            BootstrapperLogPath = Path.Combine(startupLocation, "log") + @"\bootstrapper_service.log";

            var servicePackageFolder = Path.Combine(startupLocation, "servicePackage");

            if (!Directory.Exists(servicePackageFolder))
                Directory.CreateDirectory(servicePackageFolder);

            CurrentServicePackageFile = servicePackageFolder + @"\\current.zip";

            RemoteServicePackageFile = configFile.GetSection("RemoteServicePackageFile").Value.NotNull();
            RemoteServiceHeaders = configFile.GetSection("RemoteServiceHeaders").GetChildren()
                .ToDictionary(m => m.Key, m => m.Value);

            WebUpdaterMetaFile = servicePackageFolder + @"\\web.updater.meta.json";

            TempPath = Path.Combine(startupLocation, "temp");

            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);
        }

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

        private static IConfigurationRoot ReadConfigFile(string basePath)
        {
            return new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("config.json")
                .Build();
        }
    }
}

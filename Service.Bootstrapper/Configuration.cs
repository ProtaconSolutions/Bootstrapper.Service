using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using Optional;

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
            ServiceBinPath = Path.Combine(Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location), "serviceBin");

            if (string.IsNullOrEmpty(Properties.Settings.Default.StartupFile))
                throw new InvalidOperationException($"{nameof(Properties.Settings.Default.StartupFile)} configuration is missing from config file.");

            StartupFile = Path.Combine(ServiceBinPath, Properties.Settings.Default.StartupFile);

            if (!Directory.Exists(ServiceBinPath))
                Directory.CreateDirectory(ServiceBinPath);

            BootstrapperLogPath = Path.Combine(Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location), "log") + @"\bootstrapper_service.log";

            var servicePackageFolder = Path.Combine(Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location), "servicePackage");

            if (!Directory.Exists(servicePackageFolder))
                Directory.CreateDirectory(servicePackageFolder);

            CurrentServicePackageFile = servicePackageFolder + @"\\current.zip";

            if (string.IsNullOrEmpty(Properties.Settings.Default.RemoteServicePackageFile))
                throw new InvalidOperationException($"{nameof(Properties.Settings.Default.RemoteServicePackageFile)} configuration is missing from config file.");

            RemoteServicePackageFile = Properties.Settings.Default.RemoteServicePackageFile;

            WebUpdaterMetaFile = servicePackageFolder + @"\\web.updater.meta.json";

            TempPath = Path.Combine(Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location), "temp");

            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);
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

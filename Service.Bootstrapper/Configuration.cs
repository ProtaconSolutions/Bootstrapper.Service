using System;
using System.IO;
using System.Reflection;
using Optional;

namespace Bootstrapper.Service
{
    public class Configuration
    {
        public string ServicePath { get; }
        public string CurrentServicePackageFile { get; }
        public string RemoteServicePackageFile { get; }
        public string StartupFile { get; }
        public string BootstrapperLogPath { get; }

        private Configuration()
        {
            ServicePath = Path.Combine(Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location), "serviceBin");

            StartupFile = Path.Combine(ServicePath, "ConsoleApplication2.exe");

            if (!Directory.Exists(ServicePath))
                Directory.CreateDirectory(ServicePath);

            BootstrapperLogPath = Path.Combine(Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location), "log") + @"\bootstrapper_service.log";

            var servicePackageFolder = Path.Combine(Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location), "servicePackage");

            if (!Directory.Exists(servicePackageFolder))
                Directory.CreateDirectory(servicePackageFolder);

            CurrentServicePackageFile = servicePackageFolder + @"\\current.zip";
            RemoteServicePackageFile = @"C:\temp\new.zip";
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
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;
using Topshelf;

namespace Bootstrapper.Service
{
    class Program
    {
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        [STAThread]
        static void Main(string[] args)
        {
            Configuration.Create().Match(
                some: ConfigureService,
                none: error =>
                {
                    var assemblyFolder = Path.GetDirectoryName(
                        Assembly.GetExecutingAssembly().Location);
                    File.WriteAllText($"{assemblyFolder}\\configuration_failures.log", error.Message);
                });
        }

        private static void NlogConfiguration(Configuration config)
        {
            var loggerConfig = new LoggingConfiguration();

            var consoleTarget = new ColoredConsoleTarget();
            loggerConfig.AddTarget("console", new ColoredConsoleTarget());

            var fileTarget = new FileTarget()
            {
                FileName = config.BootstrapperLogPath
            };

            loggerConfig.AddTarget("file", fileTarget);

            loggerConfig.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, consoleTarget));
            loggerConfig.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, fileTarget));

            LogManager.ThrowExceptions = true;
            LogManager.Configuration = loggerConfig;
            LogManager.ThrowExceptions = true;
        }

        private static void ConfigureService(Configuration config)
        {
            NlogConfiguration(config);

            var logger = LogManager.GetLogger("Bootstrapper.Service");

            HostFactory.Run(serviceConfig =>
            {
                serviceConfig.UseNLog(logger.Factory);

                serviceConfig.SetServiceName("Bootstrapper.Service");

                serviceConfig.Service<BootstrapperService>(service =>
                {
                    service.ConstructUsing(() => new BootstrapperService(config, logger));
                    service.WhenStarted(x => x.Start());
                    service.WhenStopped(x => x.Stop());
                    service.WhenShutdown(x => x.ShutDown());
                });
            });
        }
    }
}

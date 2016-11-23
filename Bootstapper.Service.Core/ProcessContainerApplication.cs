using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Bootstrapper.Service
{
    public class ProcessContainerApplication : IDisposable
    {
        private readonly ILogger _startupLogger;
        private readonly string _currentProcessFolder;
        private readonly Process _process;

        public ProcessContainerApplication(string startupFile, ILogger startupLogger, Configuration config)
        {
            _startupLogger = startupLogger;

            _currentProcessFolder = config.TempPath + @"\" + Guid.NewGuid();

            if (!File.Exists(startupFile))
            {
                startupLogger.Error($"Configured startup file '{startupFile}' doesnt exist, nothing to start.");
                return;
            }

            if (!Directory.Exists(_currentProcessFolder))
            {
                Directory.CreateDirectory(_currentProcessFolder);
            }

            var startupFolder = Path.GetDirectoryName(startupFile);

            if (startupFolder == null)
                throw new InvalidOperationException($"Cannot resolve directory from startup startupFile '{startupFile}'");

            CopyFilesRecursively(new DirectoryInfo(startupFolder), new DirectoryInfo(_currentProcessFolder));

            var shadowExePath = _currentProcessFolder + "\\" + Path.GetFileName(startupFile);

            startupLogger.Debug($"Starting executable from '{shadowExePath}'.");

            _process = Process.Start(shadowExePath);
        }

        private static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }

        public void Dispose()
        {
            Task.Run(() =>
            {
                if (_process == null)
                    return;

                _startupLogger.Debug($"Killing application from '{_currentProcessFolder}'");
                _process.Kill();

                Thread.Sleep(10*1000);
                _startupLogger.Debug($"Deleting old data from '{_currentProcessFolder}'");
                Directory.Delete(_currentProcessFolder, true);
            });
        }
    }
}

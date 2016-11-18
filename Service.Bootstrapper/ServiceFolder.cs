using System;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Bootstrapper.Service
{
    public class ServiceFolder : IDisposable
    {
        private readonly Configuration _configuration;
        private FileSystemWatcher _watcher;
        private readonly BehaviorSubject<string> _observable;

        public ServiceFolder(Configuration configuration)
        {
            _configuration = configuration;
            _observable = new BehaviorSubject<string>(_configuration.StartupFile);
        }

        public IObservable<string> ExecutablesChanged()
        {
            _watcher = new FileSystemWatcher
            {
                Path = _configuration.ServicePath,
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            _watcher.Changed += (_, fileInfo) =>
            {
                if (!(fileInfo.FullPath.EndsWith(".exe") || fileInfo.FullPath.EndsWith(".dll") || fileInfo.FullPath.EndsWith(".config")))
                    return;

                _observable.OnNext(_configuration.StartupFile);
            };

            return _observable
                .ObserveOn(Scheduler.Default)
                .Throttle(TimeSpan.FromSeconds(20));
        }

        public void Dispose()
        {
            _watcher.Dispose();
            _observable.Dispose();
        }
    }
}
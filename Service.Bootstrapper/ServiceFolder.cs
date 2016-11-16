using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Service.Bootstrapper
{
    public class ServiceFolder : IDisposable
    {
        private FileSystemWatcher _watcher;
        private BehaviorSubject<string> _observable;

        public IObservable<string> ExecutablesChanged(string path)
        {
            _observable = new BehaviorSubject<string>(path);

            _watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(path),
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true
            };

            _watcher.Changed += (_, fileInfo) =>
            {
                if (!(fileInfo.FullPath.EndsWith(".exe") || fileInfo.FullPath.EndsWith(".dll") || fileInfo.FullPath.EndsWith(".config")))
                    return;

                _observable.OnNext(path);
            };

            return _observable
                .Throttle(TimeSpan.FromSeconds(10));
        }

        public void Dispose()
        {
            _watcher.Dispose();
            _observable.Dispose();
        }
    }
}
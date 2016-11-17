using System;

namespace Bootstrapper.Service
{
    public class ServiceUpdaterAppWeb : IDisposable
    {
        private IDisposable _app;

        public ServiceUpdaterAppWeb()
        {
            _app = null;
        }

        public void Dispose()
        {
            _app?.Dispose();
        }
    }
}
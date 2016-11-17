using System;

namespace Bootstrapper.Service
{
    public class ServiceUpdaterApp : IDisposable
    {
        private IDisposable _app;

        public ServiceUpdaterApp()
        {
            _app = null;
        }

        public void Dispose()
        {
            _app?.Dispose();
        }
    }
}
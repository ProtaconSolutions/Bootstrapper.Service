using System;
using System.Reactive.Subjects;

namespace Bootstrapper.Service
{
    public class UpdateSniffer
    {
        public IObservable<PackageSourceInformation> SniffSniff()
        {
            return new Subject<PackageSourceInformation>();
        }
    }
}
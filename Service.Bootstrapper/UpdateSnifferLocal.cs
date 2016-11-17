using System;
using System.Reactive.Subjects;

namespace Bootstrapper.Service
{
    public class UpdateSnifferLocal
    {
        public IObservable<PackageSourceInformation> SniffSniff()
        {
            return new Subject<PackageSourceInformation>();
        }
    }
}
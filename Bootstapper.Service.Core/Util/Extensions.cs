using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Bootstrapper.Service.Util
{
    public static class Extensions
    {
        public static void Empty(this System.IO.DirectoryInfo directory)
        {
            foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            foreach (System.IO.DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }

        [Pure]
        public static T NotNull<T>(this T target, 
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (target == null)
                throw new InvalidOperationException($"{sourceFilePath}:{memberName}:{sourceLineNumber} null not allowed.");

            return target;
        }
    }
}

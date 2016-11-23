using System;
using System.IO;
using System.Security.Cryptography;

namespace Bootstapper.Service.Core.Util
{
    public static class Utilities
    {
        public static string CalculateMd5(string file)
        {
            if (!File.Exists(file))
                return "";

            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "‌​").ToLower();
                }
            }
        }

        public static DateTime LastModified(string file)
        {
            if (!File.Exists(file))
                return DateTime.MinValue;

            return File.GetCreationTimeUtc(file);
        }

        public static long FileLength(string file)
        {
            if (!File.Exists(file))
                return 0;

            return new FileInfo(file).Length;
        }
    }
}

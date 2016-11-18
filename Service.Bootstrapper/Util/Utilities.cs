using System;
using System.IO;
using System.Security.Cryptography;

namespace Bootstrapper.Service.Util
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
    }
}

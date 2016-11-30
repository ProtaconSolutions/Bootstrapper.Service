using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Example.Download.Bootstrapper.Service.Web.Controllers
{
    public class FileController : Controller
    {
        [HttpGet("/bootstrapperservice/")]
        public FileResult Get([FromQuery]string startupFile, [FromQuery]string remoteServicePackageFile)
        {
            string zipFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Package", "current.zip");

            using (var zipFileToModify = new FileStream(zipFilePath, FileMode.Open))
            using (var archive = new ZipArchive(zipFileToModify, ZipArchiveMode.Update))
            {
                var configEntry = archive.Entries.Single(x => x.Name == "config.json");

                JObject currentConfig;

                using (var file = configEntry.Open())
                using (StreamReader reader = new StreamReader(file))
                {
                    currentConfig = JObject.Parse(reader.ReadToEnd());
                }

                currentConfig["StartupFile"] = startupFile;
                currentConfig["RemoteServicePackageFile"] = remoteServicePackageFile;

                configEntry.Delete();

                var newEntry = archive.CreateEntry("config.json");

                using (var writer = new StreamWriter(newEntry.Open()))
                    writer.Write(JsonConvert.SerializeObject(currentConfig));
            }

            return File(System.IO.File.ReadAllBytes(zipFilePath), "application/octet-stream", "bootstrapperservice.zip");
        }
    }
}

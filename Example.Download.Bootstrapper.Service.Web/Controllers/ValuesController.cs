using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Example.Download.Bootstrapper.Service.Web.Controllers
{
    public class ValuesController : Controller
    {
        [HttpGet("/bootstrapperservice/")]
        public object Get(string configuredTargetFile)
        {
            return new {};
        }
    }
}

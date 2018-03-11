using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BlobUploaderNET.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (String.IsNullOrEmpty(HttpContext.Application["StorageConnString"] as string))
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                if (!System.IO.File.Exists($"{path}/credentials.json"))
                {
                    //string jsonstring = System.IO.File.ReadAllText($"{path}/credentials.json");
                    //dynamic dynObj = JsonConvert.DeserializeObject(jsonstring);
                    //string conn = dynObj.AzureStorageConnString;
                    return RedirectToAction("ConfigUploadCreds", "BlobUploader");
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult WipeConfig()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            if (System.IO.File.Exists($"{path}/credentials.json"))
            {
                System.IO.File.Delete($"{path}/credentials.json");
                HttpContext.Application["StorageConnString"] = "";
                ViewBag.Message = "Configuration Successfully Wiped";
            }

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
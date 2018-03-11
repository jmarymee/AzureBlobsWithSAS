using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace BlobUploaderNET
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //"DefaultEndpointsProtocol=https;AccountName=jdmsparktest;AccountKey=Jv7s93MYpjrNe64ohoe9OfZfyPQdRmKKmqtgwbT918ct0YCE5UW2yopGYS7RWo9f+3o35iQBMExgIEuCGFNKtg==;EndpointSuffix=core.windows.net"

            //string dirName = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;

            //string jsoncConfig = System.IO.File.ReadAllText("credentials.json");
            //string jsoncConfig = System.IO.Directory.GetCurrentDirectory();

            string path = AppDomain.CurrentDomain.BaseDirectory;
            string conn = "";

            if (File.Exists($"{path}/credentials.json"))
            {
                try
                {
                    string jsonstring = File.ReadAllText($"{path}/credentials.json");
                    dynamic dynObj = JsonConvert.DeserializeObject(jsonstring);
                    conn = dynObj.AzureStorageConnString;
                }
                catch
                {
                    conn = "";
                }
            }


            Application["StorageConnString"] = conn;
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}

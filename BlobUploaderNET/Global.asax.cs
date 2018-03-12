using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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

            Thread blobCleanerThread = new Thread(CleanerCode);
            blobCleanerThread.IsBackground = true;
            blobCleanerThread.Start();
            Application["CleanerThread"] = blobCleanerThread;
        }

        protected void Application_End()
        {
            Thread blobCleaner = (Thread)Application["CleanerThread"];
            if (blobCleaner !=null && blobCleaner.IsAlive)
            {
                blobCleaner.Abort();
            }

        }

        public void CleanerCode()
        {
            //Here we can look at blob metadata and clean based on that. Thread can run on a timer perhaps every 15 minutes?
        }
    }
}

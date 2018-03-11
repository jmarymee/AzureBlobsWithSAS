using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure;
using BlobUploaderNET.Models;
using System.IO;
using Newtonsoft.Json;

namespace BlobUploaderNET.Controllers
{
    public class BlobUploaderController : Controller
    {

        /// <summary>
        /// This method is used to upload a single blob to a container
        /// Depends upon: You'll need to pass in a generated SAS url that grants write access to a container
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult UploadBlobToContainer()
        {
            return View();
        }

        /// <summary>
        /// THis is the method that uploads. SUccessful upload will result in a simple success message
        /// </summary>
        /// <param name="postedFile">This is the selected file for upload</param>
        /// <param name="SASString">This is the generated SAS string (in URL format)</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadBlobToContainer(BlobUploadModel uploadModel)
        {
            try
            {
                CloudBlobContainer container = GetCloudBlobContainer(uploadModel.containerName);
                string containerSAS = GetContainerSasUri(container);
                UploadToContainerUsingSAS(containerSAS, uploadModel.File);
                ViewBag.Message = $"{uploadModel.File.FileName} Has been uploaded";
            }
            catch(Exception exp)
            {
                ViewBag.Message = $"There was an error in upload. Error message was {exp.Message}";
            }
            return View();
        }

        /// <summary>
        /// Obtains a URL with a SAS that can be passed to consumers for blob upload. No username/password required and can be (and should be) life-limited at creation.
        /// NOTE: This action requires that the private/priamry storage key is accessible to this app. 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult GenContainerSAS()
        {
            return View();
        }

        /// <summary>
        /// This method posts the generated URI + SAS to the rendered web response. The user must copy it in order to use it for upload.
        /// </summary>
        /// <param name="sasModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GenContainerSAS(Models.GenSASModel sasModel)
        {
            CloudBlobContainer container = GetCloudBlobContainer(sasModel.containerName);

            string containerSAS = GetContainerSasUri(container);

            ViewBag.Message = containerSAS;

            return View();
        }

        [HttpGet]
        public ActionResult SASBlobDownload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SASBlobDownload(Models.BlobDownloadModel sasModel)
        {
            CloudBlobContainer container = GetCloudBlobContainer(sasModel.containerName);
            CloudBlockBlob blob = container.GetBlockBlobReference(sasModel.blobName);
            sasModel = GetBlobSasUri(blob, sasModel);
            sasModel.URI = new Uri(blob.Uri + sasModel.generatedSAS);

            ViewBag.Message = sasModel.URI;

            return View();
        }

        // GET: Home

        [HttpGet]
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
                    return RedirectToAction("ConfigUploadCreds");
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult ConfigUploadCreds()
        {
            ViewData.Clear();
            return View();
        }

        [HttpPost]
        public ActionResult ConfigUploadCreds(ConfigCredsModel model)
        {
            try
            {
                HttpContext.Application["StorageConnString"] = model.primaryStorageKey;

                dynamic dynObj = new { AzureStorageConnString = model.primaryStorageKey };
                string jsonConfig = JsonConvert.SerializeObject(dynObj);

                string path = AppDomain.CurrentDomain.BaseDirectory;
                System.IO.File.WriteAllText($"{path}/credentials.json", jsonConfig);
                ViewBag.Message = "Configuration file written correctly";
            }
            catch
            {
                ViewBag.Message = "Error! Configuration file not written correctly";
            }

            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase postedFile)
        {
            return View();
        }

        private CloudBlobContainer GetCloudBlobContainer(string conName)
        {
            string appConnString = (HttpContext.Application["StorageConnString"] as string);

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(appConnString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(conName);
            container.CreateIfNotExists();

            return container;
        }

        static BlobDownloadModel GetBlobSasUri(CloudBlockBlob blob, Models.BlobDownloadModel model)
        {
            //Set the expiry time and permissions for the blob.
            //In this case, the start time is specified as a few minutes in the past, to mitigate clock skew.
            //The shared access signature will be valid immediately.
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(model.startTime);
            sasConstraints.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddHours(model.endTime);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read; //| SharedAccessBlobPermissions.Write;

            //Generate the shared access signature on the blob, setting the constraints directly on the signature.
            model.generatedSAS = blob.GetSharedAccessSignature(sasConstraints);

            //Return the URI string for the container, including the SAS token.
            //return blob.Uri + sasBlobToken;
            return model;
        }

        //static string GetBlobSasUri(CloudBlobContainer container)
        //{
        //    //Get a reference to a blob within the container.
        //    CloudBlockBlob blob = container.GetBlockBlobReference("sasblob.txt");

        //    //Upload text to the blob. If the blob does not yet exist, it will be created.
        //    //If the blob does exist, its existing content will be overwritten.
        //    string blobContent = "This blob will be accessible to clients via a shared access signature (SAS).";
        //    blob.UploadText(blobContent);

        //    //Set the expiry time and permissions for the blob.
        //    //In this case, the start time is specified as a few minutes in the past, to mitigate clock skew.
        //    //The shared access signature will be valid immediately.
        //    SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
        //    sasConstraints.SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5);
        //    sasConstraints.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddHours(24);
        //    sasConstraints.Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write;

        //    //Generate the shared access signature on the blob, setting the constraints directly on the signature.
        //    string sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);

        //    //Return the URI string for the container, including the SAS token.
        //    return blob.Uri + sasBlobToken;
        //}

        static string GetContainerSasUri(CloudBlobContainer container)
        {
            //Set the expiry time and permissions for the container.
            //In this case no start time is specified, so the shared access signature becomes valid immediately.
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddHours(24);
            sasConstraints.Permissions = SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Write;

            //Generate the shared access signature on the container, setting the constraints directly on the signature.
            string sasContainerToken = container.GetSharedAccessSignature(sasConstraints);

            //Return the URI string for the container, including the SAS token.
            return container.Uri + sasContainerToken;
        }

        static void CreateSharedAccessPolicyAndAddToContainer(CloudBlobClient blobClient, CloudBlobContainer container, string policyName)
        {
            //Get the container's existing permissions.
            BlobContainerPermissions permissions = container.GetPermissions();

            //Create a new shared access policy and define its constraints.
            SharedAccessBlobPolicy sharedPolicy = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddHours(24),
                Permissions = SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Read
            };

            //Add the new policy to the container's permissions, and set the container's permissions.
            permissions.SharedAccessPolicies.Add(policyName, sharedPolicy);
            container.SetPermissions(permissions);
        }

        static string GetContainerSasUriWithPolicy(CloudBlobContainer container, string policyName, SharedAccessBlobPolicy blobPolicy)
        {
            //Generate the shared access signature on the container. In this case, all of the constraints for the
            //shared access signature are specified on the stored access policy.
            string sasContainerToken = container.GetSharedAccessSignature(blobPolicy, policyName);

            //Return the URI string for the container, including the SAS token.
            return container.Uri + sasContainerToken;
        }

        /// <summary>
        /// This method uploads a bob to a container using a URL with an embedded SAS key
        /// </summary>
        /// <param name="cSAS"></param>
        /// <param name="postedFile"></param>
        private void UploadToContainerUsingSAS(string cSAS, HttpPostedFileBase postedFile)
        {
            CloudBlobContainer container = new CloudBlobContainer(new Uri(cSAS));

            string fileName = Path.GetFileName(postedFile.FileName);
            
            CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
            blob.UploadFromStream(postedFile.InputStream);
        }
    }
}
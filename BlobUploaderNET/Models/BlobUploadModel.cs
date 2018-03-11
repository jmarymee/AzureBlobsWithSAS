using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BlobUploaderNET.Models
{
    public class BlobUploadModel
    {
        [DisplayName("Container Name")]
        [Required(ErrorMessage = "You must specify a container. It will be created if it doesn't exist")]
        public string containerName { get; set; }

        [DisplayName("URL with Generated SAS")]
        [Required(ErrorMessage = "URL with SAS Required for upload")]
        public string urlSAS { get; set; }

        [Required (ErrorMessage ="File name needed for upload")]
        public HttpPostedFileBase File { get; set; }
    }
}
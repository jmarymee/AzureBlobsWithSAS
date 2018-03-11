using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BlobUploaderNET.Models
{
    public class GenSASModel
    {
        [DisplayName("Start time in minutes (integer)")]
        [Required(ErrorMessage = "You need to specify a time when this SAS becomes usable. -5 is a good option")]
        public int startTim { get; set; }

        [DisplayName("End time (in hours from now)")]
        [Required(ErrorMessage ="You should specify an end time. 24 hours from now is a good window")]
        public int endTime { get; set; }

        [ScaffoldColumn(false)]
        public string generatedSAS { get; set; }

        [ScaffoldColumn(false)]
        public Uri URI { get; set; }

        [DisplayName("Container Name")]
        [Required(ErrorMessage = "You must specify a container. It will be created if it doesn't exist")]
        public string containerName { get; set; }
    }
}
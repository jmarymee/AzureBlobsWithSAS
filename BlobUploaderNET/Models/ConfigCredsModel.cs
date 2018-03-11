using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BlobUploaderNET.Models
{
    public class ConfigCredsModel
    {
        [DisplayName("Primary Storage Key")]
        [Required(ErrorMessage ="You must supply a primary key for configuration")]
        public string primaryStorageKey { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointFileToBlob.Models
{
    public class BlobConfiguration
    {
        public string ContainerName { get; set; }
        public string StorageUri { get; set; }
    }
}

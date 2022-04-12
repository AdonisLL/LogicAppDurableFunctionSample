using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferToBlobFunction.Models
{
    public class BlobConfiguration
    {
        public string ContainerName { get; set; }
        public string StorageUri { get; set; }
    }
}

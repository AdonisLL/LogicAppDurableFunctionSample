using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferToBlobFunction.Models
{
    public  class MessageData
    {
        public Guid Id { get; set; }
        public string FileUrl { get; set; }
        public DateTime Submitted { get; set; }
        public string RequestStatusURL { get; set; }

    }
}

using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharePointFileToBlob.Interfaces
{
    public interface IGraphApiClient
    {
        GraphServiceClient GetGraphApiClient();
    }
}

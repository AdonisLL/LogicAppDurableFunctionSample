using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransferToBlobFunction.Interfaces
{
    public interface IGraphApiClient
    {
        GraphServiceClient GetGraphApiClient();
    }
}

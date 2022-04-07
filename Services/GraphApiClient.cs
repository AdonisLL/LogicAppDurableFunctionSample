using Azure.Identity;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using SharePointFileToBlob.Interfaces;
using SharePointFileToBlob.Models;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SharePointFileToBlob.Services
{
    public class GraphApiClient : IGraphApiClient
    {

        public GraphApiClient()
        {
        }


        public GraphServiceClient GetGraphApiClient()
        {

            var credential = new DefaultAzureCredential();
            var graphServiceClient = new GraphServiceClient(credential);

            return graphServiceClient;
        }
    }
}
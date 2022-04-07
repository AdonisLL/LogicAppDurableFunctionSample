using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.OpenApi.Models;
using SharePointFileToBlob.Interfaces;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SharePointFileToBlob
{
    public class SharePointToBlobFunction
    {
        private readonly ILogger<SharePointToBlobFunction> _logger;
        private IGraphApiClient _graphApiClient;
        private IBlobService _blobService;


        public SharePointToBlobFunction(ILogger<SharePointToBlobFunction> log, IGraphApiClient graphApiClient, IBlobService blobService)
        {
            _logger = log;
            _graphApiClient = graphApiClient;
            _blobService = blobService;
        }

        [FunctionName("Orchestration_Function")]
        public static async Task<IActionResult> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context
            )
        {
            try
            {
                FileData fileInfo = context.GetInput<FileData>();
                return await context.CallActivityAsync<IActionResult>("Work_Function", fileInfo);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        [FunctionName("Work_Function")]
        public async Task Work_Function([ActivityTrigger] FileData fileInfo)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var output = await getFile(fileInfo.fileUrl);
            //await spfile.WriteAsync(output, 0, output.Length);
            await _blobService.UploadBlobFromStream(fileInfo.fileName, output);

        }

        [FunctionName("SharePointFileToBlobFunction")]
        [OpenApiOperation(operationId: "Run", tags: new[] { "name" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "code", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "fileUrl", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **fileUrl** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {

            // Function input comes from the request content.
            var url = req.Query["fileUrl"];
            string instanceId = await starter.StartNewAsync("Orchestration_Function", new FileData() { fileName = GetFilenameFromUrl(url), fileUrl = url });

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
            var response = starter.CreateCheckStatusResponse(req, instanceId);

            return response;
        }

        public string GetFilenameFromUrl(string url)
        {
            return String.IsNullOrEmpty(url.Trim()) || !url.Contains('.') ?
                string.Empty : Path.GetFileName(new Uri(url).AbsolutePath);
        }

        public async Task<byte[]> getFile(string fileUrl)
        {
            fileUrl = fileUrl.Split('?')[0];
            GraphServiceClient _graphServiceClient =  _graphApiClient.GetGraphApiClient();
            var fileName = GetFilenameFromUrl(fileUrl);

            var sharedItemId = UrlToSharingToken(fileUrl);
            var requestUrl = $"{_graphServiceClient.BaseUrl}/shares/{sharedItemId}/driveitem/content";
            var message = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            await _graphServiceClient.AuthenticationProvider.AuthenticateRequestAsync(message);
            var response = await _graphServiceClient.HttpProvider.SendAsync(message);
            var bytesContent = await response.Content.ReadAsByteArrayAsync();
            return bytesContent;
        }

        private static string UrlToSharingToken(string inputUrl)
        {
            var base64Value = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(inputUrl));
            return "u!" + base64Value.TrimEnd('=').Replace('/', '_').Replace('+', '-');
        }
    }

    [Serializable]
    public class FileData
    {
        public string fileName { get; set; }
        public string fileUrl { get; set; }
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using TranslationsAPI.Models;
using static TranslationsAPI.Models.TranslationRequest;

namespace TranslationsAPI
{
    public class DocumentsTranslation
    {
        private readonly ILogger<DocumentsTranslation> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        private static readonly string _endpoint = "https://translation-123455.cognitiveservices.azure.com/";
        private static readonly string _location = "eastus";
        private static readonly string _apiVersion = "2024-05-01";
        private static readonly string _batchTranslateSuffix = "translator/document/batches";
        private static readonly string _translationStatusSuffix = "/translator/document/batches";
        private static readonly string _key = "";
        private static readonly string _sourceUrl = "";
        private static readonly string _targetUrl = "";

        public DocumentsTranslation(ILogger<DocumentsTranslation> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [Function("TranslateDocs")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "documents/translations")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var translationRequest = new TranslationRequest
            {
                Inputs = new List<Input>
                {
                    new Input
                    {
                        Source = new Source
                        {
                            SourceUrl = _sourceUrl,
                            Filter = new Filter
                            {
                                Prefix = "tempfolder", // WIN USER + timestamp
                                Suffix = ".txt"
                            }
                        },
                        Targets = new List<Target>
                        {
                            new Target
                            {
                                TargetUrl = _targetUrl,
                                Language = "bg"
                            }
                        }
                    }
                }
            };

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _key);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", _location);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            var json = JsonSerializer.Serialize(translationRequest, options);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_endpoint + _batchTranslateSuffix + "?api-version=" + _apiVersion, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var translationJob = JsonSerializer.Deserialize<TranslationResponse>(responseContent, options);

                if (response.Headers.TryGetValues("Operation-Location", out var operationLocationValues))
                {
                    translationJob.Location = operationLocationValues.FirstOrDefault()!;
                }

                //{
                //    "id": "ed6bef5a-b05f-4e28-b950-4a6642f1bb1e",
                //    "status": "NotStarted",
                //    "location": "https://translation-123455.cognitiveservices.azure.com/translator/document/batches/ed6bef5a-b05f-4e28-b950-4a6642f1bb1e?api-version=2024-05-01"
                //}

                return new OkObjectResult(translationJob);
            }
            else
            {
                _logger.LogError($"Request failed with status code: {response.StatusCode}");
                return new BadRequestObjectResult($"Request failed with status code: {response.StatusCode}");
            }
        }

        [Function("GetJobStatus")]
        public async Task<IActionResult> Run2(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "jobs/{jobId}")] HttpRequest req,
            string jobId)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _key);
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", _location);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
            var response = await client.GetAsync(_endpoint + _batchTranslateSuffix + "/" + jobId + "?api-version=" + _apiVersion);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var translationJob = JsonSerializer.Deserialize<TranslationJob>(responseContent, options);

                return new OkObjectResult(translationJob);
            }
            else
            {
                _logger.LogError($"Request failed with status code: {response.StatusCode}");
                return new BadRequestObjectResult($"Request failed with status code: {response.StatusCode}");
            }
        }
    }
}

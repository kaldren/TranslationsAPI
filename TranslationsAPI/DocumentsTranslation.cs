using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace TranslationsAPI
{
    public class DocumentsTranslation
    {
        private readonly ILogger<DocumentsTranslation> _logger;
        private static readonly string _endpoint = "https://translation-123455.cognitiveservices.azure.com/";
        private static readonly string _location = "eastus";
        private static readonly string _apiVersion = "2024-05-01";
        private static readonly string _batchTranslateSuffix = "translator/document/batches";
        private static readonly string _translationStatusSuffix = "/translator/document/batches";
        private static readonly string _key = "";
        private static readonly string _sourceUrl = "";
        private static readonly string _targetUrl = "";

        public DocumentsTranslation(ILogger<DocumentsTranslation> logger)
        {
            _logger = logger;
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

            using (HttpClient client = new HttpClient())
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(_endpoint + _batchTranslateSuffix + "?api-version=" + _apiVersion);
                request.Headers.Add("Ocp-Apim-Subscription-Key", _key);
                request.Headers.Add("Ocp-Apim-Subscription-Region", _location);

                var options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };
                var json = JsonSerializer.Serialize(translationRequest, options);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                request.Content = content;

                var res = await client.SendAsync(request);
                return new OkObjectResult("Operation completed successfully");
            }

        }

        [Function("GetJobStatus")]
        public async Task<IActionResult> Run2(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "jobs/{jobId}/status")] HttpRequest req,
            string jobId)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            using (HttpClient client = new HttpClient())
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(_endpoint + _translationStatusSuffix + $"/{jobId}" + "?api-version=" + _apiVersion);
                request.Headers.Add("Ocp-Apim-Subscription-Key", _key);

                HttpResponseMessage response = await client.SendAsync(request);
                string result = await response.Content.ReadAsStringAsync();

                _logger.LogInformation($"Status code: {response.StatusCode}");
                _logger.LogInformation($"Response Headers: {response.Headers}");
                _logger.LogInformation(result);

                return new OkObjectResult(result);
            }
        }
    }

    public class TranslationRequest
    {
        public List<Input> Inputs { get; set; }
    }

    public class Input
    {
        public Source Source { get; set; }
        public List<Target> Targets { get; set; }
        public string StorageType { get; set; }
    }

    public class Source
    {
        public string SourceUrl { get; set; }
        public Filter Filter { get; set; }
        public string Language { get; set; }
        public string StorageSource { get; set; }
    }

    public class Filter
    {
        public string Prefix { get; set; }
        public string Suffix { get; set; }
    }

    public class Target
    {
        public string TargetUrl { get; set; }
        public string Category { get; set; }
        public string Language { get; set; }
        public List<Glossary> Glossaries { get; set; }
        public string StorageSource { get; set; }
    }

    public class Glossary
    {
        public string GlossaryUrl { get; set; }
        public string Format { get; set; }
        public string Version { get; set; }
        public string StorageSource { get; set; }
    }

    public class TranslationJob
    {
        public Guid Id { get; set; }
        public DateTime CreatedDateTimeUtc { get; set; }
        public DateTime LastActionDateTimeUtc { get; set; }
        public string Status { get; set; }
        public Summary Summary { get; set; }
    }

    public class Summary
    {
        public int Total { get; set; }
        public int Failed { get; set; }
        public int Success { get; set; }
        public int InProgress { get; set; }
        public int NotYetStarted { get; set; }
        public int Cancelled { get; set; }
        public int TotalCharacterCharged { get; set; }
    }
}

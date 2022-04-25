using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Collector.Models;
using Collector.Models.Instant;
using Collector.Models.InstantClient;
using Newtonsoft.Json;
using Serilog;

namespace Collector
{
    public interface IInstantClient
    {
        Task<Result> SendAsync(string data, CancellationToken cancellationToken);
    }
    
    public class InstantClient:IInstantClient
    {
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly string _baseUrl;
        private readonly string _internalLocationId;
        private readonly ILogger _logger;

        public InstantClient(ConfigData config, ILogger logger)
        {
            _apiKey = config.InstantPaycardAPIKey;
            _apiUrl = config.InstantAPICensusFileUrl;
            _baseUrl = config.InstantAPIBaseUrl;
            _internalLocationId = config.InternalLocationId;
            _logger = logger;
        }
        
        public async Task<Result> SendAsync(string data, CancellationToken cancellationToken)
        {
            _logger.Information("JSON data for sending: {0}{1}", Environment.NewLine, data);
            
            using var httpClient = new HttpClient();

            httpClient.BaseAddress = new Uri(_baseUrl);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            
            using var httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            
            var putUrl = string.Format(_apiUrl, _internalLocationId);
            using var httpResponse = await httpClient.PutAsync(putUrl, httpContent, cancellationToken);

            string response = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            
            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorMessageResponse = JsonConvert.DeserializeObject<InstantPayCardResponseError>(response);

                var errorBySendingData = $"Process Error: Error by sending the data.ErrorName: {errorMessageResponse.Name}. Message: {errorMessageResponse.Message}";
                
                _logger.Error(errorBySendingData);
                return Result.Fail(errorBySendingData);
            }
            
            _logger.Information("Data was successfully send. Status code: {1:d} {1}.Response: {0}{2}", Environment.NewLine, httpResponse.StatusCode, response);
            return Result.Ok();
        }
    }
}
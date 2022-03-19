using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Collector.Models;
using Newtonsoft.Json;
using Serilog;

namespace Collector
{
    public interface IInstantClient
    {
        Task<Result<FileToSend>> SendAsync(FileToSend customFileInfo);
    }
    
    public class InstantClient:IInstantClient
    {
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly string _baseUrl;

        public InstantClient(ConfigData config)
        {
            _apiKey = config.InstantPaycardAPIKey;
            _apiUrl = config.InstantAPICensusFileUrl;
            _baseUrl = config.InstantAPIBaseUrl;
        }
        
        public async Task<Result<FileToSend>> SendAsync(FileToSend customFileInfo)
        {
            var dataToSend = customFileInfo.InstantPyCardExportData;
            using var httpClient = new HttpClient();

            httpClient.BaseAddress = new Uri(_baseUrl);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);

            var data = JsonConvert.SerializeObject(dataToSend);

            using var httpContent = new StringContent(data, Encoding.UTF8, "application/json");
            var putUrl = string.Format(_apiUrl, dataToSend.ExternalLocationId);
            using var httpResponse = await httpClient.PutAsync(putUrl, httpContent);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var result = await httpResponse.Content.ReadAsStringAsync();
                var errorMessageResponse = JsonConvert.DeserializeObject<InstantPayCardResponseError>(result);

                var errorBySendingData = string.Join(" ", new string[]
                {
                    $"Process Error: Error by sending the data from \"{customFileInfo.FilePath}\"",
                    $"ErrorName: {errorMessageResponse.Name}. Message: {errorMessageResponse.Message}"
                });
                
                Log.Error(errorBySendingData);
                return Result.Fail<FileToSend>(errorBySendingData);
            }

            return Result.Ok(customFileInfo);
        }
    }
}
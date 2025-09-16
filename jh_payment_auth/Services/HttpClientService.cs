using jh_payment_auth.Models;
using System.Text;
using System.Text.Json;

namespace jh_payment_auth.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class HttpClientService : IHttpClientService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<HttpClientService> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientFactory"></param>
        /// <param name="logger"></param>
        public HttpClientService(IHttpClientFactory clientFactory, ILogger<HttpClientService> logger)
        {
            _clientFactory = clientFactory;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _logger = logger;
        }

        /// <summary>
        /// Get
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<T?> GetAsync<T>(string url)
        {
            var content = string.Empty;

            try
            {
                var client = _clientFactory.CreateClient("PaymentDb-Microservice");
                var response = await client.GetAsync(url);

                response.EnsureSuccessStatusCode();
                content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<MicroserviceResponse<T>>(content, _jsonOptions);
                return result!.ResponseBody;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

            return await Task.FromResult(MicroserviceResponse<T>.GetDefault());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
        {
            try
            {
                var client = _clientFactory.CreateClient("PaymentDb-Microservice");
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<TResponse>(result, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

            return await Task.FromResult(MicroserviceResponse<TResponse>.GetDefault());
        }

        /// <summary>
        /// PUT
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest data)
        {
            try
            {
                var client = _clientFactory.CreateClient("PaymentDb-Microservice");
                var json = JsonSerializer.Serialize(data);
                var body = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(url, body);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<MicroserviceResponse<TResponse>>(content, _jsonOptions);
                if(result == null)
                {
                    throw new Exception("Fail to convert the response");
                }

                return await Task.FromResult(result.ResponseBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

            return await Task.FromResult(MicroserviceResponse<TResponse>.GetDefault());
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(string url)
        {
            try
            {
                var client = _clientFactory.CreateClient("PaymentDb-Microservice");
                var response = await client.DeleteAsync(url);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

            return await Task.FromResult(MicroserviceResponse<bool>.GetDefault());
        }
    }
}

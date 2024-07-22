using System.Text.Json;
using System.Text.Json.Serialization;

namespace ei_back.Infrastructure.Services.Client.Core
{
    public class ExternalApiWebClient
    {
        private readonly HttpClient _httpClient;

        public ExternalApiWebClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<T> Get<T>(string uri)
        {
            var response = await _httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var deserialized = JsonSerializer.Deserialize<T>(content);

            return deserialized;
        }
    }
}

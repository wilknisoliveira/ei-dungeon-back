using System.Text.Json;
using System.Text.Json.Serialization;

namespace ei_back.Infrastructure.Services.Client.Core
{
    public class ExternalApiWebClient(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        public async Task<T?> Get<T>(string uri)
        {
            var response = await _httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var deserialized = JsonSerializer.Deserialize<T>(content);

            return deserialized;
        }

        public async Task<T?> Post<T, A>(string uri, A data)
        {
            var response = await _httpClient.PostAsJsonAsync(uri, data);
            
            return await Deserialize<T>(response);
        }

        private static async Task<T?> Deserialize<T>(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var deserialized = JsonSerializer.Deserialize<T>(content);

            return deserialized;
        }
    }
}

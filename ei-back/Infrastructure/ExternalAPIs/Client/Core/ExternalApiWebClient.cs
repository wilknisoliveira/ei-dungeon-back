using System.Text.Json;

namespace ei_back.Infrastructure.ExternalAPIs.Client.Core
{
    public class ExternalApiWebClient(HttpClient httpClient)
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        public async Task<T?> Get<T>(string uri, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(uri, cancellationToken);

            return await Deserialize<T>(response, cancellationToken);
        }

        public async Task<T?> Post<T, A>(string uri, A data, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsJsonAsync(uri, data, cancellationToken);
            
            return await Deserialize<T>(response, cancellationToken);
        }

        private static async Task<T?> Deserialize<T>(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            response.EnsureSuccessStatusCode();
            var deserialized = JsonSerializer.Deserialize<T>(content);

            return deserialized;
        }
    }
}

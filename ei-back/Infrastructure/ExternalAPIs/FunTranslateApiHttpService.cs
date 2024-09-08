using ei_back.Infrastructure.ExternalAPIs.Client.FunTranslateApiClient;
using ei_back.Infrastructure.ExternalAPIs.Interfaces;

namespace ei_back.Infrastructure.ExternalAPIs
{
    public class FunTranslateApiHttpService : IFunTranslateApiHttpService
    {
        private readonly FunTranslateApiClient _externalApiClient;

        public FunTranslateApiHttpService(HttpClient httpClient)
        {
            _externalApiClient = new FunTranslateApiClient(httpClient);
        }

        public async Task<string> GetValyrianTranslate(string request, CancellationToken cancellationToken)
        {
            var response = await _externalApiClient.GetValyrianTranslate(request, cancellationToken);
            return response.contents.translated;
        }
    }
}

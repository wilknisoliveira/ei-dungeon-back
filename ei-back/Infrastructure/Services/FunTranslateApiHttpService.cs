using ei_back.Infrastructure.Services.Client.FunTranslateApiClient;
using ei_back.Infrastructure.Services.Interfaces;

namespace ei_back.Infrastructure.Services
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

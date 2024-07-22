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

        public async Task<string> GetValyrianTranslate(string request)
        {
            var response = await _externalApiClient.GetValyrianTranslate(request);
            return response.contents.translated;
        }
    }
}

using ei_back.Infrastructure.Services.Client.Core;
using ei_back.Infrastructure.Services.Dtos;

namespace ei_back.Infrastructure.Services.Client.FunTranslateApiClient
{
    public class FunTranslateApiClient : ExternalApiWebClient, IFunTranslateApiClient
    {
        public FunTranslateApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<FunTranslate> GetValyrianTranslate(string request)
        {
            return await Get<FunTranslate>($"https://api.funtranslations.com/translate/valyrian.json?text={request}");
        }
    }
}

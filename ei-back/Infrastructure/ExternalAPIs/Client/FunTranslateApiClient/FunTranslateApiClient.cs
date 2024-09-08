using ei_back.Infrastructure.ExternalAPIs.Client.Core;
using ei_back.Infrastructure.ExternalAPIs.Dtos.Response;

namespace ei_back.Infrastructure.ExternalAPIs.Client.FunTranslateApiClient
{
    public class FunTranslateApiClient : ExternalApiWebClient, IFunTranslateApiClient
    {
        public FunTranslateApiClient(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<FunTranslate> GetValyrianTranslate(string request, CancellationToken cancellationToken)
        {
            return await Get<FunTranslate>($"https://api.funtranslations.com/translate/valyrian.json?text={request}", cancellationToken);
        }
    }
}

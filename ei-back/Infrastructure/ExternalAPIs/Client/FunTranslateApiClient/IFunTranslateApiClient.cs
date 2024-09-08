using ei_back.Infrastructure.ExternalAPIs.Dtos.Response;

namespace ei_back.Infrastructure.ExternalAPIs.Client.FunTranslateApiClient
{
    public interface IFunTranslateApiClient
    {
        Task<FunTranslate> GetValyrianTranslate(string request, CancellationToken cancellationToken);
    }
}

using ei_back.Infrastructure.Services.Dtos;

namespace ei_back.Infrastructure.Services.Client.FunTranslateApiClient
{
    public interface IFunTranslateApiClient
    {
        Task<FunTranslate> GetValyrianTranslate(string request);
    }
}

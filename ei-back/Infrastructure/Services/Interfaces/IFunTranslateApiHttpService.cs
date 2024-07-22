namespace ei_back.Infrastructure.Services.Interfaces
{
    public interface IFunTranslateApiHttpService
    {
        Task<string> GetValyrianTranslate(string request);
    }
}

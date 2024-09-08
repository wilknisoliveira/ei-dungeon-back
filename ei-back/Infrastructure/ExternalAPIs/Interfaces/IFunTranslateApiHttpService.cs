namespace ei_back.Infrastructure.ExternalAPIs.Interfaces
{
    public interface IFunTranslateApiHttpService
    {
        Task<string> GetValyrianTranslate(string request, CancellationToken cancellationToken);
    }
}

namespace ei_back.Infrastructure.Services.Client.GenerativeAIApiClient
{
    public interface IGenerativeAIApiClient
    {
        Task<string> GetSimpleResponse(string prompt, CancellationToken cancellationToken);
    }
}

namespace ei_back.Infrastructure.Services.Interfaces
{
    public interface IGenerativeAIApiHttpService
    {
        Task<string> GenerateSimpleResponse(string prompt);
    }
}

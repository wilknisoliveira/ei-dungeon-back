using ei_back.Infrastructure.ExternalAPIs.Dtos.Request;

namespace ei_back.Infrastructure.ExternalAPIs.Client.GenerativeAIApiClient
{
    public interface IGenerativeAIApiClient
    {
        Task<string> GetSimpleResponse(string prompt, CancellationToken cancellationToken, double temperature = 0.5);
        Task<string> GetResponseWithRoleBase(List<IAiPromptRequest> prompts, CancellationToken cancellationToken);
        Task<string> GetStructureJsonResponse(List<IAiPromptRequest> prompts, List<string> fields, CancellationToken cancellationToken, double temperature = 0.5);
    }
}

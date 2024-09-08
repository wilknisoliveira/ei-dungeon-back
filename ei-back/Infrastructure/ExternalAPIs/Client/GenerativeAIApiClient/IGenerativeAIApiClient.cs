using ei_back.Infrastructure.ExternalAPIs.Dtos.Request;

namespace ei_back.Infrastructure.ExternalAPIs.Client.GenerativeAIApiClient
{
    public interface IGenerativeAIApiClient
    {
        Task<string> GetSimpleResponse(string prompt, CancellationToken cancellationToken);
        Task<string> GetResponseWithRoleBase(List<IAiPromptRequest> prompts, CancellationToken cancellationToken);
    }
}

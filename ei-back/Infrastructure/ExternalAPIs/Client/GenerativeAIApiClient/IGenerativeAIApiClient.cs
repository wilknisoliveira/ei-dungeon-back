using ei_back.Core.Application.Service.Prompt.Interfaces;

namespace ei_back.Infrastructure.ExternalAPIs.Client.GenerativeAIApiClient
{
    public interface IGenerativeAIApiClient
    {
        Task<string> GetSimpleResponse(string prompt, CancellationToken cancellationToken);
        Task<string> GetResponseWithRoleBase(List<IAiPrompt> prompts, CancellationToken cancellationToken);
    }
}

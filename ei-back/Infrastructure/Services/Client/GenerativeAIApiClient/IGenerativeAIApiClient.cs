using ei_back.Domain.Prompt.Interfaces;

namespace ei_back.Infrastructure.Services.Client.GenerativeAIApiClient
{
    public interface IGenerativeAIApiClient
    {
        Task<string> GetSimpleResponse(string prompt, CancellationToken cancellationToken);
        Task<string> GetResponseWithRoleBase(List<IAiPrompt> prompts, CancellationToken cancellationToken);
    }
}

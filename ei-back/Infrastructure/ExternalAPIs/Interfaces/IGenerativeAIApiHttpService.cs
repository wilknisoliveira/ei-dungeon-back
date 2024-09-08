using ei_back.Core.Application.Service.Prompt.Interfaces;

namespace ei_back.Infrastructure.ExternalAPIs.Interfaces
{
    public interface IGenerativeAIApiHttpService
    {
        Task<string> GenerateSimpleResponse(string prompt, CancellationToken cancellationToken);
        Task<string> GenerateResponseWithRoleBase(List<IAiPrompt> prompts, CancellationToken cancellationToken);
    }
}

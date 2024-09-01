using ei_back.Domain.Prompt.Interfaces;

namespace ei_back.Infrastructure.Services.Interfaces
{
    public interface IGenerativeAIApiHttpService
    {
        Task<string> GenerateSimpleResponse(string prompt, CancellationToken cancellationToken);
        Task<string> GenerateResponseWithRoleBase(List<IAiPrompt> prompts, CancellationToken cancellationToken);
    }
}

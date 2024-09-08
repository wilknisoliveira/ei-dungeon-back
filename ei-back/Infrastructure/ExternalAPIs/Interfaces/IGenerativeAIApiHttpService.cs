using ei_back.Infrastructure.ExternalAPIs.Dtos.Request;

namespace ei_back.Infrastructure.ExternalAPIs.Interfaces
{
    public interface IGenerativeAIApiHttpService
    {
        Task<string> GenerateSimpleResponse(string prompt, CancellationToken cancellationToken);
        Task<string> GenerateResponseWithRoleBase(List<IAiPromptRequest> prompts, CancellationToken cancellationToken);
    }
}

using ei_back.Infrastructure.ExternalAPIs.Dtos.Request;

namespace ei_back.Infrastructure.ExternalAPIs.Interfaces
{
    public interface IGenerativeAIApiHttpService
    {
        Task<string> GenerateSimpleResponse(string prompt, CancellationToken cancellationToken, double temperature = 0.5);
        Task<string> GenerateResponseWithRoleBase(List<IAiPromptRequest> prompts, CancellationToken cancellationToken);
        Task<string> GenerateStructureJsonResponse(List<IAiPromptRequest> prompts, List<string> fields, CancellationToken cancellationToken, double temperature = 0.5);
    }
}

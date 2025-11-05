using ei_back.Core.Application.Interfaces;
using ei_back.Infrastructure.ExternalAPIs.Dtos.Request;
using AiPromptRequest = ei_back.Infrastructure.ExternalAPIs.Dtos.Request.AiPromptRequest;

namespace ei_back.Infrastructure.ExternalAPIs.Interfaces
{
    public interface IGenerativeAIApiHttpService
    {
        Task<string> GenerateSimpleResponse(string prompt, CancellationToken cancellationToken, double temperature = 0.5);
        Task<string> GenerateResponseWithRoleBase(List<AiPromptRequest> prompts, CancellationToken cancellationToken);
        Task<string> GenerateStructureJsonResponse(List<AiPromptRequest> prompts, List<string> fields, CancellationToken cancellationToken, double temperature = 0.5);
    }
}

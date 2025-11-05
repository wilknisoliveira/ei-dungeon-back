using ei_back.Core.Application.Interfaces;
using ei_back.Infrastructure.ExternalAPIs.Dtos.Request;
using AiPromptRequest = ei_back.Infrastructure.ExternalAPIs.Dtos.Request.AiPromptRequest;

namespace ei_back.Infrastructure.ExternalAPIs.Client.GenerativeAIApiClient
{
    public interface IGenerativeAIApiClient
    {
        Task<string> GetSimpleResponse(string prompt, CancellationToken cancellationToken, double temperature = 0.5);
        Task<string> GetResponseWithRoleBase(List<AiPromptRequest> prompts, CancellationToken cancellationToken);
        Task<string> GetStructureJsonResponse(List<AiPromptRequest> prompts, List<string> fields, CancellationToken cancellationToken, double temperature = 0.5);
    }
}

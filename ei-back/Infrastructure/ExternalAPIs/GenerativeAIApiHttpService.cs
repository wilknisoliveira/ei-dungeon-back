using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using ei_back.Infrastructure.ExternalAPIs.Client.GenerativeAIApiClient;
using ei_back.Infrastructure.ExternalAPIs.Dtos.Request;
using ei_back.Infrastructure.ExternalAPIs.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace ei_back.Infrastructure.ExternalAPIs
{
    public class GenerativeAIApiHttpService(IGenerativeAIApiClient generativeAIApiClient) : IGenerativeAIApiHttpService
    {
        private readonly IGenerativeAIApiClient _generativeAIApiClient = generativeAIApiClient;

        public async Task<string> GenerateResponseWithRoleBase(List<IAiPromptRequest> prompts, CancellationToken cancellationToken)
        {
            var response = await _generativeAIApiClient.GetResponseWithRoleBase(prompts, cancellationToken);

            if (response.IsNullOrEmpty())
                throw new NoContentException($"No content was returned by the gateway.");

            return response;
        }

        public async Task<string> GenerateSimpleResponse(string prompt, CancellationToken cancellationToken, double temperature = 0.5)
        {
            var response = await _generativeAIApiClient.GetSimpleResponse(prompt, cancellationToken, temperature);

            if (response.IsNullOrEmpty())
                throw new NoContentException($"No content was returned by the gateway.");

            return response;
        }

        public async Task<string> GenerateStructureJsonResponse(List<IAiPromptRequest> prompts, List<string> fields, CancellationToken cancellationToken, double temperature = 0.5)
        {
            var response = await _generativeAIApiClient.GetStructureJsonResponse(prompts, fields, cancellationToken, temperature);

            if (response.IsNullOrEmpty())
                throw new NoContentException($"No content was returned by the gateway.");

            return response;
        }
    }
}

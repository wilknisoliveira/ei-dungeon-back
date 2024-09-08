using ei_back.Core.Application.Service.Prompt.Interfaces;
using ei_back.Infrastructure.ExternalAPIs.Client.Core;
using ei_back.Infrastructure.ExternalAPIs.Dtos.Request;
using ei_back.Infrastructure.ExternalAPIs.Dtos.Response;
using Microsoft.IdentityModel.Tokens;

namespace ei_back.Infrastructure.ExternalAPIs.Client.GenerativeAIApiClient
{
    public class GeminiApiClient(HttpClient httpClient, IConfiguration configuration) : ExternalApiWebClient(httpClient), IGenerativeAIApiClient
    {
        private readonly string _host = configuration["Gateways:GeminiGenerativeApi"] ?? "";
        private readonly string _apiKey = configuration["keys:GeminiApiKey"] ?? "";

        public Task<string> GetResponseWithRoleBase(List<IAiPrompt> prompts, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetSimpleResponse(string prompt, CancellationToken cancellationToken)
        {
            if (_host.IsNullOrEmpty() || _apiKey.IsNullOrEmpty())
                throw new Exception("Verify if all the string connections was registered properly in the appsettings.");

            var requestBody = new GeminiGenerativeAiDtoRequest()
            {
                contents = new()
                {
                    parts = [new()
                        {
                            text = prompt
                        }
                    ]
                }
            };

            var response = await Post<GeminiGenerativeAiDtoResponse, GeminiGenerativeAiDtoRequest>($"{_host}{_apiKey}", requestBody, cancellationToken);

            return response?.candidates.FirstOrDefault()?.content.parts.FirstOrDefault()?.text ?? "";
        }
    }
}

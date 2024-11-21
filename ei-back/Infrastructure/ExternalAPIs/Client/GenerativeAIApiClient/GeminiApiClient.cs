using ei_back.Infrastructure.ExternalAPIs.Client.Core;
using ei_back.Infrastructure.ExternalAPIs.Dtos.Request;
using ei_back.Infrastructure.ExternalAPIs.Dtos.Response;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Extensions;
using System.ComponentModel;

namespace ei_back.Infrastructure.ExternalAPIs.Client.GenerativeAIApiClient
{
    public class GeminiApiClient : ExternalApiWebClient, IGenerativeAIApiClient
    {
        private readonly string _host;
        private readonly string _apiKey;

        public GeminiApiClient(HttpClient httpClient, IConfiguration configuration) : base(httpClient)
        {
            _host = configuration["Gateways:GeminiGenerativeApi"] ?? "";
            _apiKey = configuration["keys:GeminiApiKey"] ?? "";

            if (_host.IsNullOrEmpty() || _apiKey.IsNullOrEmpty())
                throw new Exception("Verify if all the string connections was registered properly in the appsettings.");
        }

        public async Task<string> GetResponseWithRoleBase(List<IAiPromptRequest> prompts, CancellationToken cancellationToken)
        {
            List<Content> contents = new();
            foreach (var prompt in prompts)
            {
                if (prompt.Role != PromptRole.Instruction.GetAttributeOfType<DescriptionAttribute>().Description)
                {
                    contents.Add(new Content()
                    {
                        role = prompt.Role,
                        parts = new List<Part>()
                        {
                            new Part()
                            {
                                text = prompt.Content,
                            }
                        }
                    });
                }
            }
            
            var requestBody = new GeminiGenerativeAiDtoRequest()
            {
                contents = contents
            };

            List<string> instructionsPrompt = prompts.Where(x => x.Role.Equals(PromptRole.Instruction.GetAttributeOfType<DescriptionAttribute>().Description)).Select(x => x.Content).ToList();

            if (!instructionsPrompt.IsNullOrEmpty())
            {
                var instructions = new SystemInstruction()
                {
                    parts = new Part()
                    {
                        text = string.Join(" ", instructionsPrompt),
                    }
                };

                requestBody.system_instruction = instructions;
            }


            return await Post(requestBody, cancellationToken);
        }

        public async Task<string> GetSimpleResponse(string prompt, CancellationToken cancellationToken)
        {
            var requestBody = new GeminiGenerativeAiDtoRequest()
            {
                contents = new List<Content>()
                {
                    new Content()
                    {
                        parts = [new Part()
                        {
                            text = prompt
                        }
                    ]
                    }
                    
                }
            };

            return await Post(requestBody, cancellationToken);
        }

        private async Task<string> Post(GeminiGenerativeAiDtoRequest requestBody, CancellationToken cancellationToken)
        {
            var response = await Post<GeminiGenerativeAiDtoResponse, GeminiGenerativeAiDtoRequest>($"{_host}{_apiKey}", requestBody, cancellationToken);

            return response?.candidates.FirstOrDefault()?.content.parts.FirstOrDefault()?.text ?? "";
        }
    }
}

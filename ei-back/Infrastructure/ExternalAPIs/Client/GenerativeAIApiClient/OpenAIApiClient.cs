using ei_back.Core.Application.Service.Prompt.Interfaces;
using ei_back.Infrastructure.ExternalAPIs.Client.Core;
using ei_back.Infrastructure.ExternalAPIs.Dtos.Request;
using ei_back.Infrastructure.ExternalAPIs.Dtos.Response;
using Microsoft.IdentityModel.Tokens;

namespace ei_back.Infrastructure.ExternalAPIs.Client.GenerativeAIApiClient
{
    public class OpenAIApiClient : ExternalApiWebClient, IGenerativeAIApiClient
    {
        private readonly string _host;
        private readonly string _apiToken;
        private readonly string _openAiModel;

        public OpenAIApiClient(HttpClient httpClient, IConfiguration configuration) : base(httpClient)
        {
            _host = configuration["Gateways:OpenAIApi"] ?? ""; ;
            _apiToken = configuration["keys:OpenAIApiToken"] ?? "";
            _openAiModel = configuration["OpenAiSettings:AiModel"] ?? "";

            if (_host.IsNullOrEmpty() || _apiToken.IsNullOrEmpty() || _openAiModel.IsNullOrEmpty())
                throw new Exception("Verify if all the string connections was registered properly in the appsettings.");

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers
                .AuthenticationHeaderValue("Bearer", _apiToken);
        }

        public async Task<string> GetResponseWithRoleBase(List<IAiPrompt> prompts, CancellationToken cancellationToken)
        {
            List<Message> messages = [];
            foreach (var prompt in prompts)
            {
                messages.Add(new()
                {
                    role = prompt.Role,
                    content = prompt.Content,
                });
            }

            var requestBody = new OpenAiDtoRequest()
            {
                model = _openAiModel,
                messages = messages,
                temperature = 0.7m
            };

            var response = await Post<OpenAiDtoResponse, OpenAiDtoRequest>(_host, requestBody, cancellationToken);

            return response?.choices?.FirstOrDefault()?.message?.content ?? "";
        }

        public async Task<string> GetSimpleResponse(string prompt, CancellationToken cancellationToken)
        {
            var requestBody = new OpenAiDtoRequest()
            {
                model = _openAiModel,
                messages = [
                    new(){
                        role = "user",
                        content = prompt,
                    }
                ],
                temperature = 0.7m
            };

            var response = await Post<OpenAiDtoResponse, OpenAiDtoRequest>(_host, requestBody, cancellationToken);

            return response?.choices?.FirstOrDefault()?.message?.content ?? "";
        }
    }
}

﻿using ei_back.Infrastructure.Services.Client.Core;
using ei_back.Infrastructure.Services.Dtos.Request;
using Microsoft.IdentityModel.Tokens;

namespace ei_back.Infrastructure.Services.Client.GenerativeAIApiClient
{
    public class GenerativeAIApiClient(HttpClient httpClient, IConfiguration configuration) : ExternalApiWebClient(httpClient), IGenerativeAIApiClient
    {
        private readonly string _host = configuration["Gateways:GeminiGenerativeApi"] ?? "";
        private readonly string _apiKey = configuration["keys:GeminiApiKey"] ?? "";

        public async Task<string> GetSimpleResponse(string prompt)
        {
            if (_host.IsNullOrEmpty() || _apiKey.IsNullOrEmpty())
                throw new Exception("Verify if all the string connections was registered properly in the appsettings.");

            var requestBody = new GeminiGenerativeAiRequest()
            {
                candidates = [new()
                    {
                        content = new()
                        {
                            parts = [new()
                                {
                                    text = prompt
                                }
                            ]
                        }
                    }
                ]
            };

            var response = await Post<GeminiGenerativeAiRequest, GeminiGenerativeAiRequest>($"{_host}{_apiKey}", requestBody);

            return response?.candidates.FirstOrDefault()?.content.parts.FirstOrDefault()?.text ?? "";
        }
    }
}

﻿using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using ei_back.Infrastructure.Services.Client.GenerativeAIApiClient;
using ei_back.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;

namespace ei_back.Infrastructure.Services
{
    public class GenerativeAIApiHttpService(IGenerativeAIApiClient generativeAIApiClient) : IGenerativeAIApiHttpService
    {
        private readonly IGenerativeAIApiClient _generativeAIApiClient = generativeAIApiClient;

        public async Task<string> GenerateSimpleResponse(string prompt)
        {
            var response = await _generativeAIApiClient.GetSimpleResponse(prompt);

            if (response.IsNullOrEmpty())
                throw new NoContentException($"No content was returned by the gateway.");

            return response;
        }
    }
}

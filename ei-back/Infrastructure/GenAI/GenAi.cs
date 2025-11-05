using ei_back.Core.Application.Interfaces;
using GeminiDotnet;
using GeminiDotnet.Extensions.AI;
using Microsoft.Extensions.AI;

namespace ei_back.Infrastructure.GenAI;

public class GenAi(GeminiClient genAiClient) : IGenAi
{
    private readonly IChatClient _genAiClient = genAiClient.AsChatClient();

    public async Task<string> GenFromOnePrompt(string prompt, CancellationToken cancellationToken)
    {
        var result = await _genAiClient.GetResponseAsync(prompt, cancellationToken: cancellationToken);

        return result.Text;
    }

    public async Task<string> GenFromMultiplePrompts(List<AiPromptRequest> prompts, CancellationToken cancellationToken)
    {
        var chatRoleDict = new Dictionary<AiRole, ChatRole>
        {
            [AiRole.System] = ChatRole.System,
            [AiRole.User] = ChatRole.User,
            [AiRole.Assistant] = ChatRole.Assistant,
        };
        
        var messages = prompts.Select(prompt => new ChatMessage(chatRoleDict[prompt.Role], prompt.Content)).ToList();
        
        var result = await _genAiClient.GetResponseAsync(messages, cancellationToken: cancellationToken);
        return result.Text;
    }
}
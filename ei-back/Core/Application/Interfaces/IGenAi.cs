namespace ei_back.Core.Application.Interfaces;

public interface IGenAi
{
    Task<string> GenFromOnePrompt(string prompt, CancellationToken cancellationToken);
    Task<string> GenFromMultiplePrompts(List<AiPromptRequest> prompts, CancellationToken cancellationToken);
    Task<string> GenFromMultiplePrompts(List<AiPromptRequest> prompts, int maxOutputTokens, CancellationToken cancellationToken);
    Task<string> GenFromMultiplePrompts<T>(List<AiPromptRequest> prompts, int maxOutputTokens, CancellationToken cancellationToken);
}
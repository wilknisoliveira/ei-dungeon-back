namespace ei_back.Core.Application.Interfaces;

public interface IGenAi
{
    Task<string> GenFromOnePrompt(string prompt, CancellationToken cancellationToken);
    Task<string> GenFromMultiplePrompts(List<AiPromptRequest> prompts, CancellationToken cancellationToken);
}
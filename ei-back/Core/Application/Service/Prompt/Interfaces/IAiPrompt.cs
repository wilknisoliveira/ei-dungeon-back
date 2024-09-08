namespace ei_back.Core.Application.Service.Prompt.Interfaces
{
    public interface IAiPrompt
    {
        string Role { get; set; }
        string Content { get; set; }
    }
}

namespace ei_back.Domain.Prompt.Interfaces
{
    public interface IAiPrompt
    {
        string Role { get; set; }
        string Content { get; set; }
    }
}

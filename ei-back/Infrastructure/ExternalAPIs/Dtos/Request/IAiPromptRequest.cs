namespace ei_back.Infrastructure.ExternalAPIs.Dtos.Request
{
    public interface IAiPromptRequest
    {
        string Role { get; set; }
        string Content { get; set; }
    }
}

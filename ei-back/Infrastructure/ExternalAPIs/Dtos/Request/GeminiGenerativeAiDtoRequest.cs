using ei_back.Infrastructure.ExternalAPIs.Dtos.Response;

namespace ei_back.Infrastructure.ExternalAPIs.Dtos.Request
{
    public record GeminiGenerativeAiDtoRequest
    {
        public SystemInstruction? system_instruction {  get; set; }
        public List<Content> contents { get; set; }
    }

    public record SystemInstruction
    {
        public Part parts { get; set; }
    }
}

using ei_back.Infrastructure.ExternalAPIs.Dtos.Response;

namespace ei_back.Infrastructure.ExternalAPIs.Dtos.Request
{
    public record GeminiGenerativeAiDtoRequest
    {
        public SystemInstruction? system_instruction {  get; set; }
        public List<Content> contents { get; set; }
        public GenerationConfig? generationConfig { get; set; }
    }

    public record SystemInstruction
    {
        public Part parts { get; set; }
    }

    public record GenerationConfig
    {
        public double temperature { get; set; }
        public string? response_mime_type { get; set; }
        public ResponseSchema? response_schema { get; set; }

    }
    public record ResponseSchema
    {
        public string type { get; set; }
        public Item items { get; set; }
    }

    public record Item
    {
        public string type { get; set; }
        public Dictionary<string, PropertyType> properties { get; set; }
    }

    public record PropertyType
    {
        public string type { get; set; }
    }

}

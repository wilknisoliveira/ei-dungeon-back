﻿namespace ei_back.Infrastructure.Services.Dtos.Request
{
    public record GeminiGenerativeAiRequest
    {
        public List<Candidate> candidates;
        public MetaData usageMetaData;
    }

    public record Candidate
    {
        public Content content {  get; set; }
        public string finishReason { get; set; }
        public int index { get; set; }
        public List<SafetyRating> safetyRatings { get; set; }
    }

    public record Content
    {
        public List<Part> parts {  get; set; } 
        public string role { get; set; }
    }

    public record Part
    {
        public string text { get; set; }
    }

    public record SafetyRating
    {
        public string category { get; set; }
        public string probability { get; set; }
    }

    public record MetaData
    {
        public int promptTokenCount { get; set; }
        public int candidatesTokenCount { get; set; }
        public int totalTokenCount { get; set; }
    }
}

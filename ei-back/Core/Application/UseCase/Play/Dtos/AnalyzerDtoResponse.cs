using System.Text.Json.Serialization;

namespace ei_back.Core.Application.UseCase.Play.Dtos;

public record AnalyzerDtoResponse(
    [property: JsonPropertyName("result")] AnalyzerResult Result,
    [property: JsonPropertyName("reason")] string Reason);

public enum AnalyzerResult
{
    Ok,
    InvalidPlay,
    RollDice,
}
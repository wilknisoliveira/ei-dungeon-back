using ei_back.Core.Domain.Enums;

namespace ei_back.Core.Application.UseCase.GameInfo.Dtos
{
    public record GameInfoDto
    {
        public required InfoType Type {  get; set; }
        public required string Value { get; set; }
    }
}

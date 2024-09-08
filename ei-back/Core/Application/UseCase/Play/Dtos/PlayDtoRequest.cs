namespace ei_back.Core.Application.UseCase.Play.Dtos
{
    public record PlayDtoRequest
    {
        public Guid GameId { get; set; }
        public string Prompt { get; set; }
    }
}

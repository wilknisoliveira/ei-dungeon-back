namespace ei_back.Application.Api.Play.Dtos
{
    public record PlayDtoRequest
    {
        public Guid GameId { get; set; }
        public string Prompt { get; set; }
    }
}

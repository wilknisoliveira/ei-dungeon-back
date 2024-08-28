using ei_back.Domain.Player;

namespace ei_back.Application.Api.Play.Dtos
{
    public record PlayDtoResponse
    {
        public Guid Id { get; set; }
        public PlayerDtoResponse PlayerDtoResponse { get; set; }
        public string Prompt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public record PlayerDtoResponse
    {
        public Guid Id { get; set; }
        public string name { get; set; }
        public PlayerType Type { get; set; }
    }
}

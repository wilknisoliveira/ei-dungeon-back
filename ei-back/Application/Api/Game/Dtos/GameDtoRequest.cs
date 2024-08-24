namespace ei_back.Application.Api.Game.Dtos
{
    public record GameDtoRequest
    {
        public required string Name { get; set; }
        public required string SystemGame { get; set; }
        public required int NumberOfArtificialPlayers { get; set; }
    }
}

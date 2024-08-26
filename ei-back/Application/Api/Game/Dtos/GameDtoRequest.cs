namespace ei_back.Application.Api.Game.Dtos
{
    public record GameDtoRequest
    {
        public required string SystemGame { get; set; }
        public required int NumberOfArtificialPlayers { get; set; }
        public required string CharacterName { get; set; }
        public required string CharacterDescription { get; set; }
        public required string Name { get; set; }
    }
}

namespace ei_back.Application.Api.Game.Dtos
{
    public record GameDtoResponse
    {
        public string Name { get; set; }
        public Guid OwnerUserId { get; set; }
        public string SystemGame {  get; set; }
    }
}

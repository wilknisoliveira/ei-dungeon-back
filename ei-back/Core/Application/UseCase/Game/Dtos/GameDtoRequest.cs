using System.ComponentModel.DataAnnotations;

namespace ei_back.Core.Application.UseCase.Game.Dtos
{
    public record GameDtoRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public required string SystemGame { get; set; }
        [Required]
        [Range(0, 5)]
        public required int NumberOfArtificialPlayers { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 4)]
        public required string CharacterName { get; set; }
        [Required]
        [StringLength(2000, MinimumLength = 4)]
        public required string CharacterDescription { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 2)]
        public required string Name { get; set; }
    }
}

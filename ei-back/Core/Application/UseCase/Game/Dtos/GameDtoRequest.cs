using System.ComponentModel.DataAnnotations;
using ei_back.Core.Domain.Enums;

namespace ei_back.Core.Application.UseCase.Game.Dtos
{
    public record GameDtoRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 4)]
        public required string CharacterName { get; set; }
        [Required]
        [StringLength(2000, MinimumLength = 4)]
        public required string CharacterDescription { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 2)]
        public required string Name { get; set; }
        [Required]
        public CharacterRace Race { get; set; }
        [Required]
        public SkillsDtoRequest Skills { get; set; }
        
    }

    public record SkillsDtoRequest
    {
        [Required]
        [MinLength(8)]
        [MaxLength(18)]
        public int Strength { get; set; }
        [Required]
        [MinLength(8)]
        [MaxLength(18)]
        public int Dexterity { get; set; }
        [Required]
        [MinLength(8)]
        [MaxLength(18)]
        public int Intelligence { get; set; }
        [Required]
        [MinLength(8)]
        [MaxLength(18)]
        public int Constitution { get; set; }
        [Required]
        [MinLength(8)]
        [MaxLength(18)]
        public int Charisma { get; set; }
        [Required]
        [MinLength(8)]
        [MaxLength(18)]
        public int Wisdom { get; set; }
    }
}

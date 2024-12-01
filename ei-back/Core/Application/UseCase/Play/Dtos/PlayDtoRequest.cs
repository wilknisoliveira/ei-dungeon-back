using System.ComponentModel.DataAnnotations;

namespace ei_back.Core.Application.UseCase.Play.Dtos
{
    public record PlayDtoRequest
    {
        [Required]
        public Guid GameId { get; set; }
        [Required]
        [StringLength(2000, MinimumLength = 1)]
        public string Prompt { get; set; }
    }
}

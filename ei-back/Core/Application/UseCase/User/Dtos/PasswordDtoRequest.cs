using System.ComponentModel.DataAnnotations;

namespace ei_back.Core.Application.UseCase.User.Dtos
{
    public class PasswordDtoRequest
    {
        [Required]
        public Guid Id { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 4)]
        public string CurrentPassword { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 4)]
        public string NewPassword { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace ei_back.Core.Application.UseCase.User.Dtos
{
    public class LoginDtoRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 4)]
        public string Password { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 4)]
        public string UserName { get; set; }
    }
}

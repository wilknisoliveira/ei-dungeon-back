using System.ComponentModel.DataAnnotations;

namespace ei_back.Core.Application.UseCase.Role.Dtos
{
    public class ApplyRoleDtoRequest
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        [StringLength(10, MinimumLength = 2)]
        public List<string> Roles { get; set; }
    }
}

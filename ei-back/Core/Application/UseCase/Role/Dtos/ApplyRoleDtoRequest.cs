using ei_back.Core.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ei_back.Core.Application.UseCase.Role.Dtos
{
    public class ApplyRoleDtoRequest
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public UserRole role { get; set; }
    }
}

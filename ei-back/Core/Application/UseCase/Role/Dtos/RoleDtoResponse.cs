using ei_back.Core.Application.UseCase.User.Dtos;

namespace ei_back.Core.Application.UseCase.Role.Dtos
{
    public class RoleDtoResponse
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public List<UserGetDtoResponse> Users { get; set; }
    }
}

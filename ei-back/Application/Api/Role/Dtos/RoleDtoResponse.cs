using ei_back.Application.Api.User.Dtos;

namespace ei_back.Application.Api.Role.Dtos
{
    public class RoleDtoResponse
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public List<UserGetDtoResponse> Users { get; set; }
    }
}

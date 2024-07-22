using ei_back.Domain.User;

namespace ei_back.Application.Api.Role.Dtos
{
    public class ApplyRoleDtoResponse
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public List<string> Roles { get; set; }
    }
}

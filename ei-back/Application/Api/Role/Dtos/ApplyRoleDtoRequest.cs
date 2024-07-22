namespace ei_back.Application.Api.Role.Dtos
{
    public class ApplyRoleDtoRequest
    {
        public Guid Id { get; set; }
        public List<string> Roles { get; set; }
    }
}

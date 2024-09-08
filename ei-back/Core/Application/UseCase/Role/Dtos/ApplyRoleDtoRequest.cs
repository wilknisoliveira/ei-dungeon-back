namespace ei_back.Core.Application.UseCase.Role.Dtos
{
    public class ApplyRoleDtoRequest
    {
        public Guid Id { get; set; }
        public List<string> Roles { get; set; }
    }
}

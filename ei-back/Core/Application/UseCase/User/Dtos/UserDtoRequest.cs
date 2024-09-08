namespace ei_back.Core.Application.UseCase.User.Dtos
{
    public class UserDtoRequest
    {
        public required string UserName { get; set; }
        public required string FullName { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
    }
}

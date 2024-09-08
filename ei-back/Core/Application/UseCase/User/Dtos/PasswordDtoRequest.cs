namespace ei_back.Core.Application.UseCase.User.Dtos
{
    public class PasswordDtoRequest
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}

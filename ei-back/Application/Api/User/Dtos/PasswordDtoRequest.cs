namespace ei_back.Application.Api.User.Dtos
{
    public class PasswordDtoRequest
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}

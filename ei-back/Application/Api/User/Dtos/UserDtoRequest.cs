using ei_back.Domain.Role;
using ei_back.Domain.User;
using System.Data;
using System.Security.Cryptography;

namespace ei_back.Application.Api.User.Dtos
{
    public class UserDtoRequest
    {
        public required string UserName { get; set; }
        public required string FullName { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
    }
}

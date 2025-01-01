using ei_back.Core.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ei_back.Core.Domain.Entity
{
    public class User : Base
    {
        public User(string userName, string fullName, string email, string password, UserRole role)
        {
            UserName = userName;
            FullName = fullName;
            Email = email;
            Password = password;
            Role = role;
        }

        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public UserRole Role { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }


        public List<Game> Games { get; set; } = new();
    }
}

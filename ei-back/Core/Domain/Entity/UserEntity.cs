﻿using System.ComponentModel.DataAnnotations;

namespace ei_back.Core.Domain.Entity
{
    public class UserEntity : BaseEntity
    {
        public UserEntity(string userName, string fullName, string email, string password)
        {
            UserName = userName;
            FullName = fullName;
            Email = email;
            Password = password;
        }

        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public List<RoleEntity> Roles { get; } = new();
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }


        public List<GameEntity> Games { get; set; } = new();
    }
}
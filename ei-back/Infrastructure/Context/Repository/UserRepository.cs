﻿using ei_back.Core.Application.Repository;
using ei_back.Core.Domain.Entity;
using ei_back.Core.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ei_back.Infrastructure.Context.Repository
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {

        public UserRepository(EIContext context) : base(context) { }

        public User ValidateCredentials(string userName, string pass)
        {
            return _context.Users.FirstOrDefault(u => u.UserName == userName && u.Password == pass);
        }

        public User RefreshUserInfo(User user)
        {
            if (!_context.Users.Any(u => u.Id.Equals(user.Id))) return null;

            var result = _context.Users.SingleOrDefault(p => p.Id.Equals(user.Id));

            if (result != null)
            {
                try
                {
                    _context.Entry(result).CurrentValues.SetValues(user);
                    return result;
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return result;
        }

        public async Task<User?> GetUserAndRolesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .SingleOrDefaultAsync(u => u.Id.Equals(userId), cancellationToken);
        }

        public async Task<User?> FindByUserName(string userName, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .SingleOrDefaultAsync(x => x.UserName.Equals(userName), cancellationToken);
        }

        public async Task<Dictionary<UserRole, List<string>>> GetUsersNameGroupByRole()
        {
            return await _context.Users
                .GroupBy(x => x.Role)
                .ToDictionaryAsync(
                    group => group.Key,
                    group => group.Select(x => x.UserName).ToList()  
                );
        }
    }
}

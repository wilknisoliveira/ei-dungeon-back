using ei_back.Core.Application.Repository;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ei_back.Infrastructure.Context.Repository
{
    public class UserRepository : GenericRepository<UserEntity>, IUserRepository
    {

        public UserRepository(EIContext context) : base(context) { }

        public UserEntity ValidateCredentials(string userName, string pass)
        {
            return _context.Users.Include(u => u.Roles).FirstOrDefault(u => u.UserName == userName && u.Password == pass);
        }

        public UserEntity RefreshUserInfo(UserEntity user)
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

        public async Task<UserEntity?> GetUserAndRolesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.Roles)
                .SingleOrDefaultAsync(u => u.Id.Equals(userId), cancellationToken);
        }

        public async Task<UserEntity?> FindByUserName(string userName, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .SingleOrDefaultAsync(x => x.UserName.Equals(userName), cancellationToken);
        }
    }
}

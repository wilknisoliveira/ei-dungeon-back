using ei_back.Domain.Base;
using ei_back.Domain.Role.Interfaces;
using ei_back.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ei_back.Domain.Role
{
    public class RoleRepository : GenericRepository<RoleEntity>, IRoleRepository
    {
        public RoleRepository(EIContext context) : base(context) { }

        public async Task<List<RoleEntity>> FindRolesAndUsersAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Roles
                .Include(r => r.Users)
                .ToListAsync(cancellationToken);
        }
    }
}

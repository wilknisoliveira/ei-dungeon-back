using ei_back.Core.Application.Repository;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ei_back.Infrastructure.Context.Repository
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(EIContext context) : base(context) { }

        public async Task<List<Role>> FindRolesAndUsersAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Roles
                .Include(r => r.Users)
                .ToListAsync(cancellationToken);
        }
    }
}

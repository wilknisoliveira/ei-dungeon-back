using ei_back.Core.Application.Repository;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ei_back.Infrastructure.Context.Repository
{
    public class PlayRepository : GenericRepository<PlayEntity>, IPlayRepository
    {
        public PlayRepository(EIContext context) : base(context)
        {
        }

        public Task<List<PlayEntity>> GetPlaysByGameAndSize(Guid gameId, int size, CancellationToken cancellationToken)
        {
            return _context.Plays.Include(x => x.Player)
                .Where(x => x.GameId.Equals(gameId))
                .OrderByDescending(x => x.CreatedAt)
                .Take(size)
                .ToListAsync(cancellationToken);
        }
    }
}

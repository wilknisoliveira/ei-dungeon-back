using ei_back.Domain.Base;
using ei_back.Domain.Play.Interfaces;
using ei_back.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ei_back.Domain.Play
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

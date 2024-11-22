using ei_back.Core.Application.Repository;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ei_back.Infrastructure.Context.Repository
{
    public class PlayRepository : GenericRepository<Play>, IPlayRepository
    {
        public PlayRepository(EIContext context) : base(context)
        {
        }

        public Task<List<Play>> GetPlaysByGameAndSizeButSystemPlay(Guid gameId, int size, CancellationToken cancellationToken)
        {
            return _context.Plays.Include(x => x.Player)
                .Where(x => x.GameId.Equals(gameId) && !x.Player.Type.Equals(PlayerType.System))
                .OrderByDescending(x => x.CreatedAt)
                .Take(size)
                .ToListAsync(cancellationToken);
        }

        public Task<Play?> GetLastPlayByPlayerTypeAndGameId(Guid gameId, PlayerType playerType, CancellationToken cancellationToken)
        {
            return _context.Plays.Include(x => x.Player)
                .Where(x => x.GameId.Equals(gameId) && x.Player.Type.Equals(playerType))
                .OrderByDescending(x => x.UpdatedAt)
               .FirstOrDefaultAsync(cancellationToken);
        }

        public Task<int> CountPlaysByGameAndSizeButSystemPlay(Guid gameId, int size, CancellationToken cancellationToken)
        {
            return _context.Plays.Include(x => x.Player)
                .Where(x => x.GameId.Equals(gameId) && !x.Player.Type.Equals(PlayerType.System))
                .CountAsync(cancellationToken);
        }
    }
}

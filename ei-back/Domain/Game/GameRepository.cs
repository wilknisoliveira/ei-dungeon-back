using ei_back.Domain.Base;
using ei_back.Domain.Game.Interfaces;
using ei_back.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ei_back.Domain.Game
{
    public class GameRepository : GenericRepository<GameEntity>, IGameRepository
    {
        public GameRepository(EIContext context) : base(context)
        {
        }

        public Task<bool> CheckIfExistGameByUser(Guid gameId, Guid OwnerUserId, CancellationToken cancellationToken)
        {
            return _context.Games.AnyAsync(x => x.Id.Equals(gameId) && x.OwnerUserId.Equals(OwnerUserId), cancellationToken);
        }
    }
}

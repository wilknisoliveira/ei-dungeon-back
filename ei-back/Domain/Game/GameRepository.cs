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

        public async Task<bool> CheckIfExistGameByUser(Guid gameId, Guid OwnerUserId, CancellationToken cancellationToken)
        {
            return await _context.Games.AnyAsync(x => x.Id.Equals(gameId) && x.OwnerUserId.Equals(OwnerUserId), cancellationToken);
        }

        public async Task<GameEntity?> GetGameByIdAndOwnerUserName(Guid id, string userName, CancellationToken cancellationToken)
        {
            return await _context.Games.Include(x => x.Players).Include(x => x.OwnerUser).FirstOrDefaultAsync(x => x.Id.Equals(id) && x.OwnerUser.UserName.Equals(userName), cancellationToken: cancellationToken);
        }
    }
}

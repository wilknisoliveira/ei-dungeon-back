using ei_back.Core.Application.Repository;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ei_back.Infrastructure.Context.Repository
{
    public class GameRepository : GenericRepository<Game>, IGameRepository
    {
        public GameRepository(EIContext context) : base(context)
        {
        }

        public async Task<bool> CheckIfExistGameByUser(Guid gameId, Guid OwnerUserId, CancellationToken cancellationToken)
        {
            return await _context.Games.AnyAsync(x => x.Id.Equals(gameId) && x.OwnerUserId.Equals(OwnerUserId), cancellationToken);
        }

        public async Task<Game?> GetGameByIdAndOwnerUserName(Guid id, string userName, CancellationToken cancellationToken)
        {
            return await _context.Games.Include(x => x.Players).Include(x => x.OwnerUser).FirstOrDefaultAsync(x => x.Id.Equals(id) && x.OwnerUser.UserName.Equals(userName), cancellationToken: cancellationToken);
        }
    }
}

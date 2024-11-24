using ei_back.Core.Application.Repository;
using ei_back.Core.Domain.Entity;
using ei_back.Core.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ei_back.Infrastructure.Context.Repository
{
    public class GameInfoRepository : GenericRepository<GameInfo>, IGameInfoRepository
    {
        public GameInfoRepository(EIContext context) : base(context)
        {
        }

        public async Task<IEnumerable<GameInfo>?> GetItemsByValuesAndType(List<string> values, InfoType type, CancellationToken cancellationToken)
        {
            return await _context.GameInfos.Where(x => x.Type.Equals(type) && values.Contains(x.Value)).ToListAsync(cancellationToken);
        }
    }
}

using ei_back.Core.Application.Repository;
using ei_back.Core.Domain.Entity;

namespace ei_back.Infrastructure.Context.Repository
{
    public class GameInfoRepository : GenericRepository<GameInfo>, IGameInfoRepository
    {
        public GameInfoRepository(EIContext context) : base(context)
        {
        }
    }
}

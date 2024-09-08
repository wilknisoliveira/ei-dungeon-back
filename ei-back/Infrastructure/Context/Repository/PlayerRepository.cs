using ei_back.Core.Application.Repository;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Context;

namespace ei_back.Infrastructure.Context.Repository
{
    public class PlayerRepository : GenericRepository<PlayerEntity>, IPlayerRepository
    {
        public PlayerRepository(EIContext context) : base(context) { }
    }
}

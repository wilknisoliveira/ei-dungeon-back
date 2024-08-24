using ei_back.Domain.Base;
using ei_back.Domain.Player.Interfaces;
using ei_back.Infrastructure.Context;

namespace ei_back.Domain.Player
{
    public class PlayerRepository : GenericRepository<PlayerEntity>, IPlayerRepository
    {
        public PlayerRepository(EIContext context) : base(context) { }
    }
}

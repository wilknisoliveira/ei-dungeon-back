using ei_back.Domain.Base;
using ei_back.Domain.Game.Interfaces;
using ei_back.Infrastructure.Context;

namespace ei_back.Domain.Game
{
    public class GameRepository : GenericRepository<GameEntity>, IGameRepository
    {
        public GameRepository(EIContext context) : base(context)
        {
        }
    }
}

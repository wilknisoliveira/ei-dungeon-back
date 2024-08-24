using ei_back.Domain.Base;
using ei_back.Domain.Play.Interfaces;
using ei_back.Domain.Player.Interfaces;
using ei_back.Infrastructure.Context;

namespace ei_back.Domain.Play
{
    public class PlayRepository : GenericRepository<PlayEntity>, IPlayRepository
    {
        public PlayRepository(EIContext context) : base(context)
        {
        }
    }
}

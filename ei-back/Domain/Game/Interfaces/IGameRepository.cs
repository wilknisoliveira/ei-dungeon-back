using ei_back.Domain.Base.Interfaces;

namespace ei_back.Domain.Game.Interfaces
{
    public interface IGameRepository : IRepository<GameEntity>
    {
        Task<bool> CheckIfExistGameByUser(Guid gameId, Guid OwnerUserId, CancellationToken cancellationToken);
    }
}

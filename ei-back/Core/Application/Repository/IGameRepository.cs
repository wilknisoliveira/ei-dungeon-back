using ei_back.Core.Domain.Entity;

namespace ei_back.Core.Application.Repository
{
    public interface IGameRepository : IRepository<Game>
    {
        Task<bool> CheckIfExistGameByUser(Guid gameId, Guid OwnerUserId, CancellationToken cancellationToken);

        Task<Game?> GetGameByIdAndOwnerUserName(Guid id, string userName, CancellationToken cancellationToken);
    }
}

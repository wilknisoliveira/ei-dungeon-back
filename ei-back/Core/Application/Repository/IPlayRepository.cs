using ei_back.Core.Domain.Entity;

namespace ei_back.Core.Application.Repository
{
    public interface IPlayRepository : IRepository<PlayEntity>
    {
        Task<List<PlayEntity>> GetPlaysByGameAndSizeButSystemPlay(Guid gameId, int size, CancellationToken cancellationToken);
        Task<PlayEntity?> GetLastPlayByPlayerTypeAndGameId(Guid gameId, PlayerType playerType, CancellationToken cancellationToken);
        Task<int> CountPlaysByGameAndSizeButSystemPlay(Guid gameId, int size, CancellationToken cancellationToken);
    }
}

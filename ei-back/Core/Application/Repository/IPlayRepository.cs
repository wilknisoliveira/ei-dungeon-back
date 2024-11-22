using ei_back.Core.Domain.Entity;

namespace ei_back.Core.Application.Repository
{
    public interface IPlayRepository : IRepository<Play>
    {
        Task<List<Play>> GetPlaysByGameAndSizeButSystemPlay(Guid gameId, int size, CancellationToken cancellationToken);
        Task<Play?> GetLastPlayByPlayerTypeAndGameId(Guid gameId, PlayerType playerType, CancellationToken cancellationToken);
        Task<int> CountPlaysByGameAndSizeButSystemPlay(Guid gameId, int size, CancellationToken cancellationToken);
    }
}

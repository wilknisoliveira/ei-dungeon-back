using ei_back.Core.Domain.Entity;

namespace ei_back.Core.Application.Repository
{
    public interface IPlayRepository : IRepository<PlayEntity>
    {
        Task<List<PlayEntity>> GetPlaysByGameAndSize(Guid gameId, int size, CancellationToken cancellationToken);
    }
}

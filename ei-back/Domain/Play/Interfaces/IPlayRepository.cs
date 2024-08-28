using ei_back.Domain.Base.Interfaces;

namespace ei_back.Domain.Play.Interfaces
{
    public interface IPlayRepository : IRepository<PlayEntity>
    {
        Task<List<PlayEntity>> GetPlaysByGameAndSize(Guid gameId, int size, CancellationToken cancellationToken);
    }
}

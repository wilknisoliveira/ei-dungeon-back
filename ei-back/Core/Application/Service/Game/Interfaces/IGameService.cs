using ei_back.Core.Application.UseCase.Game.Dtos;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Context;

namespace ei_back.Core.Application.Service.Game.Interfaces
{
    public interface IGameService
    {
        Task<Domain.Entity.Game> CreateAsync(Domain.Entity.Game game, CancellationToken cancellationToken);
        Task<PagedSearchDto<GameDtoResponse>> FindWithPagedSearch(
            Guid userId,
            string column,
            string sort,
            int size,
            int offset,
            int page,
            CancellationToken cancellationToken);
        Task<bool> CheckIfExistGameByUser(Guid gameId, Guid OwnerUserId, CancellationToken cancellationToken);
        Task<Domain.Entity.Game?> GetGameByIdAndOwnerUserName(Guid id, string userName, CancellationToken cancellationToken);
    }
}

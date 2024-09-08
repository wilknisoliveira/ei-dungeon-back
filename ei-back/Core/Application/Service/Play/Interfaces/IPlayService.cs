using ei_back.Core.Application.UseCase.Play.Dtos;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Context;

namespace ei_back.Core.Application.Service.Play.Interfaces
{
    public interface IPlayService
    {
        Task<PagedSearchDto<PlayDtoResponse>> FindWithPagedSearch(
            Guid gameId,
            int size,
            CancellationToken cancellationToken);

        Task<PlayEntity> CreatePlay(PlayEntity playEntity, CancellationToken cancellationToken);
    }
}

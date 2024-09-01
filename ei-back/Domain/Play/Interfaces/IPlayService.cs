using ei_back.Application.Api.Play.Dtos;
using ei_back.Infrastructure.Context;

namespace ei_back.Domain.Play.Interfaces
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

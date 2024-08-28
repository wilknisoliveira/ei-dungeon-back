using ei_back.Application.Api.Play.Dtos;
using ei_back.Infrastructure.Context;

namespace ei_back.Application.Usecases.Play.Interfaces
{
    public interface IGetPlaysUseCase
    {
        Task<PagedSearchDto<PlayDtoResponse>> Handler(Guid gameId, int pageSize, string userName, CancellationToken cancellationToken);
    }
}

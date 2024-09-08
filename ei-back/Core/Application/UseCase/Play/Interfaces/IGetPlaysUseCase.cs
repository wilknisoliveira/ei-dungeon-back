using ei_back.Core.Application.UseCase.Play.Dtos;
using ei_back.Infrastructure.Context;

namespace ei_back.Core.Application.UseCase.Play.Interfaces
{
    public interface IGetPlaysUseCase
    {
        Task<PagedSearchDto<PlayDtoResponse>> Handler(Guid gameId, int pageSize, string userName, CancellationToken cancellationToken);
    }
}

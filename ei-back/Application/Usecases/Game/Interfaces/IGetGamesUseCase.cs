using ei_back.Application.Api.Game.Dtos;
using ei_back.Application.Api.User.Dtos;
using ei_back.Infrastructure.Context;

namespace ei_back.Application.Usecases.Game.Interfaces
{
    public interface IGetGamesUseCase
    {
        Task<PagedSearchDto<GameDtoResponse>> Handler(string sortDirection, int pageSize, int page, string userName, CancellationToken cancellationToken);
    }
}

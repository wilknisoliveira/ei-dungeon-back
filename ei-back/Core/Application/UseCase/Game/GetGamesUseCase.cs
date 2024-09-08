using ei_back.Core.Application.Service.Game.Interfaces;
using ei_back.Core.Application.Service.User.Interfaces;
using ei_back.Core.Application.UseCase.Game.Dtos;
using ei_back.Core.Application.UseCase.Game.Interfaces;
using ei_back.Infrastructure.Context;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;

namespace ei_back.Core.Application.UseCase.Game
{
    public class GetGamesUseCase : IGetGamesUseCase
    {
        private readonly IGameService _gameService;
        private readonly IUserService _userService;

        public GetGamesUseCase(IGameService gameService, IUserService userService)
        {
            _gameService = gameService;
            _userService = userService;
        }

        public async Task<PagedSearchDto<GameDtoResponse>> Handler(string sortDirection, int pageSize, int page, string userName, CancellationToken cancellationToken)
        {
            var user = await _userService.FindByUserName(userName) ??
                throw new NotFoundException($"No user found to user name {userName}.");

            var pagedSearchDto = new PagedSearchDto<GameDtoResponse>();

            var sort = pagedSearchDto.ValidateSort(sortDirection);
            var size = pagedSearchDto.ValidateSize(pageSize);
            var offset = pagedSearchDto.ValidateOffset(page, pageSize);

            return await _gameService.FindWithPagedSearch(user.Id, "owner_user_id", sort, size, offset, page, cancellationToken);
        }
    }
}

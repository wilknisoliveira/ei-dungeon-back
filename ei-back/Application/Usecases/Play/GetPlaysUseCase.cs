using ei_back.Application.Api.Play.Dtos;
using ei_back.Application.Usecases.Play.Interfaces;
using ei_back.Domain.Game.Interfaces;
using ei_back.Domain.Play.Interfaces;
using ei_back.Domain.User.Interfaces;
using ei_back.Infrastructure.Context;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;

namespace ei_back.Application.Usecases.Play
{
    public class GetPlaysUseCase : IGetPlaysUseCase
    {
        private readonly IPlayService _playService;
        private readonly IUserService _userService;
        private readonly IGameService _gameService;

        public GetPlaysUseCase(IPlayService playService, IUserService userService, IGameService gameService)
        {
            _playService = playService;
            _userService = userService;
            _gameService = gameService;
        }

        public async Task<PagedSearchDto<PlayDtoResponse>> Handler(
            Guid gameId,
            int pageSize,
            string userName,
            CancellationToken cancellationToken)
        {
            var user = await _userService.FindByUserName(userName) ??
                throw new NotFoundException($"No user found to user name {userName}.");

            if (!await _gameService.CheckIfExistGameByUser(gameId, user.Id, cancellationToken))
                throw new NotFoundException($"Not found game {gameId} to user {userName}");

            var pagedSearchDto = new PagedSearchDto<PlayDtoResponse>();
            var size = pagedSearchDto.ValidateSize(pageSize);

            var plays = await _playService.FindWithPagedSearch(gameId, size, cancellationToken);

            return plays;
        }
    }
}

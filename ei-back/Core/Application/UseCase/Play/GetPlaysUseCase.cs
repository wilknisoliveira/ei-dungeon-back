﻿using ei_back.Core.Application.Service.Game.Interfaces;
using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Application.Service.User.Interfaces;
using ei_back.Core.Application.UseCase.Play.Dtos;
using ei_back.Core.Application.UseCase.Play.Interfaces;
using ei_back.Infrastructure.Context;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;

namespace ei_back.Core.Application.UseCase.Play
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

            pagedSearchDto = await _playService.FindWithPagedSearch(gameId, size, cancellationToken);

            return pagedSearchDto;
        }
    }
}

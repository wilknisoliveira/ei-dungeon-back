using AutoMapper;
using ei_back.Application.Api.Game.Dtos;
using ei_back.Application.Usecases.Game.Interfaces;
using ei_back.Application.Usecases.User.Interfaces;
using ei_back.Domain.Game;
using ei_back.Domain.Game.Interfaces;
using ei_back.Domain.User.Interfaces;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;

namespace ei_back.Application.Usecases.Game
{
    public class CreateGameUseCase : ICreateGameUseCase
    {
        private readonly IMapper _mapper;
        private readonly IGameService _gameService;
        private readonly IUserService _userService;

        public CreateGameUseCase(
            IMapper mapper,
            IGameService gameService,
            IUserService userService)
        {
            _mapper = mapper;
            _gameService = gameService;
            _userService = userService;
        }

        public async Task<GameDtoResponse> Handler(GameDtoRequest gameDtoRequest, string userName)
        {
            var game = _mapper.Map<GameEntity>(gameDtoRequest);

            var user = await _userService.FindByUserName(userName) ??
                throw new NotFoundException($"No user found to user name {userName}.");

            game.SetOwnerUser(user);

            //TODO: Implementar
            game.SetPlayers(new());

            var gameResponse = await _gameService.CreateAsync(game);

            return _mapper.Map<GameDtoResponse>(gameResponse);
        }
    }
}

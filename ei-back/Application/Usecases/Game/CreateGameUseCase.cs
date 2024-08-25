using AutoMapper;
using ei_back.Application.Api.Game.Dtos;
using ei_back.Application.Usecases.Game.Interfaces;
using ei_back.Domain.Game;
using ei_back.Domain.Game.Interfaces;
using ei_back.Domain.Player.Interfaces;
using ei_back.Domain.User.Interfaces;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;

namespace ei_back.Application.Usecases.Game
{
    public class CreateGameUseCase : ICreateGameUseCase
    {
        private readonly IMapper _mapper;
        private readonly IGameService _gameService;
        private readonly IUserService _userService;
        private readonly IPlayerFactory _playerFactory;

        public CreateGameUseCase(
            IMapper mapper,
            IGameService gameService,
            IUserService userService,
            IPlayerFactory playerFactory)
        {
            _mapper = mapper;
            _gameService = gameService;
            _userService = userService;
            _playerFactory = playerFactory;
        }

        public async Task<GameDtoResponse> Handler(GameDtoRequest gameDtoRequest, string userName, CancellationToken cancellationToken)
        {
            var game = _mapper.Map<GameEntity>(gameDtoRequest);

            var user = await _userService.FindByUserName(userName) ??
                throw new NotFoundException($"No user found to user name {userName}.");
            game.SetOwnerUser(user);

            var artificialPlayersAndMaster = await _playerFactory
                .BuildArtificialPlayersAndMaster(gameDtoRequest.NumberOfArtificialPlayers, game, cancellationToken);
            game.SetPlayers(artificialPlayersAndMaster);

            var gameResponse = await _gameService.CreateAsync(game, cancellationToken);

            return _mapper.Map<GameDtoResponse>(gameResponse);
        }
    }
}

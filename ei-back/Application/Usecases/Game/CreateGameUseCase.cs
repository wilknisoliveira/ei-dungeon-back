using AutoMapper;
using ei_back.Application.Api.Game.Dtos;
using ei_back.Application.Usecases.Game.Interfaces;
using ei_back.Domain.Game;
using ei_back.Domain.Game.Interfaces;
using ei_back.Domain.Play.Interfaces;
using ei_back.Domain.Player;
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
        private readonly IPlayFactory _playFactory;

        public CreateGameUseCase(
            IMapper mapper,
            IGameService gameService,
            IUserService userService,
            IPlayerFactory playerFactory,
            IPlayFactory playFactory)
        {
            _mapper = mapper;
            _gameService = gameService;
            _userService = userService;
            _playerFactory = playerFactory;
            _playFactory = playFactory;
        }

        public async Task<GameDtoResponse> Handler(GameDtoRequest gameDtoRequest, string userName, CancellationToken cancellationToken)
        {
            var game = _mapper.Map<GameEntity>(gameDtoRequest);

            var user = await _userService.FindByUserName(userName) ??
                throw new NotFoundException($"No user found to user name {userName}.");
            game.SetOwnerUser(user);

            var realPlayer = new PlayerEntity(gameDtoRequest.CharacterName, gameDtoRequest.CharacterDescription, PlayerType.RealPlayer);
            realPlayer.SetCreatedDate(DateTime.Now);

            var artificialPlayersAndMaster = await _playerFactory
                .BuildArtificialPlayersAndMaster(gameDtoRequest.NumberOfArtificialPlayers, game, cancellationToken);

            artificialPlayersAndMaster.Add(realPlayer);
            game.SetPlayers(artificialPlayersAndMaster);

            var masterPlay = await _playFactory.BuildInitialMasterPlay(game, cancellationToken) ??
                throw new InternalServerErrorException("Something went wrong while attempting to generate the initial master play.");
            game.NewPlay(masterPlay);

            var gameResponse = await _gameService.CreateAsync(game, cancellationToken);

            return _mapper.Map<GameDtoResponse>(gameResponse);
        }
    }
}

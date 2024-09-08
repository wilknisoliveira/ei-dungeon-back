using AutoMapper;
using ei_back.Core.Application.Service.Game.Interfaces;
using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Application.Service.Player.Interfaces;
using ei_back.Core.Application.Service.User.Interfaces;
using ei_back.Core.Application.UseCase.Game.Dtos;
using ei_back.Core.Application.UseCase.Game.Interfaces;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;

namespace ei_back.Core.Application.UseCase.Game
{
    public class CreateGameUseCase : ICreateGameUseCase
    {
        private readonly IMapper _mapper;
        private readonly IGameService _gameService;
        private readonly IUserService _userService;
        private readonly IPlayerFactory _playerFactory;
        private readonly IInitialMasterPlayService _initialMasterPlayService;

        public CreateGameUseCase(
            IMapper mapper,
            IGameService gameService,
            IUserService userService,
            IPlayerFactory playerFactory,
            IInitialMasterPlayService initialMasterPlayService)
        {
            _mapper = mapper;
            _gameService = gameService;
            _userService = userService;
            _playerFactory = playerFactory;
            _initialMasterPlayService = initialMasterPlayService;
        }

        public async Task<GameDtoResponse> Handler(GameDtoRequest gameDtoRequest, string userName, CancellationToken cancellationToken)
        {
            var game = _mapper.Map<GameEntity>(gameDtoRequest);

            var user = await _userService.FindByUserName(userName) ??
                throw new NotFoundException($"No user found to user name {userName}.");
            game.SetOwnerUser(user);

            var systemPlayer = new PlayerEntity("System", "System", PlayerType.System, game);
            var realPlayer = new PlayerEntity(gameDtoRequest.CharacterName, gameDtoRequest.CharacterDescription, PlayerType.RealPlayer, game);

            var artificialPlayersAndMaster = await _playerFactory
                .BuildArtificialPlayersAndMaster(gameDtoRequest.NumberOfArtificialPlayers, game, cancellationToken);

            artificialPlayersAndMaster.Add(systemPlayer);
            artificialPlayersAndMaster.Add(realPlayer);
            game.SetPlayers(artificialPlayersAndMaster);

            var masterPlay = await _initialMasterPlayService.Handler(game, cancellationToken) ??
                throw new InternalServerErrorException("Something went wrong while attempting to generate the initial master play.");
            game.AddPlay(masterPlay);

            //Gerar o resumo dos Players Description + Master play

            var gameResponse = await _gameService.CreateAsync(game, cancellationToken);

            return _mapper.Map<GameDtoResponse>(gameResponse);
        }
    }
}

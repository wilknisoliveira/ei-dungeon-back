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
        private readonly IInitialMasterPlayService _initialMasterPlayService;

        public CreateGameUseCase(
            IMapper mapper,
            IGameService gameService,
            IUserService userService,
            IInitialMasterPlayService initialMasterPlayService)
        {
            _mapper = mapper;
            _gameService = gameService;
            _userService = userService;
            _initialMasterPlayService = initialMasterPlayService;
        }

        public async Task<GameDtoResponse> Handler(GameDtoRequest gameDtoRequest, string userName, CancellationToken cancellationToken)
        {
            var user = await _userService.FindByUserName(userName) ??
                throw new NotFoundException($"No user found to user name {userName}.");

            var game = new Domain.Entity.Game(user, "Dungeons & Dragons", gameDtoRequest.Name);
            
            game.SetOwnerUser(user);

            var players = new List<Player>();
            var systemPlayer = new Player("System", "System", PlayerType.System, game);
            players.Add(systemPlayer);
            
            Player master = new("Table Master", "RPG Table Master", PlayerType.Master);
            players.Add(master);
            
            var realPlayer = new Player(gameDtoRequest.CharacterName, gameDtoRequest.CharacterDescription, PlayerType.RealPlayer, game);
            players.Add(realPlayer);
            
            game.SetPlayers(players);

            var masterPlay = await _initialMasterPlayService.Handler(game, cancellationToken) ??
                throw new InternalServerErrorException("Something went wrong while attempting to generate the initial master play.");
            game.AddPlay(masterPlay);

            var gameResponse = await _gameService.CreateAsync(game, cancellationToken);

            return _mapper.Map<GameDtoResponse>(gameResponse);
        }
    }
}

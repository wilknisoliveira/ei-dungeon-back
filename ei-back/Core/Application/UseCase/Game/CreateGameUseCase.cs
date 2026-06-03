using AutoMapper;
using ei_back.Core.Application.Interfaces;
using ei_back.Core.Application.Service.Game.Interfaces;
using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Application.Service.User.Interfaces;
using ei_back.Core.Application.UseCase.Game.Dtos;
using ei_back.Core.Application.UseCase.Game.Interfaces;
using ei_back.Core.Domain.DomainExceptions.Player;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;

namespace ei_back.Core.Application.UseCase.Game
{
    public class CreateGameUseCase : ICreateGameUseCase
    {
        private readonly IMapper _mapper;
        private readonly IGameService _gameService;
        private readonly IUserService _userService;
        private readonly IGenAi _genAi;
        private readonly IUpsertWorldInfoService _upsertWorldInfoService;

        public CreateGameUseCase(
            IMapper mapper,
            IGameService gameService,
            IUserService userService,
            IGenAi genAi, 
            IUpsertWorldInfoService upsertWorldInfoService)
        {
            _mapper = mapper;
            _gameService = gameService;
            _userService = userService;
            _genAi = genAi;
            _upsertWorldInfoService = upsertWorldInfoService;
        }

        public async Task<GameDtoResponse> Handler(GameDtoRequest gameDtoRequest, string userName, CancellationToken cancellationToken)
        {
            var user = await _userService.FindByUserName(userName) ??
                throw new NotFoundException($"No user found to user name {userName}.");

            var game = new Domain.Entity.Game(user, gameDtoRequest.Name);
            
            game.SetOwnerUser(user);

            var players = new List<Player>();
            var systemPlayer = new Player("System", "System", PlayerType.System);
            players.Add(systemPlayer);
            
            Player master = new("Table Master", "RPG Table Master", PlayerType.Master);
            players.Add(master);
            
            var realPlayer = new Player(
                gameDtoRequest.CharacterName, 
                gameDtoRequest.CharacterDescription, 
                gameDtoRequest.Race, 
                PlayerType.RealPlayer);
            try
            {
                realPlayer.SetSkillPoints(
                    gameDtoRequest.Skills.Strength,
                    gameDtoRequest.Skills.Dexterity,
                    gameDtoRequest.Skills.Intelligence, 
                    gameDtoRequest.Skills.Constitution,
                    gameDtoRequest.Skills.Charisma,
                    gameDtoRequest.Skills.Wisdom);
            }
            catch (AttributePointsNotValidException ex)
            {
                throw new BadRequestException(ex.Message);
            }
            
            players.Add(realPlayer);
            
            game.SetPlayers(players);

            game.SetWorldInfo(await _upsertWorldInfoService.Handler(realPlayer.InfoToString(), cancellationToken));

            var gameResponse = await _gameService.CreateAsync(game, cancellationToken);

            return _mapper.Map<GameDtoResponse>(gameResponse);
        }
    }
}

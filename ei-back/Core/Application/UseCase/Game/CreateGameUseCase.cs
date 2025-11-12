using AutoMapper;
using ei_back.Core.Application.Interfaces;
using ei_back.Core.Application.Service.Game.Interfaces;
using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Application.Service.User.Interfaces;
using ei_back.Core.Application.UseCase.Game.Dtos;
using ei_back.Core.Application.UseCase.Game.Interfaces;
using ei_back.Core.Application.UseCase.Play.Dtos;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using Microsoft.IdentityModel.Tokens;

namespace ei_back.Core.Application.UseCase.Game
{
    public class CreateGameUseCase : ICreateGameUseCase
    {
        private readonly IMapper _mapper;
        private readonly IGameService _gameService;
        private readonly IUserService _userService;
        private readonly IInitialMasterPlayService _initialMasterPlayService;
        private readonly IGenAi _genAi;

        public CreateGameUseCase(
            IMapper mapper,
            IGameService gameService,
            IUserService userService,
            IInitialMasterPlayService initialMasterPlayService,
            IGenAi genAi)
        {
            _mapper = mapper;
            _gameService = gameService;
            _userService = userService;
            _initialMasterPlayService = initialMasterPlayService;
            _genAi = genAi;
        }

        public async Task<GameDtoResponse> Handler(GameDtoRequest gameDtoRequest, string userName, CancellationToken cancellationToken)
        {
            var user = await _userService.FindByUserName(userName) ??
                throw new NotFoundException($"No user found to user name {userName}.");

            var game = new Domain.Entity.Game(user, gameDtoRequest.Name);
            
            game.SetOwnerUser(user);

            var players = new List<Player>();
            var systemPlayer = new Player("System", "System", PlayerType.System, game);
            players.Add(systemPlayer);
            
            Player master = new("Table Master", "RPG Table Master", PlayerType.Master);
            players.Add(master);
            
            var realPlayer = new Player(gameDtoRequest.CharacterName, gameDtoRequest.CharacterDescription, PlayerType.RealPlayer, game);
            players.Add(realPlayer);
            
            game.SetPlayers(players);

            game.SetWorldInfo(await GenerateWorldInfoAsync(realPlayer, cancellationToken));
            
            var masterPlay = await _initialMasterPlayService.Handler(game, cancellationToken) ??
                throw new InternalServerErrorException("Something went wrong while attempting to generate the initial master play.");
            game.AddPlay(masterPlay);

            var gameResponse = await _gameService.CreateAsync(game, cancellationToken);

            return _mapper.Map<GameDtoResponse>(gameResponse);
        }

        private async Task<string> GenerateWorldInfoAsync(Player realPlayer, CancellationToken cancellationToken)
        {
            var systemPrompt = GetMasterPersonality();
            systemPrompt += "\n\n<player>\n" + realPlayer.InfoToString() + "\n" + @"<\/player>" + "\n";

            List<AiPromptRequest> promptList =
            [
                new(AiRole.System, systemPrompt),
                new(AiRole.User, GetWorldInfoPromptGeneration())
            ];
            
            var iaResponse = await _genAi.GenFromMultiplePrompts<WorldInfoDtoResponse>(promptList, 2000, cancellationToken);
            
            if (iaResponse.IsNullOrEmpty())
                throw new BadGatewayException("No content was returned by the gateway");
            
            return iaResponse;
        }

        private static string GetMasterPersonality()
        {
            return "Você é um mestre de RPG de mesa em uma campanha de Dungeons & Dragons. " +
                   "Você gosta de preparar as campanhas sem roteiro, apenas com criação de mundo, " +
                   "utilizando a máxima 'Crie mundos, não histórias'.";
        }

        private static string GetWorldInfoPromptGeneration()
        {
            return "Crie um mundo para uma campanha de Dungeons & Dragons. Precisa ser um mundo de fantasia original, " +
                   "coeso e detalhado. Evite clichês óbvios.";
        }
    }
}

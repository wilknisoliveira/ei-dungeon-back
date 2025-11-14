using AutoMapper;
using ei_back.Core.Application.Repository;
using ei_back.Core.Application.Service.Game.Interfaces;
using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Application.UseCase.Play.Dtos;
using ei_back.Core.Application.UseCase.Play.Interfaces;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Context.Interfaces;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using Microsoft.IdentityModel.Tokens;
using ei_back.Core.Application.Interfaces;
using Tiktoken;

namespace ei_back.Core.Application.UseCase.Play
{
    public class NewUserPlayUseCase : INewUserPlayUseCase
    {
        private readonly IMapper _mapper;
        private readonly IPlayService _playService;
        private readonly IGameService _gameService;
        private readonly IPlayRepository _playRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NewUserPlayUseCase> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly int _limitTokens;
        private readonly IGenAi _genAi;

        public NewUserPlayUseCase(
            IMapper mapper,
            IPlayService playService,
            IGameService gameService,
            IPlayRepository playRepository,
            IUnitOfWork unitOfWork,
            ILogger<NewUserPlayUseCase> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IGenAi genAi)
        {
            _mapper = mapper;
            _playService = playService;
            _gameService = gameService;
            _playRepository = playRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _genAi = genAi;

            var errorMessage = "Verify if all the string connections was registered properly in the appsettings.";

            var limitTokens = configuration["PlayOptions:LimitTokens"] ?? "";
            if (!int.TryParse(limitTokens, out _limitTokens))
                throw new InternalServerErrorException(errorMessage);
        }

        public async Task<List<PlayDtoResponse>> Handler(PlayDtoRequest playDtoRequest, string userName, CancellationToken cancellationToken)
        {
            List<Domain.Entity.Play> plays = [];
             
            var game = await _gameService.GetGameByIdAndOwnerUserName(playDtoRequest.GameId, userName, cancellationToken) ??
                throw new NotFoundException($"No game found with id {playDtoRequest.GameId} to user name {userName}.");

            var lastSummary = await _playRepository.GetLastPlayByPlayerTypeAndGameId(game.Id, PlayerType.System, cancellationToken);
            if (lastSummary != null)
            {
                List<Domain.Entity.Play> nextPlays = await _playRepository.GetPlayWhereCreatedAtIsUpperThan(game.Id, lastSummary.CreatedAt, cancellationToken);

                var numberOfPlaysRequiredForContext = 4;
                if (nextPlays.Count < numberOfPlaysRequiredForContext)
                {
                    var missingPlaysNumber = numberOfPlaysRequiredForContext - nextPlays.Count;
                    
                    List<Domain.Entity.Play> previousPlays = (await _playRepository
                        .GetLastNBeforeDate(game.Id, missingPlaysNumber, lastSummary.CreatedAt, cancellationToken))
                        .OrderBy(x => x.CreatedAt).ToList();
                    plays.AddRange(previousPlays);
                }
                plays.Add(lastSummary);
                plays.AddRange(nextPlays);
            }
            else
            {
                List<Domain.Entity.Play> allGamePlays = await _playRepository.GetAllByGameId(game.Id, cancellationToken);
                plays.AddRange(allGamePlays);
            }

            var realPlayer = game.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.RealPlayer)) ??
                throw new InternalServerErrorException($"Something went wrong while attempting to get the real player info");

            var newPlay = new Domain.Entity.Play(game, realPlayer, playDtoRequest.Prompt);
            _ = await _playService.CreatePlay(newPlay, cancellationToken) ??
                throw new InternalServerErrorException($"Something went wrong while attempting to create the play");
            plays.Add(newPlay);
            
            var masterPlay = await GenerateMasterPlay(plays, game, cancellationToken);
            _ = await _playService.CreatePlay(masterPlay, cancellationToken) ??
                throw new InternalServerErrorException($"Something went wrong while attempting to create the master play");
            plays.Add(masterPlay);
            var response = new List<PlayDtoResponse> { _mapper.Map<PlayDtoResponse>(masterPlay) };

            var changedItems = await _unitOfWork.CommitAsync(cancellationToken);
            if (changedItems == 0)
            {
                var errorMessage = "Something went wrong while attempting to create the user play.";
                _logger.LogError(errorMessage);
                throw new InternalServerErrorException(errorMessage);
            }

            int numberOfTokens = CountTokensFromPlays(plays);

            if (numberOfTokens > _limitTokens)
            {
                var timeSpan = TimeSpan.FromMinutes(2);
                var cancellationTokenTask = new CancellationTokenSource(timeSpan);

                _ = Task.Run(async () =>
                {
                    using var cancellationTokenService = new CancellationTokenSource(timeSpan);

                    using var summaryScope = _serviceProvider.CreateScope();
                    //using var worldInfoScope = _serviceProvider.CreateScope();
                    var newGeneratePlaysSummaryService = summaryScope.ServiceProvider.GetRequiredService<IGeneratePlaysSummaryService>();
                    //var upsertWorldInfoService = worldInfoScope.ServiceProvider.GetRequiredService<IUpsertWorldInfoService>();
                        
                    await newGeneratePlaysSummaryService.Handler(game.Id, plays, cancellationTokenService.Token);
                    //var worldInfoTask = upsertWorldInfoService.Handler(game.Id, plays, cancellationTokenService.Token);
                        
                    //await Task.WhenAll(summaryTask, worldInfoTask);
                }, cancellationTokenTask.Token);

                // Other ways to use async operations without create a new context:
                // For cpu heavy operations
                // _ = Task.Run(async () => await _generatePlaysResumeService.Handler(game.Plays, game, playerList.Content, CancellationToken.None), CancellationToken.None);
                // For operatons with external services like http or DbContext:
                // _ = _generatePlaysResumeService.Handler(game.Plays, game, playerList.Content, ctsg.Token);
            }

            return response;
        }

        private async Task<Domain.Entity.Play> GenerateMasterPlay(List<Domain.Entity.Play> plays, Domain.Entity.Game game, CancellationToken cancellationToken)
        {
            var realPlayer = game.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.RealPlayer));

            var systemPrompt = $"<master-instruction>\n{MasterPlayCommand()}\n</master-instruction>\n" +
                               $"<player-info>\n{realPlayer!.InfoToString()}\n</player-info>\n" + 
                               $"<world-info>\n{game.WorldInfo}\n</world-info>\n";
            
            List<AiPromptRequest> promptList = [];

            foreach (var play in plays)
            {
                var playerType = play.Player.Type;

                switch (playerType)
                {
                    case PlayerType.System:
                        systemPrompt += $"\n<summary>\n{play.Prompt}\n</summary>";
                        break;
                    case PlayerType.RealPlayer:
                        promptList.Add(new AiPromptRequest(AiRole.User, play.Prompt));
                        break;
                    case PlayerType.Master:
                        promptList.Add(new AiPromptRequest(AiRole.Assistant, play.Prompt));
                        break;
                }
            }
            
            promptList.Insert(0, new AiPromptRequest(AiRole.System, systemPrompt));
            
            var iaResponse = await _genAi.GenFromMultiplePrompts(promptList, 400, cancellationToken);

            if (iaResponse.IsNullOrEmpty())
                throw new BadGatewayException("No content was returned by the gateway");

            var masterPlayer = game.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.Master)) ??
                throw new NotFoundException($"No Master player was found to the game {game.Id}");

            return new Domain.Entity.Play(game, masterPlayer, iaResponse);
        }

        private static string MasterPlayCommand()
        {
            //Blocked the dices
            return $"Você é um mestre de mesa (Master table) em um jogo de RPG Dungeons & Dragons. Nas informações repassadas, encontra-se um breve resumo de partidas anteriores, bem como as jogadas mais recentes. Sua função é de conduzir a história, desenvolver o enredo, interpretar os NPCs, tornar o jogo sempre envolvente e emocionante, bem como quaisquer outras ações relativas a uma mestre de Mesa. \nObservações: 1. Você como Mestre da Mesa, nunca deve interpretar o papel do Player! Também nunca deve ditar as ações do Player!; 2. Não será utilizado mecanismos de rolagem de dados; \n Agora, prossiga com a próxima orientação do Mestre da Mesa!";
        }

        private int CountTokensFromPlays(List<Domain.Entity.Play> plays)
        {
            const string model = "gpt-4";
            Encoder? encoder = null;
            try
            {
                encoder = ModelToEncoder.For(model);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Something went wrong while attempting to create a encoder for {model} model.";
                _logger.LogError(errorMessage + " Error: " + ex);
                throw new InternalServerErrorException(errorMessage + " Error: " + ex.Message);
            }

            var prompts = "";
                
            plays.ForEach(x => prompts += x.Prompt + " ");

            return encoder.CountTokens(prompts);
        }
    }
}

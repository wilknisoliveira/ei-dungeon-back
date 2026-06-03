using AutoMapper;
using ei_back.Core.Application.Repository;
using ei_back.Core.Application.Service.Game.Interfaces;
using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Application.UseCase.Play.Dtos;
using ei_back.Core.Application.UseCase.Play.Interfaces;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Context.Interfaces;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using ei_back.Core.Application.Interfaces;
using ei_back.Core.Domain.Enums;
using Tiktoken;
using System.Runtime.CompilerServices;

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
        private readonly IPlayAnalyzerService _playAnalyzerService;
        private readonly IInitialMasterPlayService _initialMasterPlayService;

        public NewUserPlayUseCase(
            IMapper mapper,
            IPlayService playService,
            IGameService gameService,
            IPlayRepository playRepository,
            IUnitOfWork unitOfWork,
            ILogger<NewUserPlayUseCase> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IGenAi genAi, 
            IPlayAnalyzerService playAnalyzerService,
            IInitialMasterPlayService initialMasterPlayService)
        {
            _mapper = mapper;
            _playService = playService;
            _gameService = gameService;
            _playRepository = playRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _genAi = genAi;
            _playAnalyzerService = playAnalyzerService;
            _initialMasterPlayService = initialMasterPlayService;

            var errorMessage = "Verify if all the string connections was registered properly in the appsettings.";

            var limitTokens = configuration["PlayOptions:LimitTokens"] ?? "";
            if (!int.TryParse(limitTokens, out _limitTokens))
                throw new InternalServerErrorException(errorMessage);
        }

        public async IAsyncEnumerable<StreamPlayDtoResponse> Handler(
            PlayDtoRequest playDtoRequest, 
            string userName, 
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            List<Domain.Entity.Play> plays = [];
             
            var game = await _gameService.GetGameByIdAndOwnerUserName(playDtoRequest.GameId, userName, cancellationToken);
            if (game == null)
            {
                yield return new StreamPlayDtoResponse 
                { 
                    EventType = EventType.Error, 
                    Content = $"No game found with id {playDtoRequest.GameId} to user name {userName}." 
                };

                yield break;
            }

            if (!game.GameStatus.Equals(GameStatus.Active))
            {
                yield return new StreamPlayDtoResponse 
                { 
                    EventType = EventType.Error, 
                    Content = $"Game '{game.Id}' is not active."
                };

                yield break;
            }

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

            yield return new StreamPlayDtoResponse
            {
                EventType = EventType.Start,
            };
            var changedItems = 0;

            if (plays.Count == 0)
            {
                await foreach (var chunk in _initialMasterPlayService
                    .ExecuteStreamingAsync(game, cancellationToken)
                    .WithCancellation(cancellationToken))
                {
                    if (chunk.EventType == AIStreamEventType.Error)
                    {
                        yield return new StreamPlayDtoResponse
                        {
                            EventType = EventType.Error,
                            Content = chunk.Content
                        };
                        yield break;
                    }
                    yield return new StreamPlayDtoResponse
                    {
                        EventType = EventType.Chunk,
                        Content = chunk.Content,
                    };
                }

                changedItems = await _unitOfWork.CommitAsync(cancellationToken);
                if (changedItems == 0)
                {
                    yield return new StreamPlayDtoResponse
                    {
                        EventType = EventType.Error,
                        Content = "Something went wrong while attempting to create the user play."
                    };
                    yield break;
                }

                yield break;
            }

            var realPlayer = game.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.RealPlayer)) ??
                throw new InternalServerErrorException($"Something went wrong while attempting to get the real player info");

            var newPlay = new Domain.Entity.Play(game, realPlayer, playDtoRequest.Prompt);
            _ = await _playService.CreatePlay(newPlay, cancellationToken) ??
                throw new InternalServerErrorException($"Something went wrong while attempting to create the play");
            plays.Add(newPlay);
            
            AnalyzerDtoResponse analyzerDtoResponse = await _playAnalyzerService.Handler(
                plays, 
                game, 
                cancellationToken);
            if (analyzerDtoResponse.Result == AnalyzerResult.PlayerDied)
            {
                game.KillPlayer();
            }
            
            var masterPlayer = game.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.Master)) ??
                throw new NotFoundException($"No Master player was found to the game {game.Id}");
            
            var completedMasterResponse = "";

            await foreach (var chunk in 
                StreamGenerateMasterPlay(plays, game, analyzerDtoResponse, cancellationToken)
                .WithCancellation(cancellationToken))
            {
                if (chunk.EventType == AIStreamEventType.Error)
                {
                    yield return new StreamPlayDtoResponse
                    {
                        EventType = EventType.Error,
                        Content = chunk.Content
                    };
                    yield break;
                } 
                else
                {
                    completedMasterResponse += chunk.Content;
                    yield return new StreamPlayDtoResponse
                    {
                        EventType = EventType.Chunk,
                        Content = chunk.Content,
                    };
                }
            }

            var masterPlay = new Domain.Entity.Play(game, masterPlayer, completedMasterResponse);

            _ = await _playService.CreatePlay(masterPlay, cancellationToken) ??
                throw new InternalServerErrorException($"Something went wrong while attempting to create the master play");
            plays.Add(masterPlay);

            changedItems = await _unitOfWork.CommitAsync(cancellationToken);
            if (changedItems == 0)
            {
                yield return new StreamPlayDtoResponse 
                { 
                    EventType = EventType.Error, 
                    Content = "Something went wrong while attempting to create the user play."
                };

                yield break;
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
        }

        private async IAsyncEnumerable<StreamAIDtoResponse> StreamGenerateMasterPlay(
            List<Domain.Entity.Play> plays, 
            Domain.Entity.Game game,
            AnalyzerDtoResponse analyzerDtoResponse,
            [EnumeratorCancellation] CancellationToken cancellationToken)
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

            var maxOutputTokens = 400;
            var analysis = "<analysis>\n";
            switch (analyzerDtoResponse.Result)
            {
                case AnalyzerResult.InvalidPlay:
                    analysis += $"A nova jogada do player é inválida pela seguinte razão: {analyzerDtoResponse.Reason}\n" +
                                $"Negue a jogada do player, explique o motivo e dê a ele opções válidas.";
                    maxOutputTokens = 100;
                    break;
                case AnalyzerResult.RollDice:
                {
                    var random = new Random();
                    // Dice d20
                    var dicesResult = random.Next(1, 21);
                    var skill = analyzerDtoResponse.Skill ?? Skill.Intelligence;
                    var modifier = realPlayer.GetModifier(skill);
                    var result = (dicesResult + modifier) >= dicesResult ? "SUCCESS" : "FAIL";

                    analysis += $"A nova jogada é crítica pelo seguinte motivo: {analyzerDtoResponse.Reason}\n\n" +
                                $"O início da sua resposta como mestre deve ser parecida com essa:\n" +
                                $"'A sua jogada é crítica pois [aqui explique o motivo...]. Por isso é necessário jogar " +
                                $"um dado d20!\n" +
                                $"Para isso será necessário uma classe de dificuldade de " +
                                $"{analyzerDtoResponse.DifficultyClass ?? 12} e você poderá usar a skill {skill}.\n" +
                                $"Jogando o dado... O resultado foi {dicesResult}!\n" +
                                $"Para a Skill {skill} seu modificador é {modifier}.\n" +
                                $"Então sua jogada foi - {result}! - '\n\n" +
                                $"Na sua narração seguinte, considere o resultado dos dados para ditar o resultado" +
                                $"da jogada.";
                    break;
                }
                case AnalyzerResult.ClarificationNeeded:
                    analysis += $"A nova jogada do player é insuficiente pela seguinte razão: " +
                                $"{analyzerDtoResponse.Reason}\n\n" +
                                $"Solicite mais informações sobre a ação ou jogada do player, de forma a esclarecer suas " +
                                $"intenções.";
                    maxOutputTokens = 100;
                    break;
                case AnalyzerResult.PlayerDied:
                    analysis += $"O personagem do player morreu pela seguinte razão: {analyzerDtoResponse.Reason}\n\n" +
                                $"Finalize o jogo dando um encerramento de acordo com o clima do jogo e com a coerência " +
                                $"narrativa necessária.\n" +
                                $"Ao final agradeça ao player e o convide para iniciar um novo jogo.";
                    break;
                default:
                    analysis += "Tudo certo com a jogada do player. Pode prosseguir normalmente.";
                    break;
            }
            analysis += "\n</analysis>";
            systemPrompt += analysis;

            promptList.Insert(0, new AiPromptRequest(AiRole.System, systemPrompt));

            await foreach (var chunk in _genAi
                .StreamGetResponse(promptList, maxOutputTokens, cancellationToken)
                .WithCancellation(cancellationToken))
            {
                yield return chunk;
            }
        }

        private static string MasterPlayCommand()
        {
            //Blocked the dices
            return $"Você é um mestre de mesa (Master table) em um jogo de RPG Dungeons & Dragons. " +
                   $"Sua função é conduzir a história, desenvolver o enredo, interpretar " +
                   $"os NPCs, tornar o jogo sempre envolvente e emocionante, bem como quaisquer outras ações " +
                   $"relativas a uma mestre de Mesa.\n" +
                   $"IMPORTANTE: Você como Mestre da Mesa, NUNCA deve interpretar o papel do Player! " +
                   $"Você também NUNCA deve ditar as ações do Player!;\n\n" +
                   $"Você tem algumas informações importantes que são primordiais para seu papel como Mestre de Mesa:\n" +
                   $"- Em <world-info> estão todas as informações que você como mestre criou a respeito do mundo do jogo. " +
                   $"Utilize essas informações de forma estratégica para direcionar a história" +
                   $"- Em <summary> encontra-se um breve resumo de partidas anteriores.\n" +
                   $"- Em <analysis> está uma análise que você fez previamente sobre a nova jogada do usuário." +
                   $"Nela está o resultado da jogada e como você enquanto mestre deve prosseguir.\n\n" +
                   $"Agora, prossiga com a próxima orientação do Mestre da Mesa!";
        }

        private int CountTokensFromPlays(List<Domain.Entity.Play> plays)
        {
            // TikToken doesn't have support to the gemini models. Therefore, is used an OpenAi model to get
            // a similar result
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
                throw new InternalServerErrorException(errorMessage);
            }

            var prompts = "";
                
            plays.ForEach(x => prompts += x.Prompt + " ");

            return encoder.CountTokens(prompts);
        }
    }
}

using AutoMapper;
using ei_back.Core.Application.Repository;
using ei_back.Core.Application.Service.Game.Interfaces;
using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Application.UseCase.Play.Dtos;
using ei_back.Core.Application.UseCase.Play.Interfaces;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Context.Interfaces;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using ei_back.Infrastructure.ExternalAPIs.Dtos.Request;
using ei_back.Infrastructure.ExternalAPIs.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace ei_back.Core.Application.UseCase.Play
{
    public class NewUserPlayUseCase : INewUserPlayUseCase
    {
        private readonly IMapper _mapper;
        private readonly IPlayService _playService;
        private readonly IGameService _gameService;
        private readonly IGenerativeAIApiHttpService _generativeAIApiHttpService;
        private readonly IPlayRepository _playRepository;
        private readonly IGeneratePlaysResumeService _generatePlaysResumeService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NewUserPlayUseCase> _logger;

        public NewUserPlayUseCase(
            IMapper mapper,
            IPlayService playService,
            IGameService gameService,
            IGenerativeAIApiHttpService generativeAIApiHttpService,
            IPlayRepository playRepository,
            IGeneratePlaysResumeService generatePlaysResumeService,
            IUnitOfWork unitOfWork,
            ILogger<NewUserPlayUseCase> logger)
        {
            _mapper = mapper;
            _playService = playService;
            _gameService = gameService;
            _generativeAIApiHttpService = generativeAIApiHttpService;
            _playRepository = playRepository;
            _generatePlaysResumeService = generatePlaysResumeService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<PlayDtoResponse>> Handler(PlayDtoRequest playDtoRequest, string userName, CancellationToken cancellationToken)
        {
            List<Domain.Entity.Play> plays = [];
            List<PlayDtoResponse> response = [];
             
            var game = await _gameService.GetGameByIdAndOwnerUserName(playDtoRequest.GameId, userName, cancellationToken) ??
                throw new NotFoundException($"No game found with id {playDtoRequest.GameId} to user name {userName}.");

            var realPlayer = game.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.RealPlayer)) ??
                throw new InternalServerErrorException($"Something went wrong while attempting to get the real player info");

            var newPlay = new Domain.Entity.Play(game, realPlayer, playDtoRequest.Prompt);
            _ = await _playService.CreatePlay(newPlay, cancellationToken) ??
                throw new InternalServerErrorException($"Something went wrong while attempting to create the play");

            var lastPlay = await _playRepository.GetLastPlayByPlayerTypeAndGameId(game.Id, PlayerType.System, cancellationToken);
            if (lastPlay != null)
            {
                plays.Add(lastPlay);
            }
            else
            {
                lastPlay = await _playRepository.GetLastPlayByPlayerTypeAndGameId(game.Id, PlayerType.Master, cancellationToken) ??
                throw new InternalServerErrorException("No Table Master play was found.");
                plays.Add(lastPlay);
            }
            var lastPlayDtoResponse = _mapper.Map<PlayDtoResponse>(lastPlay);
            lastPlayDtoResponse.PlayerDtoResponse = _mapper.Map<PlayerDtoResponse>(lastPlay.Player);
            response.Add(lastPlayDtoResponse);

            plays.Add(newPlay);
            var newPlayDtoResponse = _mapper.Map<PlayDtoResponse>(newPlay);
            newPlayDtoResponse.PlayerDtoResponse = _mapper.Map<PlayerDtoResponse>(newPlay.Player);
            response.Add(newPlayDtoResponse);

            var nextArtificialPlayer = game.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.Master)) ??
                throw new InternalServerErrorException("No Table Master was found.");
            if (game.Players.Any(x => x.Type.Equals(PlayerType.ArtificialPlayer)))
            {
                string gameIntelligenceChoice = await DefineNextPlayer(plays, game, cancellationToken);

                nextArtificialPlayer = game.Players.FirstOrDefault(x => x.Name.Equals(gameIntelligenceChoice)) ??
                    nextArtificialPlayer;
            }

            if (nextArtificialPlayer.Type.Equals(PlayerType.ArtificialPlayer))
            {
                var artificialPlayerPlay = await GenerateArtificialPlay(plays, game, nextArtificialPlayer, cancellationToken);
                _ = await _playService.CreatePlay(artificialPlayerPlay, cancellationToken) ??
                    throw new InternalServerErrorException($"Something went wrong while attempting to create the play");

                plays.Add(artificialPlayerPlay);
                var artificialPlayerPlayDtoResponse = _mapper.Map<PlayDtoResponse>(artificialPlayerPlay);
                artificialPlayerPlayDtoResponse.PlayerDtoResponse = _mapper.Map<PlayerDtoResponse>(artificialPlayerPlay.Player);
                response.Add(artificialPlayerPlayDtoResponse);
            }
            else
            {
                var masterPlay = await GenerateMasterPlay(plays, game, cancellationToken);
                _ = await _playService.CreatePlay(masterPlay, cancellationToken) ??
                    throw new InternalServerErrorException($"Something went wrong while attempting to create the master play");

                plays.Add(masterPlay);
                var masterPlayDtoResponse = _mapper.Map<PlayDtoResponse>(masterPlay);
                masterPlayDtoResponse.PlayerDtoResponse = _mapper.Map<PlayerDtoResponse>(masterPlay.Player);
                response.Add(masterPlayDtoResponse);
            }

            var playerList = GeneratePlayerList(game);
            var gameResume = await _generatePlaysResumeService.Handler(game.Plays, game, playerList.Content, cancellationToken);
            _ = await _playService.CreatePlay(gameResume, cancellationToken) ??
                throw new InternalServerErrorException($"Something went wrong while attempting to create the play");

            var changedItems = await _unitOfWork.CommitAsync(cancellationToken);
            if (changedItems == 0)
            {
                var errorMessage = "Something went wrong while attempting to create the user play.";
                _logger.LogError(errorMessage);
                throw new InternalServerErrorException(errorMessage);
            }

            return response.Where(x => !x.PlayerDtoResponse.Type.Equals(PlayerType.System)).ToList();
        }

        private async Task<Domain.Entity.Play> GenerateMasterPlay(List<Domain.Entity.Play> plays, Domain.Entity.Game game, CancellationToken cancellationToken)
        {
            List<IAiPromptRequest> promptList = [];
            promptList.Add(GeneratePlayersDescription(game));

            foreach (var play in plays)
            {
                var playerType = play.Player.Type;

                switch (playerType)
                {
                    case PlayerType.System:
                        promptList.Add(new AiPromptRequest(PromptRole.Instruction, "#Resume\n" + play.Prompt));
                        break;
                    case PlayerType.RealPlayer:
                        promptList.Add(new AiPromptRequest(PromptRole.Instruction, $"#Player: {play.Player.Name}\n" + play.Prompt));
                        break;
                    case PlayerType.Master:
                        promptList.Add(new AiPromptRequest(PromptRole.Model, $"#Master Table\n" + play.Prompt));
                        break;
                    case PlayerType.ArtificialPlayer:
                        promptList.Add(new AiPromptRequest(PromptRole.Instruction, $"#Player: {play.Player.Name}\n" + play.Prompt));
                        break;
                }
            }
            promptList.Add(new AiPromptRequest(PromptRole.User, MasterPlayCommand(game.SystemGame)));

            var iaResponse = await _generativeAIApiHttpService.GenerateResponseWithRoleBase(promptList, cancellationToken);

            if (iaResponse.IsNullOrEmpty())
                throw new BadGatewayException("No content was returned by the gateway");

            var masterPlayer = game.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.Master)) ??
                throw new NotFoundException($"No Master player was found to the game {game.Id}");

            return new Domain.Entity.Play(game, masterPlayer, iaResponse);
        }

        private static IAiPromptRequest GeneratePlayerList(Domain.Entity.Game game)
        {
            var players = "#Lista de players do jogo: \n";
            foreach (var player in game.Players.Where(x => x.Type.Equals(PlayerType.RealPlayer) || x.Type.Equals(PlayerType.ArtificialPlayer)))
                players += player.Name + "\n";

            return new AiPromptRequest(PromptRole.Instruction, players);
        }

        private static IAiPromptRequest GeneratePlayersDescription(Domain.Entity.Game game)
        {
            var players = "#Lista de players do jogo: \n";
            foreach (var player in game.Players.Where(x => x.Type.Equals(PlayerType.RealPlayer) || x.Type.Equals(PlayerType.ArtificialPlayer)))
                players += "# Player: " + player.Name + "\nDescription: " + player.Description + "\n\n";

            return new AiPromptRequest(PromptRole.Instruction, players);
        }

        private static string MasterPlayCommand(string systemGame)
        {
            //Blocked the dices
            return $"Você é um mestre de mesa (Master table) em um jogo de RPG, sistema {systemGame}. Nas informações repassadas, encontra-se um breve resumo de partidas anteriores, bem como as últimas jogadas dos demais players. Sua função é de conduzir a história, desenvolver o enredo, interpretar os NPCs, tornar o jogo sempre envolvente e emocionante, bem como quaisquer outras ações relativas a uma mestre de Mesa. \nObservações: 1. Você como Mestre da Mesa, nunca deve interpretar o papel dos Players! Também nunca deve ditar as ações dos Players!; 2. Foi acordado entre os jogadores que não será usado mecanismos de rolagem de dados. \n Agora, prossiga com a próxima orientação do Mestre da Mesa!";
        }

        private async Task<string> DefineNextPlayer(List<Domain.Entity.Play> plays, Domain.Entity.Game game, CancellationToken cancellationToken)
        {
            List<string> properties = new()
            {
                "name"
            };

            string playersOptions = "";
            //

            foreach(var player in game.Players.Where(x => x.Type.Equals(PlayerType.ArtificialPlayer) || x.Type.Equals(PlayerType.Master)))
            {
                playersOptions += "[{\"name\": \"" + player.Name + "\"}], ";
            }

            AiPromptRequest instruction = new(PromptRole.Instruction,
                "A resposta deve ser exclusivamente no seguite modelo [{\"name\": \"example\"}].");

            List<IAiPromptRequest> playPrompts = new()
            {
                instruction
            };

            IAiPromptRequest playersDescription = GeneratePlayerList(game);
            playPrompts.Add(new AiPromptRequest(PromptRole.User, playersDescription.Content));

            string resume = plays.Where(x => x.Player.Type.Equals(PlayerType.System)).FirstOrDefault()?.Prompt ?? "";
            if (!resume.IsNullOrEmpty())
            {
                playPrompts.Add(new AiPromptRequest(PromptRole.User, resume));
            }

            string realPlay = plays.Where(x => x.Player.Type.Equals(PlayerType.RealPlayer)).FirstOrDefault()?.Prompt ??
                throw new InternalServerErrorException("Something went wrong while attempting to get the real play.");
            playPrompts.Add(new AiPromptRequest(PromptRole.User, realPlay));

            playPrompts.Add(new AiPromptRequest(PromptRole.User,
                "Você está observando um jogo de RPG de Mesa. Com base nas informações anteriores, qual o participante mais adequado para " +
                $"responder a última mensagem? Você poderá escolher entre as seguintes opções: {playersOptions}"));

            var iaResponse = await _generativeAIApiHttpService.GenerateStructureJsonResponse(playPrompts, properties, cancellationToken, 0);

            if (iaResponse.IsNullOrEmpty())
            {
                var errorMessage = "No content was returned by the gateway";
                _logger.LogError(errorMessage);
                throw new BadGatewayException(errorMessage);
            }

            JsonElement charactersDescription = ConvertStringToArrayJson(iaResponse);

            string nextPlayer = charactersDescription.EnumerateArray().FirstOrDefault().GetProperty("name").GetString() ?? "";

            if (nextPlayer.IsNullOrEmpty())
            {
                var errorMessage = "Something went wrong while attempting to define the next player";
                _logger.LogError(errorMessage);
                throw new InternalServerErrorException(errorMessage);
            }

            return nextPlayer;
        }

        private JsonElement ConvertStringToArrayJson(string value)
        {
            try
            {
                return JsonSerializer.Deserialize<JsonElement>(value);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Something went wrong while attempting to convert the IA response to json: {ex.Message}";
                _logger.LogError(errorMessage);
                throw new InternalServerErrorException(errorMessage);
            }
        }

        private async Task<Domain.Entity.Play> GenerateArtificialPlay(List<Domain.Entity.Play> plays, Domain.Entity.Game game, Player currentPlayer, CancellationToken cancellationToken)
        {
            List<IAiPromptRequest> promptList = [];
            promptList.Add(GeneratePlayerList(game));
            promptList.Add(new AiPromptRequest(PromptRole.Instruction, $"#Your Player Description: {currentPlayer.InfoToString()}"));

            foreach (var play in plays)
            {
                var playerType = play.Player.Type;

                switch (playerType)
                {
                    case PlayerType.System:
                        promptList.Add(new AiPromptRequest(PromptRole.Instruction, "#Resume\n" + play.Prompt));
                        break;
                    case PlayerType.RealPlayer:
                        promptList.Add(new AiPromptRequest(PromptRole.Instruction, $"#Player: {play.Player.Name}\n" + play.Prompt));
                        break;
                    case PlayerType.Master:
                        promptList.Add(new AiPromptRequest(PromptRole.Instruction, $"#Master Table\n" + play.Prompt));
                        break;
                    case PlayerType.ArtificialPlayer:
                        var promptRole = PromptRole.Instruction;
                        if (play.Player.Id.Equals(currentPlayer.Id))
                            promptRole = PromptRole.Model;
                        promptList.Add(new AiPromptRequest(promptRole, $"#Player: {play.Player.Name}\n" + play.Prompt));
                        break;
                }
            }

            promptList.Add(new AiPromptRequest(PromptRole.User, ArtificialPlayCommand(currentPlayer)));

            var iaResponse = await _generativeAIApiHttpService.GenerateResponseWithRoleBase(promptList, cancellationToken);

            if (iaResponse.IsNullOrEmpty())
                throw new BadGatewayException("No content was returned by the gateway");

            return new Domain.Entity.Play(game, currentPlayer, iaResponse);
        }

        private static string ArtificialPlayCommand(Player currentPlayer)
        {
            return $"Você está participando de um jogo de RPG de Mesa. Seu personagem é {currentPlayer.Name}. Você deve se comportar unicamente como um jogador, interpretando seu personagem ou fazendo perguntas ao Mestre da Mesa quando achar necessário. Observações importantes: 1. Está vedado e proibido tomar ações por outros personagens e/ou NPCs; 2. Você não deve tomar ações próprias de um Mestre de Mesa, ou seja, não deve ditar o rumo da história ou acontecimentos; 3. Sempre que possível, dê preferência para a narração em primeira pessoa. Agora é com você! Com base nas informações repassadas, qual a sua próxima jogada?";
        }
    }
}

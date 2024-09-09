using AutoMapper;
using ei_back.Core.Application.Service.Game.Interfaces;
using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Application.UseCase.Play.Dtos;
using ei_back.Core.Application.UseCase.Play.Interfaces;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using ei_back.Infrastructure.ExternalAPIs.Dtos.Request;
using ei_back.Infrastructure.ExternalAPIs.Interfaces;
using ei_back.Migrations;
using Microsoft.IdentityModel.Tokens;

namespace ei_back.Core.Application.UseCase.Play
{
    public class NewUserPlayUseCase : INewUserPlayUseCase
    {
        private readonly IMapper _mapper;
        private readonly IPlayService _playService;
        private readonly IGameService _gameService;
        private readonly IGenerativeAIApiHttpService _generativeAIApiHttpService;

        public NewUserPlayUseCase(
            IMapper mapper,
            IPlayService playService,
            IGameService gameService,
            IGenerativeAIApiHttpService generativeAIApiHttpService)
        {
            _mapper = mapper;
            _playService = playService;
            _gameService = gameService;
            _generativeAIApiHttpService = generativeAIApiHttpService;
        }

        public async Task<List<PlayDtoResponse>> Handler(PlayDtoRequest playDtoRequest, string userName, CancellationToken cancellationToken)
        {
            var game = await _gameService.GetGameByIdAndOwnerUserName(playDtoRequest.GameId, userName, cancellationToken) ??
                throw new NotFoundException($"No game found with id {playDtoRequest.GameId} to user name {userName}.");

            var player = game.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.RealPlayer)) ??
                throw new InternalServerErrorException($"Something went wrong while attempting to get the real player info");

            var newPlay = new PlayEntity(game, player, playDtoRequest.Prompt);
            var playResponse = await _playService.CreatePlay(newPlay, cancellationToken) ??
                throw new InternalServerErrorException($"Something went wrong while attempting to create the play");

            var playDtoResponse = _mapper.Map<PlayDtoResponse>(playResponse);
            playDtoResponse.PlayerDtoResponse = _mapper.Map<PlayerDtoResponse>(player);

            //Master play
            //Get the last resume
            //Generate MasterPlay

            //Game Intelligence
            //Verify if exist ArtificialPlayer
            //Order Players by name and get the sequence by the last artificial player played

            //Artificial Player

            //Master play

            var response = new List<PlayDtoResponse>
            {
                playDtoResponse
            };

            return response;
        }

        private async Task<PlayEntity> GenerateMasterPlay(List<PlayEntity> plays, GameEntity game, CancellationToken cancellationToken)
        {
            List<IAiPromptRequest> promptList = [];
            var players = "#Lista de players do jogo: \n";
            foreach (var player in game.Players.Where(x => x.Type.Equals(PlayerType.RealPlayer) || x.Type.Equals(PlayerType.ArtificialPlayer)))
                players += player.Name + "\n";
            promptList.Add(new AiPromptRequest(PromptRole.System, players));

            foreach (var play in plays)
            {
                var playerType = play.Player.Type;

                switch (playerType)
                {
                    case PlayerType.System:
                        promptList.Add(new AiPromptRequest(PromptRole.System, "#Resume\n" + play.Prompt));
                        break;
                    case PlayerType.RealPlayer:
                        promptList.Add(new AiPromptRequest(PromptRole.System, $"#Player: {play.Player.Name}\n" + play.Prompt));
                        break;
                    case PlayerType.Master:
                        promptList.Add(new AiPromptRequest(PromptRole.Assistant, $"#Master Table\n" + play.Prompt));
                        break;
                    case PlayerType.ArtificialPlayer:
                        promptList.Add(new AiPromptRequest(PromptRole.System, $"#Player: {play.Player.Name}\n" + play.Prompt));
                        break;
                }
            }
            promptList.Add(new AiPromptRequest(PromptRole.User, MasterPlayCommand()));

            var iaResponse = await _generativeAIApiHttpService.GenerateResponseWithRoleBase(promptList, cancellationToken);

            if (iaResponse.IsNullOrEmpty())
                throw new BadGatewayException("No content was returned by the gateway");

            var masterPlayer = game.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.Master)) ??
                throw new NotFoundException($"No Master player was found to the game {game.Id}");

            return new PlayEntity(game, masterPlayer, iaResponse);
        }

        private static string MasterPlayCommand()
        {
            //Blocked the dices
            return "Você é um mestre de mesa (Master table) em um jogo de RPG, sistema {systemGame}. Nas informações repassadas, encontra-se um breve resumo de partidas anteriores, bem como as últimas jogadas dos demais players. Sua função é de conduzir a história, desenvolver o enredo, interpretar os NPCs, tornar o jogo sempre envolvente e emocionante, bem como quaisquer outras ações relativas a uma mestre de Mesa. \nObservações: 1. Importante atentar que está vedado e proibido ao Mestre de Mesa tomar ações pelos Players do jogo! 2. Foi acordado entre os jogadores que não será usado mecanismos de rolagem de dados. \n Agora, prossiga com a próxima orientação do Mestre da Mesa!";
        }
    }
}

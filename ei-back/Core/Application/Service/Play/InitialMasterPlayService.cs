using ei_back.Core.Application.Interfaces;
using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Domain.Entity;
using ei_back.Core.Domain.Enums;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using System.Runtime.CompilerServices;

namespace ei_back.Core.Application.Service.Play
{
    public class InitialMasterPlayService : IInitialMasterPlayService
    {
        private readonly ILogger<InitialMasterPlayService> _logger;
        private readonly IGenAi _genAi;
        private readonly IPlayService _playService;

        public InitialMasterPlayService(
            ILogger<InitialMasterPlayService> logger,
            IGenAi genAi,
            IPlayService playService)
        {
            _logger = logger;
            _genAi = genAi;
            _playService = playService;
        }

        public async IAsyncEnumerable<StreamAIDtoResponse> ExecuteStreamingAsync(
            Domain.Entity.Game game,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var initialPrompt = BuildInitialMasterPrompt(game);
            var promptList = new List<AiPromptRequest>
            {
                new(AiRole.System, initialPrompt),
                new(AiRole.User, "Crie uma introdução para o jogo como se fosse o início da campanha.")
            };

            var completedResponse = "";
            await foreach (var chunk in _genAi
                .StreamGetResponse(promptList, 650, cancellationToken)
                .WithCancellation(cancellationToken))
            {
                if (chunk.EventType == AIStreamEventType.Error)
                {
                    yield return new StreamAIDtoResponse
                    {
                        EventType = AIStreamEventType.Error,
                        Content = chunk.Content
                    };
                    yield break;
                }
                completedResponse += chunk.Content;
                yield return new StreamAIDtoResponse
                {
                    EventType = AIStreamEventType.Chunk,
                    Content = chunk.Content,
                };
            }

            var masterPlayer = game.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.Master)) ??
                throw new NotFoundException($"No Master player was found to the game {game.Id}");

            var masterPlay = new Domain.Entity.Play(game, masterPlayer, completedResponse);
            _ = await _playService.CreatePlay(masterPlay, cancellationToken) ??
                throw new InternalServerErrorException("Something went wrong while attempting to create the master play");
        }

        private static string BuildInitialMasterPrompt(Domain.Entity.Game game)
        {
            var realPlayer = game.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.RealPlayer));
            var playerInfo = realPlayer?.InfoToString() ?? "";
            return $"<guidance>\nVocê é um mestre de RPG de mesa em uma campanha de Dungeons & Dragons. " +
                   $"Lembre-se que como Mestre da Mesa, você NÃO deve agir como player ou ditar as ações do player. " +
                   $"O player da campanha está descrito dentro das tags <player></player>.\n</guidance>\n" +
                   $"<player>\n{playerInfo}\n</player>\n" +
                   $"<world-info>\n{game.WorldInfo}\n</world-info>";
        }
    }
}

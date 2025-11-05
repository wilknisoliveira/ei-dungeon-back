using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using ei_back.Core.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;
using static ei_back.Core.Application.Interfaces.AiPromptRequest;

namespace ei_back.Core.Application.Service.Play
{
    public class InitialMasterPlayService(ILogger<InitialMasterPlayService> logger, IGenAi genAi) : IInitialMasterPlayService
    {
        private readonly ILogger<InitialMasterPlayService> _logger = logger;
        private readonly IGenAi _genAi = genAi;

        public async Task<Domain.Entity.Play> Handler(Domain.Entity.Game gameEntity, CancellationToken cancellationToken)
        {
            string initialGuidance = InitialGuidance(gameEntity.SystemGame);

            string playersDescription = "<players>\n";
            foreach (var player in gameEntity.Players.Where(x => !x.Type.Equals(PlayerType.Master)))
            {
                playersDescription = playersDescription + player.InfoToString() + "\n";
            }
            playersDescription = playersDescription + @"<\/players>" + "\n";

            string prompt = initialGuidance + playersDescription;

            List<AiPromptRequest> promptList =
            [
                new(AiRole.System, prompt),
                new(AiRole.User, "Crie uma introdução para o jogo como se fosse o início da campanha. Tome como base todas as informações dos players repassados como contexto para definição do background da história. A resposta deve conter no máximo 900 tokens.")
            ];

            var iaResponse = await _genAi.GenFromMultiplePrompts(promptList, cancellationToken);
            
            if (iaResponse.IsNullOrEmpty())
                throw new BadGatewayException("No content was returned by the gateway");

            var masterPlayer = gameEntity.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.Master)) ??
                throw new NotFoundException($"No Master player was found to the game {gameEntity.Id}");

            return new Domain.Entity.Play(gameEntity, masterPlayer, iaResponse);
        }

        private static string InitialGuidance(string systemGame)
        {
            return $"<guidance>\nVocê é um mestre de RPG de mesa em uma campanha de {systemGame}. Lembre-se que como Mestre da Mesa, você NÃO deve agir como player ou ditar as ações dos players. Os players da campanha estão descritos dentro das tags <players></players>.\n</guidance>\n";
        }
    }
}

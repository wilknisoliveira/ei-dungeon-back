using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using ei_back.Core.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace ei_back.Core.Application.Service.Play
{
    public class InitialMasterPlayService(ILogger<InitialMasterPlayService> logger, IGenAi genAi) : IInitialMasterPlayService
    {
        private readonly ILogger<InitialMasterPlayService> _logger = logger;
        private readonly IGenAi _genAi = genAi;

        public async Task<Domain.Entity.Play> Handler(Domain.Entity.Game gameEntity, CancellationToken cancellationToken)
        {
            string initialGuidance = InitialGuidance();

            var player = gameEntity.Players.Find(x => x.Type.Equals(PlayerType.RealPlayer));
            string playerDescription = "<player>\n" + player!.InfoToString() + "\n" + @"<\/player>" + "\n";

            string prompt = $"{initialGuidance}\n{playerDescription}\n" +
                            $"<world-info>\n{gameEntity.WorldInfo}\n</world-info>";
            
            List<AiPromptRequest> promptList =
            [
                new(AiRole.System, prompt),
                new(
                    AiRole.User, 
                    "Crie uma introdução para o jogo como se fosse o início da campanha. " + 
                    "Tome como base todas as informações repassadas como contexto para crição da introdução.")
            ];

            var iaResponse = await _genAi.GenFromMultiplePrompts(promptList, 650, cancellationToken);
            
            if (iaResponse.IsNullOrEmpty())
                throw new BadGatewayException("No content was returned by the gateway");

            var masterPlayer = gameEntity.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.Master)) ??
                throw new NotFoundException($"No Master player was found to the game {gameEntity.Id}");

            return new Domain.Entity.Play(gameEntity, masterPlayer, iaResponse);
        }

        private static string InitialGuidance()
        {
            return $"<guidance>\nVocê é um mestre de RPG de mesa em uma campanha de Dungeons & Dragons. Lembre-se que como Mestre da Mesa, você NÃO deve agir como player ou ditar as ações do player. O player da campanha está descrito dentro das tags <player></player>.\n</guidance>\n";
        }
    }
}

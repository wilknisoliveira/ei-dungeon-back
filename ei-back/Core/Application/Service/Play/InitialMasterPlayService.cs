using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using ei_back.Infrastructure.ExternalAPIs.Dtos.Request;
using ei_back.Infrastructure.ExternalAPIs.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace ei_back.Core.Application.Service.Play
{
    public class InitialMasterPlayService : IInitialMasterPlayService
    {
        private readonly IGenerativeAIApiHttpService _generativeAIApiHttpService;
        private readonly ILogger<InitialMasterPlayService> _logger;

        public InitialMasterPlayService(
            IGenerativeAIApiHttpService generativeAIApiHttpService,
            ILogger<InitialMasterPlayService> logger)
        {
            _generativeAIApiHttpService = generativeAIApiHttpService;
            _logger = logger;
        }

        public async Task<PlayEntity> Handler(GameEntity gameEntity, CancellationToken cancellationToken)
        {
            string initialGuidance = InitialGuidance(gameEntity.SystemGame);

            string playersDescription = "<players>\n";
            foreach (var player in gameEntity.Players.Where(x => !x.Type.Equals(PlayerType.Master)))
            {
                playersDescription = playersDescription + player.InfoToString() + "\n";
            }
            playersDescription = playersDescription + @"<\/players>" + "\n";

            string prompt = initialGuidance + playersDescription;

            List<IAiPromptRequest> promptList =
            [
                new AiPromptRequest(PromptRole.System, prompt),
                new AiPromptRequest(PromptRole.User, "Crie uma introdução para o jogo como se fosse o início da campanha. Tome como base todas as informações dos players repassados como contexto para definição do background da história. A resposta deve conter no máximo 900 tokens.")
            ];

            var iaResponse = await _generativeAIApiHttpService.GenerateResponseWithRoleBase(promptList, cancellationToken);

            if (iaResponse.IsNullOrEmpty())
                throw new BadGatewayException("No content was returned by the gateway");

            var masterPlayer = gameEntity.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.Master)) ??
                throw new NotFoundException($"No Master player was found to the game {gameEntity.Id}");

            return new PlayEntity(gameEntity, masterPlayer, iaResponse);
        }

        private static string InitialGuidance(string systemGame)
        {
            return $"<guidance>\nVocê é um mestre de RPG de mesa em uma campanha de {systemGame}. Lembre-se que como Mestre da Mesa, você NÂO deve agir como player ou ditar as ações dos players. Os players da campanha estão descritos dentro das tags <players></players>.\n</guidance>\n";
        }
    }
}

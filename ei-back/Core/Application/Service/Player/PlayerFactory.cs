using ei_back.Core.Application.Service.Player.Interfaces;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using ei_back.Infrastructure.ExternalAPIs.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;

namespace ei_back.Core.Application.Service.Player
{
    public class PlayerFactory : IPlayerFactory
    {
        private readonly IGenerativeAIApiHttpService _generativeAIApiHttpService;
        private readonly ILogger<PlayerFactory> _logger;

        public PlayerFactory(IGenerativeAIApiHttpService generativeAIApiHttpService, ILogger<PlayerFactory> logger)
        {
            _generativeAIApiHttpService = generativeAIApiHttpService;
            _logger = logger;
        }

        public async Task<List<Domain.Entity.Player>> BuildArtificialPlayersAndMaster(int numberOfArtificalPlayers, Domain.Entity.Game game, CancellationToken cancellationToken = default)
        {
            List<Domain.Entity.Player> players =
            [
                new("Table Master", "RPG Table Master", PlayerType.Master)
            ];

            if (numberOfArtificalPlayers > 0)
            {
                var iaResponse = await _generativeAIApiHttpService.GenerateSimpleResponse(PlayersGeneratorPrompt(numberOfArtificalPlayers, game.SystemGame), cancellationToken);

                if (iaResponse.IsNullOrEmpty())
                {
                    var errorMessage = "No content was returned by the gateway";
                    _logger.LogError(errorMessage);
                    throw new BadGatewayException(errorMessage);
                }

                List<Domain.Entity.Player> artificialPlayers = [];
                for (int i = 1; i <= numberOfArtificalPlayers; i++)
                {
                    string namePattern = @$"<character-name-{i}>(.*?)<\/character-name-{i}>";
                    Match nameMatch = Regex.Match(iaResponse, namePattern, RegexOptions.Singleline);

                    string contentPattern = @$"<content-{i}>(.*?)<\/content-{i}>";
                    Match contentMatch = Regex.Match(iaResponse, contentPattern, RegexOptions.Singleline);

                    if (nameMatch.Success && contentMatch.Success)
                    {
                        var playerName = nameMatch.Groups[1].Value.Replace("\n", "");
                        var playerContent = contentMatch.Groups[1].Value;

                        if (playerName.IsNullOrEmpty() || playerContent.IsNullOrEmpty())
                            ConvertRegexError();

                        var player = new Domain.Entity.Player(playerName, playerContent, PlayerType.ArtificialPlayer);
                        player.SetCreatedDate(DateTime.Now);
                        players.Add(player);
                    }
                    else
                        ConvertRegexError();
                }
            }

            players.ForEach(x => x.SetGame(game));

            return players;
        }

        private static string PlayersGeneratorPrompt(int quantity, string systemGame)
        {
            return $"Gere {quantity} personagens aleatórios e jogáveis para o sistema {systemGame}. Cada personagem deve seguir a estrutura com delimitadores: <character-name-X>Insira aqui o conteúdo</character-name-X> <content-X>Insira aqui o conteúdo</content-X>. <character-name-X> deve conter apenas o nome do personagem. <content-X> deve conter todas as informações do personagem, incluindo raça, classe, background, alinhamento, habilidades, aparência física, história, personalidade, itens e motivação. Não inclua nada fora dos delimitadores.";
        }

        private void ConvertRegexError()
        {
            var errorMessage = "The gateway content is out of prompt pattern";
            _logger.LogError(errorMessage);
            throw new InternalServerErrorException(errorMessage);
        }
    }
}

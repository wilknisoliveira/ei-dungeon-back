using ei_back.Domain.Game;
using ei_back.Domain.Player.Interfaces;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using ei_back.Infrastructure.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;

namespace ei_back.Domain.Player
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

        public async Task<List<PlayerEntity>> BuildArtificialPlayersAndMaster(int numberOfArtificalPlayers, GameEntity game, CancellationToken cancellationToken = default)
        {
            List<PlayerEntity> players =
            [
                new("Master", "Master Player", PlayerType.Master)
            ];

            if (numberOfArtificalPlayers > 0)
            {
                var iaResponse = await _generativeAIApiHttpService.GenerateSimpleResponse(PlayersGeneratorPrompt(), cancellationToken);

                if (iaResponse.IsNullOrEmpty())
                {
                    var errorMessage = "No content was returned by the gateway";
                    _logger.LogError(errorMessage);
                    throw new BadGatewayException(errorMessage);
                }

                List<PlayerEntity> artificialPlayers = [];
                for (int i = 1; i <= numberOfArtificalPlayers; i++)
                {
                    string namePattern = @$"<character-name-{i}>(.*?)<\/character-name-{i}>";
                    Match nameMatch = Regex.Match(iaResponse, namePattern);

                    string contentPattern = @$"<content-{i}>(.*?)<\/content-{i}>";
                    Match contentMatch = Regex.Match(iaResponse, contentPattern);

                    if (nameMatch.Success && contentMatch.Success)
                    {
                        players.Add(new(nameMatch.Groups[1].Value, contentMatch.Groups[1].Value, PlayerType.ArtificialPlayer));
                    }
                    else
                    {
                        var errorMessage = "The gateway content is out of prompt pattern";
                        _logger.LogError(errorMessage);
                        throw new Exception(errorMessage);
                    }
                }
            }

            players.ForEach(x => x.SetGame(game));

            return players;
        }

        private static string PlayersGeneratorPrompt()
        {
            return "Gere a seguinte quantidade de personagens aleatórios e jogáveis para o sistema D&D 5e: 3.  <character-name-{number}>Insira exclusivamente o nome gerado para o personagem neste espaço. Nenhuma outra palavra, frase ou caractere deve ser adicionado dentro destes delimitadores.</character-name-{number}> <content-{number}>Insira todas as informações do personagem estritamente dentro desta seção. Absolutamente nada além disso deve ser incluído dentro destes delimitadores. As informações podem conter: raça, classe, background, alinhamento, habilidades, aparência física, história, personalidade, itens e motivação. </content-{number}> IMPORTANTE: A resposta à este prompt não deve conter nenhuma informação, texto ou caractere fora dos delimitadores. Não adicione nenhuma informação, texto ou caractere a mais do que o solicitado dentro dos delimitadores.";
        }
    }
}

using ei_back.Core.Application.Repository;
using ei_back.Core.Application.Service.Player.Interfaces;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using ei_back.Infrastructure.ExternalAPIs.Dtos.Request;
using ei_back.Infrastructure.ExternalAPIs.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace ei_back.Core.Application.Service.Player
{
    public class PlayerFactory : IPlayerFactory
    {
        private readonly IGenerativeAIApiHttpService _generativeAIApiHttpService;
        private readonly ILogger<PlayerFactory> _logger;
        private readonly IGameInfoRepository _gameInfoRepository;

        public PlayerFactory(
            IGenerativeAIApiHttpService generativeAIApiHttpService,
            ILogger<PlayerFactory> logger,
            IGameInfoRepository gameInfoRepository)
        {
            _generativeAIApiHttpService = generativeAIApiHttpService;
            _logger = logger;
            _gameInfoRepository = gameInfoRepository;
        }

        public async Task<List<Domain.Entity.Player>> BuildArtificialPlayersAndMaster(int numberOfArtificalPlayers, Domain.Entity.Game game, CancellationToken cancellationToken = default)
        {
            List<Domain.Entity.Player> players =
            [
                new("Table Master", "RPG Table Master", PlayerType.Master)
            ];

            if (numberOfArtificalPlayers > 0)
            {
                List<GameInfo> charactersName = (await _gameInfoRepository.GetLimitRandomValuesByType(numberOfArtificalPlayers, Domain.Enums.InfoType.CharacterName, cancellationToken)).ToList();

                List<string> characters = await RandomPlayersGenerate(charactersName, numberOfArtificalPlayers, cancellationToken);

                var prompt = PlayersGeneratorPrompt(game.SystemGame, characters);

                List<string> properties = new()
                {
                    "description"
                };
                var systemGuide = StructuredResponseGuide(properties);

                List<IAiPromptRequest> promptList = new()
                {
                    new AiPromptRequest(PromptRole.User, prompt),
                    new AiPromptRequest(PromptRole.Instruction, systemGuide)
                };

                var iaResponse = await _generativeAIApiHttpService.GenerateStructureJsonResponse(promptList, properties, cancellationToken, 0);

                if (iaResponse.IsNullOrEmpty())
                {
                    var errorMessage = "No content was returned by the gateway";
                    _logger.LogError(errorMessage);
                    throw new BadGatewayException(errorMessage);
                }

                JsonElement charactersDescription = ConvertStringToArrayJson(iaResponse);

                var index = 0;
                foreach (var characterDescription in charactersDescription.EnumerateArray())
                {
                    var playerName = charactersName[index].Value;

                    if (playerName.IsNullOrEmpty())
                    {
                        var errorMessage = "The gateway content is out of prompt pattern";
                        _logger.LogError(errorMessage);
                        throw new InternalServerErrorException(errorMessage);
                    }

                    var playerContent = characters[index] + ", Description: " + characterDescription.GetProperty("description").GetString();

                    var player = new Domain.Entity.Player(playerName, playerContent, PlayerType.ArtificialPlayer);
                    player.SetCreatedDate(DateTime.Now);
                    players.Add(player);

                    index++;
                }
            }

            players.ForEach(x => x.SetGame(game));

            return players;
        }

        private async Task<List<string>> RandomPlayersGenerate(List<GameInfo> charactersName, int quantity, CancellationToken cancellationToken)
        {
            List<GameInfo> charactersRace = (await _gameInfoRepository.GetLimitRandomValuesByType(quantity, Domain.Enums.InfoType.CharacterRace, cancellationToken)).ToList();

            List<GameInfo> charactersClass = (await _gameInfoRepository.GetLimitRandomValuesByType(quantity, Domain.Enums.InfoType.CharacterClass, cancellationToken)).ToList();

            if (charactersName.IsNullOrEmpty() || charactersRace.IsNullOrEmpty() || charactersClass.IsNullOrEmpty())
            {
                var errorMessage = "Verify with the admin if the game info is properly register.";
                _logger.LogError(errorMessage);
                throw new InternalServerErrorException(errorMessage);
            }

            var characters = new List<string>();
            for (int i = 0; i < quantity; i++)
                characters.Add($"#Personagem {i + 1}# Nome: {charactersName[i].Value}, Raça: {charactersRace[i].Value}, Classe: {charactersClass[i].Value}, Alignment: {RandomAlignment()}");

            return characters;
        }


        private static string RandomAlignment()
        {
            List<string> alignments = new()
            {
                "Lawful Good — Just",
                "Neutral Good — Benefactor",
                "Chaotic Good — Revolutionary",
                "Lawful Neutral — Judge",
                "Neutral — Realist",
                "Chaotic Neutral — Individualist",
                "Lawful Evil — Tyrant",
                "Neutral Evil — Mercenary",
                "Chaotic Evil — Destroyer"
            };

            Random random = new();

            int randomIndex = random.Next(alignments.Count) - 1;

            return alignments[randomIndex];
        }


        private static string PlayersGeneratorPrompt(string systemGame, List<string> characters)
        {
            var allCharacters = string.Join(" ", characters);

            return $"Considere os seguintes personagens de RPG {systemGame}: {allCharacters} Gere uma descrição para cada personagem contemplando aparência, história, personalidade, motivação, habilidades, itens e etc.";
        }
        

        private static string StructuredResponseGuide(List<string> properties)
        {
            //TODO: dinamic response by properties
            return ("A resposta deve ser exclusivamente no seguite modelo [{\"description\": \"example\"}]. Segue um exemplo: [{\"description\": \"This is the description for example 1.\"}, {\"description\": \"This is the description for example 2.\"}, {\"description\": \"This is the description for example 3.\"}]");
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
    }
}

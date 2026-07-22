using ei_back.Core.Application.Interfaces;
using ei_back.Core.Application.Repository;
using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Context.Interfaces;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using Microsoft.IdentityModel.Tokens;

namespace ei_back.Core.Application.Service.Play
{
    public class GeneratePlaysSummaryService : IGeneratePlaysSummaryService
    {
        private readonly ILogger<GeneratePlaysSummaryService> _logger;
        private readonly IPlayRepository _playRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenAi _genAi;
        private readonly IGameRepository _gameRepository;

        public GeneratePlaysSummaryService(
            ILogger<GeneratePlaysSummaryService> logger,
            IUnitOfWork unitOfWork,
            IPlayRepository playRepository,
            IGenAi genAi, IGameRepository gameRepository)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _playRepository = playRepository;
            _genAi = genAi;
            _gameRepository = gameRepository;
        }

        public async Task Handler(
            Guid gameId, 
            List<Domain.Entity.Play> plays, 
            CancellationToken cancellationToken)
        {
            var game = await _gameRepository.FindByIdAsync(gameId, cancellationToken: cancellationToken);
            
            var systemPlayer = game.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.System)) ??
                throw new InternalServerErrorException("Something went wrong while attempting to get the system player entity");

            // It's necessary instantiate the Play at the beginning to ensure the createdAt date
            var newPlay = new Domain.Entity.Play(game, systemPlayer, "");
            
            List<AiPromptRequest> promptList = [];
            var lastSystemPlay = plays.FirstOrDefault(x => x.Player.Type.Equals(PlayerType.System));
            if (lastSystemPlay != null)
                promptList.Add(new AiPromptRequest(AiRole.Assistant, lastSystemPlay.Prompt));
            
            var realPlayer = game.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.RealPlayer));
            var systemPrompt = $"<player-info>\n{realPlayer?.InfoToString()}\n</player-info> \n\n " +
                               $"{GetAssistantPersonality()}";
            promptList.Add(new AiPromptRequest(AiRole.System, systemPrompt));
            
            var lastPlays = "# Last Plays\n";
            foreach (var play in plays.Where(x => !x.Player.Type.Equals(PlayerType.System)))
            {
                if (play.Player.Type.Equals(PlayerType.Master))
                    lastPlays += $"## Master Table: \n";
                else
                    lastPlays += $"## {play.Player.Name}(player): \n";

                lastPlays += play.Prompt + "\n\n";
            }

            var userPrompt = $"<plays>\n{lastPlays}\n</plays> \n\n {PromptCommand()}";
            promptList.Add(new AiPromptRequest(AiRole.User, userPrompt));

            var iaResponse = "";
            try
            {
                iaResponse = await _genAi.GetResponse(promptList, 2000, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Something went wrong while attempting to generate the summary: {ex}", ex);
                return;
            }

            if (iaResponse.IsNullOrEmpty())
            {
                _logger.LogError("Something went wrong while attempting to generate the summary.");
                return;
            }

            newPlay.SetPrompt(iaResponse);

            _ = await _playRepository.CreateAsync(newPlay, cancellationToken) ??
                throw new InternalServerErrorException($"Something went wrong while attempting to create the master play");

            var changedItems = await _unitOfWork.CommitAsync(cancellationToken);
            if (changedItems == 0)
            {
                _logger.LogError("Something went wrong while attempting to save the summary.");
            }
        }

        private static string GetAssistantPersonality()
        {
            return "Você está observando uma partida de RPG de mesa e é responsável por fazer resumos das partidas.";
        }

        private static string PromptCommand()
        {
            const int minOutputTokens = 500;
            // LLMs better understand characters instead of tokens. So it's why we convert it by inference.
            return $"Faça um resumo de todas as informações passadas, inclusive das últimas jogadas localizadas " +
                   $"dentro das tags <play>. O resumo gerado deve ter no mínimo {minOutputTokens * 4} caracteres. " +
                   $"O texto a ser gerado será utilizado posteriormente por uma IA generativa como base de dados " +
                   $"para geração de novos resumos, ou seja, a linguagem e síntese utilizada deve ser direcionado " +
                   $"para leitura por IA. Não se preocupe em economizar tokens, priorizando o registro da história " +
                   $"e os detalhes importantes.";
        }
    }
}

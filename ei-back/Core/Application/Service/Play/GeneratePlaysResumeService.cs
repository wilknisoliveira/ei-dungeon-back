using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Context.Interfaces;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using Microsoft.IdentityModel.Tokens;
using ei_back.Core.Application.Interfaces;

namespace ei_back.Core.Application.Service.Play
{
    public class GeneratePlaysResumeService : IGeneratePlaysResumeService
    {
        private readonly ILogger<GeneratePlaysResumeService> _logger;
        private readonly IPlayService _playService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenAi _genAi;

        public GeneratePlaysResumeService(
            ILogger<GeneratePlaysResumeService> logger,
            IUnitOfWork unitOfWork,
            IPlayService playService,
            IGenAi genAi)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _playService = playService;
            _genAi = genAi;
        }

        public async Task Handler(List<Domain.Entity.Play> plays, Domain.Entity.Game game, string initialAddicionalInfo, CancellationToken cancellationToken)
        {
            var systemPlayer = game.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.System)) ??
                throw new InternalServerErrorException("Something went wrong while attempting to get the system player entity");

            // It's necessary instantiate the PLay at the beginning to ensure the createdAt date
            var newPlay = new Domain.Entity.Play(game, systemPlayer, "");
            
            var lastSystemPlay = plays.FirstOrDefault(x => x.Player.Type.Equals(PlayerType.System));

            List<AiPromptRequest> promptList = [];
            if (lastSystemPlay != null)
                promptList.Add(new AiPromptRequest(AiRole.Assistant, lastSystemPlay.Prompt));
            
            var systemPrompt = "";
            if (!initialAddicionalInfo.IsNullOrEmpty())
                systemPrompt += $"<additional-info>\n{initialAddicionalInfo}\n</additional-info>\n\n";
            
            var lastPlays = "# Last Plays\n";
            foreach (var play in plays.Where(x => !x.Player.Type.Equals(PlayerType.System)))
            {
                if (play.Player.Type.Equals(PlayerType.Master))
                    lastPlays += $"## Master Table: \n";
                else
                    lastPlays += $"## {play.Player.Name}(player): \n";

                lastPlays += play.Prompt + "\n\n";
            }

            systemPrompt += $"<plays>\n{lastPlays}\n</plays>";
            promptList.Add(new AiPromptRequest(AiRole.System, systemPrompt));

            promptList.Add(new AiPromptRequest(AiRole.User, PromptCommand()));

            var iaResponse = "";
            try
            {
                iaResponse = await _genAi.GenFromMultiplePrompts(promptList, 2000, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Something went wrong while attempting to generate resume: {ex}", ex);
                return;
            }

            if (iaResponse.IsNullOrEmpty())
            {
                _logger.LogError("Something went wrong while attempting to generate resume.");
                return;
            }

            newPlay.SetPrompt(iaResponse);

            _ = await _playService.CreatePlay(newPlay, cancellationToken) ??
                throw new InternalServerErrorException($"Something went wrong while attempting to create the master play");

            var changedItems = await _unitOfWork.CommitAsync(cancellationToken);
            if (changedItems == 0)
            {
                _logger.LogError("Something went wrong while attempting to create the user play.");
            }
        }

        private static string PromptCommand()
        {
            const int minOutputTokens = 500;
            // LLMs better understand characters instead of tokens. So it's why we convert it by inference.
            return $"Você está observando uma partida de RPG de mesa. Faça um resumo de todas as informações passadas. O resumo gerado deve ter no mínimo {minOutputTokens * 4} caracteres. O texto a ser gerado será utilizado posteriormente por uma IA generativa como base de dados para geração de novos resumos, ou seja, a linguagem e síntese utilizada deve ser direcionado para leitura por IA. Não se preocupe em economizar tokens, priorizando o registro da história e os detalhes importantes.";
        }
    }
}

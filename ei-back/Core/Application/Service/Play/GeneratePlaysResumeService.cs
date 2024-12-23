using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Context.Interfaces;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using ei_back.Infrastructure.ExternalAPIs.Dtos.Request;
using ei_back.Infrastructure.ExternalAPIs.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Data;

namespace ei_back.Core.Application.Service.Play
{
    public class GeneratePlaysResumeService : IGeneratePlaysResumeService
    {
        private readonly IGenerativeAIApiHttpService _generativeAIApiHttpService;
        private readonly ILogger<GeneratePlaysResumeService> _logger;
        private readonly IPlayService _playService;
        private readonly IUnitOfWork _unitOfWork;

        public GeneratePlaysResumeService(
            IGenerativeAIApiHttpService generativeAIApiHttpService,
            ILogger<GeneratePlaysResumeService> logger,
            IUnitOfWork unitOfWork,
            IPlayService playService)
        {
            _generativeAIApiHttpService = generativeAIApiHttpService;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _playService = playService;
        }

        public async Task Handler(List<Domain.Entity.Play> plays, Domain.Entity.Game game, string initialAddicionalInfo, CancellationToken cancellationToken)
        {
            var systemPlayer = game.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.System)) ??
                throw new InternalServerErrorException("Something went wrong while attempting to get the system player entity");

            var newPlay = new Domain.Entity.Play(game.Id, systemPlayer.Id, "");
            newPlay.SetCreatedDate(DateTime.Now);

            var lastSystemPlay = plays.FirstOrDefault(x => x.Player.Type.Equals(PlayerType.System));

            List<IAiPromptRequest> promptList = [];
            if (lastSystemPlay != null)
                promptList.Add(new AiPromptRequest(PromptRole.Model, "#Resume\n" + lastSystemPlay.Prompt));

            if (!initialAddicionalInfo.IsNullOrEmpty())
                promptList.Add(new AiPromptRequest(PromptRole.Instruction, "#Additional Info\n" + initialAddicionalInfo));

            var lastPlays = "#Last Plays\n";
            foreach (var play in plays.Where(x => !x.Player.Type.Equals(PlayerType.System)))
            {
                if (play.Player.Type.Equals(PlayerType.Master))
                    lastPlays += $"Master Table: \n";
                else
                    lastPlays += $"Player: {play.Player.Name} \n";

                lastPlays += play.Prompt + "\n\n";
            }
            promptList.Add(new AiPromptRequest(PromptRole.Instruction, lastPlays));

            promptList.Add(new AiPromptRequest(PromptRole.User, PromptCommand()));

            var iaResponse = "";
            try
            {
                iaResponse = await _generativeAIApiHttpService.GenerateResponseWithRoleBase(promptList, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Something went wrong while attempting to generate resume: " + ex);
            }

            if (iaResponse.IsNullOrEmpty())
                throw new BadGatewayException("No content was returned by the gateway");

            newPlay.SetPrompt(iaResponse);

            var response = await _playService.CreatePlay(newPlay, cancellationToken) ??
                throw new InternalServerErrorException($"Something went wrong while attempting to create the master play");

            var changedItems = await _unitOfWork.CommitAsync(cancellationToken);
            if (changedItems == 0)
            {
                var errorMessage = "Something went wrong while attempting to create the user play.";
                _logger.LogError(errorMessage);
                throw new InternalServerErrorException(errorMessage);
            }
        }

        private static string PromptCommand()
        {
            var maxOutputTokens = 2000;
            var minOutputTokens = 100;
            return $"Você está observando uma partida de RPG de mesa. Faça um resumo de todas as informações passadas. O resumo gerado deve ter no mínimo {minOutputTokens} tokens e no máximo {maxOutputTokens} tokens. O texto a ser gerado será utilizado posteriormente por uma IA generativa como base de dados para geração de novos resumos, ou seja, a linguagem e síntese utilizada deve ser direcionado para leitura por IA. Utilize bem a quantidade máxima de {maxOutputTokens} tokens, de forma a registrar a história e os detalhes importantes.";
        }
    }
}

using ei_back.Core.Application.Interfaces;
using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Application.UseCase.Play.Dtos;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using Microsoft.IdentityModel.Tokens;

namespace ei_back.Core.Application.Service.Play;

public class PlayAnalyzerService(ILogger<PlayAnalyzerService> logger, IGenAi genAi) : IPlayAnalyzerService
{
    private readonly ILogger<PlayAnalyzerService> _logger = logger;
    private readonly IGenAi _genAi = genAi;

    public async Task<AnalyzerDtoResponse> Handler(
        List<Domain.Entity.Play> plays, 
        Domain.Entity.Game game, 
        CancellationToken cancellationToken)
    {
        var realPlayer = game.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.RealPlayer));

        var systemPrompt = $"<master-instruction>\n{GetAssistantPersonality()}\n</master-instruction>\n" +
                           $"<player-info>\n{realPlayer!.InfoToString()}\n</player-info>\n" + 
                           $"<world-info>\n{game.WorldInfo}\n</world-info>\n";
        
        var newPlay = plays.Where(x => x.Player.Type.Equals(PlayerType.RealPlayer))
            .OrderByDescending(x => x.CreatedAt).First();
        
        var playsWithoutTheLastPlay = plays.Where(x => 
            !x.Player.Type.Equals(PlayerType.RealPlayer) && !x.Player.CreatedAt.Equals(newPlay.CreatedAt))
            .OrderByDescending(x => x.CreatedAt);

        var lastPlays = "<last-plays>\n";
        foreach (var play in playsWithoutTheLastPlay)
        {
            lastPlays += play.Player.Type switch
            {
                PlayerType.System => $"# Summary: \n",
                PlayerType.Master => $"# Master Table: \n",
                _ => $"# {play.Player.Name}(player): \n"
            };

            lastPlays += play.Prompt + "\n\n";
        }
        lastPlays += "\n</last-plays>\n";

        systemPrompt += lastPlays;

        var userPrompt = $"{GetUserPrompt()}\n<user-play># {newPlay.Player.Name}(player):\n{newPlay.Prompt}</user-play>\n>";

        List<AiPromptRequest> promptList = [
            new(AiRole.System, systemPrompt),
            new(AiRole.User, userPrompt),];
        
        try
        {
            return await _genAi.GetStructureResponse<AnalyzerDtoResponse>(promptList, 50, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError("Something went wrong while attempting to analyse the new play: {ex}", ex);
            throw new BadGatewayException("Something went wrong while attempting to analyse the new play.");
        }
    }

    private string GetAssistantPersonality()
    {
        return "Você é responsável por analisar a jogada do usuário em uma partida de Dungeons & Dragons. Sua resposta" +
               "deve corresponder a uma das seguintes opções:\n" +
               "- Ok: Jogada válida, plausível, segue as regras e a lógica narrativa;\n" +
               "- InvalidPlay: Jogada que tenta quebrar as regras fundamentais, como controlar NPCs, definir " +
               "consequências diretamente, alterar o mundo sem permissão, assumir poderes irreais, burlar a " +
               "coerência narrativa;\n" +
               "- RollDice: Jogada possível, mas com risco e incerteza. Exige rolagem de dados;\n" +
               "- ClarificationNeeded: Quando a jogada é ambígua ou incompleta. Ex: “Tento me esconder” <- onde? " +
               "como? de quem? | “Ataco ele” <- qual arma? qual alvo? | “Procuro coisas úteis” <- onde? " +
               "como você procura?\n" +
               "- PlayerDied: Quando o personagem do jogador morreu.\n\n" +
               "Além de responder o resultado, esclareça o motivo da sua análise. Ex:\n" +
               "{'Result': 'RollDices', 'Reason': 'O personagem está tentando realizar uma ação ousada que " +
               "depende de fatores incertos'}"; 
    }

    private string GetUserPrompt()
    {
        return "Analise a jogada do usuário abaixo em <user-play>, considerando as informações do mundo <world-info>, o resumo das" +
               "jogadas antigas em '# Summary', bem como as últimas jogadas em <last-plays>.";
    }
}
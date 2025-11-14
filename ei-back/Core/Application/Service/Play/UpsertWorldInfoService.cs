using ei_back.Core.Application.Interfaces;
using ei_back.Core.Application.Repository;
using ei_back.Core.Application.Service.Game.Interfaces;
using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Application.UseCase.Play.Dtos;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Context.Interfaces;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using Microsoft.IdentityModel.Tokens;

namespace ei_back.Core.Application.Service.Play;

public class UpsertWorldInfoService(
    ILogger<UpsertWorldInfoService> logger,
    IPlayService playService,
    IUnitOfWork unitOfWork,
    IGenAi genAi,
    IGameRepository gameRepository)
    : IUpsertWorldInfoService
{
    private readonly ILogger<UpsertWorldInfoService> _logger = logger;
    private readonly IPlayService _playService = playService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IGenAi _genAi = genAi;
    private readonly IGameRepository _gameRepository = gameRepository;

    public async Task<string> Handler(string playerInfo, CancellationToken cancellationToken)
    {
        var systemPrompt = GetMasterPersonality();
        systemPrompt += "\n\n<player>\n" + playerInfo + "\n" + @"<\/player>" + "\n";

        List<AiPromptRequest> promptList =
        [
            new(AiRole.System, systemPrompt),
            new(AiRole.User, GetWorldInfoGenerationPrompt())
        ];
            
        var iaResponse = await _genAi.GenFromMultiplePrompts<WorldInfoDtoResponse>(promptList, 2000, cancellationToken);
            
        if (iaResponse.IsNullOrEmpty())
            throw new BadGatewayException("No content was returned by the gateway");
            
        return iaResponse;
    }

    public async Task Handler(Guid gameId, List<Domain.Entity.Play> plays, CancellationToken cancellationToken)
    {
        var game = await _gameRepository.FindByIdAsync(gameId, cancellationToken: cancellationToken);
        var realPlayer = game.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.RealPlayer));
        
        var systemPrompt = $"{GetMasterPersonality()} \n {GetAdditionalUpdatePrompt()}";
        systemPrompt += "\n\n<player>\n" + realPlayer?.InfoToString() + "\n" + @"<\/player>" + "\n";
        
        var assistantPrompt = $"<world-info>\n{game.WorldInfo}\n</world-info>";
        
        var lastPlays = "# Last Plays\n";
        foreach (var play in plays.Where(x => !x.Player.Type.Equals(PlayerType.System)))
        {
            if (play.Player.Type.Equals(PlayerType.Master))
                lastPlays += $"## Master Table: \n";
            else
                lastPlays += $"## {play.Player.Name}(player): \n";

            lastPlays += play.Prompt + "\n\n";
        }
        
        var userPrompt = $"<plays>\n{lastPlays}\n</plays> \n\n {GetWorldInfoUpdatePrompt()}";

        List<AiPromptRequest> promptList =
        [
            new(AiRole.System, systemPrompt),
            new(AiRole.Assistant, assistantPrompt),
            new(AiRole.User, userPrompt)
        ];
            
        
        var iaResponse = "";
        try
        {
            iaResponse = await _genAi.GenFromMultiplePrompts<WorldInfoDtoResponse>(
                promptList, 
                2000, 
                cancellationToken);

        }
        catch (Exception ex)
        {
            _logger.LogError("Something went wrong while attempting to update the world-info: {ex}", ex);
            return;
        }
        
        if (iaResponse.IsNullOrEmpty())
        {
            _logger.LogError("Something went wrong while attempting to generate the world-info.");
            return;
        }
        
        game.SetWorldInfo(iaResponse);
        
        _ = _gameRepository.Update(game);
        
        var changedItems = await _unitOfWork.CommitAsync(cancellationToken);
        if (changedItems == 0)
        {
            _logger.LogError("Something went wrong while attempting to save the world-info.");
        }
    }

    private static string GetMasterPersonality()
    {
        return "Você é um mestre de RPG de mesa em uma campanha de Dungeons & Dragons. " +
               "Você gosta de preparar as campanhas sem roteiro, apenas com criação de mundo, " +
               "utilizando a máxima 'Crie mundos, não histórias'.";
    }

    private static string GetAdditionalUpdatePrompt()
    {
        return "Você é responsável por manter o estado atualizado de um mundo de RPG. \n" +
               "Analise <plays> enviados pelo usuário e atualize apenas o que for necessário em <world-info>.\n" +
               "Preserve tudo que estiver correto e atual. Não reescreva o mundo.\n";
    }

    private static string GetWorldInfoGenerationPrompt()
    {
        return "Crie um mundo para uma campanha de Dungeons & Dragons. Precisa ser um mundo de fantasia original, " +
               "coeso e detalhado. Evite clichês óbvios.";
    }

    private static string GetWorldInfoUpdatePrompt()
    {
        return "Analise cuidadosamente as informações dentro das tags <plays> e compare-as com o conteúdo " +
               "existente em <world-info>. \nAtualize apenas os trechos de <world-info> que estiverem desatualizados " +
               "ou que precisem refletir novas informações \nprovenientes de <plays>. \nMantenha todos os demais " +
               "dados de <world-info> inalterados, preservando sua coerência e consistência narrativa. \nNão " +
               "reescreva o mundo inteiro — apenas modifique o necessário para que <world-info> permaneça atualizado " +
               "e fiel aos eventos recentes.\n Caso <world-info> esteja vazio, crie o mundo completamente do zero.";
    }
}
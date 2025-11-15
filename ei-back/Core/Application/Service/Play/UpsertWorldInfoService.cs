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
        systemPrompt += $"\n{GetResponseDetailsPrompt()}";

        List<AiPromptRequest> promptList =
        [
            new(AiRole.System, systemPrompt),
            new(AiRole.User, GetWorldInfoGenerationPrompt())
        ];
            
        var iaResponse = await _genAi.GetResponse<WorldInfoDtoResponse>(promptList, 2000, cancellationToken);
            
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
            iaResponse = await _genAi.GetResponse<WorldInfoDtoResponse>(
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

    private static string GetResponseDetailsPrompt()
    {
        return "Abaixo segue uma explicação sobre como o mundo deve ser criado. \n" +
               "Onde estiver indicando MIN-x, significa que devem ser gerados no mínimo 'x' objetos para o array." +
               "Ex: MIN-3 - deve gerar no mínimo 3 objetos; MIN-5 - deve gerar no mínimo 5 objetos. \n\n" +
               "# CampaignStyle: enum ['Epic', 'Dark', 'Exploration', 'Politic', 'Mystery', 'Horror']\n" +
               "# MagicLevel: enum ['High', 'Medium', 'Low']\n" +
               "# SocietalEntities: MIN-3\n" +
               "## SocietalType: enum ['Faction', 'Alliance', 'Guild', 'Kingdom', 'Priest', 'Organization', 'Peoples']\n" +
               "## Name: nome da sociedade\n" +
               "## Background: História, objetivos, motivações internas, conflitos, inimigos e impacto no mundo.\n" +
               "# NPCs: MIN-5\n" +
               "## Name: nome do NPC\n" +
               "## Background: Breve história, papel social, origem e eventos marcantes de sua vida.\n" +
               "## Race: Raça do NPC (humano, elfo, anão, tiefling etc.).\n" +
               "## Profession: Ocupação atual ou papel que exerce no mundo (mago, comerciante, espião, rei, pesquisador etc.).\n" +
               "## Personality: Traços de personalidade marcantes: comportamento, vícios, virtudes, medos e objetivos.\n" +
               "# Events: MIN-5 Lista de eventos relevantes ocorridos no mundo recentemente ou no passado que " +
               "influenciam a narrativa. Podem ser guerras, desastres, descobertas, assassinatos, aparições mágicas, profecias etc.\n" +
               "# Consequences: MIN-5 Prevê resultados para possíveis ações do jogador\n" +
               "## Action: Ação que pode acontecer no futuro\n" +
               "## Consequence: Consequência caso o jogado realize a ação.\n" +
               "# Locations: MIN-5 Locais significativos do mundo, pode ser cidades, ruínas, territórios, estabelecimentos e etc...\n" +
               "## Name: nome da locação\n" +
               "## Background: Descrição geral, história do local, reputação e importância.\n" +
               "## MainPoints: MIN-3 Pontos de interesse dentro da localização (marcos, áreas importantes, estruturas, perigos).\n" +
               "## CurrentEvents: MIN-3 O que está acontecendo no local no momento: conflitos, problemas, rumores, crises, oportunidades.\n" +
               "# Treasures: MIN-3 Tesouros, relíquias ou artefatos importantes e únicos dentro do mundo.\n" +
               "## Name: Nome do tesouro.\n" +
               "## Background: Origem, lenda ou história por trás do item.\n" +
               "## Location: Onde o tesouro pode ser encontrado atualmente.\n" +
               "## Properties: Poderes, efeitos, utilidades ou maldições associadas ao item.\n";
    }
}
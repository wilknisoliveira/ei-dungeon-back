using ei_back.Core.Application.Interfaces;
using ei_back.Core.Application.Repository;
using ei_back.Core.Application.Service.Play;
using ei_back.Core.Domain.Entity;
using ei_back.Core.Domain.Enums;
using ei_back.Infrastructure.Context.Interfaces;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlayEntity = ei_back.Core.Domain.Entity.Play;

namespace ei_back.Tests.Core.Application.Service.Play
{
    public class GeneratePlaysSummaryServiceTests
    {
        private readonly IGenAi _genAi;
        private readonly IPlayRepository _playRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGameRepository _gameRepository;
        private readonly GeneratePlaysSummaryService _service;

        public GeneratePlaysSummaryServiceTests()
        {
            _genAi = A.Fake<IGenAi>();
            _playRepository = A.Fake<IPlayRepository>();
            _unitOfWork = A.Fake<IUnitOfWork>();
            _gameRepository = A.Fake<IGameRepository>();
            var logger = A.Fake<ILogger<GeneratePlaysSummaryService>>();
            var configuration = A.Fake<IConfiguration>();

            A.CallTo(() => configuration["PlayOptions:SummaryMaxTokens"]).Returns("4000");

            _service = new GeneratePlaysSummaryService(logger, _unitOfWork, _playRepository, _genAi, _gameRepository, configuration);
        }

        [Fact]
        public async Task Handler_WhenValidPlays_CreatesSummary()
        {
            var gameId = Guid.NewGuid();
            var game = CreateGameWithPlayers(gameId, GameLanguage.English);
            var realPlayer = game.Players.First(x => x.Type == PlayerType.RealPlayer);

            var userPlay = new PlayEntity(game, realPlayer, "I search the treasure room");
            var masterPlay = new PlayEntity(game, game.Players.First(x => x.Type == PlayerType.Master), "You find a chest");
            var plays = new List<PlayEntity> { userPlay, masterPlay };

            var summaryText = "The player searched the treasure room and found a chest with gold.";

            A.CallTo(() => _gameRepository.FindByIdAsync(gameId, A<CancellationToken>._))
                .Returns(Task.FromResult(game));
            A.CallTo(() => _genAi.GetResponse(A<List<AiPromptRequest>>._, A<int>._, A<CancellationToken>._))
                .Returns(Task.FromResult(summaryText));
            A.CallTo(() => _playRepository.CreateAsync(A<PlayEntity>._, A<CancellationToken>._))
                .Returns(Task.FromResult(A.Fake<PlayEntity>()));
            A.CallTo(() => _unitOfWork.CommitAsync(A<CancellationToken>._))
                .Returns(Task.FromResult(1));

            await _service.Handler(gameId, plays, CancellationToken.None);

            A.CallTo(() => _playRepository.CreateAsync(A<PlayEntity>._, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _unitOfWork.CommitAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task Handler_WhenGenAiReturnsEmpty_LogsAndReturns()
        {
            var gameId = Guid.NewGuid();
            var game = CreateGameWithPlayers(gameId);
            var realPlayer = game.Players.First(x => x.Type == PlayerType.RealPlayer);

            var userPlay = new PlayEntity(game, realPlayer, "I look around");
            var plays = new List<PlayEntity> { userPlay };

            A.CallTo(() => _gameRepository.FindByIdAsync(gameId, A<CancellationToken>._))
                .Returns(Task.FromResult(game));
            A.CallTo(() => _genAi.GetResponse(A<List<AiPromptRequest>>._, A<int>._, A<CancellationToken>._))
                .Returns(Task.FromResult<string?>(null));

            await _service.Handler(gameId, plays, CancellationToken.None);

            A.CallTo(() => _playRepository.CreateAsync(A<PlayEntity>._, A<CancellationToken>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.CommitAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Handler_WhenGenAiThrows_LogsAndReturns()
        {
            var gameId = Guid.NewGuid();
            var game = CreateGameWithPlayers(gameId);
            var realPlayer = game.Players.First(x => x.Type == PlayerType.RealPlayer);

            var userPlay = new PlayEntity(game, realPlayer, "I open the door");
            var plays = new List<PlayEntity> { userPlay };

            A.CallTo(() => _gameRepository.FindByIdAsync(gameId, A<CancellationToken>._))
                .Returns(Task.FromResult(game));
            A.CallTo(() => _genAi.GetResponse(A<List<AiPromptRequest>>._, A<int>._, A<CancellationToken>._))
                .Throws(new Exception("AI service error"));

            await _service.Handler(gameId, plays, CancellationToken.None);

            A.CallTo(() => _playRepository.CreateAsync(A<PlayEntity>._, A<CancellationToken>._)).MustNotHaveHappened();
            A.CallTo(() => _unitOfWork.CommitAsync(A<CancellationToken>._)).MustNotHaveHappened();
        }

        [Fact]
        public async Task Handler_WithPortugueseLanguage_IncludesLanguageInstructionInPrompt()
        {
            var gameId = Guid.NewGuid();
            var game = CreateGameWithPlayers(gameId, GameLanguage.Portuguese);
            var realPlayer = game.Players.First(x => x.Type == PlayerType.RealPlayer);

            var userPlay = new PlayEntity(game, realPlayer, "Eu procuro tesouros");
            var plays = new List<PlayEntity> { userPlay };

            var summaryText = "Resumo da partida.";
            List<AiPromptRequest>? capturedPrompts = null;

            A.CallTo(() => _gameRepository.FindByIdAsync(gameId, A<CancellationToken>._))
                .Returns(Task.FromResult(game));
            A.CallTo(() => _genAi.GetResponse(A<List<AiPromptRequest>>._, A<int>._, A<CancellationToken>._))
                .Invokes(call => capturedPrompts = call.GetArgument<List<AiPromptRequest>>(0))
                .Returns(Task.FromResult(summaryText));
            A.CallTo(() => _playRepository.CreateAsync(A<PlayEntity>._, A<CancellationToken>._))
                .Returns(Task.FromResult(A.Fake<PlayEntity>()));
            A.CallTo(() => _unitOfWork.CommitAsync(A<CancellationToken>._))
                .Returns(Task.FromResult(1));

            await _service.Handler(gameId, plays, CancellationToken.None);

            capturedPrompts.Should().NotBeNull();
            var systemPrompt = capturedPrompts!.First(x => x.Role == AiRole.System).Content;
            systemPrompt.Should().Contain("<language>");
            systemPrompt.Should().Contain("português brasileiro");
        }

        private static Game CreateGameWithPlayers(Guid gameId, GameLanguage language = GameLanguage.English)
        {
            var game = new Game("Test Game");
            typeof(Base).GetProperty(nameof(Base.Id))!.SetValue(game, gameId);
            game.SetWorldInfo("A fantasy world");
            game.SetGameLanguage(language);

            var realPlayer = new Player(
                "Hero", "Brave adventurer", CharacterRace.Human, PlayerType.RealPlayer);
            typeof(Base).GetProperty(nameof(Base.Id))!.SetValue(realPlayer, Guid.NewGuid());

            var master = new Player(
                "Table Master", "RPG Table Master", PlayerType.Master);
            typeof(Base).GetProperty(nameof(Base.Id))!.SetValue(master, Guid.NewGuid());

            var system = new Player(
                "System", "System", PlayerType.System);
            typeof(Base).GetProperty(nameof(Base.Id))!.SetValue(system, Guid.NewGuid());

            game.SetPlayers(new List<Player> { realPlayer, master, system });

            return game;
        }
    }
}

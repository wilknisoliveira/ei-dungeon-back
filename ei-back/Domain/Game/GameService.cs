using ei_back.Domain.Game.Interfaces;

namespace ei_back.Domain.Game
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;

        public GameService(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<GameEntity> CreateAsync(GameEntity game, CancellationToken cancellationToken)
        {
            game.SetCreatedDate(DateTime.Now);

            return await _gameRepository.CreateAsync(game, cancellationToken);
        }
    }
}

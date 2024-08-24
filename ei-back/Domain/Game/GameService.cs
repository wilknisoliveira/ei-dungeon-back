using AutoMapper;
using ei_back.Domain.Game.Interfaces;

namespace ei_back.Domain.Game
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IMapper _mapper;

        public GameService(IGameRepository gameRepository, IMapper mapper)
        {
            _gameRepository = gameRepository;
            _mapper = mapper;
        }

        public async Task<GameEntity> CreateAsync(GameEntity game)
        {
            game.SetCreatedDate(DateTime.Now);

            return await _gameRepository.CreateAsync(game);
        }
    }
}

using AutoMapper;
using ei_back.Application.Api.Game.Dtos;
using ei_back.Domain.Game.Interfaces;
using ei_back.Infrastructure.Context;

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


        public async Task<GameEntity> CreateAsync(GameEntity game, CancellationToken cancellationToken)
        {
            game.SetCreatedDate(DateTime.Now);

            return await _gameRepository.CreateAsync(game, cancellationToken);
        }


        public async Task<PagedSearchDto<GameDtoResponse>> FindWithPagedSearch(
            Guid userId,
            string column,
            string sort,
            int size,
            int offset,
            int page,
            CancellationToken cancellationToken)
        {
            var games = await _gameRepository.FindWithPagedSearchAsync(
                sort,
                size,
                page,
                offset,
                userId,
                column,
                "games",
                cancellationToken);

            int totalResults = await _gameRepository.GetCountAsync(
                userId,
                column,
                "games",
                cancellationToken);

            return new PagedSearchDto<GameDtoResponse>
            {
                CurrentPage = page,
                List = games.Select(_mapper.Map<GameDtoResponse>).ToList(),
                PageSize = size,
                SortDirections = sort,
                TotalResults = totalResults
            };

        }


        public async Task<bool> CheckIfExistGameByUser(Guid gameId, Guid OwnerUserId, CancellationToken cancellationToken)
        {
            return await _gameRepository.CheckIfExistGameByUser(gameId, OwnerUserId, cancellationToken);
        }

        public async Task<GameEntity?> GetGameByIdAndOwnerUserName(Guid id, string userName, CancellationToken cancellationToken)
        {
            return await _gameRepository.GetGameByIdAndOwnerUserName(id, userName, cancellationToken);
        }
    }
}

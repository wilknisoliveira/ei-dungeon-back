using AutoMapper;
using ei_back.Application.Api.Game.Dtos;
using ei_back.Application.Api.Play.Dtos;
using ei_back.Domain.Play.Interfaces;
using ei_back.Infrastructure.Context;
using k8s.KubeConfigModels;

namespace ei_back.Domain.Play
{
    public class PlayService : IPlayService
    {
        private readonly IPlayRepository _playRepository;
        private readonly IMapper _mapper;

        public PlayService(IPlayRepository playRepository, IMapper mapper)
        {
            _playRepository = playRepository;
            _mapper = mapper;
        }

        public async Task<PagedSearchDto<PlayDtoResponse>> FindWithPagedSearch(
            Guid gameId,
            int size,
            CancellationToken cancellationToken)
        {
            var plays = await _playRepository.GetPlaysByGameAndSize(gameId, size, cancellationToken);

            int totalResults = await _playRepository.GetCountAsync(
                gameId,
                "game_id",
                "plays",
                cancellationToken);

            return new PagedSearchDto<PlayDtoResponse>
            {
                CurrentPage = 1,
                List = plays.Select(_mapper.Map<PlayDtoResponse>).ToList(),
                PageSize = size,
                SortDirections = "desc",
                TotalResults = totalResults
            };
        }
    }
}

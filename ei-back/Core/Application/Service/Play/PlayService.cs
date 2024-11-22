using AutoMapper;
using ei_back.Core.Application.Repository;
using ei_back.Core.Application.Service.Play.Interfaces;
using ei_back.Core.Application.UseCase.Play.Dtos;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Context;

namespace ei_back.Core.Application.Service.Play
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

        public Task<Domain.Entity.Play> CreatePlay(Domain.Entity.Play playEntity, CancellationToken cancellationToken)
        {
            return _playRepository.CreateAsync(playEntity, cancellationToken);
        }

        public async Task<PagedSearchDto<PlayDtoResponse>> FindWithPagedSearch(
            Guid gameId,
            int size,
            CancellationToken cancellationToken)
        {
            var plays = await _playRepository.GetPlaysByGameAndSizeButSystemPlay(gameId, size, cancellationToken);

            int totalResults = await _playRepository.CountPlaysByGameAndSizeButSystemPlay(gameId, size, cancellationToken);   

            List<PlayDtoResponse> playDtoResponseList = [];
            foreach (var play in plays)
            {
                var playDtoResponse = _mapper.Map<PlayDtoResponse>(play);
                playDtoResponse.PlayerDtoResponse = _mapper.Map<PlayerDtoResponse>(play.Player);

                playDtoResponseList.Add(playDtoResponse);
            }

            return new PagedSearchDto<PlayDtoResponse>
            {
                CurrentPage = 1,
                List = playDtoResponseList,
                PageSize = size,
                SortDirections = "desc",
                TotalResults = totalResults
            };
        }
    }
}

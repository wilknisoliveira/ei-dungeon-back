using AutoMapper;
using ei_back.Domain.Play.Interfaces;

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
    }
}

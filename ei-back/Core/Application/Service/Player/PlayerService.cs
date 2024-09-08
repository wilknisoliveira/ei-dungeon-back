using AutoMapper;
using ei_back.Core.Application.Repository;
using ei_back.Core.Application.Service.Player.Interfaces;

namespace ei_back.Core.Application.Service.Player
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IMapper _mapper;

        public PlayerService(IPlayerRepository playerRepository, IMapper mapper)
        {
            _playerRepository = playerRepository;
            _mapper = mapper;
        }
    }
}

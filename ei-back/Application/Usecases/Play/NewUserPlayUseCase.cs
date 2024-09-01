using AutoMapper;
using ei_back.Application.Api.Play.Dtos;
using ei_back.Application.Usecases.Play.Interfaces;
using ei_back.Domain.Game;
using ei_back.Domain.Game.Interfaces;
using ei_back.Domain.Play;
using ei_back.Domain.Play.Interfaces;
using ei_back.Domain.Player;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;

namespace ei_back.Application.Usecases.Play
{
    public class NewUserPlayUseCase : INewUserPlayUseCase
    {
        private readonly IMapper _mapper;
        private readonly IPlayService _playService;
        private readonly IGameService _gameService;

        public NewUserPlayUseCase(IMapper mapper, IPlayService playService, IGameService gameService)
        {
            _mapper = mapper;
            _playService = playService;
            _gameService = gameService;
        }

        public async Task<List<PlayDtoResponse>> Handler(PlayDtoRequest playDtoRequest, string userName, CancellationToken cancellationToken)
        {
            var game = await _gameService.GetGameByIdAndOwnerUserName(playDtoRequest.GameId, userName, cancellationToken) ??
                throw new NotFoundException($"No game found with id {playDtoRequest.GameId} to user name {userName}.");

            var player = game.Players.FirstOrDefault(x => x.Type.Equals(PlayerType.RealPlayer)) ??
                throw new InternalServerErrorException($"Something went wrong while attempting to get the real player info");

            var newPlay = new PlayEntity(game, player, playDtoRequest.Prompt);
            var playResponse = await _playService.CreatePlay(newPlay, cancellationToken) ??
                throw new InternalServerErrorException($"Something went wrong while attempting to create the play");

            var playDtoResponse = _mapper.Map<PlayDtoResponse>(playResponse);
            playDtoResponse.PlayerDtoResponse = _mapper.Map<PlayerDtoResponse>(player);

            //Master play

            //Game Intelligence

            //Artificial Player

            //Master play

            var response = new List<PlayDtoResponse>
            {
                playDtoResponse
            };

            return response;
        }
    }
}

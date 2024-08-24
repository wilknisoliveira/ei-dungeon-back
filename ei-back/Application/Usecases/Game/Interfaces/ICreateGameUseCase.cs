using ei_back.Application.Api.Game.Dtos;

namespace ei_back.Application.Usecases.Game.Interfaces
{
    public interface ICreateGameUseCase
    {
        Task<GameDtoResponse> Handler(GameDtoRequest gameDtoRequest, string userName);
    }
}

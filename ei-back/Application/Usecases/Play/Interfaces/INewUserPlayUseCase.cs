using ei_back.Application.Api.Play.Dtos;

namespace ei_back.Application.Usecases.Play.Interfaces
{
    public interface INewUserPlayUseCase
    {
        Task<List<PlayDtoResponse>> Handler(PlayDtoRequest playDtoRequest, string userName, CancellationToken cancellationToken);
    }
}

using ei_back.Core.Application.UseCase.Play.Dtos;

namespace ei_back.Core.Application.UseCase.Play.Interfaces
{
    public interface INewUserPlayUseCase
    {
        Task<List<PlayDtoResponse>> Handler(PlayDtoRequest playDtoRequest, string userName, CancellationToken cancellationToken);
    }
}

using ei_back.Application.Api.User.Dtos;

namespace ei_back.Application.Usecases.User.Interfaces
{
    public interface ICreateUserUseCase
    {
        Task<UserDtoResponse> Handler(UserDtoRequest userDtoRequest);
    }
}

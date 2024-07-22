using ei_back.Application.Api.User.Dtos;

namespace ei_back.Application.Usecases.User.Interfaces
{
    public interface IChangePasswordUseCase
    {
        Task<UserGetDtoResponse> Handler(PasswordDtoRequest passwordDtoRequest);
    }
}

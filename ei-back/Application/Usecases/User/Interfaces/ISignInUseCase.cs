using ei_back.Application.Api.User.Dtos;

namespace ei_back.Application.Usecases.User.Interfaces
{
    public interface ISignInUseCase
    {
        TokenDtoReponse Handler(LoginDtoRequest loginDtoRequest);
    }
}

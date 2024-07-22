using ei_back.Application.Api.User.Dtos;

namespace ei_back.Domain.User.Interfaces
{
    public interface ILoginService
    {
        TokenDtoReponse ValidateCredentials(LoginDtoRequest userDtoRequest);
    }
}

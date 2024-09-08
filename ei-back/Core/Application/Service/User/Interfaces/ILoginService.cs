using ei_back.Core.Application.UseCase.User.Dtos;

namespace ei_back.Core.Application.Service.User.Interfaces
{
    public interface ILoginService
    {
        TokenDtoReponse ValidateCredentials(LoginDtoRequest userDtoRequest);
    }
}

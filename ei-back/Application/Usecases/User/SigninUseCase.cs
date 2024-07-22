using ei_back.Application.Api.User.Dtos;
using ei_back.Application.Usecases.User.Interfaces;
using ei_back.Domain.User;
using ei_back.Domain.User.Interfaces;

namespace ei_back.Application.Usecases.User
{
    public class SigninUseCase : ISignInUseCase
    {
        private readonly ILoginService _loginService;

        public SigninUseCase(ILoginService loginService)
        {
            _loginService = loginService;
        }

        public TokenDtoReponse Handler(LoginDtoRequest loginDtoRequest)
        {
            var token = _loginService.ValidateCredentials(loginDtoRequest);

            return token;
        }
    }
}

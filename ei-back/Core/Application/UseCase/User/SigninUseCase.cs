using ei_back.Core.Application.Service.User.Interfaces;
using ei_back.Core.Application.UseCase.User.Dtos;
using ei_back.Core.Application.UseCase.User.Interfaces;

namespace ei_back.Core.Application.UseCase.User
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

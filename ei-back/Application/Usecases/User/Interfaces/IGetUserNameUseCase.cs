using System.Security.Claims;

namespace ei_back.Application.Usecases.User.Interfaces
{
    public interface IGetUserNameUseCase
    {
        string Handler(ClaimsPrincipal user);
    }
}

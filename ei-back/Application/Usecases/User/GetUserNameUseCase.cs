using ei_back.Application.Usecases.User.Interfaces;
using System.Security.Claims;

namespace ei_back.Application.Usecases.User
{
    public class GetUserNameUseCase : IGetUserNameUseCase
    {
        public string Handler(ClaimsPrincipal user)
        {
            return user.Identity?.Name ?? "";
        }
    }
}

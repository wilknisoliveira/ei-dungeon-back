using ei_back.Core.Application.UseCase.User.Interfaces;
using System.Security.Claims;

namespace ei_back.Core.Application.UseCase.User
{
    public class GetUserNameUseCase : IGetUserNameUseCase
    {
        public string Handler(ClaimsPrincipal user)
        {
            return user.Identity?.Name ?? "";
        }
    }
}

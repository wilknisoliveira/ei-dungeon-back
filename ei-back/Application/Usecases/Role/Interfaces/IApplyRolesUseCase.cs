using ei_back.Application.Api.Role.Dtos;
using ei_back.Domain.Role.Interfaces;
using ei_back.Domain.User.Interfaces;
using System.Runtime.CompilerServices;

namespace ei_back.Application.Usecases.Role.Interfaces
{
    public interface IApplyRolesUseCase
    {
        Task<ApplyRoleDtoResponse> Handler(ApplyRoleDtoRequest applyRoleDtoRequest);
    }
}

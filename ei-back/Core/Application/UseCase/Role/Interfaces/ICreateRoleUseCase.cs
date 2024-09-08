using ei_back.Core.Application.UseCase.Role.Dtos;

namespace ei_back.Core.Application.UseCase.Role.Interfaces
{
    public interface ICreateRoleUseCase
    {
        Task<RoleDto> Handler(RoleDto request);
    }
}

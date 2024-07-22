using ei_back.Application.Api.Role.Dtos;

namespace ei_back.Application.Usecases.Role.Interfaces
{
    public interface IGetAllRoleUseCase
    {
        Task<List<RoleDtoResponse>> Handler();
    }
}

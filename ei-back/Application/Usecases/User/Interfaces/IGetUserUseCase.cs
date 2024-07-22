using ei_back.Application.Api.User.Dtos;
using ei_back.Infrastructure.Context;

namespace ei_back.Application.Usecases.User.Interfaces
{
    public interface IGetUserUseCase
    {
        Task<PagedSearchDto<UserGetDtoResponse>> Handler(
            string? name,
            string sortDirection,
            int pageSize,
            int page);
    }
}

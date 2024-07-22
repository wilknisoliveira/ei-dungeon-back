using ei_back.Application.Api.User.Dtos;
using ei_back.Application.Usecases.User.Interfaces;
using ei_back.Domain.User.Interfaces;
using ei_back.Infrastructure.Context;

namespace ei_back.Application.Usecases.User
{
    public class GetUserUseCase : IGetUserUseCase
    {
        private readonly IUserService _userService;

        public GetUserUseCase(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<PagedSearchDto<UserGetDtoResponse>> Handler(string? name, string sortDirection, int pageSize, int page)
        {
            var pagedSearchDto = new PagedSearchDto<UserGetDtoResponse>();

            var sort = pagedSearchDto.ValidateSort(sortDirection);
            var size = pagedSearchDto.ValidateSize(pageSize);
            var offset = pagedSearchDto.ValidateOffset(page, pageSize);

            return await _userService.FindWithPagedSearch(name, sort, size, offset, page);
        }
    }
}

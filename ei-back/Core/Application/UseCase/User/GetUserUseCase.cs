using ei_back.Core.Application.Service.User.Interfaces;
using ei_back.Core.Application.UseCase.User.Dtos;
using ei_back.Core.Application.UseCase.User.Interfaces;
using ei_back.Infrastructure.Context;

namespace ei_back.Core.Application.UseCase.User
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

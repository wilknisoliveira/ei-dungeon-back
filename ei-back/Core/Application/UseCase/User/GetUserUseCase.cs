using AutoMapper;
using ei_back.Core.Application.Repository;
using ei_back.Core.Application.UseCase.User.Dtos;
using ei_back.Core.Application.UseCase.User.Interfaces;
using ei_back.Infrastructure.Context;

namespace ei_back.Core.Application.UseCase.User
{
    public class GetUserUseCase : IGetUserUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public GetUserUseCase(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<PagedSearchDto<UserGetDtoResponse>> Handler(string? name, string sortDirection, int pageSize, int page)
        {
            var pagedSearchDto = new PagedSearchDto<UserGetDtoResponse>();

            var sort = pagedSearchDto.ValidateSort(sortDirection);
            var size = pagedSearchDto.ValidateSize(pageSize);
            var offset = pagedSearchDto.ValidateOffset(page, pageSize);

            var users = await _userRepository.FindWithPagedSearchAsync(
                sort,
                size,
                page,
                offset,
                name,
                "user_name",
                "users");

            int totalResults = await _userRepository.GetCountAsync(
                name,
                "user_name",
                "users");

            return new PagedSearchDto<UserGetDtoResponse>
            {
                CurrentPage = page,
                List = users.Select(user => _mapper.Map<UserGetDtoResponse>(user)).ToList(),
                PageSize = size,
                SortDirections = sort,
                TotalResults = totalResults
            };
        }
    }
}

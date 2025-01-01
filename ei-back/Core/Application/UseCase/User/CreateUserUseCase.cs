using AutoMapper;
using ei_back.Core.Application.Service.User.Interfaces;
using ei_back.Core.Application.UseCase.User.Dtos;
using ei_back.Core.Application.UseCase.User.Interfaces;
using ei_back.Core.Domain.Enums;

namespace ei_back.Core.Application.UseCase.User
{
    public class CreateUserUseCase : ICreateUserUseCase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public CreateUserUseCase(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<UserDtoResponse> Handler(UserDtoRequest userDtoRequest)
        {
            var user = _mapper.Map<Domain.Entity.User>(userDtoRequest);

            user.Role = UserRole.CommonUser;

            var userResponse = await _userService.CreateAsync(user);

            return _mapper.Map<UserDtoResponse>(userResponse);
        }
    }
}

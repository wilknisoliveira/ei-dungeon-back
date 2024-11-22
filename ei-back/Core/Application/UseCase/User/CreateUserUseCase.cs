using AutoMapper;
using ei_back.Core.Application.Service.Role.Interfaces;
using ei_back.Core.Application.Service.User.Interfaces;
using ei_back.Core.Application.UseCase.User.Dtos;
using ei_back.Core.Application.UseCase.User.Interfaces;
using ei_back.Core.Domain.Entity;

namespace ei_back.Core.Application.UseCase.User
{
    public class CreateUserUseCase : ICreateUserUseCase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IRoleService _roleService;

        public CreateUserUseCase(IUserService userService, IMapper mapper, IRoleService roleService)
        {
            _userService = userService;
            _mapper = mapper;
            _roleService = roleService;
        }

        public async Task<UserDtoResponse> Handler(UserDtoRequest userDtoRequest)
        {
            var user = _mapper.Map<Domain.Entity.User>(userDtoRequest);

            var selectedRoles = await _roleService.FindSelectedRoles(
            [
                "CommonUser"
            ]);
            user.Roles.AddRange(selectedRoles);

            var userResponse = await _userService.CreateAsync(user);

            return _mapper.Map<UserDtoResponse>(userResponse);
        }
    }
}

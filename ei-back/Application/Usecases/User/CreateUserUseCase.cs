using AutoMapper;
using ei_back.Application.Api.User.Dtos;
using ei_back.Application.Usecases.Role.Interfaces;
using ei_back.Application.Usecases.User.Interfaces;
using ei_back.Domain.Role.Interfaces;
using ei_back.Domain.User;
using ei_back.Domain.User.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ei_back.Application.Usecases.User
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
            var user = _mapper.Map<UserEntity>(userDtoRequest);

            if (userDtoRequest.Roles != null)
            {
                var selectedRoles = await _roleService.FindSelectedRoles(userDtoRequest.Roles);
                user.Roles.AddRange(selectedRoles);
            }

            var userResponse = await _userService.CreateAsync(user);

            return _mapper.Map<UserDtoResponse>(userResponse); 
        }
    }
}

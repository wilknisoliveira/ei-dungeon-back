using AutoMapper;
using ei_back.Application.Api.Role.Dtos;
using ei_back.Application.Api.User.Dtos;
using ei_back.Application.Usecases.Role.Interfaces;
using ei_back.Domain.Role.Interfaces;
using ei_back.Domain.User.Interfaces;

namespace ei_back.Application.Usecases.Role
{
    public class ApplyRolesUseCase : IApplyRolesUseCase
    {
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IMapper _mapper;

        public ApplyRolesUseCase(IUserService userService, IRoleService roleService, IMapper mapper)
        {
            _userService = userService;
            _roleService = roleService;
            _mapper = mapper;
        }

        public async Task<ApplyRoleDtoResponse> Handler(ApplyRoleDtoRequest applyRoleDtoRequest)
        {
            var user = await _userService.FindUserAndRoles(applyRoleDtoRequest.Id);
            var selectedRoles = await _roleService.FindSelectedRoles(applyRoleDtoRequest.Roles);

            user.Roles.Clear();
            user.Roles.AddRange(selectedRoles);
            
            var userResponse = _userService.Update(user);

            var applyRoleDtoResponse = _mapper.Map<ApplyRoleDtoResponse>(userResponse);
            applyRoleDtoResponse.Roles = userResponse.Roles.Select(r => r.Name).ToList();

            return applyRoleDtoResponse;
        }
    }
}

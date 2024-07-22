using AutoMapper;
using ei_back.Application.Api.Role.Dtos;
using ei_back.Application.Usecases.Role.Interfaces;
using ei_back.Domain.Role;
using ei_back.Domain.Role.Interfaces;

namespace ei_back.Application.Usecases.Role
{
    public class CreateRoleUseCase : ICreateRoleUseCase
    {
        private readonly IRoleService _roleService;
        private readonly IMapper _mapper;

        public CreateRoleUseCase(IRoleService roleService, IMapper mapper)
        {
            _roleService = roleService;
            _mapper = mapper;
        }

        public async Task<RoleDto> Handler(RoleDto request)
        {
            var role = _mapper.Map<RoleEntity>(request);
            var response = await _roleService.CreateAsync(role);
            return _mapper.Map<RoleDto>(response);
        }
    }
}

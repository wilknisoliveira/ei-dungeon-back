﻿using AutoMapper;
using ei_back.Core.Application.Service.Role.Interfaces;
using ei_back.Core.Application.UseCase.Role.Dtos;
using ei_back.Core.Application.UseCase.Role.Interfaces;
using ei_back.Core.Application.UseCase.User.Dtos;

namespace ei_back.Core.Application.UseCase.Role
{
    public class GetAllRoleUseCase : IGetAllRoleUseCase
    {
        private readonly IRoleService _roleService;
        private readonly IMapper _mapper;

        public GetAllRoleUseCase(IRoleService roleService, IMapper mapper)
        {
            _roleService = roleService;
            _mapper = mapper;
        }

        public async Task<List<RoleDtoResponse>> Handler()
        {
            var roles = await _roleService.FindAllAsync();

            var rolesResponse = roles.Select(role =>
            {
                var roleDto = _mapper.Map<RoleDtoResponse>(role);

                roleDto.Users = role.Users.Select(user => _mapper.Map<UserGetDtoResponse>(user)).ToList();

                return roleDto;
            }).ToList();

            return rolesResponse;
        }
    }
}

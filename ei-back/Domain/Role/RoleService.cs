using ei_back.Application.Api.User.Dtos;
using ei_back.Domain.Role.Interfaces;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;

namespace ei_back.Domain.Role
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<RoleEntity> CreateAsync(RoleEntity entity)
        {
            entity.CreatedAt = DateTime.Now;
            entity.UpdatedAt = DateTime.Now;
            return await _roleRepository.CreateAsync(entity);
        }

        public async Task<List<RoleEntity>> FindAllAsync()
        {
            return await _roleRepository.FindRolesAndUsersAsync();
        }

        public async Task<List<RoleEntity>> FindSelectedRoles(List<string> rolesList)
        {
            var roles = await FindAllAsync();
            var selectedRoles = roles.Where(r => rolesList.Contains(r.Name)).ToList();

            if (selectedRoles.Count != rolesList.Count)
                throw new BadRequestException("Some of the roles are incorrect");

            return selectedRoles;
        }
    }
}

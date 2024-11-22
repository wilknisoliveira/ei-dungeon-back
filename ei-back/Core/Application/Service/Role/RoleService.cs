using ei_back.Core.Application.Repository;
using ei_back.Core.Application.Service.Role.Interfaces;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;

namespace ei_back.Core.Application.Service.Role
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<Domain.Entity.Role> CreateAsync(Domain.Entity.Role entity)
        {
            entity.CreatedAt = DateTime.Now;
            entity.UpdatedAt = DateTime.Now;
            return await _roleRepository.CreateAsync(entity);
        }

        public async Task<List<Domain.Entity.Role>> FindAllAsync()
        {
            return await _roleRepository.FindRolesAndUsersAsync();
        }

        public async Task<List<Domain.Entity.Role>> FindSelectedRoles(List<string> rolesList)
        {
            var roles = await FindAllAsync();
            var selectedRoles = roles.Where(r => rolesList.Contains(r.Name)).ToList();

            if (selectedRoles.Count != rolesList.Count)
                throw new BadRequestException("Some of the roles are incorrect");

            return selectedRoles;
        }
    }
}

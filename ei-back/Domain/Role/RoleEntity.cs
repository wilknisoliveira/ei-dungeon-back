using ei_back.Domain.Base;
using ei_back.Domain.User;

namespace ei_back.Domain.Role
{
    public class RoleEntity(
        string name, 
        string? description) : BaseEntity
    {
        public string Name { get; set; } = name;
        public string? Description { get; set; } = description;
        public List<UserEntity> Users { get; } = new();
    }
}

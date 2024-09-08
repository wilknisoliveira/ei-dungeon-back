namespace ei_back.Core.Domain.Entity
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

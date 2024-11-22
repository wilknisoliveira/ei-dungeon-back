namespace ei_back.Core.Domain.Entity
{
    public class Role(
        string name,
        string? description) : Base
    {
        public string Name { get; set; } = name;
        public string? Description { get; set; } = description;
        public List<User> Users { get; } = new();
    }
}

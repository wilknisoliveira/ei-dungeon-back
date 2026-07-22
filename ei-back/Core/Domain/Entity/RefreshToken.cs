namespace ei_back.Core.Domain.Entity
{
    public class RefreshToken : Base
    {
        public Guid UserId { get; set; }
        public string TokenHash { get; set; }
        public DateTime ExpiresAt { get; set; }
        public User User { get; set; }
    }
}

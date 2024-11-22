using ei_back.Core.Domain.Entity;

namespace ei_back.Core.Application.Repository
{
    public interface IUserRepository : IRepository<User>
    {
        User ValidateCredentials(string userName, string pass);
        User RefreshUserInfo(User user);
        Task<User?> GetUserAndRolesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<User?> FindByUserName(string userName, CancellationToken cancellationToken = default);
    }
}

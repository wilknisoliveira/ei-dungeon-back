using ei_back.Core.Domain.Entity;

namespace ei_back.Core.Application.Repository
{
    public interface IUserRepository : IRepository<UserEntity>
    {
        UserEntity ValidateCredentials(string userName, string pass);
        UserEntity RefreshUserInfo(UserEntity user);
        Task<UserEntity?> GetUserAndRolesAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<UserEntity?> FindByUserName(string userName, CancellationToken cancellationToken = default);
    }
}

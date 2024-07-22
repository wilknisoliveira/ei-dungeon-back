using ei_back.Domain.Base.Interfaces;

namespace ei_back.Domain.User.Interfaces
{
    public interface IUserRepository : IRepository<UserEntity>
    {
        UserEntity ValidateCredentials(string userName, string pass);
        UserEntity RefreshUserInfo(UserEntity user);
        Task<UserEntity> GetUserAndRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}

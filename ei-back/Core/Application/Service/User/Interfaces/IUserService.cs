using ei_back.Core.Application.UseCase.User.Dtos;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Context;
using System.Security.Cryptography;

namespace ei_back.Core.Application.Service.User.Interfaces
{
    public interface IUserService
    {
        string ComputeHash(string input, HashAlgorithm hashAlgorithm);
        Task<UserEntity> CreateAsync(UserEntity userEntity);
        Task<PagedSearchDto<UserGetDtoResponse>> FindWithPagedSearch(
            string name,
            string sortDirection,
            int pageSize,
            int offset,
            int page);
        Task<UserEntity> FindByIdAsync(Guid userId);
        Task<UserEntity> FindUserAndRoles(Guid userId);
        UserEntity Update(UserEntity user);
        Task<UserEntity?> FindByUserName(string userName);
    }
}

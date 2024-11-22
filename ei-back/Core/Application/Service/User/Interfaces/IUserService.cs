using ei_back.Core.Application.UseCase.User.Dtos;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Context;
using System.Security.Cryptography;

namespace ei_back.Core.Application.Service.User.Interfaces
{
    public interface IUserService
    {
        string ComputeHash(string input, HashAlgorithm hashAlgorithm);
        Task<Domain.Entity.User> CreateAsync(Domain.Entity.User userEntity);
        Task<PagedSearchDto<UserGetDtoResponse>> FindWithPagedSearch(
            string name,
            string sortDirection,
            int pageSize,
            int offset,
            int page);
        Task<Domain.Entity.User> FindByIdAsync(Guid userId);
        Task<Domain.Entity.User> FindUserAndRoles(Guid userId);
        Domain.Entity.User Update(Domain.Entity.User user);
        Task<Domain.Entity.User?> FindByUserName(string userName);
    }
}

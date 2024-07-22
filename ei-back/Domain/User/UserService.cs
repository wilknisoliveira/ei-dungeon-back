using AutoMapper;
using ei_back.Application.Api.User.Dtos;
using ei_back.Domain.User.Interfaces;
using ei_back.Infrastructure.Context;
using System.Security.Cryptography;
using System.Text;

namespace ei_back.Domain.User
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public string ComputeHash(string input, HashAlgorithm hashAlgorithm)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashedBytes = hashAlgorithm.ComputeHash(inputBytes);

            return BitConverter.ToString(hashedBytes);
        }

        public async Task<UserEntity> CreateAsync(UserEntity userEntity)
        {
            userEntity.CreatedAt = DateTime.Now;
            userEntity.UpdatedAt = DateTime.Now;

            userEntity.Password = ComputeHash(userEntity.Password, SHA256.Create());
            return await _userRepository.CreateAsync(userEntity);
            
        }

        public async Task<PagedSearchDto<UserGetDtoResponse>> FindWithPagedSearch(
            string name,
            string sort,
            int size,
            int offset,
            int page)
        {
            var users = await _userRepository.FindWithPagedSearchAsync(
                sort,
                size,
                page,
                offset,
                name,
                "user_name",
                "users");

            int totalResults = await _userRepository.GetCountAsync(
                name,
                "user_name",
                "users");

            return new PagedSearchDto<UserGetDtoResponse>
            {
                CurrentPage = page,
                List = users.Select(user => _mapper.Map<UserGetDtoResponse>(user)).ToList(),
                PageSize = size,
                SortDirections = sort,
                TotalResults = totalResults
            };
        
        }

        public async Task<UserEntity> FindByIdAsync(Guid userId)
        {
            return await _userRepository.FindByIdAsync(userId);
        }

        public async Task<UserEntity> FindUserAndRoles(Guid userId)
        {
            return await _userRepository.GetUserAndRolesAsync(userId);
        }

        public UserEntity Update(UserEntity user)
        {
            user.UpdatedAt = DateTime.Now;
            return _userRepository.Update(user);
        }
    }
}

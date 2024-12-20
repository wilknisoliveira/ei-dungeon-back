﻿using AutoMapper;
using ei_back.Core.Application.Repository;
using ei_back.Core.Application.Service.User.Interfaces;
using ei_back.Core.Application.UseCase.User.Dtos;
using ei_back.Core.Domain.Entity;
using ei_back.Infrastructure.Context;
using System.Security.Cryptography;
using System.Text;

namespace ei_back.Core.Application.Service.User
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

        public async Task<Domain.Entity.User> CreateAsync(Domain.Entity.User userEntity)
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

        public async Task<Domain.Entity.User> FindByIdAsync(Guid userId)
        {
            return await _userRepository.FindByIdAsync(userId);
        }

        public async Task<Domain.Entity.User> FindUserAndRoles(Guid userId)
        {
            return await _userRepository.GetUserAndRolesAsync(userId);
        }

        public Domain.Entity.User Update(Domain.Entity.User user)
        {
            user.UpdatedAt = DateTime.Now;
            return _userRepository.Update(user);
        }

        public async Task<Domain.Entity.User?> FindByUserName(string userName)
        {
            return await _userRepository.FindByUserName(userName);
        }
    }
}

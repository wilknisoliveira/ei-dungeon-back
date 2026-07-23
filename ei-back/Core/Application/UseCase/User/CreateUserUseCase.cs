using AutoMapper;
using ei_back.Core.Application.Repository;
using ei_back.Core.Application.UseCase.User.Dtos;
using ei_back.Core.Application.UseCase.User.Interfaces;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using ei_back.Core.Domain.Enums;

namespace ei_back.Core.Application.UseCase.User
{
    public class CreateUserUseCase : ICreateUserUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public CreateUserUseCase(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserDtoResponse> Handler(UserDtoRequest userDtoRequest)
        {
            var existing = await _userRepository.FindByUserName(userDtoRequest.UserName);
            if (existing != null)
                throw new BadRequestException("Invalid registration data.");

            var user = _mapper.Map<Domain.Entity.User>(userDtoRequest);

            user.Role = UserRole.CommonUser;
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.CreatedAt = DateTimeOffset.UtcNow;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            var userResponse = await _userRepository.CreateAsync(user);

            return _mapper.Map<UserDtoResponse>(userResponse);
        }
    }
}

using AutoMapper;
using ei_back.Core.Application.Repository;
using ei_back.Core.Application.UseCase.User;
using ei_back.Core.Application.UseCase.User.Dtos;
using ei_back.Core.Application.UseCase.User.Interfaces;
using ei_back.Core.Domain.Enums;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using UserEntity = ei_back.Core.Domain.Entity.User;

namespace ei_back.Tests.Core.Application.UseCase.User
{
    public class CreateUserUseCaseTests
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ICreateUserUseCase _useCase;

        public CreateUserUseCaseTests()
        {
            _userRepository = A.Fake<IUserRepository>();
            _mapper = A.Fake<IMapper>();
            _useCase = new CreateUserUseCase(_userRepository, _mapper);
        }

        [Fact]
        public async Task Handler_WhenUsernameAlreadyExists_ThrowsBadRequestException()
        {
            var request = new UserDtoRequest
            {
                UserName = "existing",
                FullName = "Existing",
                Password = "pass123",
                Email = "e@e.com"
            };
            var existingUser = new UserEntity("existing", "Existing", "e@e.com", "hash", UserRole.CommonUser);

            A.CallTo(() => _userRepository.FindByUserName("existing", CancellationToken.None))
                .Returns(Task.FromResult<UserEntity?>(existingUser));

            Func<Task> act = () => _useCase.Handler(request);

            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Invalid registration data.");
        }

        [Fact]
        public async Task Handler_WhenValidRequest_CreatesUserAndReturnsDto()
        {
            var request = new UserDtoRequest
            {
                UserName = "newuser",
                FullName = "New User",
                Password = "pass123",
                Email = "new@test.com"
            };

            var mappedUser = new UserEntity("newuser", "New User", "new@test.com", "pass123", UserRole.CommonUser);
            var savedUser = new UserEntity("newuser", "New User", "new@test.com", "$2a$11$hashedpassword", UserRole.CommonUser);
            typeof(ei_back.Core.Domain.Entity.Base).GetProperty("Id")!.SetValue(savedUser, Guid.NewGuid());

            var response = new UserDtoResponse
            {
                UserName = "newuser",
                FullName = "New User",
                Email = "new@test.com"
            };

            A.CallTo(() => _userRepository.FindByUserName("newuser", CancellationToken.None))
                .Returns(Task.FromResult<UserEntity?>(null));
            A.CallTo(() => _mapper.Map<UserEntity>(request))
                .Returns(mappedUser);
            A.CallTo(() => _userRepository.CreateAsync(A<UserEntity>.That.Matches(u =>
                u.UserName == "newuser" && u.Password.StartsWith("$2")), CancellationToken.None))
                .Returns(Task.FromResult(savedUser));
            A.CallTo(() => _mapper.Map<UserDtoResponse>(savedUser))
                .Returns(response);

            var result = await _useCase.Handler(request);

            result.Should().Be(response);
            mappedUser.Role.Should().Be(UserRole.CommonUser);
        }
    }
}

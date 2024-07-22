using AutoMapper;
using ei_back.Domain.Role;
using ei_back.Domain.User;
using ei_back.Domain.User.Interfaces;

namespace ei_back.Tests.Domain.User
{
    public class UserServiceTests
    {
        //SUT
        private readonly IUserService _userService;

        //Dependencies
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserServiceTests()
        {
            //Dependencies
            _userRepository = A.Fake<IUserRepository>();
            _mapper = A.Fake<IMapper>();

            //SUT
            _userService = new UserService(_userRepository, _mapper);
        }

        //This is a example of how make your unitary tests
        [Fact]
        public async void TestUserService_WhenFindByIdAsync_ShouldReturnUser()
        {
            //Arrange
            var userId = Guid.NewGuid();
            CancellationToken cancellationToken = default;

            var userResponse = A.Fake<UserEntity>();
            A.CallTo(() => _userRepository.FindByIdAsync(userId, cancellationToken))
                .Returns(Task.FromResult(userResponse));

            //Act
            var result = await _userService.FindByIdAsync(userId);

            //Assert
            result.Should().BeAssignableTo<UserEntity>();
            result.Should().NotBeNull();
        }
    }
}

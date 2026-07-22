using AutoMapper;
using ei_back.Core.Application.Repository;
using ei_back.Core.Application.UseCase.User;
using ei_back.Core.Application.UseCase.User.Dtos;
using ei_back.Core.Application.UseCase.User.Interfaces;
using ei_back.Core.Domain.Enums;
using ei_back.Infrastructure.Context;
using UserEntity = ei_back.Core.Domain.Entity.User;

namespace ei_back.Tests.Core.Application.UseCase.User
{
    public class GetUserUseCaseTests
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IGetUserUseCase _useCase;

        public GetUserUseCaseTests()
        {
            _userRepository = A.Fake<IUserRepository>();
            _mapper = A.Fake<IMapper>();
            _useCase = new GetUserUseCase(_userRepository, _mapper);
        }

        [Fact]
        public async Task Handler_ReturnsPagedSearchWithMappedUsers()
        {
            var users = new List<UserEntity>
            {
                new UserEntity("user1", "User One", "u1@t.com", "hash", UserRole.CommonUser),
                new UserEntity("user2", "User Two", "u2@t.com", "hash", UserRole.CommonUser)
            };

            var dtos = users.Select(u => new UserGetDtoResponse
            {
                Id = Guid.NewGuid(),
                UserName = u.UserName,
                FullName = u.FullName,
                Email = u.Email
            }).ToList();

            A.CallTo(() => _userRepository.FindWithPagedSearchAsync("asc", 10, 1, 0, null, "user_name", "users", CancellationToken.None))
                .Returns(Task.FromResult(users.Cast<ei_back.Core.Domain.Entity.User>().ToList()));
            A.CallTo(() => _userRepository.GetCountAsync(null, "user_name", "users", CancellationToken.None))
                .Returns(Task.FromResult(2));
            A.CallTo(() => _mapper.Map<UserGetDtoResponse>(users[0]))
                .Returns(dtos[0]);
            A.CallTo(() => _mapper.Map<UserGetDtoResponse>(users[1]))
                .Returns(dtos[1]);

            var result = await _useCase.Handler(null, "asc", 10, 1);

            result.CurrentPage.Should().Be(1);
            result.PageSize.Should().Be(10);
            result.TotalResults.Should().Be(2);
            result.SortDirections.Should().Be("asc");
            result.List.Should().HaveCount(2);
        }
    }
}

using AutoMapper;
using ei_back.Core.Application.Service.User.Interfaces;
using ei_back.Core.Application.UseCase.Role.Dtos;
using ei_back.Core.Application.UseCase.Role.Interfaces;

namespace ei_back.Core.Application.UseCase.Role
{
    public class ApplyRolesUseCase : IApplyRolesUseCase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public ApplyRolesUseCase(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<ApplyRoleDtoResponse> Handler(ApplyRoleDtoRequest applyRoleDtoRequest)
        {
            var user = await _userService.FindUserAndRoles(applyRoleDtoRequest.Id);
            user.Role = applyRoleDtoRequest.role;

            var userResponse = _userService.Update(user);

            var applyRoleDtoResponse = _mapper.Map<ApplyRoleDtoResponse>(userResponse);

            return applyRoleDtoResponse;
        }
    }
}

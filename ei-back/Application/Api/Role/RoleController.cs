using ei_back.Application.Api.FunTranslate;
using ei_back.Application.Api.Role.Dtos;
using ei_back.Application.Usecases.Role.Interfaces;
using ei_back.Infrastructure.Context.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ei_back.Application.Api.Role
{
    [ApiController]
    [Route("api/user/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly ILogger<RoleController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICreateRoleUseCase _createRoleUseCase;
        private readonly IGetAllRoleUseCase _getAllRoleUseCase;
        private readonly IApplyRolesUseCase _applyRolesUseCase;

        public RoleController(
            ILogger<RoleController> logger,
            IUnitOfWork unitOfWork,
            ICreateRoleUseCase createRoleUseCase,
            IGetAllRoleUseCase getAllRoleUseCase,
            IApplyRolesUseCase applyRolesUseCase)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _createRoleUseCase = createRoleUseCase;
            _getAllRoleUseCase = getAllRoleUseCase;
            _applyRolesUseCase = applyRolesUseCase;
        }

        [HttpPost]
        [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] RoleDto roleDtoRequest, CancellationToken cancellationToken = default)
        {
            if (roleDtoRequest == null) return BadRequest();
            var response = await _createRoleUseCase.Handler(roleDtoRequest);
            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("API: New role created");

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType(typeof(RoleDtoResponse), StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin, CommonUser")]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("API: Getting all the roles");

            return Ok(await _getAllRoleUseCase.Handler());
        }

        [HttpPut("apply")]
        [ProducesResponseType(typeof(ApplyRoleDtoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApplyRoles(
            [FromBody] ApplyRoleDtoRequest applyRoleDtoRequest,
            CancellationToken cancellationToken = default)
        {
            if (applyRoleDtoRequest == null) return BadRequest();
            var response = await _applyRolesUseCase.Handler(applyRoleDtoRequest);
            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("API: Roles applied to user");

            return Ok(response);
        }
    }
}

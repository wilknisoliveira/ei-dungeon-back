﻿using ei_back.Core.Application.UseCase.Role.Dtos;
using ei_back.Core.Application.UseCase.Role.Interfaces;
using ei_back.Infrastructure.Context.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ei_back.UserInterface.Api
{
    [ApiController]
    [Route("api/user/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly ILogger<RoleController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGetAllRoleUseCase _getAllRoleUseCase;
        private readonly IApplyRolesUseCase _applyRolesUseCase;

        public RoleController(
            ILogger<RoleController> logger,
            IUnitOfWork unitOfWork,
            IGetAllRoleUseCase getAllRoleUseCase,
            IApplyRolesUseCase applyRolesUseCase)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _getAllRoleUseCase = getAllRoleUseCase;
            _applyRolesUseCase = applyRolesUseCase;
        }

        [HttpGet]
        [ProducesResponseType(typeof(RoleDtoResponse), StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin")]
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

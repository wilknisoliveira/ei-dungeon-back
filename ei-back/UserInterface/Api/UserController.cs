﻿using ei_back.Core.Application.UseCase.User.Dtos;
using ei_back.Core.Application.UseCase.User.Interfaces;
using ei_back.Infrastructure.Context;
using ei_back.Infrastructure.Context.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ei_back.UserInterface.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICreateUserUseCase _createUserUseCase;
        private readonly IGetUserUseCase _getUserUseCase;

        public UserController(
            ILogger<UserController> logger,
            ICreateUserUseCase createUserUseCase,
            IUnitOfWork unitOfWork,
            IGetUserUseCase getUserUseCase)
        {
            _logger = logger;
            _createUserUseCase = createUserUseCase;
            _unitOfWork = unitOfWork;
            _getUserUseCase = getUserUseCase;
        }

        [HttpPost]
        [ProducesResponseType(typeof(UserDtoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] UserDtoRequest userDtoRequest, CancellationToken cancellationToken = default)
        {
            if (userDtoRequest == null) return BadRequest();
            var userDtoResponse = await _createUserUseCase.Handler(userDtoRequest);
            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("API: New user created - " + userDtoRequest.UserName);

            return Ok(userDtoResponse);
        }

        [HttpGet("{sortDirection}/{pageSize}/{page}")]
        [ProducesResponseType(typeof(PagedSearchDto<UserGetDtoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Get(
            [FromQuery] string? name,
            string sortDirection,
            int pageSize,
            int page)
        {
            _logger.LogInformation("API: Getting paged user list");

            return Ok(await _getUserUseCase.Handler(name, sortDirection, pageSize, page));
        }
    }
}

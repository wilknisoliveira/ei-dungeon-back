﻿using ei_back.Core.Application.UseCase.User.Dtos;
using ei_back.Core.Application.UseCase.User.Interfaces;
using ei_back.Infrastructure.Context.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace ei_back.UserInterface.Api
{
    [ApiController]
    [Route("api/user/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ISignInUseCase _signInUseCase;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStringLocalizer<AuthController> _stringLocalizer;
        private readonly ILogger<AuthController> _logger;
        private readonly IChangePasswordUseCase _changePasswordUseCase;


        public AuthController(
            ISignInUseCase signInUseCase,
            IUnitOfWork unitOfWork,
            IStringLocalizer<AuthController> stringLocalizer,
            ILogger<AuthController> logger,
            IChangePasswordUseCase changePasswordUseCase)
        {
            _signInUseCase = signInUseCase;
            _unitOfWork = unitOfWork;
            _stringLocalizer = stringLocalizer;
            _logger = logger;
            _changePasswordUseCase = changePasswordUseCase;
        }

        [HttpPost]
        [ProducesResponseType(typeof(TokenDtoReponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("signin")]
        public IActionResult Signin([FromBody] LoginDtoRequest loginDtoRequest)
        {
            if (loginDtoRequest == null) return BadRequest(_stringLocalizer["AuthInvalidRequest"].Value);

            var token = _signInUseCase.Handler(loginDtoRequest);
            _unitOfWork.Commit();

            if (token == null) return Unauthorized();

            _logger.LogInformation("API: Logged user - " + loginDtoRequest.UserName);

            return Ok(token);
        }

        [HttpPatch]
        [ProducesResponseType(typeof(UserGetDtoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin, CommonUser, PremiumUser")]
        [Route("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordDtoRequest passwordDtoRequest)
        {
            if (passwordDtoRequest == null) return BadRequest(_stringLocalizer["AuthInvalidRequest"].Value);

            var userChanged = await _changePasswordUseCase.Handler(passwordDtoRequest);

            if (userChanged == null) return BadRequest();

            await _unitOfWork.CommitAsync();

            _logger.LogInformation("API: Password changed for user - " + userChanged.UserName);

            return Ok(userChanged);
        }
    }
}

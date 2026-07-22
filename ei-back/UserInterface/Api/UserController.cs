using ei_back.Core.Application.UseCase.User.Dtos;
using ei_back.Core.Application.UseCase.User.Interfaces;
using ei_back.Core.Application.Utils;
using ei_back.Infrastructure.Context.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
        private readonly ICheckUserInfoUseCase _checkUserInfoUseCase;

        public UserController(
            ILogger<UserController> logger,
            ICreateUserUseCase createUserUseCase,
            IUnitOfWork unitOfWork,
            IGetUserUseCase getUserUseCase,
            ICheckUserInfoUseCase checkUserInfoUseCase)
        {
            _logger = logger;
            _createUserUseCase = createUserUseCase;
            _unitOfWork = unitOfWork;
            _getUserUseCase = getUserUseCase;
            _checkUserInfoUseCase = checkUserInfoUseCase;
        }

        /// <summary>Creates a new user account</summary>
        /// <remarks>
        /// Registration endpoint. No authentication required.
        ///
        /// Request body (UserDtoRequest):
        ///   - UserName (string, 4-20 characters, required)
        ///   - FullName (string, 4-50 characters, required)
        ///   - Password (string, 4-50 characters, required)
        ///   - Email (string, valid email format, required)
        ///
        /// Response 200 (UserDtoResponse):
        ///   - UserName (string)
        ///   - FullName (string)
        ///   - Email (string)
        ///
        /// Response 400: Username already exists or validation fails.
        ///
        /// Password is BCrypt-hashed server-side. Duplicate username returns
        /// a generic "Invalid registration data." message to prevent user
        /// enumeration. New users default to CommonUser role.
        /// </remarks>
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

        /// <summary>Checks availability of username and/or email</summary>
        /// <remarks>
        /// Rate limited: 10 requests per minute sliding window (PublicApi policy).
        /// No authentication required. Public endpoint.
        ///
        /// Query parameters (both optional):
        ///   - username (string): checks if this username is taken
        ///   - email (string): checks if this email is taken
        ///
        /// Response 200 (UserInfoCheckResponseDto):
        ///   - UsernameAvailable (bool)
        ///   - EmailAvailable (bool)
        ///
        /// Only checks parameters that are provided. Omitted parameters
        /// default to available (true).
        /// </remarks>
        [EnableRateLimiting("PublicApi")]
        [HttpGet("check-userinfo")]
        [ProducesResponseType(typeof(UserInfoCheckResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckUserInfo([FromQuery] string? username, [FromQuery] string? email)
        {
            var result = await _checkUserInfoUseCase.Handler(username, email);
            return Ok(result);
        }

        /// <summary>Lists users with paginated search (Admin only)</summary>
        /// <remarks>
        /// Requires authentication with Admin role.
        ///
        /// Query parameters:
        ///   - sortDirection (string): "asc" or "desc"
        ///   - pageSize (int): results per page
        ///   - page (int): page number (1-based)
        ///   - name (string?, optional): filters results by username
        ///
        /// Response 200 (PagedSearchDto&lt;UserGetDtoResponse&gt;):
        ///   - CurrentPage (int)
        ///   - PageSize (int)
        ///   - TotalResults (int)
        ///   - SortDirection (string)
        ///   - Items (UserGetDtoResponse[]): array of users with Id, UserName,
        ///     FullName, Email
        ///
        /// Response 400: Invalid sort direction, page size, or page number.
        /// Sort direction defaults to "desc" if not "asc".
        /// Page size defaults to 10 if less than 1.
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(PagedSearchDto<UserGetDtoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Get(
            [FromQuery] string? name,
            [FromQuery] string sortDirection,
            [FromQuery] int pageSize,
            [FromQuery] int page,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("API: Getting paged user list");

            return Ok(await _getUserUseCase.Handler(name, sortDirection, pageSize, page, cancellationToken));
        }
    }
}

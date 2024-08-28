using ei_back.Application.Api.Game.Dtos;
using ei_back.Application.Api.Play.Dtos;
using ei_back.Application.Usecases.Play.Interfaces;
using ei_back.Application.Usecases.User.Interfaces;
using ei_back.Infrastructure.Context;
using ei_back.Infrastructure.Context.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ei_back.Application.Api.Play
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayController : ControllerBase
    {
        private readonly ILogger<PlayController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGetUserNameUseCase _getUserNameUseCase;
        private readonly IGetPlaysUseCase _getPlaysUseCase;


        public PlayController(
            ILogger<PlayController> logger,
            IUnitOfWork unitOfWork,
            IGetUserNameUseCase getUserNameUseCase,
            IGetPlaysUseCase getPlaysUseCase)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _getUserNameUseCase = getUserNameUseCase;
            _getPlaysUseCase = getPlaysUseCase;
        }

        [HttpGet("{gameId}/{pageSize}")]
        [ProducesResponseType(typeof(PagedSearchDto<PlayDtoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin, CommonUser")]
        public async Task<IActionResult> Get(
            Guid gameId,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var userName = _getUserNameUseCase.Handler(User);

            if (userName.IsNullOrEmpty())
            {
                var errorMessage = "Something went wrong while attempting to get the user logged credential.";
                _logger.LogError(errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, errorMessage);
            }

            _logger.LogInformation($"Get the play list of the game {gameId} to user {userName}");

            var response = await _getPlaysUseCase.Handler(gameId, pageSize, userName, cancellationToken);

            return Ok(response);
        }
    }
}

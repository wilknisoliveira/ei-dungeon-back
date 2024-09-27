using ei_back.Core.Application.UseCase.Play.Dtos;
using ei_back.Core.Application.UseCase.Play.Interfaces;
using ei_back.Core.Application.UseCase.User.Interfaces;
using ei_back.Infrastructure.Context;
using ei_back.Infrastructure.Context.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ei_back.UserInterface.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayController : ControllerBase
    {
        private readonly ILogger<PlayController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGetUserNameUseCase _getUserNameUseCase;
        private readonly IGetPlaysUseCase _getPlaysUseCase;
        private readonly INewUserPlayUseCase _newUserPlayUseCase;


        public PlayController(
            ILogger<PlayController> logger,
            IUnitOfWork unitOfWork,
            IGetUserNameUseCase getUserNameUseCase,
            IGetPlaysUseCase getPlaysUseCase,
            INewUserPlayUseCase newUserPlayUseCase)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _getUserNameUseCase = getUserNameUseCase;
            _getPlaysUseCase = getPlaysUseCase;
            _newUserPlayUseCase = newUserPlayUseCase;
        }

        [HttpGet("{gameId}/{pageSize}")]
        [ProducesResponseType(typeof(PagedSearchDto<PlayDtoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = "Admin, CommonUser, PremiumUser")]
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


        [HttpPost]
        [ProducesResponseType(typeof(List<PlayDtoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin, PremiumUser")]
        public async Task<IActionResult> CreateUserPlay([FromBody] PlayDtoRequest playDtoRequest, CancellationToken cancellationToken = default)
        {
            var userName = _getUserNameUseCase.Handler(User);

            if (userName.IsNullOrEmpty())
            {
                var errorMessage = "Something went wrong while attempting to get the user logged credential.";
                _logger.LogError(errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, errorMessage);
            }

            _logger.LogInformation($"New play by user {userName} to game {playDtoRequest.GameId}.");

            var response = await _newUserPlayUseCase.Handler(playDtoRequest, userName, cancellationToken);

            _logger.LogInformation($"User play process success.");

            return Ok(response);
        }


    }
}

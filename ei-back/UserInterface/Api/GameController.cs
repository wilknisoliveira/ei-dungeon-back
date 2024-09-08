using ei_back.Core.Application.UseCase.Game.Dtos;
using ei_back.Core.Application.UseCase.Game.Interfaces;
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
    public class GameController : ControllerBase
    {
        private readonly ILogger<GameController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICreateGameUseCase _createGameUseCase;
        private readonly IGetUserNameUseCase _getUserNameUseCase;
        private readonly IGetGamesUseCase _getGamesUseCase;

        public GameController(
            ILogger<GameController> logger,
            IUnitOfWork unitOfWork,
            ICreateGameUseCase createGameUseCase,
            IGetUserNameUseCase getUserNameUseCase,
            IGetGamesUseCase getGamesUseCase)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _createGameUseCase = createGameUseCase;
            _getUserNameUseCase = getUserNameUseCase;
            _getGamesUseCase = getGamesUseCase;
        }

        [HttpPost]
        [ProducesResponseType(typeof(GameDtoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin, CommonUser")]
        public async Task<IActionResult> Create([FromBody] GameDtoRequest gameDtoRequest, CancellationToken cancellationToken = default)
        {
            var userName = _getUserNameUseCase.Handler(User);

            if (userName.IsNullOrEmpty())
            {
                var errorMessage = "Something went wrong while attempting to get the user logged credential.";
                _logger.LogError(errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, errorMessage);
            }

            _logger.LogInformation($"Creating {gameDtoRequest.Name} game to user {userName}...");

            var gameDtoResponse = await _createGameUseCase.Handler(gameDtoRequest, userName, cancellationToken);
            var changedItems = await _unitOfWork.CommitAsync(cancellationToken);

            if (changedItems == 0)
            {
                var errorMessage = "Something went wrong while attempting to create game.";
                _logger.LogError(errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, errorMessage);
            }

            _logger.LogInformation($"Game {gameDtoRequest.Name} created.");

            return Ok(gameDtoResponse);
        }

        [HttpGet("{sortDirection}/{pageSize}/{page}")]
        [ProducesResponseType(typeof(PagedSearchDto<GameDtoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin, CommonUser")]
        public async Task<IActionResult> Get(
            string sortDirection,
            int pageSize,
            int page,
            CancellationToken cancellationToken)
        {
            var userName = _getUserNameUseCase.Handler(User);

            if (userName.IsNullOrEmpty())
            {
                var errorMessage = "Something went wrong while attempting to get the user logged credential.";
                _logger.LogError(errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, errorMessage);
            }

            _logger.LogInformation($"Get the list game to user {userName}");

            var response = await _getGamesUseCase.Handler(sortDirection, pageSize, page, userName, cancellationToken);

            return Ok(response);
        }
    }
}

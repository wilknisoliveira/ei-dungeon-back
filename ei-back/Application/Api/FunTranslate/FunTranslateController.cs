using ei_back.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ei_back.Application.Api.FunTranslate
{
    [Route("api/[controller]")]
    [ApiController]
    public class FunTranslateController : ControllerBase
    {
        private readonly IFunTranslateApiHttpService _externalApiHttpService;
        private readonly ILogger<FunTranslateController> _logger;

        public FunTranslateController(IFunTranslateApiHttpService externalApiHttpService, ILogger<FunTranslateController> logger)
        {
            _externalApiHttpService = externalApiHttpService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin, CommonUser")]
        public async Task<IActionResult> Get([FromBody] string textRequest, CancellationToken cancellationToken = default)
        {
            if (textRequest.IsNullOrEmpty()) return BadRequest();

            _logger.LogInformation("API: Requesting translation to FunTranslate.");

            return Ok(await _externalApiHttpService.GetValyrianTranslate(textRequest));
        }
    }
}

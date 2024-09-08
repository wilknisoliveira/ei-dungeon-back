using ei_back.Core.Application.UseCase.GenerativeAi.Dtos;
using ei_back.Infrastructure.Exceptions.ExceptionTypes;
using ei_back.Infrastructure.ExternalAPIs.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ei_back.UserInterface.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenerativeAIController(
        ILogger<RoleController> logger,
        IGenerativeAIApiHttpService generativeAIApiHttpService) : ControllerBase
    {
        private readonly ILogger<RoleController> _logger = logger;
        private readonly IGenerativeAIApiHttpService _generativeAIApiHttpService = generativeAIApiHttpService;


        [HttpPost]
        [ProducesResponseType(typeof(PromptDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin, CommonUser")]
        public async Task<IActionResult> Create([FromBody] PromptDto promptDto, CancellationToken cancellationToken = default)
        {
            if (promptDto == null || promptDto.Prompt == "") throw new BadRequestException("Verify if the content is valid.");

            var promptResponse = await _generativeAIApiHttpService.GenerateSimpleResponse(promptDto.Prompt, cancellationToken);

            var response = new PromptDto() { Prompt = promptResponse };

            return Ok(response);
        }
    }
}

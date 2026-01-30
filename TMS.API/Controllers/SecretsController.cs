using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using TMS.Core.Queries;
using TMS.Models.Secrets;


namespace TMS.API.Controllers
{
    [ApiController]
    [Authorize]
    [RequireHttps]
    [Route("api/v1/secrets")]
    public sealed class SecretsController : ControllerBase
    {
        private readonly ILogger<SecretsController> _logger;
        private readonly IMediator _mediator;

        public SecretsController(ILogger<SecretsController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet("arcgis")]
        [RequiredScope("Arcgis.Read")]
        [ProducesResponseType<ArcgisSecret>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status499ClientClosedRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetArcgisSecretAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received request to get ArcGIS secret.");

            ArcgisSecret secret = await _mediator.Send(new GetArcgisSecretQuery(), cancellationToken);

            _logger.LogInformation("Successfully retrieved ArcGIS secret");
            
            return Ok(secret);
        }
    }
}
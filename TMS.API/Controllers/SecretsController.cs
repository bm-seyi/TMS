using System.Diagnostics;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using TMS.Application.Queries;
using TMS.Domain.Secrets;


namespace TMS.API.Controllers
{
    [ApiController]
    [Authorize]
    [RequireHttps]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Route("api/v{version:apiVersion}/secrets")]
    public sealed class SecretsController(ILogger<SecretsController> logger, IMediator mediator) : ControllerBase
    {
        private readonly ILogger<SecretsController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        private static readonly ActivitySource _activitySource = new ActivitySource("TMS.API");

        [HttpGet("arcgis")]
        [MapToApiVersion(1.0)]
        [RequiredScope("Arcgis.Read")]
        [ProducesResponseType<ArcgisSecret>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status499ClientClosedRequest)]
        public async Task<IActionResult> GetArcgisSecretAsync(CancellationToken cancellationToken)
        {
            using Activity? _ = _activitySource.StartActivity("SecretsController.GetArcgisSecretAsync");
            
            _logger.LogInformation("Received request to get ArcGIS secret.");

            ArcgisSecret secret = await _mediator.Send(new GetArcgisSecretQuery(), cancellationToken);

            _logger.LogInformation("Successfully retrieved ArcGIS secret");
            
            return Ok(secret);
        }
    }
}
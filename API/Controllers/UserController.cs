namespace API.Controllers;

/// <summary>
/// UserController
/// </summary>
/// <param name="mediator"></param>
/// <param name="logger"></param>
[ApiVersion("1.0")]
[ApiVersion(0.9, Deprecated = true)]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class UserController(IMediator mediator, ILogger logger) : MControllerBase(mediator, logger)
{
    /// <summary>
    /// test get use
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Get(CancellationToken cancellationToken)
    {
        Logger.Information("Get user");
        return Ok("hello");
    }
}
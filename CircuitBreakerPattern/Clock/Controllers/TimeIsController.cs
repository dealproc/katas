namespace Clock.Controllers;

using Microsoft.AspNetCore.Mvc;

public class TimeIsController : Controller {
    private readonly ILogger _logger;

    public TimeIsController(ILoggerFactory loggerFactory) {
        _logger = loggerFactory.CreateLogger<TimeIsController>();
    }

    public IActionResult Index() {
        _logger.LogInformation($"Clock is being queried on {DateTime.UtcNow} and will {(Switches.ApiShouldThrowException ? "NOT " : "")}return the current time.");
        return Switches.ApiShouldThrowException
            ? BadRequest()
            : Json(new { Time = DateTime.UtcNow });
    }
}
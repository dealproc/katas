namespace Clock.Controllers;

using Microsoft.AspNetCore.Mvc;

public class TimeIsController : Controller {
    public IActionResult Index() => Switches.ApiShouldThrowException
        ? BadRequest()
        : Json(new { Time = DateTime.UtcNow });
}
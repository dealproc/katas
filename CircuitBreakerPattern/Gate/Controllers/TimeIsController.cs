using Microsoft.AspNetCore.Mvc;

namespace Gate.Controllers;

[ApiController]
[Route("[controller]")]
public class TimeIsController : ControllerBase
{
    private static HttpClient _client = new HttpClient() {
        BaseAddress = new Uri("https://localhost:7120/")
    };

    private readonly ILogger<TimeIsController> _logger;

    public TimeIsController(ILogger<TimeIsController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var response = await _client.GetAsync("TimeIs");

        try {
            response.EnsureSuccessStatusCode();
        }
        catch {
            return StatusCode((int)response.StatusCode);
        }

        return Content(await response.Content.ReadAsStringAsync());
    }
}

using Microsoft.AspNetCore.Mvc;

using Toolkit;

namespace Gate.Controllers;

[ApiController]
[Route("[controller]")]
public class TimeIsController : ControllerBase
{
    private static HttpClient _client = new HttpClient() {
        BaseAddress = new Uri("https://localhost:7120/")
    };
    private Breaker<IActionResult> _breaker;

    private readonly ILogger<TimeIsController> _logger;

    public TimeIsController(IClock clock, ILogger<TimeIsController> logger) {
        _breaker = new Breaker<IActionResult>(clock);
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try {
            return await _breaker.Execute(async () => {
                var response = await _client.GetAsync("TimeIs");

                response.EnsureSuccessStatusCode();

                return Content(await response.Content.ReadAsStringAsync());
            });
        } catch (TimeoutException) {
            return this.Problem(detail: "service is unavailable.");
        } catch {
            throw;
        }
    }
}

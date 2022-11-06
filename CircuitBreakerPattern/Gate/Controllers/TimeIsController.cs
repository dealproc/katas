using Microsoft.AspNetCore.Mvc;

namespace Gate.Controllers;

[ApiController]
[Route("[controller]")]
public class TimeIsController : ControllerBase
{
    private static HttpClient _client = new HttpClient() {
        BaseAddress = new Uri("https://localhost:7120/")
    };
    private static Breaker<IActionResult> _breaker;
    static TimeIsController()
    {
        _breaker = new Breaker<IActionResult>(){
            WhenOpen = Task.FromResult<IActionResult>(new BadRequestResult())
        };
    }

    private readonly ILogger<TimeIsController> _logger;

    public TimeIsController(ILogger<TimeIsController> logger)
    {
        _logger = logger;
        _breaker.Logger = _logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try{
        return await _breaker.Execute(async () => {
            var response = await _client.GetAsync("TimeIs");

            response.EnsureSuccessStatusCode();

            return Content(await response.Content.ReadAsStringAsync());
        });
        }catch{
            return BadRequest();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WeatherAPI.Application.Services;

namespace WeatherAPI.Controllers;

[ApiController][Route("api/[controller]")]
[Authorize]
public class AiController : ControllerBase {
    private readonly AiPredictionService _ai;
    public AiController(AiPredictionService ai) => _ai = ai;

    [HttpGet("predictions/customer/{customerId}")]
    public async Task<IActionResult> GetPredictions(Guid customerId) {
        var result = await _ai.PredictFailureAsync(customerId);
        return Ok(result);
    }
}

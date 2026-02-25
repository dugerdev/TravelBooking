using TravelBooking.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace TravelBooking.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestimonialsController : BaseController
{
    private readonly ITestimonialService _testimonialService;

    public TestimonialsController(ITestimonialService testimonialService)
    {
        _testimonialService = testimonialService;
    }

    /// <summary>
    /// Get all approved testimonials (public endpoint)
    /// </summary>
    [HttpGet("approved")]
    public async Task<IActionResult> GetApproved()
    {
        var result = await _testimonialService.GetApprovedAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
